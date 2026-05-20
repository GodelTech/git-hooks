using GitHooks.Handlers;
using GitHooks.Infrastructure.Cli;
using GitHooks.Infrastructure.Git;
using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.Expansion.Exceptions;
using GitHooks.Workflow.Application.Parsing.Exceptions;
using GitHooks.Workflow.Application.Resolution;
using GitHooks.Workflow.Domain.Model;

using Spectre.Console;

namespace GitHooks.Tests.Handlers;

public sealed class RunHandlerTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public RunHandlerTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public async Task HandleAsync_WithUnavailableGit_ReturnsOne()
    {
        var gitCommandLine = new FakeGitCommandLine { IsAvailable = false };
        var pipelineResolver = new FakePipelineResolver();
        var writer = new StringWriter();
        var console = CreateConsole(writer);
        var handler = new RunHandler(gitCommandLine, pipelineResolver, console);

        var result = await handler.HandleAsync("pipeline.yaml", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, result);
        Assert.False(pipelineResolver.ResolveCalled);
        Assert.Contains("Git not found in PATH", writer.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithValidPipeline_CallsResolverAndReturnsZero()
    {
        var pipelineFilePath = WriteTempFile("pipeline.yaml", "steps:\n  - script: echo hello\n");
        var parameterOverrides = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["branch"] = "main",
        };

        var gitCommandLine = new FakeGitCommandLine();
        var pipelineResolver = new FakePipelineResolver();
        var handler = new RunHandler(gitCommandLine, pipelineResolver, CreateConsole(new StringWriter()));

        var result = await handler.HandleAsync(pipelineFilePath, parameterOverrides, TestContext.Current.CancellationToken);

        Assert.Equal(0, result);
        Assert.True(pipelineResolver.ResolveCalled);
        Assert.Equal(PipelineSource.LocalFile(Path.GetFullPath(pipelineFilePath)), pipelineResolver.Source);
        Assert.Same(parameterOverrides, pipelineResolver.ParameterOverrides);
    }

    [Fact]
    public async Task HandleAsync_WhenResolverThrowsPipelineParsingException_ReturnsOne()
    {
        var pipelineFilePath = WriteTempFile("pipeline.yaml", "steps:\n");
        var writer = new StringWriter();
        var gitCommandLine = new FakeGitCommandLine();
        var pipelineResolver = new FakePipelineResolver
        {
            ExceptionToThrow = new PipelineParsingException("Invalid YAML.", CreateSpan("pipeline.yaml", 3, 7)),
        };

        var handler = new RunHandler(gitCommandLine, pipelineResolver, CreateConsole(writer));

        var result = await handler.HandleAsync(pipelineFilePath, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, result);
        Assert.Contains("pipeline.yaml:3:7: Invalid YAML.", writer.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WhenResolverThrowsPipelineParameterBindingException_ReturnsOne()
    {
        var pipelineFilePath = WriteTempFile("pipeline.yaml", "steps:\n");
        var writer = new StringWriter();
        var gitCommandLine = new FakeGitCommandLine();
        var pipelineResolver = new FakePipelineResolver
        {
            ExceptionToThrow = new PipelineParameterBindingException("Binding failed.", CreateSpan("pipeline.yaml", 4, 2)),
        };

        var handler = new RunHandler(gitCommandLine, pipelineResolver, CreateConsole(writer));

        var result = await handler.HandleAsync(pipelineFilePath, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, result);
        Assert.Contains("pipeline.yaml:4:2: Binding failed.", writer.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WhenResolverThrowsPipelineTemplateExpansionException_ReturnsOne()
    {
        var pipelineFilePath = WriteTempFile("pipeline.yaml", "steps:\n");
        var writer = new StringWriter();
        var gitCommandLine = new FakeGitCommandLine();
        var pipelineResolver = new FakePipelineResolver
        {
            ExceptionToThrow = new PipelineTemplateExpansionException(
                "Template failed.",
                CreateSpan("pipeline.yaml", 5, 9),
                [PipelineSource.LocalFile("root.yaml"), PipelineSource.LocalFile("template.yaml")]),
        };

        var handler = new RunHandler(gitCommandLine, pipelineResolver, CreateConsole(writer));

        var result = await handler.HandleAsync(pipelineFilePath, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, result);
        var output = writer.ToString();

        Assert.Contains("pipeline.yaml:5:9: Template failed.", output, StringComparison.Ordinal);
        Assert.Contains("Include chain:", output, StringComparison.Ordinal);
        Assert.Contains("root.yaml", output, StringComparison.Ordinal);
        Assert.Contains("template.yaml", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WhenResolverThrowsIOException_ReturnsOne()
    {
        var pipelineFilePath = WriteTempFile("pipeline.yaml", "steps:\n");
        var writer = new StringWriter();
        var gitCommandLine = new FakeGitCommandLine();
        var pipelineResolver = new FakePipelineResolver
        {
            ExceptionToThrow = new IOException("Access denied."),
        };

        var handler = new RunHandler(gitCommandLine, pipelineResolver, CreateConsole(writer));

        var result = await handler.HandleAsync(pipelineFilePath, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, result);
        Assert.Contains("Failed to read YAML file: Access denied.", writer.ToString(), StringComparison.Ordinal);
    }

    private static IAnsiConsole CreateConsole(StringWriter writer)
    {
        return AnsiConsole.Create(
            new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.No,
                ColorSystem = ColorSystemSupport.NoColors,
                Interactive = InteractionSupport.No,
                Out = new AnsiConsoleOutput(writer),
            });
    }

    private static SourceSpan CreateSpan(string sourceName, int line, int column)
    {
        var source = new SourceRef(sourceName);
        var position = new SourceLocation(line, column);
        return new SourceSpan(source, position, position);
    }

    private static PipelineNode CreatePipeline()
    {
        return new PipelineNode([], [], [], SourceSpan.Unknown(new SourceRef("pipeline.yaml")));
    }

    private string WriteTempFile(string fileName, string content)
    {
        var filePath = Path.Combine(_tempDir, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }

    private sealed class FakePipelineResolver : IPipelineResolver
    {
        public bool ResolveCalled { get; private set; }

        public PipelineSource Source { get; private set; }

        public IReadOnlyDictionary<string, string>? ParameterOverrides { get; private set; }

        public Exception? ExceptionToThrow { get; init; }

        public Task<PipelineNode> ResolveAsync(
            PipelineSource source,
            IReadOnlyDictionary<string, string>? parameterOverrides = null,
            CancellationToken cancellationToken = default)
        {
            ResolveCalled = true;
            Source = source;
            ParameterOverrides = parameterOverrides;

            return ExceptionToThrow is not null ? throw ExceptionToThrow : Task.FromResult(CreatePipeline());
        }
    }

    private sealed class FakeGitCommandLine : IGitCommandLine
    {
        public bool IsAvailable { get; init; } = true;

        public bool IsInsideRepository { get; init; } = true;

        public CommandLineResult RepositoryRootPathResult { get; init; } = CommandLineResult.FromProcess("C:/repo", string.Empty, 0);

        public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(IsAvailable);
        }

        public Task<bool> IsInsideGitRepositoryAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(IsInsideRepository);
        }

        public Task<CommandLineResult> GetRepositoryRootPathAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RepositoryRootPathResult);
        }

        public Task<CommandLineResult> GetCoreHooksPathAsync(GitConfigScope scope, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<CommandLineResult> SetCoreHooksPathAsync(GitConfigScope scope, string value, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<CommandLineResult> UnsetCoreHooksPathAsync(GitConfigScope scope, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
