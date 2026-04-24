using GitHooks.Pipeline.Contracts;

using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace GitHooks.Pipeline.Parser.Yaml.Validation;

/// <summary>
/// Validates YAML text structure and deserializes it into the internal DTO model.
/// </summary>
internal sealed class PipelineSchemaValidator(IDeserializer deserializer)
{
    private readonly IDeserializer _deserializer = deserializer;

    public bool TryDeserialize(string yaml, List<ParseError> errors, out PipelineYamlDto? dto)
    {
        ArgumentNullException.ThrowIfNull(errors);

        dto = null;

        if (string.IsNullOrWhiteSpace(yaml))
        {
            errors.Add(new ParseError("YAML content cannot be empty.", 0, 0));
            return false;
        }

        try
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(yaml));
        }
        catch (YamlException ex)
        {
            errors.Add(new ParseError($"Invalid YAML: {ex.Message}", ex.Start.Line, ex.Start.Column));
            return false;
        }

        try
        {
            dto = _deserializer.Deserialize<PipelineYamlDto>(yaml);
        }
        catch (YamlException ex)
        {
            errors.Add(new ParseError($"Schema error: {ex.Message}", ex.Start.Line, ex.Start.Column));
            return false;
        }

        if (dto is null)
        {
            errors.Add(new ParseError("YAML content is empty or produced no document.", 0, 0));
            return false;
        }

        return true;
    }
}
