using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Values;
using GitHooks.Testing.Ast.Builders.Expressions;
using GitHooks.Testing.Ast.Builders.Fields;
using GitHooks.Testing.Ast.Builders.Mappings;
using GitHooks.Testing.Ast.Builders.Mappings.Parameters;
using GitHooks.Testing.Ast.Builders.Mappings.Steps;
using GitHooks.Testing.Ast.Builders.Values;

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

            // Fields
            (CreateStringKeyFieldNode(), AstNodeKind.StringKeyField, nameof(StringKeyFieldNode<>)),
            (CreateComplexKeyFieldNode(), AstNodeKind.ComplexKeyField, nameof(ComplexKeyFieldNode<>)),
            (CreateSequenceFieldNode(), AstNodeKind.SequenceField, nameof(SequenceFieldNode<>)),
            (CreateMappingFieldNode(), AstNodeKind.MappingField, nameof(MappingFieldNode<>)),

            // Values
            (CreateScalarNode(), AstNodeKind.Scalar, nameof(ScalarNode)),
            (CreateSequenceNode(), AstNodeKind.Sequence, nameof(SequenceNode)),
            (CreateMappingNode(), AstNodeKind.Mapping, nameof(MappingNode)),

            // Expressions
            (CreateBooleanNode(), AstNodeKind.BooleanLiteralExpression, nameof(BooleanLiteralExpressionNode)),
            (CreateIntegerNode(), AstNodeKind.IntegerLiteralExpression, nameof(IntegerLiteralExpressionNode)),
            (CreateStringNode(), AstNodeKind.StringLiteralExpression, nameof(StringLiteralExpressionNode)),
            (CreateInterpolatedNode(), AstNodeKind.InterpolatedStringExpression, nameof(InterpolatedStringExpressionNode)),
            (CreateParameterVariableNode(), AstNodeKind.ParameterVariableExpression, nameof(ParameterVariableExpressionNode)),
            (CreateInvalidVariableNode(), AstNodeKind.InvalidVariableExpression, nameof(InvalidVariableExpressionNode))
        ];

    public static ParameterNode CreateParameterNode()
    {
        return new ParameterNodeBuilder()
            .WithName("configuration")
            .Build();
    }

    public static ScriptStepNode CreateScriptStepNode()
    {
        return new ScriptStepNodeBuilder()
            .WithScript("dotnet test")
            .Build();
    }

    public static TemplateStepNode CreateTemplateStepNode()
    {
        return new TemplateStepNodeBuilder()
            .WithTemplate("build.yml")
            .Build();
    }

    public static InvalidStepNode CreateInvalidStepNode()
    {
        return new InvalidStepNodeBuilder()
            .WithField("custom", "value")
            .Build();
    }

    private static PipelineNode CreatePipelineNode()
    {
        return new PipelineNodeBuilder()
            .WithParameter(x => x.WithName("configuration"))
            .WithScriptStep(x => x.WithScript("dotnet test"))
            .Build();
    }

    // Fields
    private static StringKeyFieldNode<ExpressionNode> CreateStringKeyFieldNode()
    {
        return new StringKeyFieldNodeBuilder()
            .WithKey("field")
            .WithValue("value")
            .Build();
    }

    private static ComplexKeyFieldNode<AstNode> CreateComplexKeyFieldNode()
    {
        return new ComplexKeyFieldNodeBuilder()
            .WithKey("complexKey")
            .WithScalarValue(x => x.WithValue("value"))
            .Build();
    }

    private static SequenceFieldNode<ExpressionNode> CreateSequenceFieldNode()
    {
        return new SequenceFieldNodeBuilder()
            .WithKey("complexKey")
            .WithStringLiteralExpressionItem(x => x.WithValue("value"))
            .Build();
    }

    private static MappingFieldNode<StringKeyFieldNode<ExpressionNode>> CreateMappingFieldNode()
    {
        return new MappingFieldNodeBuilder()
            .WithKey("field")
            .WithStringKeyField(x => x
                .WithKey("key")
                .WithValue("value"))
            .Build();
    }

    // Values
    private static ScalarNode CreateScalarNode()
    {
        return new ScalarNodeBuilder()
            .WithValue("raw")
            .Build();
    }

    private static SequenceNode CreateSequenceNode()
    {
        return new SequenceNodeBuilder()
            .WithScalarItem(x => x.WithValue("item"))
            .Build();
    }

    private static MappingNode CreateMappingNode()
    {
        return new MappingNodeBuilder()
            .WithStringKeyField(x => x
                .WithKey("key")
                .WithValue("value"))
            .Build();
    }

    // Expressions
    private static BooleanLiteralExpressionNode CreateBooleanNode()
    {
        return new BooleanLiteralExpressionNodeBuilder()
            .WithValue(true)
            .Build();
    }

    private static IntegerLiteralExpressionNode CreateIntegerNode()
    {
        return new IntegerLiteralExpressionNodeBuilder()
            .WithValue(10)
            .Build();
    }

    private static StringLiteralExpressionNode CreateStringNode()
    {
        return new StringLiteralExpressionNodeBuilder()
            .WithValue("value")
            .Build();
    }

    private static InterpolatedStringExpressionNode CreateInterpolatedNode()
    {
        return new InterpolatedStringExpressionNodeBuilder()
            .WithStringPart(x => x.WithValue("value"))
            .Build();
    }

    private static ParameterVariableExpressionNode CreateParameterVariableNode()
    {
        return new ParameterVariableExpressionNodeBuilder()
            .WithName("configuration")
            .Build();
    }

    private static InvalidVariableExpressionNode CreateInvalidVariableNode()
    {
        return new InvalidVariableExpressionNodeBuilder()
            .WithText("foo.bar")
            .Build();
    }
}
