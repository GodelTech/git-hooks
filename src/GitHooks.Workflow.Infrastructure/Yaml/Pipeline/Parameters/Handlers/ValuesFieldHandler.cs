using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters.Handlers;

internal sealed class ValuesFieldHandler
    : IParameterFieldHandler
{
    public string Key => "values";

    public ParameterFields Apply(YamlReader reader, ParameterFields fields)
    {
        return fields with
        {
            Values = ParseValuesList(reader)
        };
    }

    private static List<string> ParseValuesList(YamlReader reader)
    {
        _ = reader.Read<SequenceStart>();

        var values = new List<string>();

        while (!reader.Is<SequenceEnd>())
        {
            values.Add(reader.Read<Scalar>().Value);
        }

        _ = reader.Read<SequenceEnd>();

        return values;
    }
}
