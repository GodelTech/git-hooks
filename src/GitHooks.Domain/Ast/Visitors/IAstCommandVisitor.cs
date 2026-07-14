using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Values;

namespace GitHooks.Domain.Ast.Visitors;

public interface IAstCommandVisitor
{
    public void Visit(PipelineNode node);

    public void Visit(ParameterNode node);

    public void Visit(ScriptStepNode node);

    public void Visit(TemplateStepNode node);

    public void Visit(InvalidStepNode node);

    // Fields
    public void Visit<TValue>(StringKeyFieldNode<TValue> node)
        where TValue : AstNode;

    public void Visit<TValue>(ComplexKeyFieldNode<TValue> node)
        where TValue : AstNode;

    public void Visit<TValue>(SequenceFieldNode<TValue> node)
        where TValue : AstNode;

    public void Visit<TField>(MappingFieldNode<TField> node)
        where TField : FieldNode;

    // Values
    public void Visit(ScalarNode node);

    public void Visit(SequenceNode node);

    public void Visit(MappingNode node);

    // TODO: Split expression visitors when expressions gain independent processing.
    // Expressions
    public void Visit(BooleanLiteralExpressionNode node);

    public void Visit(IntegerLiteralExpressionNode node);

    public void Visit(StringLiteralExpressionNode node);

    public void Visit(InterpolatedStringExpressionNode node);

    public void Visit(ParameterVariableExpressionNode node);

    public void Visit(InvalidVariableExpressionNode node);
}
