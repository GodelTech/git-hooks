using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Values;
using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Validation;

internal sealed class ValidationVisitor(
    ValidationRuleSet rules,
    ValidationContext context)
    : IAstCommandVisitor
{
    private readonly ValidationRuleSet _rules = rules;
    private readonly ValidationContext _context = context;

    public void Visit(
        PipelineNode node)
    {
        foreach (var rule in _rules.PipelineRules)
        {
            rule.Validate(node, _context);
        }

        foreach (var parameter in node.Parameters)
        {
            parameter.Accept(this);
        }

        foreach (var step in node.Steps)
        {
            step.Accept(this);
        }
    }

    public void Visit(
        ParameterNode node)
    {
        foreach (var rule in _rules.ParameterRules)
        {
            rule.Validate(node, _context);
        }
    }

    public void Visit(
        ScriptStepNode node)
    {
        foreach (var rule in _rules.ScriptStepRules)
        {
            rule.Validate(node, _context);
        }
    }

    public void Visit(
        TemplateStepNode node)
    {
        foreach (var rule in _rules.TemplateStepRules)
        {
            rule.Validate(node, _context);
        }
    }

    public void Visit(
        InvalidStepNode node)
    {
        foreach (var rule in _rules.InvalidStepRules)
        {
            rule.Validate(node, _context);
        }
    }

    // Fields
    public void Visit<TValue>(
        StringKeyFieldNode<TValue> node)
        where TValue : AstNode
    {
        throw new NotImplementedException();
    }

    public void Visit<TValue>(
        ComplexKeyFieldNode<TValue> node)
        where TValue : AstNode
    {
        throw new NotImplementedException();
    }

    public void Visit<TValue>(
        SequenceFieldNode<TValue> node)
        where TValue : AstNode
    {
        throw new NotImplementedException();
    }

    public void Visit<TField>(
        MappingFieldNode<TField> node)
        where TField : FieldNode
    {
        throw new NotImplementedException();
    }

    // Values
    public void Visit(
        ScalarNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(
        SequenceNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(
        MappingNode node)
    {
        throw new NotImplementedException();
    }

    // Expressions
    public void Visit(
        BooleanLiteralExpressionNode node)
    {
        // No validation rules for boolean literals yet.
    }

    public void Visit(
        IntegerLiteralExpressionNode node)
    {
        // No validation rules for integer literals yet.
    }

    public void Visit(
        StringLiteralExpressionNode node)
    {
        // No validation rules for plain string literals yet.
    }

    public void Visit(
        InterpolatedStringExpressionNode node)
    {
        foreach (var part in node.Parts)
        {
            part.Accept(this);
        }
    }

    public void Visit(
        VariableExpressionNode node)
    {
        foreach (var rule in _rules.VariableRules)
        {
            rule.Validate(node, _context);
        }
    }
}
