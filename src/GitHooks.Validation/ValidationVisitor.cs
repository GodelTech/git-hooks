using GitHooks.Diagnostics;
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
    DiagnosticBag diagnostics)
    : IAstCommandVisitor
{
    private readonly ValidationRuleSet _rules = rules;
    private readonly DiagnosticBag _diagnostics = diagnostics;

    public void Visit(PipelineNode node)
    {
        foreach (var rule in _rules.PipelineRules)
        {
            rule.Validate(node, _diagnostics);
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

    public void Visit(ParameterNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(ScriptStepNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(TemplateStepNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(InvalidStepNode node)
    {
        throw new NotImplementedException();
    }

    // Fields
    public void Visit<TValue>(StringKeyFieldNode<TValue> node)
        where TValue : AstNode
    {
        throw new NotImplementedException();
    }

    public void Visit<TValue>(ComplexKeyFieldNode<TValue> node)
        where TValue : AstNode
    {
        throw new NotImplementedException();
    }

    public void Visit<TValue>(SequenceFieldNode<TValue> node)
        where TValue : AstNode
    {
        throw new NotImplementedException();
    }

    public void Visit<TField>(MappingFieldNode<TField> node)
        where TField : FieldNode
    {
        throw new NotImplementedException();
    }

    // Values
    public void Visit(ScalarNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(SequenceNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(MappingNode node)
    {
        throw new NotImplementedException();
    }

    // Expressions
    public void Visit(BooleanLiteralExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(IntegerLiteralExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(StringLiteralExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(InterpolatedStringExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public void Visit(VariableExpressionNode node)
    {
        throw new NotImplementedException();
    }
}
