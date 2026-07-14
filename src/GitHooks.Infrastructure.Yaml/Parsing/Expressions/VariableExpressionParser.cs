using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Common;

namespace GitHooks.Infrastructure.Yaml.Parsing.Expressions;

internal sealed class VariableExpressionParser
{
    private const string ParametersPrefix = "parameters.";

#pragma warning disable CA1822 // Mark members as static
    public VariableExpressionNode Parse(
        string expression,
        SourceSpan span,
        ParsingContext context)
#pragma warning restore CA1822 // Mark members as static
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);

        if (expression.StartsWith(
                ParametersPrefix,
                StringComparison.Ordinal))
        {
            return ParseParameter(
                expression,
                span,
                context);
        }

        return ReportInvalid(
            expression,
            span,
            context);
    }

    private static VariableExpressionNode ParseParameter(
        string expression,
        SourceSpan span,
        ParsingContext context)
    {
        var name = expression[ParametersPrefix.Length..];

        if (string.IsNullOrWhiteSpace(name) ||
            name != name.Trim())
        {
            return ReportInvalid(
                expression,
                span,
                context);
        }

        return new ParameterVariableExpressionNode
        {
            Name = name,
            Span = span
        };
    }

    private static InvalidVariableExpressionNode ReportInvalid(
        string expression,
        SourceSpan span,
        ParsingContext context)
    {
        context.Report(
            Diagnostic.Create(
                DiagnosticDescriptors.InvalidVariableExpression,
                span,
                expression));

        return new InvalidVariableExpressionNode
        {
            Text = expression,
            Span = span
        };
    }
}
