namespace GitHooks.Domain.Ast.Unknown;

public abstract record UnknownFieldNode
    : UnknownNode
{
    public required UnknownNode Value { get; init; }
}
