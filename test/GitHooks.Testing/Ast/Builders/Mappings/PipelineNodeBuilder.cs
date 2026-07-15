using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Testing.Ast.Builders.Mappings.Parameters;
using GitHooks.Testing.Ast.Builders.Mappings.Steps;

namespace GitHooks.Testing.Ast.Builders.Mappings;

public sealed class PipelineNodeBuilder
    : PipelineNodeBuilderBase<PipelineNodeBuilder, PipelineNode>
{
    private readonly List<ParameterNode> _parameters = [];
    private readonly List<StepNode> _steps = [];

    public PipelineNodeBuilder WithParameter(
        Action<ParameterNodeBuilder> configure)
    {
        return WithParameter(
            Configure(configure).Build());
    }

    public PipelineNodeBuilder WithoutParameters()
    {
        _parameters.Clear();

        return Self;
    }

    public PipelineNodeBuilder WithScriptStep(
        Action<ScriptStepNodeBuilder> configure)
    {
        return WithStep(
            Configure(configure).Build());
    }

    public PipelineNodeBuilder WithTemplateStep(
        Action<TemplateStepNodeBuilder> configure)
    {
        return WithStep(
            Configure(configure).Build());
    }

    public PipelineNodeBuilder WithInvalidStep(
        Action<InvalidStepNodeBuilder> configure)
    {
        return WithStep(
            Configure(configure).Build());
    }

    public PipelineNodeBuilder WithoutSteps()
    {
        _steps.Clear();

        return Self;
    }

    public override PipelineNode Build()
    {
        return new PipelineNode
        {
            Parameters = [.. _parameters],
            Steps = [.. _steps],
            UnknownFields = [.. UnknownFields],
            Span = Span
        };
    }

    public PipelineNodeBuilder WithParameter(
        ParameterNode parameter)
    {
        _parameters.Add(parameter);

        return Self;
    }

    public PipelineNodeBuilder WithStep(
        StepNode step)
    {
        _steps.Add(step);

        return Self;
    }
}
