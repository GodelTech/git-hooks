using GitHooks.Workflow.Application.Ast;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters.Handlers;

internal sealed class TypeFieldHandler : IParameterFieldHandler
{
    public string Key => "type";

    public ParameterFields Apply(YamlReader reader, ParameterFields fields)
    {
        return fields with
        {
            Type = ParseParameterType(reader.Read<Scalar>().Value)
        };
    }

    private static ParameterType ParseParameterType(string value)
    {
        return value switch
        {
            "boolean" => ParameterType.Boolean,
            "number" => ParameterType.Number,
            "object" => ParameterType.Mapping,
            _ => ParameterType.Text
        };
    }
}
