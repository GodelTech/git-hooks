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

    // GH1000-GH1999 Structural validation
    public static readonly DiagnosticDescriptor DuplicateField =
        new()
        {
            Code = DiagnosticCode.Create(1001),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Duplicate '{0}' field."
        };

    public static readonly DiagnosticDescriptor ExpectedScalarKeyInMapping =
        new()
        {
            Code = DiagnosticCode.Create(1002),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Expected a scalar key in '{0}' mapping."
        };

    // GH2000-GH2999 Pipeline
    public static readonly DiagnosticDescriptor PipelineMustContainStep =
        new()
        {
            Code = DiagnosticCode.Create(2001),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Pipeline must contain at least one step."
        };

    // GH3000-GH3999 Parameters
    public static readonly DiagnosticDescriptor ParameterNameRequired =
        new()
        {
            Code = DiagnosticCode.Create(3001),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Parameter name is required."
        };

    public static readonly DiagnosticDescriptor UnsupportedParameterType =
        new()
        {
            Code = DiagnosticCode.Create(3002),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Unsupported parameter type '{0}'."
        };

    // GH4000-GH4999 Steps
    public static readonly DiagnosticDescriptor InvalidStepField =
        new()
        {
            Code = DiagnosticCode.Create(4001),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Field '{0}' is not valid for {1} steps."
        };

    public static readonly DiagnosticDescriptor MultipleStepTypes =
        new()
        {
            Code = DiagnosticCode.Create(4002),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Step cannot contain multiple step type fields."
        };

    public static readonly DiagnosticDescriptor MissingStepType =
        new()
        {
            Code = DiagnosticCode.Create(4003),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Step must contain either 'script' or 'template' field."
        };

    // GH5000-GH5999 Semantic validation

    // GH9000-GH9999 Internal
}
