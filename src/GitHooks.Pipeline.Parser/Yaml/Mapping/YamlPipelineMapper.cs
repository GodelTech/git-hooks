using System.Globalization;

using GitHooks.Pipeline.Domain;

namespace GitHooks.Pipeline.Parser.Yaml.Mapping;

/// <summary>
/// Maps validated YAML DTOs to pipeline domain models.
/// </summary>
internal sealed class YamlPipelineMapper
{
    public static PipelineDefinition Map(PipelineYamlDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var parameters = MapParameters(dto.Parameters);
        var steps = MapSteps(dto.Steps);

        return new PipelineDefinition(parameters, steps);
    }

    private static List<PipelineParameter> MapParameters(List<ParameterYamlDto>? dtos)
    {
        if (dtos is null)
        {
            return [];
        }

        var result = new List<PipelineParameter>(capacity: dtos.Count);

        foreach (var dto in dtos)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Type))
            {
                continue;
            }

            result.Add(
                new PipelineParameter(
                    Name: dto.Name,
                    Type: dto.Type,
                    DisplayName: dto.DisplayName,
                    DefaultValue: ToRawString(dto.Default),
                    Values: ToRawStringList(dto.Values)));
        }

        return result;
    }

    private static List<PipelineStep> MapSteps(List<StepYamlDto>? dtos)
    {
        if (dtos is null)
        {
            return [];
        }

        var result = new List<PipelineStep>(capacity: dtos.Count);

        foreach (var dto in dtos)
        {
            if (dto is null)
            {
                continue;
            }

            if (!TryResolveStep(dto, out var stepKind, out var command))
            {
                continue;
            }

            result.Add(
                new PipelineStep(
                    Kind: stepKind,
                    Command: command,
                    Name: dto.Name,
                    DisplayName: dto.DisplayName,
                    Enabled: ToRawString(dto.Enabled),
                    ContinueOnError: ToRawString(dto.ContinueOnError),
                    Inputs: MapInputs(dto.Inputs)));
        }

        return result;
    }

    private static bool TryResolveStep(StepYamlDto dto, out PipelineStepKind kind, out string command)
    {
        if (!string.IsNullOrWhiteSpace(dto.Task))
        {
            kind = PipelineStepKind.Task;
            command = dto.Task;
            return true;
        }

        if (!string.IsNullOrWhiteSpace(dto.Script))
        {
            kind = PipelineStepKind.Script;
            command = dto.Script;
            return true;
        }

        if (!string.IsNullOrWhiteSpace(dto.Pwsh))
        {
            kind = PipelineStepKind.Pwsh;
            command = dto.Pwsh;
            return true;
        }

        kind = default;
        command = string.Empty;
        return false;
    }

    private static Dictionary<string, string?> MapInputs(Dictionary<string, object?>? inputs)
    {
        if (inputs is null)
        {
            return [];
        }

        var result = new Dictionary<string, string?>(capacity: inputs.Count, comparer: StringComparer.Ordinal);

        foreach (var pair in inputs)
        {
            if (string.IsNullOrWhiteSpace(pair.Key))
            {
                continue;
            }

            result[pair.Key] = ToRawString(pair.Value);
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

    private static List<string?> ToRawStringList(List<object?>? values)
    {
        if (values is null)
        {
            return [];
        }

        var result = new List<string?>(capacity: values.Count);

        foreach (var value in values)
        {
            result.Add(ToRawString(value));
        }

        return result;
    }
}
