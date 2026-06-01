namespace GitHooks.Diagnostics;

public readonly record struct DiagnosticCode(
    string Value)
{
    public static readonly DiagnosticCode InvalidYaml
        = new("GH0001");

    public static readonly DiagnosticCode UnexpectedNode
        = new("GH0002");

    public static readonly DiagnosticCode MissingRequiredProperty
        = new("GH0003");

    public static readonly DiagnosticCode InvalidPropertyType
        = new("GH0004");

    public static readonly DiagnosticCode UnknownStepType
        = new("GH0005");

    public static readonly DiagnosticCode InvalidScriptValue
        = new("GH0006");

    public override string ToString()
    {
        return Value;
    }
}
