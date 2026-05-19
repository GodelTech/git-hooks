using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Compilation;
using GitHooks.Workflow.Application.Expansion;
using GitHooks.Workflow.Application.Expansion.Exceptions;
using GitHooks.Workflow.Application.Resolution;
using GitHooks.Workflow.Domain.Model;
using GitHooks.Workflow.Infrastructure.DependencyInjection;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Workflow.Infrastructure.Tests.Resolution;

public sealed class PipelineResolverTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public PipelineResolverTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public async Task ResolveAsync_WithEmptyFilePath_ThrowsArgumentException()
    {
        var resolver = CreateResolver(new RecordingPipelineCompiler(), new RecordingPipelineTemplateExpander());

        await Assert.ThrowsAsync<ArgumentException>(
            () => resolver.ResolveAsync(string.Empty, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ResolveAsync_WithMissingFile_ThrowsFileNotFoundException()
    {
        var resolver = CreateResolver(new RecordingPipelineCompiler(), new RecordingPipelineTemplateExpander());
        var missingFilePath = Path.Combine(_tempDir, "missing.yaml");

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => resolver.ResolveAsync(missingFilePath, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ResolveAsync_WithValidFile_CompilesAndExpandsPipeline()
    {
        var compiledPipeline = CreatePipeline([CreateScriptStep("echo compiled")]);
        var expandedPipeline = CreatePipeline([CreateScriptStep("echo expanded")]);
        var compiler = new RecordingPipelineCompiler { CompileResult = compiledPipeline };
        var expander = new RecordingPipelineTemplateExpander { ExpandResult = expandedPipeline };
        var resolver = CreateResolver(compiler, expander);
        var pipelineFilePath = WriteTempFile("pipeline.yaml", "steps:\n  - script: echo hello\n");
        var parameterOverrides = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["name"] = "value",
        };

        var result = await resolver.ResolveAsync(pipelineFilePath, parameterOverrides, TestContext.Current.CancellationToken);

        Assert.Same(expandedPipeline, result);
        Assert.Equal("steps:\n  - script: echo hello\n", compiler.Content);
        Assert.Equal(Path.GetFullPath(pipelineFilePath), compiler.SourceName);
        Assert.Same(parameterOverrides, compiler.ParameterOverrides);
        Assert.Same(compiledPipeline, expander.Pipeline);
        Assert.Equal(Path.GetFullPath(pipelineFilePath), expander.PipelineFilePath);
    }

    [Fact]
    public async Task ResolveAsync_WhenCompilerThrows_PropagatesYamlParseException()
    {
        var compiler = new RecordingPipelineCompiler
        {
            ExceptionToThrow = new YamlParseException("Invalid YAML.", CreateSpan("pipeline.yaml")),
        };

        var resolver = CreateResolver(compiler, new RecordingPipelineTemplateExpander());
        var pipelineFilePath = WriteTempFile("pipeline.yaml", "steps:\n");

        var exception = await Assert.ThrowsAsync<YamlParseException>(
            () => resolver.ResolveAsync(pipelineFilePath, cancellationToken: TestContext.Current.CancellationToken));

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
                ["root.yaml", "template.yaml"]),
        };

        var resolver = CreateResolver(new RecordingPipelineCompiler(), expander);
        var pipelineFilePath = WriteTempFile("pipeline.yaml", "steps:\n");

        var exception = await Assert.ThrowsAsync<PipelineTemplateExpansionException>(
            () => resolver.ResolveAsync(pipelineFilePath, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("Template failed.", exception.Message);
        Assert.Equal(2, exception.IncludeChain.Count);
    }

    private static IPipelineResolver CreateResolver(
        IPipelineCompiler pipelineCompiler,
        IPipelineTemplateExpander pipelineTemplateExpander)
    {
        var services = new ServiceCollection();

        _ = services.AddPipelineResolution();
        _ = services.AddSingleton(pipelineCompiler);
        _ = services.AddSingleton(pipelineTemplateExpander);

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

    private string WriteTempFile(string fileName, string content)
    {
        var filePath = Path.Combine(_tempDir, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }

    private sealed class RecordingPipelineCompiler : IPipelineCompiler
    {
        public string? Content { get; private set; }

        public string? SourceName { get; private set; }

        public IReadOnlyDictionary<string, string>? ParameterOverrides { get; private set; }

        public PipelineNode CompileResult { get; init; } = CreatePipeline([]);

        public Exception? ExceptionToThrow { get; init; }

        public PipelineNode Compile(
            string content,
            string sourceName,
            IReadOnlyDictionary<string, string>? parameterOverrides = null)
        {
            Content = content;
            SourceName = sourceName;
            ParameterOverrides = parameterOverrides;

            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return CompileResult;
        }
    }

    private sealed class RecordingPipelineTemplateExpander : IPipelineTemplateExpander
    {
        public PipelineNode Pipeline { get; private set; } = CreatePipeline([]);

        public string? PipelineFilePath { get; private set; }

        public PipelineNode ExpandResult { get; init; } = CreatePipeline([]);

        public Exception? ExceptionToThrow { get; init; }

        public Task<PipelineNode> ExpandAsync(
            PipelineNode pipeline,
            string pipelineFilePath,
            CancellationToken cancellationToken = default)
        {
            Pipeline = pipeline;
            PipelineFilePath = pipelineFilePath;

            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.FromResult(ExpandResult);
        }
    }
}
