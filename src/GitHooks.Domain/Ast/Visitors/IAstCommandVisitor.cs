using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Unknown;

namespace GitHooks.Domain.Ast.Visitors;

public interface IAstCommandVisitor
{
    public void Visit(PipelineNode node);

    public void Visit(ParameterNode node);

    public void Visit(ScriptStepNode node);

    // TODO: split into IExpressionVisitor (decide later)
    public void Visit(BooleanLiteralExpressionNode node);

    public void Visit(IntegerLiteralExpressionNode node);

    public void Visit(StringLiteralExpressionNode node);

    // Unknown
    public void VisitUnknownNode(UnknownNode node);
}
