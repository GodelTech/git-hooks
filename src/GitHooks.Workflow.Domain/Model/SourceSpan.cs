namespace GitHooks.Workflow.Domain.Model;

public readonly record struct SourceSpan(
    SourceRef Source,
    SourceLocation Start,
    SourceLocation End)
{
    public static SourceSpan Unknown(SourceRef source)
    {
        return new SourceSpan(
            source,
            new SourceLocation(0, 0),
            new SourceLocation(0, 0)
        );
    }
}
