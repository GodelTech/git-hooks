namespace GitHooks.Diagnostics;

public static class DiagnosticDescriptors
{
    // GH0001-GH0999 Parsing
    public static readonly DiagnosticDescriptor InvalidYaml =
        new()
        {
            Code = DiagnosticCode.Create(1),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Invalid YAML: {0}"
        };

    public static readonly DiagnosticDescriptor DuplicateField =
        new()
        {
            Code = DiagnosticCode.Create(2),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Duplicate '{0}' field."
        };

    // GH1000-GH1999 Pipeline
    public static readonly DiagnosticDescriptor PipelineMustContainStep =
        new()
        {
            Code = DiagnosticCode.Create(1001),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Pipeline must contain at least one step."
        };

    // GH2000-GH2999 Parameters

    // GH3000-GH3999 Steps

    // GH9000-GH9999 Internal
}
