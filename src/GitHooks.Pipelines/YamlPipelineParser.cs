using System.Globalization;

using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace GitHooks.Pipelines;

/// <summary>
/// Default strict parser for hook pipeline YAML files.
/// </summary>
public sealed class YamlPipelineParser : IYamlPipelineParser
{
    private readonly IDeserializer _deserializer = new DeserializerBuilder().Build();

    /// <inheritdoc />
    public PipelineParseResult Parse(string yamlContent)
    {
        if (string.IsNullOrWhiteSpace(yamlContent))
        {
            return PipelineParseResult.Failure(
            [
                new PipelineParseDiagnostic("$", "YAML content cannot be empty."),
            ]);
        }

        PipelineDocument? document;

        try
        {
            document = _deserializer.Deserialize<PipelineDocument>(yamlContent);
        }
        catch (YamlException ex)
        {
            var path = $"line {ex.Start.Line + 1}, column {ex.Start.Column + 1}";
            var message = ex.InnerException is null
                ? ex.Message
                : $"{ex.Message}: {ex.InnerException.Message}";

            return PipelineParseResult.Failure(
            [
                new PipelineParseDiagnostic(path, message),
            ]);
        }

        if (document is null)
        {
            return PipelineParseResult.Failure(
            [
                new PipelineParseDiagnostic("$", "YAML content is empty or invalid."),
            ]);
        }

        var diagnostics = new List<PipelineParseDiagnostic>();
        var parameters = ParseParameters(document.Parameters, diagnostics);
        var steps = ParseSteps(document.Steps, diagnostics);

        if (diagnostics.Count > 0)
        {
            return PipelineParseResult.Failure(diagnostics);
        }

        return PipelineParseResult.Success(new HookPipelineDefinition(parameters, steps));
    }

    private static List<PipelineParameterDefinition> ParseParameters(
        IReadOnlyList<PipelineParameterDocument>? parameters,
        List<PipelineParseDiagnostic> diagnostics)
    {
        if (parameters is null)
        {
            diagnostics.Add(new PipelineParseDiagnostic("$.parameters", "Required section 'parameters' is missing."));
            return [];
        }

        var result = new List<PipelineParameterDefinition>(parameters.Count);

        for (var index = 0; index < parameters.Count; index++)
        {
            var parameter = parameters[index];
            var path = string.Create(CultureInfo.InvariantCulture, $"$.parameters[{index}]");

            if (parameter is null)
            {
                diagnostics.Add(new PipelineParseDiagnostic(path, "Parameter item cannot be null."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(parameter.Name))
            {
                diagnostics.Add(new PipelineParseDiagnostic($"{path}.name", "Field 'name' is required."));
            }

            if (string.IsNullOrWhiteSpace(parameter.Type))
            {
                diagnostics.Add(new PipelineParseDiagnostic($"{path}.type", "Field 'type' is required."));
            }

            if (string.IsNullOrWhiteSpace(parameter.Name) || string.IsNullOrWhiteSpace(parameter.Type))
            {
                continue;
            }

            result.Add(
                new PipelineParameterDefinition(
                    Name: parameter.Name,
                    Type: parameter.Type,
                    DisplayName: parameter.DisplayName,
                    DefaultValue: ToRawString(parameter.Default),
                    Values: ParseValues(parameter.Values)));
        }

        return result;
    }

    private static List<PipelineStepDefinition> ParseSteps(
        IReadOnlyList<PipelineStepDocument>? steps,
        List<PipelineParseDiagnostic> diagnostics)
    {
        if (steps is null)
        {
            diagnostics.Add(new PipelineParseDiagnostic("$.steps", "Required section 'steps' is missing."));
            return [];
        }

        var result = new List<PipelineStepDefinition>(steps.Count);

        for (var index = 0; index < steps.Count; index++)
        {
            var step = steps[index];
            var path = string.Create(CultureInfo.InvariantCulture, $"$.steps[{index}]");

            if (step is null)
            {
                diagnostics.Add(new PipelineParseDiagnostic(path, "Step item cannot be null."));
                continue;
            }

            var kinds = 0;
            var kind = PipelineStepKind.Task;
            var command = string.Empty;

            if (!string.IsNullOrWhiteSpace(step.Task))
            {
                kind = PipelineStepKind.Task;
                command = step.Task;
                kinds++;
            }

            if (!string.IsNullOrWhiteSpace(step.Script))
            {
                kind = PipelineStepKind.Script;
                command = step.Script;
                kinds++;
            }

            if (!string.IsNullOrWhiteSpace(step.Pwsh))
            {
                kind = PipelineStepKind.Pwsh;
                command = step.Pwsh;
                kinds++;
            }

            if (kinds is 0)
            {
                diagnostics.Add(new PipelineParseDiagnostic(path, "Exactly one of 'task', 'script', or 'pwsh' is required."));
                continue;
            }

            if (kinds > 1)
            {
                diagnostics.Add(new PipelineParseDiagnostic(path, "Only one of 'task', 'script', or 'pwsh' can be set."));
                continue;
            }

            var inputs = new Dictionary<string, string?>(StringComparer.Ordinal);
            if (step.Inputs is not null)
            {
                foreach (var pair in step.Inputs)
                {
                    if (string.IsNullOrWhiteSpace(pair.Key))
                    {
                        diagnostics.Add(new PipelineParseDiagnostic($"{path}.inputs", "Input keys cannot be empty."));
                        continue;
                    }

                    inputs[pair.Key] = ToRawString(pair.Value);
                }
            }

            result.Add(
                new PipelineStepDefinition(
                    Kind: kind,
                    Command: command,
                    Name: step.Name,
                    DisplayName: step.DisplayName,
                    Enabled: ToRawString(step.Enabled),
                    ContinueOnError: ToRawString(step.ContinueOnError),
                    Inputs: inputs));
        }

        return result;
    }

    private static string? ToRawString(object? value)
    {
        return value switch
        {
            null => null,
            string str => str,
            _ => Convert.ToString(value, CultureInfo.InvariantCulture),
        };
    }

    private static List<string?> ParseValues(List<object?>? values)
    {
        if (values is null)
        {
            return [];
        }

        var result = new List<string?>(values.Count);
        foreach (var value in values)
        {
            result.Add(ToRawString(value));
        }

        return result;
    }

    private sealed class PipelineDocument
    {
        [YamlMember(Alias = "parameters")]
        public List<PipelineParameterDocument>? Parameters { get; init; }

        [YamlMember(Alias = "steps")]
        public List<PipelineStepDocument>? Steps { get; init; }
    }

    private sealed class PipelineParameterDocument
    {
        [YamlMember(Alias = "name")]
        public string? Name { get; init; }

        [YamlMember(Alias = "displayName")]
        public string? DisplayName { get; init; }

        [YamlMember(Alias = "type")]
        public string? Type { get; init; }

        [YamlMember(Alias = "default")]
        public object? Default { get; init; }

        [YamlMember(Alias = "values")]
        public List<object?>? Values { get; init; }
    }

    private sealed class PipelineStepDocument
    {
        [YamlMember(Alias = "task")]
        public string? Task { get; init; }

        [YamlMember(Alias = "script")]
        public string? Script { get; init; }

        [YamlMember(Alias = "pwsh")]
        public string? Pwsh { get; init; }

        [YamlMember(Alias = "name")]
        public string? Name { get; init; }

        [YamlMember(Alias = "displayName")]
        public string? DisplayName { get; init; }

        [YamlMember(Alias = "enabled")]
        public object? Enabled { get; init; }

        [YamlMember(Alias = "continueOnError")]
        public object? ContinueOnError { get; init; }

        [YamlMember(Alias = "inputs")]
        public Dictionary<string, object?>? Inputs { get; init; }
    }
}
