using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Unknown;
using GitHooks.Domain.Common;

namespace GitHooks.Testing.Ast;

public static class TestAst
{
    private static readonly ParameterNode[] s_emptyParameters = [];
    private static readonly StepNode[] s_emptySteps = [];
    private static readonly UnknownFieldNode[] s_emptyUnknownFields = [];

    public static PipelineNode Pipeline(
        IEnumerable<ParameterNode> parameters,
        IEnumerable<StepNode> steps,
        IEnumerable<UnknownFieldNode> unknownFields,
        SourceSpan span)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentNullException.ThrowIfNull(unknownFields);

        return new()
        {
            Span = span,
            Parameters = [.. parameters],
            Steps = [.. steps],
            UnknownFields = [.. unknownFields]
        };
    }

    public static PipelineNode Pipeline(
        SourceSpan span)
    {
        return Pipeline(
            s_emptyParameters,
            s_emptySteps,
            s_emptyUnknownFields,
            span);
    }

    public static PipelineNode Pipeline(
        IEnumerable<ParameterNode>? parameters = null,
        IEnumerable<StepNode>? steps = null,
        IEnumerable<UnknownFieldNode>? unknownFields = null)
    {
        return Pipeline(
            parameters ?? s_emptyParameters,
            steps ?? s_emptySteps,
            unknownFields ?? s_emptyUnknownFields,
            SourceSpan.Unknown);
    }

    public static ParameterNode Parameter(
        string name,
        string defaultValue,
        IEnumerable<UnknownFieldNode> unknownFields,
        SourceSpan span)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultValue);
        ArgumentNullException.ThrowIfNull(unknownFields);

        return new()
        {
            Name = name,
            DefaultValue = defaultValue,
            UnknownFields = [.. unknownFields],
            Span = span
        };
    }

    public static ParameterNode Parameter(
        string name = "configuration",
        string defaultValue = "Release",
        IEnumerable<UnknownFieldNode>? unknownFields = null)
    {
        return Parameter(
            name,
            defaultValue,
            unknownFields ?? s_emptyUnknownFields,
            SourceSpan.Unknown);
    }

    public static ScriptStepNode Script(
        string script,
        IEnumerable<UnknownFieldNode> unknownFields,
        SourceSpan span)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(script);

        return new()
        {
            Script = StringLiteral(script, span),
            UnknownFields = [.. unknownFields],
            Span = span
        };
    }

    public static ScriptStepNode Script(
        string script = "dotnet test",
        IEnumerable<UnknownFieldNode>? unknownFields = null)
    {
        return Script(
            script,
            unknownFields ?? s_emptyUnknownFields,
            SourceSpan.Unknown);
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
