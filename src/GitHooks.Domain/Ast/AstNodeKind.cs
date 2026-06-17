namespace GitHooks.Domain.Ast;

public enum AstNodeKind
{
    Pipeline,

    Parameter,

    ScriptStep,
    TemplateStep,
    InvalidStep,

    // Fields
    StringKeyField,
    ComplexKeyField,
    SequenceField,
    MappingField,

    // Values
    Scalar,
    Sequence,
    Mapping,

    // Expressions
    BooleanLiteralExpression,
    IntegerLiteralExpression,
    StringLiteralExpression,
    VariableExpression,
    InterpolatedStringExpression
}
