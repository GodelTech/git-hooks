namespace GitHooks.Diagnostics;

public readonly record struct DiagnosticCode(
    string Value)
{
    public static DiagnosticCode Create(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        return new($"GH{value:D4}");
    }

    public override string ToString()
    {
        return Value;
    }
}
