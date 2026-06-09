using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Unknown;
using GitHooks.Domain.Common;
using GitHooks.Testing.Ast;

namespace GitHooks.Testing.Builders;

public sealed class PipelineNodeBuilder
{
    private readonly List<ParameterNode> _parameters = [];
    private readonly List<StepNode> _steps = [];
    private readonly List<UnknownFieldNode> _unknownFields = [];

    private SourceSpan _span = SourceSpan.Unknown;

    public PipelineNodeBuilder WithParameter(
        ParameterNode parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        _parameters.Add(parameter);

        return this;
    }

    public PipelineNodeBuilder WithParameters(
        IEnumerable<ParameterNode> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        _parameters.AddRange(parameters);

        return this;
    }

    public PipelineNodeBuilder WithStep(
        StepNode step)
    {
        ArgumentNullException.ThrowIfNull(step);

        _steps.Add(step);

        return this;
    }

    public PipelineNodeBuilder WithSteps(
        IEnumerable<StepNode> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);

        _steps.AddRange(steps);

        return this;
    }

    public PipelineNodeBuilder WithScriptStep(
        string script = "dotnet test")
    {
        _steps.Add(
            TestAst.ScriptStep(script));

        return this;
    }

    public PipelineNodeBuilder WithTemplateStep(
        string template = "build.yml")
    {
        _steps.Add(
            TestAst.TemplateStep(template));

        return this;
    }

    public PipelineNodeBuilder WithoutSteps()
    {
        _steps.Clear();

        return this;
    }

    public PipelineNodeBuilder WithUnknownField(
        UnknownFieldNode field)
    {
        ArgumentNullException.ThrowIfNull(field);

        _unknownFields.Add(field);

        return this;
    }

    public PipelineNodeBuilder WithSpan(
        SourceSpan span)
    {
        _span = span;

        return this;
    }

    public PipelineNode Build()
    {
        return new PipelineNode
        {
            Parameters = [.. _parameters],
            Steps = [.. _steps],
            UnknownFields = [.. _unknownFields],
            Span = _span
        };
    }
}
