namespace GitHooks.Domain.Common;

public readonly record struct SourceSpan(
    SourceDocument? Document,
    SourcePosition Start,
    SourcePosition End)
{
    public static readonly SourceSpan Unknown
        = new(null, SourcePosition.Unknown, SourcePosition.Unknown);

    public bool HasDocument
        => Document is not null;

    public bool HasUnknownPosition
        => Start.IsUnknown &&
           End.IsUnknown;

    public static SourceSpan Combine(
        SourceSpan start,
        SourceSpan end)
    {
        if (!Equals(start.Document, end.Document))
        {
            throw new ArgumentException(
                "Cannot combine spans from different documents.");
        }

        return new SourceSpan(
            start.Document,
            start.Start,
            end.End);
    }
}
