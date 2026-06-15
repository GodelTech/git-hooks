namespace GitHooks.Domain.Ast;

public enum AstNodeKind
{
    Pipeline,

    Parameter,

    ScriptStep,
    TemplateStep,
    InvalidStep,

    BooleanLiteralExpression,
    IntegerLiteralExpression,
    StringLiteralExpression,
    VariableExpression,
    InterpolatedStringExpression,

    UnknownSimpleField,
    UnknownComplexField,
    UnknownScalar,
    UnknownSequence,
    UnknownMapping,

    StringKeyField,
    ComplexKeyField,
    MappingField,

    Scalar,
    Sequence,
    Mapping
}
