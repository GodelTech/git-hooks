using GitHooks.Domain.Ast.Expressions;

namespace GitHooks.Domain.Ast.Visitors;

public interface IAstQueryVisitor<out TResult>
{
    public TResult Visit(PipelineNode node);

    public TResult Visit(ParameterNode node);

    public TResult Visit(ScriptStepNode node);

    // TODO: split into IExpressionVisitor (decide later)
    public TResult Visit(BooleanLiteralExpressionNode node);

    public TResult Visit(IntegerLiteralExpressionNode node);

    public TResult Visit(StringLiteralExpressionNode node);
}
