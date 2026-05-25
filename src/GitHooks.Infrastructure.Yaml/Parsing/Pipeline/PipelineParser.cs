using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Unknown;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Parameters;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline;

internal sealed class PipelineParser(
    ParametersParser parametersParser,
    StepsParser stepsParser,
    UnknownNodeParser unknownNodeParser)
{
    private readonly ParametersParser _parametersParser =
        parametersParser
        ?? throw new ArgumentNullException(nameof(parametersParser));

    private readonly StepsParser _stepsParser =
        stepsParser
        ?? throw new ArgumentNullException(nameof(stepsParser));

    private readonly UnknownNodeParser _unknownNodeParser =
        unknownNodeParser
        ?? throw new ArgumentNullException(nameof(unknownNodeParser));

    public PipelineNode Parse(
        YamlParserCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);

        var start = cursor.Read<MappingStart>();

        IReadOnlyList<ParameterNode> parameters = [];
        IReadOnlyList<StepNode> steps = [];
        var unknownFields = new List<UnknownFieldNode>();

        while (!cursor.Is<MappingEnd>())
        {
            if (!cursor.Is<Scalar>())
            {
                throw cursor.CreateInvalidOperationException(
                    "Expected scalar mapping key");
            }

            var key = cursor.Read<Scalar>();

            if (key.Value == "parameters")
            {
                parameters = _parametersParser.Parse(cursor);
            }
            else if (key.Value == "steps")
            {
                steps = _stepsParser.Parse(cursor);
            }
            else
            {
                var unknownField = _unknownNodeParser.ParseField(cursor, key);

                unknownFields.Add(unknownField);

                break;
            }
        }

        var end = cursor.Read<MappingEnd>();

        var span = cursor.CreateSpan(start, end);

        return new PipelineNode
        {
            Parameters = parameters,
            Steps = steps,
            UnknownFields = unknownFields,
            Span = span
        };
    }
}
