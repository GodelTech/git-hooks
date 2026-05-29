namespace GitHooks.Domain.Ast;

public enum AstNodeKind
{
    Pipeline,

    Parameter,

    ScriptStep,
    TemplateStep,

    StringLiteralExpression,
    IntegerLiteralExpression,
    BooleanLiteralExpression,
    VariableExpression,
    InterpolatedStringExpression,

    UnknownSimpleField,
    UnknownComplexField,
    UnknownScalar,
    UnknownSequence,
    UnknownMapping
}
