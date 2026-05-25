namespace GitHooks.Domain.Ast;

public enum AstNodeKind
{
    UnknownField,
    UnknownScalar,
    UnknownSequence,
    UnknownMapping,

    Pipeline,
    Parameter,
    ScriptStep,

    StringLiteralExpression,
    IntegerLiteralExpression,
    BooleanLiteralExpression
}
