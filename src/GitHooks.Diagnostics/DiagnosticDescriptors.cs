namespace GitHooks.Diagnostics;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor InvalidYaml =
        new()
        {
            Code = DiagnosticCode.InvalidYaml,
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Invalid YAML: {0}"
        };

    public static readonly DiagnosticDescriptor PipelineMustContainStep =
        new()
        {
            Code = DiagnosticCode.PipelineMustContainStep,
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Pipeline must contain at least one step."
        };
}
