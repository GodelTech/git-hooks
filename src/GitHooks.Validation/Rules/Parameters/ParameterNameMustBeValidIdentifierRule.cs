using System.Text.RegularExpressions;

using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Parameters;

namespace GitHooks.Validation.Rules.Parameters;

internal sealed partial class ParameterNameMustBeValidIdentifierRule
    : IValidationRule<ParameterNode>
{
    public void Validate(
        ParameterNode node,
        ValidationContext context)
    {
        if (!node.Name.Value.TryGetStringValue(out var value))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (!IdentifierPattern().IsMatch(value))
        {
            context.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.ParameterNameMustBeValidIdentifier,
                    node.Span,
                    value));
        }
    }

    [GeneratedRegex(@"^[a-zA-Z_][a-zA-Z0-9_]*$")]
    private static partial Regex IdentifierPattern();
}
