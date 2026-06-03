using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Unknown;
using GitHooks.Domain.Common;

namespace GitHooks.Domain.Tests.Ast;

internal static class TestAstFactory
{
    public static IReadOnlyList<(AstNode Node, AstNodeKind ExpectedKind, string ExpectedVisit)> NodeCases { get; } =
    [
        (CreatePipelineNode(), AstNodeKind.Pipeline, nameof(PipelineNode)),
        (CreateParameterNode(), AstNodeKind.Parameter, nameof(ParameterNode)),
        (CreateScriptStepNode(), AstNodeKind.ScriptStep, nameof(ScriptStepNode)),
        (CreateTemplateStepNode(), AstNodeKind.TemplateStep, nameof(TemplateStepNode)),
        (CreateBooleanNode(), AstNodeKind.BooleanLiteralExpression, nameof(BooleanLiteralExpressionNode)),
        (CreateIntegerNode(), AstNodeKind.IntegerLiteralExpression, nameof(IntegerLiteralExpressionNode)),
        (CreateStringNode(), AstNodeKind.StringLiteralExpression, nameof(StringLiteralExpressionNode)),
        (CreateVariableNode(), AstNodeKind.VariableExpression, nameof(VariableExpressionNode)),
        (CreateInterpolatedNode(), AstNodeKind.InterpolatedStringExpression, nameof(InterpolatedStringExpressionNode)),
        (CreateUnknownSimpleFieldNode(), AstNodeKind.UnknownSimpleField, nameof(UnknownNode)),
        (CreateUnknownComplexFieldNode(), AstNodeKind.UnknownComplexField, nameof(UnknownNode)),
        (CreateUnknownScalarNode(), AstNodeKind.UnknownScalar, nameof(UnknownNode)),
        (CreateUnknownSequenceNode(), AstNodeKind.UnknownSequence, nameof(UnknownNode)),
        (CreateUnknownMappingNode(), AstNodeKind.UnknownMapping, nameof(UnknownNode))
    ];

    public static ParameterNode CreateParameterNode()
    {
        return new ParameterNode
        {
            Span = SourceSpan.Unknown,
            UnknownFields = [],
            Name = "configuration"
        };
    }

    public static ScriptStepNode CreateScriptStepNode()
    {
        return new ScriptStepNode
        {
            Span = SourceSpan.Unknown,
            UnknownFields = [],
            Script = CreateStringNode()
        };
    }

    public static TemplateStepNode CreateTemplateStepNode()
    {
        return new TemplateStepNode
        {
            Span = SourceSpan.Unknown,
            UnknownFields = [],
            Template = CreateStringNode()
        };
    }

    private static PipelineNode CreatePipelineNode()
    {
        return new PipelineNode
        {
            Span = SourceSpan.Unknown,
            UnknownFields = [],
            Parameters = [],
            Steps = []
        };
    }

    private static BooleanLiteralExpressionNode CreateBooleanNode()
    {
        return new BooleanLiteralExpressionNode
        {
            Span = SourceSpan.Unknown,
            Value = true
        };
    }

    private static IntegerLiteralExpressionNode CreateIntegerNode()
    {
        return new IntegerLiteralExpressionNode
        {
            Span = SourceSpan.Unknown,
            Value = 10
        };
    }

    private static StringLiteralExpressionNode CreateStringNode()
    {
        return new StringLiteralExpressionNode
        {
            Span = SourceSpan.Unknown,
            Value = "value"
        };
    }

    private static VariableExpressionNode CreateVariableNode()
    {
        return new VariableExpressionNode
        {
            Span = SourceSpan.Unknown,
            Path = "parameters.configuration"
        };
    }

    private static InterpolatedStringExpressionNode CreateInterpolatedNode()
    {
        return new InterpolatedStringExpressionNode
        {
            Span = SourceSpan.Unknown,
            Parts = [CreateStringNode(), CreateVariableNode()]
        };
    }

    private static UnknownScalarNode CreateUnknownScalarNode()
    {
        return new UnknownScalarNode
        {
            Span = SourceSpan.Unknown,
            Value = "raw"
        };
    }

    private static UnknownSimpleFieldNode CreateUnknownSimpleFieldNode()
    {
        return new UnknownSimpleFieldNode
        {
            Span = SourceSpan.Unknown,
            Key = "custom",
            Value = CreateUnknownScalarNode()
        };
    }

    private static UnknownComplexFieldNode CreateUnknownComplexFieldNode()
    {
        return new UnknownComplexFieldNode
        {
            Span = SourceSpan.Unknown,
            Key = CreateUnknownScalarNode(),
            Value = CreateUnknownScalarNode()
        };
    }

    private static UnknownSequenceNode CreateUnknownSequenceNode()
    {
        return new UnknownSequenceNode
        {
            Span = SourceSpan.Unknown,
            Items = [CreateUnknownScalarNode()]
        };
    }

    private static UnknownMappingNode CreateUnknownMappingNode()
    {
        return new UnknownMappingNode
        {
            Span = SourceSpan.Unknown,
            Fields = [CreateUnknownSimpleFieldNode()]
        };
    }
}
