using System.Globalization;

using GitHooks.Domain.Ast.Expressions;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Expressions;

internal sealed class ExpressionParser(
    InterpolatedStringParser interpolatedStringParser)
{
    private readonly InterpolatedStringParser _interpolatedStringParser
        = interpolatedStringParser ?? throw new ArgumentNullException(nameof(interpolatedStringParser));

    public ExpressionNode Parse(
        ParsingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var scalar = context.Cursor.Read<Scalar>();

        var span = context.Cursor.CreateSpan(scalar.Start, scalar.End);

        if (InterpolatedStringParser.ContainsInterpolation(scalar.Value))
        {
            return _interpolatedStringParser.Parse(
                scalar.Value,
                span,
                context);
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
