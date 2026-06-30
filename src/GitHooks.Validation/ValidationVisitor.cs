using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Values;
using GitHooks.Domain.Ast.Visitors;
using GitHooks.Validation.Rules;

namespace GitHooks.Validation;

internal sealed class ValidationVisitor(
    ValidationRuleRegistry rules,
    ValidationContext context)
    : AstWalker
{
    private readonly ValidationRuleRegistry _rules = rules;
    private readonly ValidationContext _context = context;

    public void Validate(
        AstNode root)
    {
        Walk(root);
    }

    public override void Visit(
        PipelineNode node)
    {
        ApplyRules(node);

        base.Visit(node);
    }

    public override void Visit(
        ParameterNode node)
    {
        ApplyRules(node);

        base.Visit(node);
    }

    public override void Visit(
        ScriptStepNode node)
    {
        ApplyRules(node);

        base.Visit(node);
    }

    public override void Visit(
        TemplateStepNode node)
    {
        ApplyRules(node);

        base.Visit(node);
    }

    public override void Visit(
        InvalidStepNode node)
    {
        ApplyRules(node);

        base.Visit(node);
    }

    // Fields
    public override void Visit<TValue>(
        StringKeyFieldNode<TValue> node)
    {
        ApplyRules(node);

        base.Visit(node);
    }

    public override void Visit<TValue>(
        ComplexKeyFieldNode<TValue> node)
    {
        ApplyRules(node);

        base.Visit(node);
    }

    public override void Visit<TValue>(
        SequenceFieldNode<TValue> node)
    {
        ApplyRules(node);

        base.Visit(node);
    }

    public override void Visit<TField>(
        MappingFieldNode<TField> node)
    {
        ApplyRules(node);

        base.Visit(node);
    }

    // Values
    public override void Visit(
        ScalarNode node)
    {
        ApplyRules(node);

        base.Visit(node);
    }

    public override void Visit(
        SequenceNode node)
    {
        ApplyRules(node);

        base.Visit(node);
    }

    public override void Visit(
        MappingNode node)
    {
        ApplyRules(node);

        base.Visit(node);
    }

    // Expressions
    public override void Visit(
        BooleanLiteralExpressionNode node)
    {
        ApplyRules(node);

        base.Visit(node);
    }

    public override void Visit(
        IntegerLiteralExpressionNode node)
    {
        ApplyRules(node);

        base.Visit(node);
    }

    public override void Visit(
        StringLiteralExpressionNode node)
    {
        ApplyRules(node);

        base.Visit(node);
    }

    public override void Visit(
        InterpolatedStringExpressionNode node)
    {
        ApplyRules(node);

        base.Visit(node);
    }

    public override void Visit(
        VariableExpressionNode node)
    {
        ApplyRules(node);

        base.Visit(node);
    }

    private void ApplyRules<TNode>(
        TNode node)
        where TNode : AstNode
    {
        foreach (var rule in _rules.GetRules<TNode>())
        {
            rule.Validate(node, _context);
        }
    }
}
