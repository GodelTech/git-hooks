using GitHooks.Pipelines.Models;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GitHooks.Pipelines;

public sealed class PipelineParser
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public ParseResult<Pipeline> Parse(string yaml)
    {
        var errors = new List<ParseError>();

        if (yaml == null)
        {
            errors.Add(new ParseError("Input YAML cannot be null.", 0, 0));
            return ParseResult<Pipeline>.Fail(errors);
        }

        // Phase 1: structural parse — detect malformed YAML early
        YamlStream stream;
        try
        {
            stream = new YamlStream();
            stream.Load(new StringReader(yaml));
        }
        catch (YamlException ex)
        {
            errors.Add(new ParseError(ex.Message, ex.Start.Line, ex.Start.Column));
            return ParseResult<Pipeline>.Fail(errors);
        }

        // Phase 2: deserialize
        Pipeline pipeline;
        try
        {
            pipeline = Deserializer.Deserialize<Pipeline>(yaml);
        }
        catch (YamlException ex)
        {
            errors.Add(new ParseError(ex.Message, ex.Start.Line, ex.Start.Column));
            return ParseResult<Pipeline>.Fail(errors);
        }

        return errors.Count > 0
            ? ParseResult<Pipeline>.Fail(errors)
            : ParseResult<Pipeline>.Ok(pipeline);
    }
}
