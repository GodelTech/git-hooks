namespace GitHooks.Domain.Common;

public readonly record struct SourcePosition(
    long Line,
    long Column)
{
    public static readonly SourcePosition Unknown
        = new(-1, -1);

    public bool HasKnownLine
        => Line >= 0;

    public bool HasKnownColumn
        => Column >= 0;

    public bool IsUnknown
        => !HasKnownLine &&
           !HasKnownColumn;
}
