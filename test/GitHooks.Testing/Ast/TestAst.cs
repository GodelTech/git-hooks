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
    private static readonly IReadOnlyList<ExpressionNode> s_emptyExpressionList = [];
    private static readonly IReadOnlyDictionary<string, ExpressionNode> s_emptyExpressionDictionary
        = new Dictionary<string, ExpressionNode>();

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
        IEnumerable<UnknownFieldNode>? unknownFields = null,
        SourceSpan? span = null)
    {
        return Pipeline(
            parameters ?? s_emptyParameters,
            steps ?? s_emptySteps,
            unknownFields ?? s_emptyUnknownFields,
            span ?? SourceSpan.Unknown);
    }

    public static ParameterNode Parameter(
        string name,
        string defaultValue,
        IEnumerable<UnknownFieldNode> unknownFields,
        SourceSpan span)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(defaultValue);
        ArgumentNullException.ThrowIfNull(unknownFields);

        return new()
        {
            Name = name,
            Type = ParameterType.String,
            DefaultValue = StringLiteral(defaultValue),
            UnknownFields = [.. unknownFields],
            Span = span
        };
    }

    public static ParameterNode Parameter(
        string name = "configuration",
        string? displayName = null,
        ParameterType? type = null,
        string? defaultValue = null,
        IEnumerable<string>? values = null,
        IEnumerable<UnknownFieldNode>? unknownFields = null,
        SourceSpan? span = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new()
        {
            Name = name,
            DisplayName = displayName,
            Type = type ?? ParameterType.String,
            DefaultValue = NullableStringLiteral(defaultValue),
            Values = StringLiteralList(values),
            UnknownFields = [.. unknownFields ?? s_emptyUnknownFields],
            Span = span ?? SourceSpan.Unknown
        };
    }

    public static ScriptStepNode ScriptStep(
        string script,
        IEnumerable<UnknownFieldNode> unknownFields,
        SourceSpan span)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(script);

        return new()
        {
            Script = StringLiteral(script),
            UnknownFields = [.. unknownFields],
            Span = span
        };
    }

    public static ScriptStepNode ScriptStep(
        string script = "dotnet test",
        string? displayName = null,
        string? condition = null,
        int? timeoutInMinutes = null,
        string? workingDirectory = null,
        IReadOnlyDictionary<string, string>? env = null,
        IEnumerable<UnknownFieldNode>? unknownFields = null,
        SourceSpan? span = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(script);

        return new()
        {
            Script = StringLiteral(script),
            DisplayName = NullableStringLiteral(displayName),
            Condition = NullableStringLiteral(condition),
            TimeoutInMinutes = NullableIntegerLiteral(timeoutInMinutes),
            WorkingDirectory = NullableStringLiteral(workingDirectory),
            Env = StringLiteralDictionary(env),
            UnknownFields = [.. unknownFields ?? s_emptyUnknownFields],
            Span = span ?? SourceSpan.Unknown
        };
    }

    public static TemplateStepNode TemplateStep(
        string template,
        IEnumerable<UnknownFieldNode> unknownFields,
        SourceSpan span)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);

        return new()
        {
            Template = StringLiteral(template),
            UnknownFields = [.. unknownFields],
            Span = span
        };
    }

    public static TemplateStepNode TemplateStep(
        string template = "build.yml",
        IReadOnlyDictionary<string, string>? parameters = null,
        IEnumerable<UnknownFieldNode>? unknownFields = null,
        SourceSpan? span = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);

        return new()
        {
            Template = StringLiteral(template),
            Parameters = StringLiteralDictionary(parameters),
            UnknownFields = [.. unknownFields ?? s_emptyUnknownFields],
            Span = span ?? SourceSpan.Unknown
        };
    }

    public static UnknownSimpleFieldNode UnknownSimpleField(
        string key,
        UnknownNode value)
    {
        return new()
        {
            Key = key,
            Value = value,
            Span = SourceSpan.Unknown
        };
    }

    public static UnknownSimpleFieldNode UnknownSimpleField(
        string key,
        string value)
    {
        return UnknownSimpleField(
            key,
            UnknownScalar(value));
    }

    public static UnknownComplexFieldNode UnknownComplexField(
        string key,
        UnknownNode value)
    {
        return new()
        {
            Key = UnknownScalar(key),
            Value = value,
            Span = SourceSpan.Unknown
        };
    }

    public static UnknownSequenceNode UnknownSequence(
        IEnumerable<UnknownNode> items)
    {
        return new()
        {
            Items = [.. items],
            Span = SourceSpan.Unknown
        };
    }

    public static UnknownMappingNode UnknownMapping(
        IEnumerable<UnknownFieldNode> fields)
    {
        return new()
        {
            Fields = [.. fields],
            Span = SourceSpan.Unknown
        };
    }

    public static UnknownScalarNode UnknownScalar(
        string value)
    {
        return new()
        {
            Value = value,
            Span = SourceSpan.Unknown
        };
    }

    public static BooleanLiteralExpressionNode BooleanLiteral(
        bool value)
    {
        return new()
        {
            Value = value,
            Span = SourceSpan.Unknown
        };
    }

    public static IntegerLiteralExpressionNode IntegerLiteral(
        int value,
        SourceSpan span)
    {
        return new()
        {
            Value = value,
            Span = span
        };
    }

    public static IntegerLiteralExpressionNode IntegerLiteral(
        int value)
    {
        return IntegerLiteral(
            value,
            SourceSpan.Unknown);
    }

    public static StringLiteralExpressionNode StringLiteral(
        string value,
        SourceSpan span)
    {
        return new()
        {
            Value = value,
            Span = span
        };
    }

    public static StringLiteralExpressionNode StringLiteral(
        string value)
    {
        return StringLiteral(
            value,
            SourceSpan.Unknown);
    }

    public static InterpolatedStringExpressionNode InterpolatedString(
        params ExpressionNode[] parts)
    {
        return new()
        {
            Parts = parts,
            Span = SourceSpan.Unknown
        };
    }

    public static VariableExpressionNode Variable(
        string path)
    {
        return new()
        {
            Path = path,
            Span = SourceSpan.Unknown
        };
    }

    private static IntegerLiteralExpressionNode? NullableIntegerLiteral(
        int? value)
    {
        return value is null
            ? null
            : IntegerLiteral(value.Value);
    }

    private static StringLiteralExpressionNode? NullableStringLiteral(
        string? value)
    {
        return value is null
            ? null
            : StringLiteral(value);
    }

    private static IReadOnlyList<ExpressionNode> StringLiteralList(
        IEnumerable<string>? values)
    {
        return values?
            .Select(static value => (ExpressionNode)StringLiteral(value))
            .ToArray()
            ?? s_emptyExpressionList;
    }

    private static IReadOnlyDictionary<string, ExpressionNode> StringLiteralDictionary(
        IReadOnlyDictionary<string, string>? values)
    {
        return values?.ToDictionary(
            static pair => pair.Key,
            static pair => (ExpressionNode)StringLiteral(pair.Value))
            ?? s_emptyExpressionDictionary;
    }
}
