using System.Text;
using System.Text.RegularExpressions;

using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Binding.Parameters;

internal sealed partial class ParameterScalarBinder
{
    [GeneratedRegex("\\$\\{\\{(?<expression>.*?)\\}\\}", RegexOptions.CultureInvariant)]
    private static partial Regex InterpolationPattern();

    public static string Bind(string value, SourceSpan span, ParameterBindingContext context)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var matches = InterpolationPattern().Matches(value);

        if (matches.Count == 0)
        {
            return value;
        }

        var builder = new StringBuilder(value.Length);
        var currentIndex = 0;

        foreach (Match match in matches)
        {
            _ = builder.Append(value, currentIndex, match.Index - currentIndex);

            var expression = match.Groups["expression"].Value.Trim();
            _ = builder.Append(ResolveExpression(expression, span, context));

            currentIndex = match.Index + match.Length;
        }

        _ = builder.Append(value, currentIndex, value.Length - currentIndex);
        return builder.ToString();
    }

    private static string ResolveExpression(string expression, SourceSpan span, ParameterBindingContext context)
    {
        const string parameterPrefix = "parameters.";

        if (!expression.StartsWith(parameterPrefix, StringComparison.Ordinal))
        {
            throw new PipelineParameterBindingException(
                $"Unsupported expression '{expression}'. Only 'parameters.<name>' is currently supported.",
                span
            );
        }

        var parameterName = expression[parameterPrefix.Length..].Trim();

        _ = string.IsNullOrWhiteSpace(parameterName)
            ? throw new PipelineParameterBindingException(
                "Parameter expression must specify a parameter name.",
                span
            )
            : parameterName;

        return context.GetResolvedValue(parameterName, span);
    }
}
