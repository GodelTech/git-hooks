using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;

namespace GitHooks.Testing.Ast.Builders;

public sealed class PipelineNodeBuilder
    : MappingNodeBuilder<PipelineNodeBuilder>
{
    private readonly List<ParameterNode> _parameters = [];
    private readonly List<StepNode> _steps = [];

    public PipelineNodeBuilder WithParameter(
        ParameterNode parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        _parameters.Add(parameter);

        return this;
    }

    public PipelineNodeBuilder WithParameter(
        string name = "configuration")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var parameter = new ParameterNodeBuilder()
            .WithName(name)
            .Build();

        WithParameter(parameter);

        return this;
    }

    public PipelineNodeBuilder WithParameter(
        Action<ParameterNodeBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new ParameterNodeBuilder();

        configure(builder);

        _parameters.Add(builder.Build());

        return this;
    }

    public PipelineNodeBuilder WithStep(
        StepNode step)
    {
        ArgumentNullException.ThrowIfNull(step);

        _steps.Add(step);

        return this;
    }

    public PipelineNodeBuilder WithScriptStep(
        string script = "dotnet test")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(script);

        var scriptStep = new ScriptStepNodeBuilder()
            .WithScript(script)
            .Build();

        WithStep(scriptStep);

        return this;
    }

    public PipelineNodeBuilder WithScriptStep(
        Action<ScriptStepNodeBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new ScriptStepNodeBuilder();

        configure(builder);

        _steps.Add(builder.Build());

        return this;
    }

    public PipelineNodeBuilder WithTemplateStep(
        string template = "build.yml")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);

        var templateStep = new TemplateStepNodeBuilder()
            .WithTemplate(template)
            .Build();

        WithStep(templateStep);

        return this;
    }

    public PipelineNodeBuilder WithTemplateStep(
        Action<TemplateStepNodeBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new TemplateStepNodeBuilder();

        configure(builder);

        _steps.Add(builder.Build());

        return this;
    }

    public PipelineNodeBuilder WithoutSteps()
    {
        _steps.Clear();

        return this;
    }

    public PipelineNode Build()
    {
        return new PipelineNode
        {
            Parameters = [.. _parameters],
            Steps = [.. _steps],
            UnknownFields = [.. UnknownFields],
            Span = Span
        };
    }
}
