using System.Text.RegularExpressions;

using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Common;

namespace GitHooks.Infrastructure.Yaml.Parsing.Expressions;

internal sealed partial class InterpolatedStringParser(
    VariableExpressionParser variableExpressionParser)
{
    private readonly VariableExpressionParser _variableExpressionParser
        = variableExpressionParser ?? throw new ArgumentNullException(nameof(variableExpressionParser));

    public static bool ContainsInterpolation(
        string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return InterpolationPattern().IsMatch(value);
    }

    public ExpressionNode Parse(
        string value,
        SourceSpan span)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var matches = InterpolationPattern().Matches(value);

        if (matches.Count == 1 &&
            matches[0].Index == 0 &&
            matches[0].Length == value.Length)
        {
            return _variableExpressionParser.Parse(
                matches[0].Groups["expression"].Value,
                span);
        }

        var parts = new List<ExpressionNode>();

        var currentIndex = 0;

        foreach (Match match in matches)
        {
            if (match.Index > currentIndex)
            {
                parts.Add(
                    new StringLiteralExpressionNode
                    {
                        Value = value[currentIndex..match.Index],
                        Span = span
                    });
            }

            parts.Add(
                _variableExpressionParser.Parse(
                    match.Groups["expression"].Value,
                    span));

            currentIndex = match.Index + match.Length;
        }

        if (currentIndex < value.Length)
        {
            parts.Add(
                new StringLiteralExpressionNode
                {
                    Value = value[currentIndex..],
                    Span = span
                });
        }

        return new InterpolatedStringExpressionNode
        {
            Parts = parts,
            Span = span
        };
    }

    [GeneratedRegex("\\$\\{\\{(?<expression>.*?)\\}\\}", RegexOptions.CultureInvariant)]
    private static partial Regex InterpolationPattern();
}
