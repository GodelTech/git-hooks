using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline;

internal sealed class PipelineRootParser(
    PipelineParametersParser pipelineParametersParser,
    StepsParser stepsParser)
{
    private readonly PipelineParametersParser _pipelineParametersParser = pipelineParametersParser;
    private readonly StepsParser _stepsParser = stepsParser;

    public PipelineNode Parse(YamlReader reader)
    {
        var start = reader.Read<MappingStart>();

        IReadOnlyList<ParameterNode> parameters = [];
        IReadOnlyList<StepNode>? steps = null;
        var unknownFields = new List<UnknownFieldNode>();

        while (!reader.Is<MappingEnd>())
        {
            if (!reader.Is<Scalar>())
            {
                var keyNode = reader.ReadUnknownNode();
                unknownFields.Add(reader.ReadUnknownField(keyNode));
                continue;
            }

            var key = reader.Read<Scalar>();

            if (key.Value == "parameters")
            {
                parameters = _pipelineParametersParser.Parse(reader);
            }
            else if (key.Value == "steps")
            {
                steps = _stepsParser.Parse(reader);
            }
            else
            {
                unknownFields.Add(reader.ReadUnknownField(key));
            }
        }

        var end = reader.Read<MappingEnd>();

        var span = reader.SpanOf(start, end);

        return steps is null
            ? throw new YamlParseException("Pipeline must contain 'steps'", span)
            : new PipelineNode(parameters, steps, unknownFields, span);
    }
}
