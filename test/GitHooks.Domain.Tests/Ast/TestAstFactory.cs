using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Unknown;
using GitHooks.Domain.Ast.Values;
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
        (CreateInvalidStepNode(), AstNodeKind.InvalidStep, nameof(InvalidStepNode)),
        (CreateBooleanNode(), AstNodeKind.BooleanLiteralExpression, nameof(BooleanLiteralExpressionNode)),
        (CreateIntegerNode(), AstNodeKind.IntegerLiteralExpression, nameof(IntegerLiteralExpressionNode)),
        (CreateStringNode(), AstNodeKind.StringLiteralExpression, nameof(StringLiteralExpressionNode)),
        (CreateVariableNode(), AstNodeKind.VariableExpression, nameof(VariableExpressionNode)),
        (CreateInterpolatedNode(), AstNodeKind.InterpolatedStringExpression, nameof(InterpolatedStringExpressionNode)),
        (CreateUnknownSimpleFieldNode(), AstNodeKind.UnknownSimpleField, nameof(UnknownNode)),
        (CreateUnknownComplexFieldNode(), AstNodeKind.UnknownComplexField, nameof(UnknownNode)),
        (CreateUnknownScalarNode(), AstNodeKind.UnknownScalar, nameof(UnknownNode)),
        (CreateUnknownSequenceNode(), AstNodeKind.UnknownSequence, nameof(UnknownNode)),
        (CreateUnknownMappingNode(), AstNodeKind.UnknownMapping, nameof(UnknownNode)),
        (CreateStringKeyFieldNode(), AstNodeKind.StringKeyField, nameof(StringKeyFieldNode<>)),
        (CreateComplexKeyFieldNode(), AstNodeKind.ComplexKeyField, nameof(ComplexKeyFieldNode<>)),
        (CreateMappingFieldNode(), AstNodeKind.MappingField, nameof(MappingFieldNode<>)),
        (CreateScalarNode(), AstNodeKind.Scalar, nameof(ScalarNode)),
        (CreateSequenceNode(), AstNodeKind.Sequence, nameof(SequenceNode)),
        (CreateMappingNode(), AstNodeKind.Mapping, nameof(MappingNode))
    ];

    public static ParameterNode CreateParameterNode()
    {
        return new ParameterNode
        {
            Name = "configuration",
            UnknownFields = [],
            Span = SourceSpan.Unknown
        };
    }

    public static ScriptStepNode CreateScriptStepNode()
    {
        return new ScriptStepNode
        {
            Script = CreateStringKeyFieldNode(),
            UnknownFields = [],
            Span = SourceSpan.Unknown
        };
    }

    public static TemplateStepNode CreateTemplateStepNode()
    {
        return new TemplateStepNode
        {
            Template = CreateStringKeyFieldNode(),
            UnknownFields = [],
            Span = SourceSpan.Unknown
        };
    }

    public static InvalidStepNode CreateInvalidStepNode()
    {
        return new InvalidStepNode
        {
            Fields = [],
            UnknownFields = [],
            Span = SourceSpan.Unknown
        };
    }

    private static PipelineNode CreatePipelineNode()
    {
        return new PipelineNode
        {
            Parameters = [],
            Steps = [],
            UnknownFields = [],
            Span = SourceSpan.Unknown
        };
    }

    private static BooleanLiteralExpressionNode CreateBooleanNode()
    {
        return new BooleanLiteralExpressionNode
        {
            Value = true,
            Span = SourceSpan.Unknown
        };
    }

    private static IntegerLiteralExpressionNode CreateIntegerNode()
    {
        return new IntegerLiteralExpressionNode
        {
            Value = 10,
            Span = SourceSpan.Unknown
        };
    }

    private static StringLiteralExpressionNode CreateStringNode()
    {
        return new StringLiteralExpressionNode
        {
            Value = "value",
            Span = SourceSpan.Unknown
        };
    }

    private static VariableExpressionNode CreateVariableNode()
    {
        return new VariableExpressionNode
        {
            Path = "parameters.configuration",
            Span = SourceSpan.Unknown
        };
    }

    private static InterpolatedStringExpressionNode CreateInterpolatedNode()
    {
        return new InterpolatedStringExpressionNode
        {
            Parts = [CreateStringNode(), CreateVariableNode()],
            Span = SourceSpan.Unknown
        };
    }

    private static UnknownSimpleFieldNode CreateUnknownSimpleFieldNode()
    {
        return new UnknownSimpleFieldNode
        {
            Key = "custom",
            Value = CreateUnknownScalarNode(),
            Span = SourceSpan.Unknown
        };
    }

    private static UnknownComplexFieldNode CreateUnknownComplexFieldNode()
    {
        return new UnknownComplexFieldNode
        {
            Key = CreateUnknownScalarNode(),
            Value = CreateUnknownScalarNode(),
            Span = SourceSpan.Unknown
        };
    }

    private static UnknownScalarNode CreateUnknownScalarNode()
    {
        return new UnknownScalarNode
        {
            Value = "raw",
            Span = SourceSpan.Unknown
        };
    }

    private static UnknownSequenceNode CreateUnknownSequenceNode()
    {
        return new UnknownSequenceNode
        {
            Items = [CreateUnknownScalarNode()],
            Span = SourceSpan.Unknown
        };
    }

    private static UnknownMappingNode CreateUnknownMappingNode()
    {
        return new UnknownMappingNode
        {
            Fields = [CreateUnknownSimpleFieldNode()],
            Span = SourceSpan.Unknown
        };
    }

    private static StringKeyFieldNode<ExpressionNode> CreateStringKeyFieldNode()
    {
        return new StringKeyFieldNode<ExpressionNode>
        {
            Key = "field",
            Value = CreateStringNode(),
            Span = SourceSpan.Unknown
        };
    }

    private static ComplexKeyFieldNode<ExpressionNode> CreateComplexKeyFieldNode()
    {
        return new ComplexKeyFieldNode<ExpressionNode>
        {
            Key = CreateScalarNode(),
            Value = CreateStringNode(),
            Span = SourceSpan.Unknown
        };
    }

    private static MappingFieldNode<StringKeyFieldNode<ExpressionNode>> CreateMappingFieldNode()
    {
        return new MappingFieldNode<StringKeyFieldNode<ExpressionNode>>
        {
            Key = "field",
            Fields = [],
            Span = SourceSpan.Unknown
        };
    }

    private static ScalarNode CreateScalarNode()
    {
        return new ScalarNode
        {
            Value = "raw",
            Span = SourceSpan.Unknown
        };
    }

    private static SequenceNode CreateSequenceNode()
    {
        return new SequenceNode
        {
            Items = [CreateScalarNode()],
            Span = SourceSpan.Unknown
        };
    }

    private static MappingNode CreateMappingNode()
    {
        return new MappingNode
        {
            Fields = [CreateStringKeyFieldNode()],
            Span = SourceSpan.Unknown
        };
    }
}
