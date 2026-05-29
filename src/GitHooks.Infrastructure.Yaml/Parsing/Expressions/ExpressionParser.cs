using System.Globalization;

using GitHooks.Domain.Ast.Expressions;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Expressions;

internal sealed class ExpressionParser(
    InterpolatedStringParser interpolatedStringParser)
{
    private readonly InterpolatedStringParser _interpolatedStringParser
        = interpolatedStringParser ?? throw new ArgumentNullException(nameof(interpolatedStringParser));

    public ExpressionNode Parse(YamlParserCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);

        var scalar = cursor.Read<Scalar>();

        var span = cursor.CreateSpan(scalar.Start, scalar.End);

        if (InterpolatedStringParser.ContainsInterpolation(scalar.Value))
        {
            return _interpolatedStringParser.Parse(
                scalar.Value,
                span);
        }

        if (bool.TryParse(scalar.Value, out var boolean))
        {
            return new BooleanLiteralExpressionNode
            {
                Value = boolean,
                Span = span
            };
        }

        if (int.TryParse(
                scalar.Value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var integer))
        {
            return new IntegerLiteralExpressionNode
            {
                Value = integer,
                Span = span
            };
        }

        return new StringLiteralExpressionNode
        {
            Value = scalar.Value,
            Span = span
        };
    }
}
