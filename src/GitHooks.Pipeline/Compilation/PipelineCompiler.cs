using System.Globalization;

using GitHooks.Pipeline.Contracts;
using GitHooks.Pipeline.Domain;

namespace GitHooks.Pipeline.Compilation;

/// <summary>
/// Compiles parsed pipeline definitions into executable plans.
/// </summary>
public sealed class PipelineCompiler : IPipelineCompiler
{
    /// <inheritdoc />
    public CompileResult Compile(CompileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = new List<ParseError>();
        var parameters = ResolveParameters(request, errors);
        var steps = CompileSteps(request.Pipeline.Steps, errors);

        if (errors.Count > 0)
        {
            return CompileResult.Fail(errors);
        }

        return CompileResult.Ok(new PipelineExecutionPlan(parameters, steps));
    }

    private static Dictionary<string, ResolvedParameter> ResolveParameters(CompileRequest request, List<ParseError> errors)
    {
        var queueParameters = request.QueueParameters ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var result = new Dictionary<string, ResolvedParameter>(StringComparer.OrdinalIgnoreCase);

        foreach (var parameter in request.Pipeline.Parameters)
        {
            if (result.ContainsKey(parameter.Name))
            {
                errors.Add(new ParseError($"$.parameters.{parameter.Name}: Duplicate parameter name.", 0, 0));
                continue;
            }

            var hasQueueValue = queueParameters.TryGetValue(parameter.Name, out var queueValue);
            var hasDefaultValue = parameter.DefaultValue is not null;
            var rawValue = hasQueueValue ? queueValue : parameter.DefaultValue;

            if (rawValue is null)
            {
                errors.Add(new ParseError($"$.parameters.{parameter.Name}: Required parameter value is missing.", 0, 0));
                continue;
            }

            var allowedValues = parameter.Values.Where(static value => value is not null).Select(static value => value!).ToList();

            if (allowedValues.Count > 0 && !allowedValues.Contains(rawValue, StringComparer.OrdinalIgnoreCase))
            {
                errors.Add(new ParseError($"$.parameters.{parameter.Name}: Value '{rawValue}' is not in the allowed values list.", 0, 0));
                continue;
            }

            if (!TryConvertParameterValue(parameter.Name, parameter.Type, rawValue, out var typedValue, out var conversionError))
            {
                errors.Add(new ParseError(conversionError, 0, 0));
                continue;
            }

            result[parameter.Name] = new ResolvedParameter(
                Name: parameter.Name,
                Type: parameter.Type,
                Value: typedValue,
                IsDefaulted: !hasQueueValue && hasDefaultValue);
        }

        return result;
    }

    private static List<ExecutableStep> CompileSteps(IReadOnlyList<PipelineStep> steps, List<ParseError> errors)
    {
        var result = new List<ExecutableStep>(steps.Count);

        for (var index = 0; index < steps.Count; index++)
        {
            var step = steps[index];
            var stepPath = FormattableString.Invariant($"$.steps[{index}]");

            if (!TryParseBooleanFlag(step.Enabled, defaultValue: true, out var enabled))
            {
                errors.Add(new ParseError($"{stepPath}.enabled: Value must be a boolean.", 0, 0));
                continue;
            }

            if (!TryParseBooleanFlag(step.ContinueOnError, defaultValue: false, out var continueOnError))
            {
                errors.Add(new ParseError($"{stepPath}.continueOnError: Value must be a boolean.", 0, 0));
                continue;
            }

            result.Add(
                new ExecutableStep(
                    Order: index + 1,
                    Kind: step.Kind,
                    Command: step.Command,
                    Name: step.Name,
                    DisplayName: step.DisplayName,
                    Enabled: enabled,
                    ContinueOnError: continueOnError,
                    Inputs: ToInputObjectDictionary(step.Inputs)));
        }

        return result;
    }

    private static bool TryConvertParameterValue(string parameterName, string type, string rawValue, out object? value, out string error)
    {
        var normalizedType = type.Trim().ToLowerInvariant();

        switch (normalizedType)
        {
            case "string":
                value = rawValue;
                error = string.Empty;
                return true;

            case "boolean":
            case "bool":
                if (bool.TryParse(rawValue, out var boolValue))
                {
                    value = boolValue;
                    error = string.Empty;
                    return true;
                }

                value = null;
                error = $"$.parameters.{parameterName}: Value '{rawValue}' cannot be converted to boolean.";
                return false;

            case "number":
                if (decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue))
                {
                    value = decimalValue;
                    error = string.Empty;
                    return true;
                }

                value = null;
                error = $"$.parameters.{parameterName}: Value '{rawValue}' cannot be converted to number.";
                return false;

            default:
                value = rawValue;
                error = string.Empty;
                return true;
        }
    }

    private static bool TryParseBooleanFlag(string? rawValue, bool defaultValue, out bool value)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            value = defaultValue;
            return true;
        }

        if (bool.TryParse(rawValue, out var parsed))
        {
            value = parsed;
            return true;
        }

        value = defaultValue;
        return false;
    }

    private static Dictionary<string, object?> ToInputObjectDictionary(IReadOnlyDictionary<string, string?> inputs)
    {
        var result = new Dictionary<string, object?>(inputs.Count, StringComparer.Ordinal);

        foreach (var pair in inputs)
        {
            result[pair.Key] = pair.Value;
        }

        return result;
    }
}
