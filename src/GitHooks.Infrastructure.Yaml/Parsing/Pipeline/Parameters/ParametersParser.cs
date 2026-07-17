using GitHooks.Domain.Ast.Mappings.Parameters;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Parameters;

internal sealed class ParametersParser(
    ParameterParser parameterParser)
{
    private readonly ParameterParser _parameterParser
        = parameterParser ?? throw new ArgumentNullException(nameof(parameterParser));

    public IReadOnlyList<ParameterNode> Parse(
        YamlParserContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _ = context.Cursor.Read<SequenceStart>();

        var parameters = new List<ParameterNode>();

        while (!context.Cursor.Is<SequenceEnd>())
        {
            var parameter = _parameterParser.Parse(context);

            parameters.Add(parameter);
        }

        _ = context.Cursor.Read<SequenceEnd>();

        return parameters;
    }
}
