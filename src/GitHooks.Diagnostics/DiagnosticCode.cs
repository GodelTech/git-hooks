namespace GitHooks.Diagnostics;

public readonly record struct DiagnosticCode(
    string Value)
{
    public static readonly DiagnosticCode InvalidYaml
        = new("GH0001");

    public static readonly DiagnosticCode PipelineMustContainStep
        = new("GH1001");

    public override string ToString()
    {
        return Value;
    }
}
