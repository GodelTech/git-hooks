namespace GitHooks.Domain.Ast;

public abstract record StepNode
    : AstNode
{
    public string? Name { get; init; }
}
