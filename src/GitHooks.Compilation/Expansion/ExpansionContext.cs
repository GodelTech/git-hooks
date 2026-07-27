namespace GitHooks.Compilation.Expansion;

public sealed class ExpansionContext
{
    public Stack<TemplateReference> ExpansionStack { get; }
        = new();
}
