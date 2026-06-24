using System.Text.RegularExpressions;

using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;

namespace GitHooks.Validation.Rules.Variables;

internal sealed partial class VariableSyntaxMustBeValidRule
    : IValidationRule<VariableExpressionNode>
{
    public void Validate(
        VariableExpressionNode node,
        ValidationContext context)
    {
        if (!ParameterVariablePattern().IsMatch(node.Path))
        {
            context.Report(
                DiagnosticDescriptors.VariableSyntaxMustBeValid,
                node.Span,
                node.Path);
        }
    }

    [GeneratedRegex(@"^parameters\.[a-zA-Z_][a-zA-Z0-9_]*$")]
    private static partial Regex ParameterVariablePattern();
}
