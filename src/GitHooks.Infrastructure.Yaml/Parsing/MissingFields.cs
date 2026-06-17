using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Common;

namespace GitHooks.Infrastructure.Yaml.Parsing;

internal static class MissingFields
{
    /// <summary>
    /// Creates a synthetic field used for parser recovery when a required field is missing.
    /// </summary>
    /// <param name="key">The missing field name.</param>
    /// <param name="span">The span assigned to the synthetic field.</param>
    /// <returns>A synthetic string field.</returns>
    public static StringKeyFieldNode<ExpressionNode> StringKeyField(
        string key,
        SourceSpan span)
    {
        return new StringKeyFieldNode<ExpressionNode>
        {
            Key = key,
            Value = new StringLiteralExpressionNode
            {
                Value = string.Empty,
                Span = span
            },
            Span = span
        };
    }
}
