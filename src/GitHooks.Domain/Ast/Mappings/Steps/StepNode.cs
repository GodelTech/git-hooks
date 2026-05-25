namespace GitHooks.Domain.Ast.Mappings.Steps;

public abstract record StepNode
    : MappingNode
{
    public string? Name { get; init; }
}
