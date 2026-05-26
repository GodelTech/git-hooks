using GitHooks.Domain.Ast.Mappings.Parameters;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Parameters;

internal sealed class ParametersParser(
    ParameterParser parameterParser)
{
    private readonly ParameterParser _parameterParser =
        parameterParser
        ?? throw new ArgumentNullException(nameof(parameterParser));

    public IReadOnlyList<ParameterNode> Parse(
        YamlParserCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);

        _ = cursor.Read<SequenceStart>();

        var parameters = new List<ParameterNode>();

        while (!cursor.Is<SequenceEnd>())
        {
            var parameter = _parameterParser.Parse(cursor);

            parameters.Add(parameter);
        }

        _ = cursor.Read<SequenceEnd>();

        return parameters;
    }
}
