namespace GitHooks.Domain.Common;

public readonly record struct SourceSpan(
    SourcePosition Start,
    SourcePosition End)
{
    public static SourceSpan Unknown { get; }
        = new(SourcePosition.Unknown, SourcePosition.Unknown);

    public bool IsUnknown
        => Start.IsUnknown &&
           End.IsUnknown;

    public static SourceSpan Combine(
        SourceSpan start,
        SourceSpan end)
    {
        return new SourceSpan(
            start.Start,
            end.End);
    }
}
