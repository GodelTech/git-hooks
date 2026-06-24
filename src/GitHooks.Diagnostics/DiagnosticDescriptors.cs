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

    // GH1000-GH1999 YAML Structure
    public static readonly DiagnosticDescriptor DuplicateField =
        new()
        {
            Code = DiagnosticCode.Create(1001),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Duplicate '{0}' field."
        };

    public static readonly DiagnosticDescriptor MappingKeyMustBeScalar =
        new()
        {
            Code = DiagnosticCode.Create(1002),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Only scalar keys are supported in '{0}' mapping."
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
    public static readonly DiagnosticDescriptor ParameterNameIsRequired =
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

    public static readonly DiagnosticDescriptor ParameterDefaultValueTypeMismatch =
        new()
        {
            Code = DiagnosticCode.Create(3003),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Parameter '{0}': default value type does not match declared type '{1}'."
        };

    public static readonly DiagnosticDescriptor ParameterDefaultValueMustBeInValues =
        new()
        {
            Code = DiagnosticCode.Create(3004),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Parameter '{0}': default value is not in the allowed values list."
        };

    public static readonly DiagnosticDescriptor ParameterValuesMustBeSequence =
        new()
        {
            Code = DiagnosticCode.Create(3005),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Parameter '{0}': 'values' field must be a sequence."
        };

    public static readonly DiagnosticDescriptor ParameterNameMustBeValidIdentifier =
        new()
        {
            Code = DiagnosticCode.Create(3006),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Parameter name '{0}' must start with a letter or underscore and contain only alphanumeric characters and underscores."
        };

    public static readonly DiagnosticDescriptor ParameterNamesMustBeUnique =
        new()
        {
            Code = DiagnosticCode.Create(3007),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Duplicate parameter name '{0}'."
        };

    public static readonly DiagnosticDescriptor ParameterDisplayNameMustNotBeEmpty =
        new()
        {
            Code = DiagnosticCode.Create(3008),
            Severity = DiagnosticSeverity.Warning,
            MessageFormat = "Parameter '{0}': 'displayName' field should not be empty."
        };

    // GH4000-GH4999 Steps
    public static readonly DiagnosticDescriptor InvalidStepField =
        new()
        {
            Code = DiagnosticCode.Create(4001),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Field '{0}' is not allowed in a {1} step."
        };

    public static readonly DiagnosticDescriptor MultipleStepTypes =
        new()
        {
            Code = DiagnosticCode.Create(4002),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Step must contain exactly one step type field."
        };

    public static readonly DiagnosticDescriptor MissingStepType =
        new()
        {
            Code = DiagnosticCode.Create(4003),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Step must contain either 'script' or 'template' field."
        };

    public static readonly DiagnosticDescriptor ScriptFieldMustNotBeEmpty =
        new()
        {
            Code = DiagnosticCode.Create(4004),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Script step 'script' field cannot be empty."
        };

    public static readonly DiagnosticDescriptor TemplateFieldMustNotBeEmpty =
        new()
        {
            Code = DiagnosticCode.Create(4005),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Template step 'template' field cannot be empty."
        };

    public static readonly DiagnosticDescriptor InvalidStep =
        new()
        {
            Code = DiagnosticCode.Create(4006),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Step contains errors and could not be interpreted."
        };

    public static readonly DiagnosticDescriptor StepTimeoutMustBePositive =
        new()
        {
            Code = DiagnosticCode.Create(4007),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Step 'timeoutInMinutes' must be a positive number."
        };

    // GH5000-GH5999 Variables
    public static readonly DiagnosticDescriptor ParameterVariableMustBeResolvable =
        new()
        {
            Code = DiagnosticCode.Create(5001),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Parameter '{0}' referenced but not defined."
        };

    public static readonly DiagnosticDescriptor VariableSyntaxMustBeValid =
        new()
        {
            Code = DiagnosticCode.Create(5002),
            Severity = DiagnosticSeverity.Error,
            MessageFormat = "Invalid variable syntax '{0}'. Expected format: parameters.<name>."
        };

    // GH9000-GH9999 Internal
}
