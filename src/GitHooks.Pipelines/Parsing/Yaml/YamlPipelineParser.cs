using GitHooks.Pipelines.Contracts;
using GitHooks.Pipelines.Parsing.Yaml.Mapping;
using GitHooks.Pipelines.Parsing.Yaml.Validation;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GitHooks.Pipelines.Parsing.Yaml;

/// <summary>
/// Strict YAML parser for hook pipeline definitions.
/// </summary>
/// <remarks>
/// Parsing proceeds in three phases:
/// 1. Structural validation via <see cref="YamlDotNet.RepresentationModel.YamlStream"/> to detect malformed YAML early.
/// 2. Schema deserialization via YamlDotNet into internal DTOs.
/// 3. Domain model transformation with semantic validation.
/// </remarks>
public sealed class YamlPipelineParser : IYamlPipelineParser
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private static readonly PipelineSchemaValidator SchemaValidator = new(Deserializer);

    /// <inheritdoc />
    public ParseResult Parse(string yaml)
    {
        var errors = new List<ParseError>();

        if (!SchemaValidator.TryDeserialize(yaml, errors, out var dto) || dto is null)
        {
            return ParseResult.Fail(errors);
        }

        if (!PipelineSemanticValidator.Validate(dto, errors))
        {
            return ParseResult.Fail(errors);
        }

        var pipeline = YamlPipelineMapper.Map(dto);
        return ParseResult.Ok(pipeline);
    }
}
