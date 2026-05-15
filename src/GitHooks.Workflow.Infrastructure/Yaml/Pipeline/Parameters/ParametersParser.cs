using GitHooks.Workflow.Application.Ast;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters;

internal sealed class ParametersParser(ParameterParser parameterParser)
{
    private readonly ParameterParser _parameterParser = parameterParser;

    public IReadOnlyList<ParameterNode> Parse(YamlReader reader)
    {
        _ = reader.Read<SequenceStart>();

        var parameters = new List<ParameterNode>();

        while (!reader.Is<SequenceEnd>())
        {
            var parameter = _parameterParser.Parse(reader);

            parameters.Add(parameter);
        }

        _ = reader.Read<SequenceEnd>();

        return parameters;
    }
}
