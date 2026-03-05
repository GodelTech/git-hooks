namespace GitHooks.Pipelines.Models;

public sealed record ParseError(string Message, long Line, long Column)
{
    public override string ToString() => $"({Line},{Column}): {Message}";
}
