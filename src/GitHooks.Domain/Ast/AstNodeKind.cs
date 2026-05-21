namespace GitHooks.Domain.Ast;

public enum AstNodeKind
{
    Pipeline,

    Parameter,

    ScriptStep,

    StringLiteralExpression,
    IntegerLiteralExpression,
    BooleanLiteralExpression
}
