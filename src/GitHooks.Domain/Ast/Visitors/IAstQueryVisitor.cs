using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Unknown;
using GitHooks.Domain.Ast.Values;

namespace GitHooks.Domain.Ast.Visitors;

public interface IAstQueryVisitor<out TResult>
{
    public TResult Visit(PipelineNode node);

    public TResult Visit(ParameterNode node);

    public TResult Visit(ScriptStepNode node);

    public TResult Visit(TemplateStepNode node);

    public TResult Visit(InvalidStepNode node);

    // TODO: split into IExpressionVisitor (decide later)
    public TResult Visit(BooleanLiteralExpressionNode node);

    public TResult Visit(IntegerLiteralExpressionNode node);

    public TResult Visit(StringLiteralExpressionNode node);

    public TResult Visit(VariableExpressionNode node);

    public TResult Visit(InterpolatedStringExpressionNode node);

    // Unknown
    public TResult VisitUnknownNode(UnknownNode node);

    // Fields
    public TResult Visit<TValue>(StringKeyFieldNode<TValue> node)
        where TValue : AstNode;

    public TResult Visit<TValue>(ComplexKeyFieldNode<TValue> node)
        where TValue : AstNode;

    public TResult Visit<TField>(MappingFieldNode<TField> node)
        where TField : FieldNode;

    public TResult Visit(ScalarNode node);

    public TResult Visit(SequenceNode node);

    public TResult Visit(MappingNode node);
}
