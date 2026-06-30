using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Values;

namespace GitHooks.Domain.Ast.Visitors;

public abstract class AstWalker
    : IAstCommandVisitor
{
    public virtual void Visit(
        PipelineNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        VisitNodes(node.Parameters);
        VisitNodes(node.Steps);

        WalkUnknownFields(node);
    }

    public virtual void Visit(
        ParameterNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        VisitNode(node.Name);
        VisitNode(node.DisplayName);
        VisitNode(node.Type);
        VisitNode(node.DefaultValue);
        VisitNode(node.Values);

        WalkUnknownFields(node);
    }

    public virtual void Visit(
        ScriptStepNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        VisitNode(node.Script);
        VisitNode(node.DisplayName);
        VisitNode(node.Condition);
        VisitNode(node.TimeoutInMinutes);
        VisitNode(node.WorkingDirectory);
        VisitNode(node.Env);

        WalkUnknownFields(node);
    }

    public virtual void Visit(
        TemplateStepNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        VisitNode(node.Template);
        VisitNode(node.Parameters);

        WalkUnknownFields(node);
    }

    public virtual void Visit(
        InvalidStepNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        VisitNodes(node.Fields);

        WalkUnknownFields(node);
    }

    // Fields
    public virtual void Visit<TValue>(
        StringKeyFieldNode<TValue> node)
        where TValue : AstNode
    {
        ArgumentNullException.ThrowIfNull(node);

        VisitNode(node.Value);
    }

    public virtual void Visit<TValue>(
        ComplexKeyFieldNode<TValue> node)
        where TValue : AstNode
    {
        ArgumentNullException.ThrowIfNull(node);

        WalkComplexFieldKey(node);
        WalkComplexFieldValue(node);
    }

    public virtual void Visit<TValue>(
        SequenceFieldNode<TValue> node)
        where TValue : AstNode
    {
        ArgumentNullException.ThrowIfNull(node);

        VisitNodes(node.Items);
    }

    public virtual void Visit<TField>(
        MappingFieldNode<TField> node)
        where TField : FieldNode
    {
        ArgumentNullException.ThrowIfNull(node);

        VisitNodes(node.Fields);
    }

    // Values
    public virtual void Visit(
        ScalarNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
    }

    public virtual void Visit(
        SequenceNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        VisitNodes(node.Items);
    }

    public virtual void Visit(
        MappingNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        VisitNodes(node.Fields);
    }

    // Expressions
    public virtual void Visit(
        BooleanLiteralExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
    }

    public virtual void Visit(
        IntegerLiteralExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
    }

    public virtual void Visit(
        StringLiteralExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
    }

    public virtual void Visit(
        InterpolatedStringExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        VisitNodes(node.Parts);
    }

    public virtual void Visit(
        VariableExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
    }

    protected void Walk(
        AstNode root)
    {
        ArgumentNullException.ThrowIfNull(root);

        root.Accept(this);
    }

    protected virtual void WalkUnknownFields(
        PipelineNodeBase node)
    {
        ArgumentNullException.ThrowIfNull(node);

        VisitNodes(node.UnknownFields);
    }

    protected virtual void WalkComplexFieldKey<T>(
        ComplexKeyFieldNode<T> node)
        where T : AstNode
    {
        ArgumentNullException.ThrowIfNull(node);

        VisitNode(node.Key);
    }

    protected virtual void WalkComplexFieldValue<T>(
        ComplexKeyFieldNode<T> node)
        where T : AstNode
    {
        ArgumentNullException.ThrowIfNull(node);

        VisitNode(node.Value);
    }

    private void VisitNode(
        AstNode? node)
    {
        node?.Accept(this);
    }

    private void VisitNodes<TNode>(
        IEnumerable<TNode> nodes)
        where TNode : AstNode
    {
        ArgumentNullException.ThrowIfNull(nodes);

        foreach (var node in nodes)
        {
            VisitNode(node);
        }
    }
}
