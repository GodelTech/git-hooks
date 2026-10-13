using GitHooks.Compilation;
using GitHooks.Compilation.DependencyInjection;
using GitHooks.Handlers;
using GitHooks.Infrastructure.Cli;
using GitHooks.Infrastructure.Git;
using GitHooks.Infrastructure.Yaml.DependencyInjection;
using GitHooks.Validation.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

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
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task HandleAsync_WithUnavailableGit_ReturnsOne()
    {
        var gitCommandLine = new FakeGitCommandLine { IsAvailable = false };
        var writer = new StringWriter();
        var console = CreateConsole(writer);
        var handler = new RunHandler(gitCommandLine, CreateCompiler(), console);

        var result = await handler.HandleAsync("pipeline.yaml", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, result);
        Assert.Contains("Git not found in PATH", writer.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithValidPipeline_ReturnsZero()
    {
        var pipelineFilePath = WriteTempFile("pipeline.yaml", "steps:\n  - script: echo hello\n");
        var writer = new StringWriter();
        var handler = new RunHandler(new FakeGitCommandLine(), CreateCompiler(), CreateConsole(writer));

        var result = await handler.HandleAsync(pipelineFilePath, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(0, result);
        Assert.Contains("[SUCCESS]", writer.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("[FAILED]", writer.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithCompilationErrors_ReturnsOneAndPrintsDiagnostics()
    {
        var pipelineFilePath = WriteTempFile("pipeline.yaml", "steps: []\n");
        var writer = new StringWriter();
        var handler = new RunHandler(new FakeGitCommandLine(), CreateCompiler(), CreateConsole(writer));

        var result = await handler.HandleAsync(pipelineFilePath, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, result);

        var output = writer.ToString();
        Assert.Contains("[ERROR]", output, StringComparison.Ordinal);
        Assert.Contains("Compilation completed with errors.", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithMissingFile_ReturnsOne()
    {
        var writer = new StringWriter();
        var handler = new RunHandler(new FakeGitCommandLine(), CreateCompiler(), CreateConsole(writer));

        var result = await handler.HandleAsync(Path.Combine(_tempDir, "missing.yaml"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, result);
        Assert.Contains("YAML file not found", writer.ToString(), StringComparison.Ordinal);
    }

    private static PipelineCompiler CreateCompiler()
    {
        var services = new ServiceCollection();

        _ = services.AddYamlPipelineCompilation();
        _ = services.AddTargetPipelineCompilation();
        _ = services.AddPipelineValidation();

        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<PipelineCompiler>();
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

    private string WriteTempFile(string fileName, string content)
    {
        var filePath = Path.Combine(_tempDir, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }

    private sealed class FakeGitCommandLine : IGitCommandLine
    {
        public bool IsAvailable { get; init; }
            = true;

        public bool IsInsideRepository { get; init; }
            = true;

        public CommandLineResult RepositoryRootPathResult { get; init; }
            = CommandLineResult.FromProcess("C:/repo", string.Empty, 0);

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
