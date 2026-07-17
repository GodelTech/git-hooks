using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Infrastructure.Yaml.Parsing.Fields;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Parameters;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline;

internal sealed class PipelineParser(
    ParametersParser parametersParser,
    StepsParser stepsParser,
    FieldValueParser fieldValueParser)
{
    private readonly ParametersParser _parametersParser
        = parametersParser ?? throw new ArgumentNullException(nameof(parametersParser));

    private readonly StepsParser _stepsParser
        = stepsParser ?? throw new ArgumentNullException(nameof(stepsParser));

    private readonly FieldValueParser _fieldValueParser
        = fieldValueParser ?? throw new ArgumentNullException(nameof(fieldValueParser));

    public PipelineNode Parse(
        YamlParserContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var start = context.Cursor.Read<MappingStart>();

        var fieldTracker = new FieldTracker(_fieldValueParser);

        IReadOnlyList<ParameterNode> parameters = [];
        IReadOnlyList<StepNode> steps = [];

        while (!context.Cursor.Is<MappingEnd>())
        {
            if (!context.Cursor.Is<Scalar>())
            {
                _ = fieldTracker.AddUnknownField(context);

                continue;
            }

            var key = context.Cursor.Read<Scalar>();

            switch (key.Value.ToLowerInvariant())
            {
                case "parameters":
                    parameters = fieldTracker.ReadFirst(
                        key,
                        context,
                        parameters,
                        _parametersParser.Parse);
                    break;

                case "steps":
                    steps = fieldTracker.ReadFirst(
                        key,
                        context,
                        steps,
                        _stepsParser.Parse);
                    break;

                default:
                    fieldTracker.AddUnknownField(key, context);
                    break;
            }
        }

        var end = context.Cursor.Read<MappingEnd>();

        var span = context.Cursor.CreateSpan(start, end);

        return new PipelineNode
        {
            Parameters = parameters,
            Steps = steps,
            UnknownFields = fieldTracker.GetUnknownFields(),
            Span = span
        };
    }
}
