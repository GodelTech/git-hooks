namespace GitHooks.Domain.Common;

public readonly record struct SourcePosition(
    int Line,
    int Column)
{
    public static SourcePosition Unknown { get; }
        = new(-1, -1);

    public bool HasKnownLine
        => Line >= 0;

    public bool HasKnownColumn
        => Column >= 0;

    public bool IsUnknown
        => !HasKnownLine &&
           !HasKnownColumn;
}
