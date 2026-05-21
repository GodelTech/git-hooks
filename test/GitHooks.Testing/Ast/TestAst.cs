using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Common;

namespace GitHooks.Testing.Ast;

public static class TestAst
{
    private static readonly ParameterNode[] EmptyParameters = [];
    private static readonly StepNode[] EmptySteps = [];

    public static PipelineNode Pipeline(
        IEnumerable<ParameterNode> parameters,
        IEnumerable<StepNode> steps,
        SourceSpan span)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(steps);

        return new()
        {
            Span = span,
            Parameters = [.. parameters],
            Steps = [.. steps]
        };
    }

    public static PipelineNode Pipeline(
        SourceSpan span)
    {
        return Pipeline(
            EmptyParameters,
            EmptySteps,
            span);
    }

    public static PipelineNode Pipeline(
        IEnumerable<ParameterNode>? parameters = null,
        IEnumerable<StepNode>? steps = null)
    {
        return Pipeline(
            parameters ?? EmptyParameters,
            steps ?? EmptySteps,
            SourceSpan.Unknown);
    }

    public static ParameterNode Parameter(
        string name,
        string value,
        SourceSpan span)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new()
        {
            Name = name,
            Value = StringLiteral(value, span),
            Span = span
        };
    }

    public static ParameterNode Parameter(
        string name = "configuration",
        string value = "Release")
    {
        return Parameter(name, value, SourceSpan.Unknown);
    }

    public static ScriptStepNode Script(
        string script,
        SourceSpan span)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(script);

        return new()
        {
            Script = StringLiteral(script, span),
            Span = span
        };
    }

    public static ScriptStepNode Script(
        string script = "dotnet test")
    {
        return Script(script, SourceSpan.Unknown);
    }

    private static StringLiteralExpressionNode StringLiteral(
        string value,
        SourceSpan span)
    {
        return new()
        {
            Value = value,
            Span = span
        };
    }
}
