using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.DependencyInjection;
using GitHooks.Workflow.Application.Expansion;
using GitHooks.Workflow.Application.Expansion.Exceptions;
using GitHooks.Workflow.Domain.Model;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Workflow.Application.Tests.Expansion;

public sealed class TemplateExpansionOrchestratorTests
{
    [Fact]
    public async Task OrchestrateAsync_WithNullPipeline_ThrowsArgumentNullException()
    {
        var orchestrator = CreateOrchestrator();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => orchestrator.OrchestrateAsync(null!, PipelineSource.LocalFile("/root.yaml"), NeverCalledCallback, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task OrchestrateAsync_WithNullCallback_ThrowsArgumentNullException()
    {
        var orchestrator = CreateOrchestrator();
        var pipeline = CreateEmptyPipeline();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => orchestrator.OrchestrateAsync(pipeline, PipelineSource.LocalFile("/root.yaml"), null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task OrchestrateAsync_WithNoTemplateSteps_ReturnsPipelineUnchanged()
    {
        var orchestrator = CreateOrchestrator();
        var span = CreateSpan();
        var scriptStep = new ScriptStepNode(new InterpolatedStringNode("echo hello"), [], span);
        var pipeline = CreatePipeline([scriptStep]);

        var result = await orchestrator.OrchestrateAsync(pipeline, PipelineSource.LocalFile("/root.yaml"), NeverCalledCallback, TestContext.Current.CancellationToken);

        var step = Assert.Single(result.Steps);
        Assert.Same(scriptStep, step);
    }

    [Fact]
    public async Task OrchestrateAsync_WithTemplateStep_CallsCallbackAndExpandsSteps()
    {
        var orchestrator = CreateOrchestrator();
        var span = CreateSpan();
        var templateStep = new TemplateStepNode("template.yaml", new Dictionary<string, InterpolatedStringNode>(), [], span);
        var pipeline = CreatePipeline([templateStep]);

        var expandedScriptStep = new ScriptStepNode(new InterpolatedStringNode("echo from template"), [], span);
        var templatePipeline = CreatePipeline([expandedScriptStep]);
        var callbackInvoked = false;

        var result = await orchestrator.OrchestrateAsync(
            pipeline,
            PipelineSource.LocalFile("/root.yaml"),
            (_, _, _, _) =>
            {
                callbackInvoked = true;
                return Task.FromResult(new ResolvedTemplate(PipelineSource.LocalFile("/template.yaml"), templatePipeline));
            },
            TestContext.Current.CancellationToken);

        Assert.True(callbackInvoked);
        var step = Assert.Single(result.Steps);
        Assert.Same(expandedScriptStep, step);
    }

    [Fact]
    public async Task OrchestrateAsync_WithMixedSteps_PreservesNonTemplateStepsAndExpandsTemplates()
    {
        var orchestrator = CreateOrchestrator();
        var span = CreateSpan();

        var scriptBefore = new ScriptStepNode(new InterpolatedStringNode("echo before"), [], span);
        var templateStep = new TemplateStepNode("template.yaml", new Dictionary<string, InterpolatedStringNode>(), [], span);
        var scriptAfter = new ScriptStepNode(new InterpolatedStringNode("echo after"), [], span);

        var pipeline = CreatePipeline([scriptBefore, templateStep, scriptAfter]);

        var expandedScript = new ScriptStepNode(new InterpolatedStringNode("echo from template"), [], span);
        var templatePipeline = CreatePipeline([expandedScript]);

        var result = await orchestrator.OrchestrateAsync(
            pipeline,
            PipelineSource.LocalFile("/root.yaml"),
            (_, _, _, _) => Task.FromResult(new ResolvedTemplate(PipelineSource.LocalFile("/template.yaml"), templatePipeline)),
            TestContext.Current.CancellationToken);

        Assert.Equal(3, result.Steps.Count);
        Assert.Same(scriptBefore, result.Steps[0]);
        Assert.Same(expandedScript, result.Steps[1]);
        Assert.Same(scriptAfter, result.Steps[2]);
    }

    [Fact]
    public async Task OrchestrateAsync_WithCycleDetected_ThrowsPipelineTemplateExpansionException()
    {
        var orchestrator = CreateOrchestrator();
        var span = CreateSpan();

        var templateStep = new TemplateStepNode("template.yaml", new Dictionary<string, InterpolatedStringNode>(), [], span);
        var pipeline = CreatePipeline([templateStep]);

        // The template path returned matches the starting file path — cycle!
        var exception = await Assert.ThrowsAsync<PipelineTemplateExpansionException>(
            () => orchestrator.OrchestrateAsync(
                pipeline,
                PipelineSource.LocalFile("/root.yaml"),
                (_, _, _, _) =>
                {
                    // Returning the root path triggers cycle detection
                    var selfReferencedTemplate = CreatePipeline([]);
                    return Task.FromResult(new ResolvedTemplate(PipelineSource.LocalFile("/root.yaml"), selfReferencedTemplate));
                },
                TestContext.Current.CancellationToken));

        Assert.Contains("cycle", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/root.yaml", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OrchestrateAsync_WithCycleDetected_IncludeChainContainsBothPaths()
    {
        var orchestrator = CreateOrchestrator();
        var span = CreateSpan();

        var templateStep = new TemplateStepNode("template.yaml", new Dictionary<string, InterpolatedStringNode>(), [], span);
        var pipeline = CreatePipeline([templateStep]);

        var exception = await Assert.ThrowsAsync<PipelineTemplateExpansionException>(
            () => orchestrator.OrchestrateAsync(
                pipeline,
                PipelineSource.LocalFile("/root.yaml"),
                (_, _, _, _) =>
                {
                    var selfReferencedTemplate = CreatePipeline([]);
                    return Task.FromResult(new ResolvedTemplate(PipelineSource.LocalFile("/root.yaml"), selfReferencedTemplate));
                },
                TestContext.Current.CancellationToken));

        Assert.Contains(PipelineSource.LocalFile("/root.yaml"), exception.IncludeChain);
    }

    [Fact]
    public async Task OrchestrateAsync_WithNestedTemplates_RecursivelyExpandsAllSteps()
    {
        var orchestrator = CreateOrchestrator();
        var span = CreateSpan();

        var innerScript = new ScriptStepNode(new InterpolatedStringNode("echo inner"), [], span);
        var innerTemplate = CreatePipeline([innerScript]);

        var outerTemplateStep = new TemplateStepNode("outer.yaml", new Dictionary<string, InterpolatedStringNode>(), [], span);
        var innerTemplateStep = new TemplateStepNode("inner.yaml", new Dictionary<string, InterpolatedStringNode>(), [], span);
        var outerTemplate = CreatePipeline([innerTemplateStep]);

        var root = CreatePipeline([outerTemplateStep]);

        var result = await orchestrator.OrchestrateAsync(
            root,
            PipelineSource.LocalFile("/root.yaml"),
            (step, _, _, _) =>
            {
                var templatePath = step.Template == "outer.yaml"
                    ? PipelineSource.LocalFile("/outer.yaml")
                    : PipelineSource.LocalFile("/inner.yaml");
                var templatePipeline = step.Template == "outer.yaml" ? outerTemplate : innerTemplate;
                return Task.FromResult(new ResolvedTemplate(templatePath, templatePipeline));
            },
            TestContext.Current.CancellationToken);

        var expandedStep = Assert.Single(result.Steps);
        Assert.Same(innerScript, expandedStep);
    }

    [Fact]
    public async Task OrchestrateAsync_WithCallbackPassesCurrentFilePath()
    {
        var orchestrator = CreateOrchestrator();
        var span = CreateSpan();

        var templateStep = new TemplateStepNode("template.yaml", new Dictionary<string, InterpolatedStringNode>(), [], span);
        var pipeline = CreatePipeline([templateStep]);

        PipelineSource? receivedSource = null;

        await orchestrator.OrchestrateAsync(
            pipeline,
            PipelineSource.LocalFile("/root.yaml"),
            (_, currentSource, _, _) =>
            {
                receivedSource = currentSource;
                return Task.FromResult(new ResolvedTemplate(PipelineSource.LocalFile("/template.yaml"), CreateEmptyPipeline()));
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(PipelineSource.LocalFile("/root.yaml"), receivedSource);
    }

    [Fact]
    public async Task OrchestrateAsync_WithCallbackPassesCurrentIncludeChain()
    {
        var orchestrator = CreateOrchestrator();
        var span = CreateSpan();

        var templateStep = new TemplateStepNode("template.yaml", new Dictionary<string, InterpolatedStringNode>(), [], span);
        var pipeline = CreatePipeline([templateStep]);

        IReadOnlyList<PipelineSource>? receivedChain = null;

        await orchestrator.OrchestrateAsync(
            pipeline,
            PipelineSource.LocalFile("/root.yaml"),
            (_, _, includeChain, _) =>
            {
                receivedChain = includeChain;
                return Task.FromResult(new ResolvedTemplate(PipelineSource.LocalFile("/template.yaml"), CreateEmptyPipeline()));
            },
            TestContext.Current.CancellationToken);

        Assert.NotNull(receivedChain);
        Assert.Contains(PipelineSource.LocalFile("/root.yaml"), receivedChain);
    }

    private static ITemplateExpansionOrchestrator CreateOrchestrator()
    {
        var services = new ServiceCollection();
        services.AddPipelineTemplateExpansionOrchestration();

        return services
            .BuildServiceProvider()
            .GetRequiredService<ITemplateExpansionOrchestrator>();
    }

    private static PipelineNode CreateEmptyPipeline()
    {
        return CreatePipeline([]);
    }

    private static PipelineNode CreatePipeline(IReadOnlyList<StepNode> steps)
    {
        return new PipelineNode([], steps, [], CreateSpan());
    }

    private static SourceSpan CreateSpan()
    {
        return SourceSpan.Unknown(new SourceRef("test.yaml"));
    }

    private static Task<ResolvedTemplate> NeverCalledCallback(
        TemplateStepNode templateStep,
        PipelineSource currentSource,
        IReadOnlyList<PipelineSource> includeChain,
        CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Callback should not have been called.");
    }
}
