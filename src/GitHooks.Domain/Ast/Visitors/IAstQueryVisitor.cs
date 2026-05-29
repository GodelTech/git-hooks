using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Unknown;

namespace GitHooks.Domain.Ast.Visitors;

public interface IAstQueryVisitor<out TResult>
{
    public TResult Visit(PipelineNode node);

    public TResult Visit(ParameterNode node);

    public TResult Visit(ScriptStepNode node);

    public TResult Visit(TemplateStepNode node);

    // TODO: split into IExpressionVisitor (decide later)
    public TResult Visit(BooleanLiteralExpressionNode node);

    public TResult Visit(IntegerLiteralExpressionNode node);

    public TResult Visit(StringLiteralExpressionNode node);

    public TResult Visit(VariableExpressionNode node);

    public TResult Visit(InterpolatedStringExpressionNode node);

    // Unknown
    public TResult VisitUnknownNode(UnknownNode node);
}
