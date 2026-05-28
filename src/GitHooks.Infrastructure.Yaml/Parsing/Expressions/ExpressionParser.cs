using System.Globalization;

using GitHooks.Domain.Ast.Expressions;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Expressions;

internal sealed class ExpressionParser
{
#pragma warning disable CA1822 // Mark members as static
    public ExpressionNode Parse(YamlParserCursor cursor)
#pragma warning restore CA1822 // Mark members as static
    {
        ArgumentNullException.ThrowIfNull(cursor);

        var scalar = cursor.Read<Scalar>();

        var span = cursor.CreateSpan(scalar.Start, scalar.End);

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
