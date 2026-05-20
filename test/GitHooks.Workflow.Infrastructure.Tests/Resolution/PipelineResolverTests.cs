using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Compilation;
using GitHooks.Workflow.Application.Expansion;
using GitHooks.Workflow.Application.Expansion.Exceptions;
using GitHooks.Workflow.Application.IO;
using GitHooks.Workflow.Application.Parsing.Exceptions;
using GitHooks.Workflow.Application.Resolution;
using GitHooks.Workflow.Domain.Model;
using GitHooks.Workflow.Infrastructure.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Workflow.Infrastructure.Tests.Resolution;

public sealed class PipelineResolverTests
{
    [Fact]
    public void ResolveAsync_WithEmptyFilePath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => PipelineSource.LocalFile(string.Empty));
    }

    [Fact]
    public async Task ResolveAsync_WithMissingFile_ThrowsFileNotFoundException()
    {
        var pipelineContentReader = new StubPipelineContentReader();
        var resolver = CreateResolver(new RecordingPipelineCompiler(), new RecordingPipelineTemplateExpander(), pipelineContentReader);
        var missingFilePath = Path.GetFullPath("missing.yaml");

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => resolver.ResolveAsync(PipelineSource.LocalFile(missingFilePath), cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ResolveAsync_WithValidFile_CompilesAndExpandsPipeline()
    {
        var compiledPipeline = CreatePipeline([CreateScriptStep("echo compiled")]);
        var expandedPipeline = CreatePipeline([CreateScriptStep("echo expanded")]);
        var compiler = new RecordingPipelineCompiler { CompileResult = compiledPipeline };
        var expander = new RecordingPipelineTemplateExpander { ExpandResult = expandedPipeline };
        var pipelineFilePath = Path.GetFullPath("pipeline.yaml");
        var pipelineContentReader = new StubPipelineContentReader(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [pipelineFilePath] = "steps:\n  - script: echo hello\n",
        });
        var resolver = CreateResolver(compiler, expander, pipelineContentReader);
        var parameterOverrides = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["name"] = "value",
        };

        var result = await resolver.ResolveAsync(PipelineSource.LocalFile(pipelineFilePath), parameterOverrides, TestContext.Current.CancellationToken);

        Assert.Same(expandedPipeline, result);
        Assert.Equal("steps:\n  - script: echo hello\n", compiler.Content);
        Assert.Equal(PipelineSource.LocalFile(pipelineFilePath), compiler.Source);
        Assert.Same(parameterOverrides, compiler.ParameterOverrides);
        Assert.Same(compiledPipeline, expander.Pipeline);
        Assert.Equal(PipelineSource.LocalFile(pipelineFilePath), expander.Source);
    }

    [Fact]
    public async Task ResolveAsync_WhenCompilerThrows_PropagatesPipelineParsingException()
    {
        var compiler = new RecordingPipelineCompiler
        {
            ExceptionToThrow = new PipelineParsingException("Invalid YAML.", CreateSpan("pipeline.yaml")),
        };

        var pipelineFilePath = Path.GetFullPath("pipeline.yaml");
        var pipelineContentReader = new StubPipelineContentReader(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [pipelineFilePath] = "steps:\n",
        });
        var resolver = CreateResolver(compiler, new RecordingPipelineTemplateExpander(), pipelineContentReader);

        var exception = await Assert.ThrowsAsync<PipelineParsingException>(
            () => resolver.ResolveAsync(PipelineSource.LocalFile(pipelineFilePath), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("Invalid YAML.", exception.Message);
    }

    [Fact]
    public async Task ResolveAsync_WhenExpanderThrows_PropagatesPipelineTemplateExpansionException()
    {
        var expander = new RecordingPipelineTemplateExpander
        {
            ExceptionToThrow = new PipelineTemplateExpansionException(
                "Template failed.",
                CreateSpan("template.yaml"),
                [PipelineSource.LocalFile("root.yaml"), PipelineSource.LocalFile("template.yaml")]),
        };

        var pipelineFilePath = Path.GetFullPath("pipeline.yaml");
        var pipelineContentReader = new StubPipelineContentReader(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [pipelineFilePath] = "steps:\n",
        });
        var resolver = CreateResolver(new RecordingPipelineCompiler(), expander, pipelineContentReader);

        var exception = await Assert.ThrowsAsync<PipelineTemplateExpansionException>(
            () => resolver.ResolveAsync(PipelineSource.LocalFile(pipelineFilePath), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("Template failed.", exception.Message);
        Assert.Equal(2, exception.IncludeChain.Count);
    }

    private static IPipelineResolver CreateResolver(
        IPipelineCompiler pipelineCompiler,
        IPipelineTemplateExpander pipelineTemplateExpander,
        IPipelineContentReader? pipelineContentReader = null)
    {
        var services = new ServiceCollection();

        _ = services.AddPipelineResolution();
        _ = services.AddSingleton(pipelineCompiler);
        _ = services.AddSingleton(pipelineTemplateExpander);

        if (pipelineContentReader is not null)
        {
            _ = services.AddSingleton(pipelineContentReader);
        }

        return services
            .BuildServiceProvider()
            .GetRequiredService<IPipelineResolver>();
    }

    private static PipelineNode CreatePipeline(IReadOnlyList<StepNode> steps)
    {
        return new PipelineNode([], steps, [], CreateSpan("pipeline.yaml"));
    }

    private static ScriptStepNode CreateScriptStep(string script)
    {
        return new ScriptStepNode(new InterpolatedStringNode(script), [], CreateSpan("pipeline.yaml"));
    }

    private static SourceSpan CreateSpan(string sourceName)
    {
        return SourceSpan.Unknown(new SourceRef(sourceName));
    }

    private sealed class StubPipelineContentReader(Dictionary<string, string>? files = null) : IPipelineContentReader
    {
        private readonly Dictionary<string, string> _files = files
            ?? new Dictionary<string, string>(StringComparer.Ordinal);

        public Task<string> ReadAsync(PipelineSource source, CancellationToken cancellationToken = default)
        {
            var sourceId = source.Identifier;

            return _files.TryGetValue(sourceId, out var content)
                ? Task.FromResult(content)
                : Task.FromException<string>(new FileNotFoundException($"Could not find file '{sourceId}'.", sourceId));
        }
    }

    private sealed class RecordingPipelineCompiler : IPipelineCompiler
    {
        public string? Content { get; private set; }

        public PipelineSource? Source { get; private set; }

        public IReadOnlyDictionary<string, string>? ParameterOverrides { get; private set; }

        public PipelineNode CompileResult { get; init; } = CreatePipeline([]);

        public Exception? ExceptionToThrow { get; init; }

        public PipelineNode Compile(
            string content,
            PipelineSource source,
            IReadOnlyDictionary<string, string>? parameterOverrides = null)
        {
            Content = content;
            Source = source;
            ParameterOverrides = parameterOverrides;

            return ExceptionToThrow is not null
                ? throw ExceptionToThrow
                : CompileResult;
        }
    }

    private sealed class RecordingPipelineTemplateExpander : IPipelineTemplateExpander
    {
        public PipelineNode Pipeline { get; private set; } = CreatePipeline([]);

        public PipelineSource? Source { get; private set; }

        public PipelineNode ExpandResult { get; init; } = CreatePipeline([]);

        public Exception? ExceptionToThrow { get; init; }

        public Task<PipelineNode> ExpandAsync(
            PipelineNode pipeline,
            PipelineSource source,
            CancellationToken cancellationToken = default)
        {
            Pipeline = pipeline;
            Source = source;

            return ExceptionToThrow is not null
                ? throw ExceptionToThrow
                : Task.FromResult(ExpandResult);
        }
    }
}
