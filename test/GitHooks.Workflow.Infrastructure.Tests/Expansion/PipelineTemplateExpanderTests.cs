using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.DependencyInjection;
using GitHooks.Workflow.Application.Expansion;
using GitHooks.Workflow.Application.Expansion.Exceptions;
using GitHooks.Workflow.Domain.Model;
using GitHooks.Workflow.Infrastructure.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Workflow.Infrastructure.Tests.Expansion;

public sealed class PipelineTemplateExpanderTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public PipelineTemplateExpanderTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public async Task ExpandAsync_WithNullPipeline_ThrowsArgumentNullException()
    {
        var expander = CreateExpander();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => expander.ExpandAsync(null!, PipelineSource.LocalFile("/root.yaml"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExpandAsync_WithNoTemplateSteps_ReturnsPipelineUnchanged()
    {
        var expander = CreateExpander();
        var span = CreateSpan();
        var scriptStep = new ScriptStepNode(new InterpolatedStringNode("echo hello"), [], span);
        var pipeline = CreatePipeline([scriptStep]);
        var rootSource = PipelineSource.LocalFile(Path.GetFullPath("root.yaml"));

        var result = await expander.ExpandAsync(pipeline, rootSource, TestContext.Current.CancellationToken);

        var step = Assert.Single(result.Steps);
        Assert.Same(scriptStep, step);
    }

    [Fact]
    public async Task ExpandAsync_WithValidTemplateFile_ExpandsTemplateSteps()
    {
        var expander = CreateExpander();
        var span = CreateSpan();

        var templateContent = """
            steps:
              - script: echo from template
            """;

        var templateFile = WriteTempFile("template.yaml", templateContent);
        Path.GetFullPath("root.yaml");

        var templateStep = new TemplateStepNode(
            templateFile,
            new Dictionary<string, InterpolatedStringNode>(),
            [],
            span);

        var pipeline = CreatePipeline([templateStep]);

        var result = await expander.ExpandAsync(pipeline, PipelineSource.LocalFile(Path.GetFullPath("root.yaml")), TestContext.Current.CancellationToken);

        var expandedStep = Assert.Single(result.Steps);
        var scriptStep = Assert.IsType<ScriptStepNode>(expandedStep);
        Assert.Equal("echo from template", scriptStep.Script.Value);
    }

    [Fact]
    public async Task ExpandAsync_WithMissingTemplateFile_ThrowsPipelineTemplateExpansionException()
    {
        var expander = CreateExpander();
        var span = CreateSpan();

        var missingPath = Path.Combine(_tempDir, "missing.yaml");
        var rootFile = Path.GetFullPath("root.yaml");

        var templateStep = new TemplateStepNode(
            missingPath,
            new Dictionary<string, InterpolatedStringNode>(),
            [],
            span);

        var pipeline = CreatePipeline([templateStep]);

        var exception = await Assert.ThrowsAsync<PipelineTemplateExpansionException>(
            () => expander.ExpandAsync(pipeline, PipelineSource.LocalFile(rootFile), TestContext.Current.CancellationToken));

        Assert.Contains("missing.yaml", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExpandAsync_WithCycle_ThrowsPipelineTemplateExpansionExceptionWithCycleMessage()
    {
        var expander = CreateExpander();
        var span = CreateSpan();

        var rootFile = WriteTempFile("root.yaml", "steps:\n  - script: echo root\n");
        var rootFileFull = Path.GetFullPath(rootFile);

        var templateContent = $"""
            steps:
              - template: {rootFileFull}
            """;

        var templateFile = WriteTempFile("template.yaml", templateContent);

        var templateStep = new TemplateStepNode(
            templateFile,
            new Dictionary<string, InterpolatedStringNode>(),
            [],
            span);

        var pipeline = CreatePipeline([templateStep]);

        var exception = await Assert.ThrowsAsync<PipelineTemplateExpansionException>(
            () => expander.ExpandAsync(pipeline, PipelineSource.LocalFile(rootFile), TestContext.Current.CancellationToken));

        Assert.Contains("cycle", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExpandAsync_WhenTemplateExpansionThrows_IncludeChainContainsRoot()
    {
        var expander = CreateExpander();
        var span = CreateSpan();

        var missingPath = Path.Combine(_tempDir, "nonexistent.yaml");
        var rootFile = Path.GetFullPath("root.yaml");

        var templateStep = new TemplateStepNode(
            missingPath,
            new Dictionary<string, InterpolatedStringNode>(),
            [],
            span);

        var pipeline = CreatePipeline([templateStep]);

        var exception = await Assert.ThrowsAsync<PipelineTemplateExpansionException>(
            () => expander.ExpandAsync(pipeline, PipelineSource.LocalFile(rootFile), TestContext.Current.CancellationToken));

        Assert.NotEmpty(exception.IncludeChain);
        Assert.Contains(PipelineSource.LocalFile(Path.GetFullPath(rootFile)), exception.IncludeChain);
    }

    private static IPipelineTemplateExpander CreateExpander()
    {
        var services = new ServiceCollection();
        services.AddYamlPipelineParsing();
        services.AddPipelineParameterBinding();
        services.AddPipelineTemplateExpansion();

        return services
            .BuildServiceProvider()
            .GetRequiredService<IPipelineTemplateExpander>();
    }

    private static PipelineNode CreatePipeline(IReadOnlyList<StepNode> steps)
    {
        return new PipelineNode([], steps, [], CreateSpan());
    }

    private static SourceSpan CreateSpan()
    {
        return SourceSpan.Unknown(new SourceRef("test.yaml"));
    }

    private string WriteTempFile(string fileName, string content)
    {
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, content);
        return path;
    }
}
