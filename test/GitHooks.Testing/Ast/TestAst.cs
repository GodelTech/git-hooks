using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Unknown;
using GitHooks.Domain.Common;

namespace GitHooks.Testing.Ast;

public static class TestAst
{
    private static readonly ParameterNode[] EmptyParameters = [];
    private static readonly StepNode[] EmptySteps = [];
    private static readonly UnknownFieldNode[] EmptyUnknownFields = [];

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
            EmptyParameters,
            EmptySteps,
            EmptyUnknownFields,
            span);
    }

    public static PipelineNode Pipeline(
        IEnumerable<ParameterNode>? parameters = null,
        IEnumerable<StepNode>? steps = null,
        IEnumerable<UnknownFieldNode>? unknownFields = null)
    {
        return Pipeline(
            parameters ?? EmptyParameters,
            steps ?? EmptySteps,
            unknownFields ?? EmptyUnknownFields,
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
            unknownFields ?? EmptyUnknownFields,
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
            unknownFields ?? EmptyUnknownFields,
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
