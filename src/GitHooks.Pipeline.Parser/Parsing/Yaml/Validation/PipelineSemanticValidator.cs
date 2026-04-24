using GitHooks.Pipeline.Contracts;

namespace GitHooks.Pipeline.Parser.Parsing.Yaml.Validation;

/// <summary>
/// Validates semantic invariants of a deserialized pipeline YAML document.
/// </summary>
internal sealed class PipelineSemanticValidator
{
    public static bool Validate(PipelineYamlDto dto, List<ParseError> errors)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(errors);

        ValidateParameters(dto.Parameters, errors);
        ValidateSteps(dto.Steps, errors);

        return errors.Count is 0;
    }

    private static void ValidateParameters(List<ParameterYamlDto>? dtos, List<ParseError> errors)
    {
        if (dtos is null)
        {
            errors.Add(new ParseError("$.parameters: Required section 'parameters' is missing.", 0, 0));
            return;
        }

        for (var index = 0; index < dtos.Count; index++)
        {
            var dto = dtos[index];
            var path = FormattableString.Invariant($"$.parameters[{index}]");

            if (dto is null)
            {
                errors.Add(new ParseError($"{path}: Parameter item cannot be null.", 0, 0));
                continue;
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                errors.Add(new ParseError($"{path}.name: Field 'name' is required.", 0, 0));
            }

            if (string.IsNullOrWhiteSpace(dto.Type))
            {
                errors.Add(new ParseError($"{path}.type: Field 'type' is required.", 0, 0));
            }
        }
    }

    private static void ValidateSteps(List<StepYamlDto>? dtos, List<ParseError> errors)
    {
        if (dtos is null)
        {
            errors.Add(new ParseError("$.steps: Required section 'steps' is missing.", 0, 0));
            return;
        }

        for (var index = 0; index < dtos.Count; index++)
        {
            var dto = dtos[index];
            var path = FormattableString.Invariant($"$.steps[{index}]");

            if (dto is null)
            {
                errors.Add(new ParseError($"{path}: Step item cannot be null.", 0, 0));
                continue;
            }

            var commandCount = 0;

            if (!string.IsNullOrWhiteSpace(dto.Task))
            {
                commandCount++;
            }

            if (!string.IsNullOrWhiteSpace(dto.Script))
            {
                commandCount++;
            }

            if (!string.IsNullOrWhiteSpace(dto.Pwsh))
            {
                commandCount++;
            }

            if (commandCount is 0)
            {
                errors.Add(new ParseError($"{path}: Exactly one of 'task', 'script', or 'pwsh' must be set.", 0, 0));
            }
            else if (commandCount > 1)
            {
                errors.Add(new ParseError($"{path}: Only one of 'task', 'script', or 'pwsh' can be set.", 0, 0));
            }

            ValidateInputs(dto, path, errors);
        }
    }

    private static void ValidateInputs(StepYamlDto dto, string path, List<ParseError> errors)
    {
        if (dto.Inputs is null)
        {
            return;
        }

        foreach (var pair in dto.Inputs)
        {
            if (string.IsNullOrWhiteSpace(pair.Key))
            {
                errors.Add(new ParseError($"{path}.inputs: Input keys cannot be empty.", 0, 0));
            }
        }
    }
}
