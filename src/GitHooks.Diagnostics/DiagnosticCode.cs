namespace GitHooks.Diagnostics;

public sealed record DiagnosticCode(
    string Value)
{
    public override string ToString()
    {
        return Value;
    }
}
