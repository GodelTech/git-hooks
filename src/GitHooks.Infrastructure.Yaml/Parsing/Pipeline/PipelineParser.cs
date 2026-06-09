using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Parameters;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;
using GitHooks.Infrastructure.Yaml.Parsing.Unknown;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline;

internal sealed class PipelineParser(
    ParametersParser parametersParser,
    StepsParser stepsParser,
    UnknownNodeParser unknownNodeParser)
{
    private readonly ParametersParser _parametersParser
        = parametersParser ?? throw new ArgumentNullException(nameof(parametersParser));

    private readonly StepsParser _stepsParser
        = stepsParser ?? throw new ArgumentNullException(nameof(stepsParser));

    private readonly UnknownNodeParser _unknownNodeParser
        = unknownNodeParser ?? throw new ArgumentNullException(nameof(unknownNodeParser));

    public PipelineNode Parse(YamlParserCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);

        var start = cursor.Read<MappingStart>();

        var fields = new MappingFields(_unknownNodeParser);

        IReadOnlyList<ParameterNode> parameters = [];
        IReadOnlyList<StepNode> steps = [];

        while (!cursor.Is<MappingEnd>())
        {
            if (!cursor.Is<Scalar>())
            {
                fields.AddUnknownField(cursor);

                continue;
            }

            var key = cursor.Read<Scalar>();

            switch (key.Value.ToLowerInvariant())
            {
                case "parameters":
                    fields.MarkSeen(key, cursor);
                    parameters = _parametersParser.Parse(cursor);
                    break;

                case "steps":
                    fields.MarkSeen(key, cursor);
                    steps = _stepsParser.Parse(cursor);
                    break;

                default:
                    fields.AddUnknownField(key, cursor);
                    break;
            }
        }

        var end = cursor.Read<MappingEnd>();

        var span = cursor.CreateSpan(start, end);

        return new PipelineNode
        {
            Parameters = parameters,
            Steps = steps,
            UnknownFields = fields.GetUnknownFields(),
            Span = span
        };
    }
}
