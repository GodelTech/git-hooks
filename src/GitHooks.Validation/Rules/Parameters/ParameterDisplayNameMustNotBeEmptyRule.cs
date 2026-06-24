using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Validation.Extensions;

namespace GitHooks.Validation.Rules.Parameters;

internal sealed class ParameterDisplayNameMustNotBeEmptyRule
    : IValidationRule<ParameterNode>
{
    public void Validate(
        ParameterNode node,
        ValidationContext context)
    {
        if (node.DisplayName is null)
        {
            return;
        }

        if (!node.DisplayName.Value.TryGetStringValue(out var value))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        context.Report(
            DiagnosticDescriptors.ParameterDisplayNameMustNotBeEmpty,
            node.DisplayName.Span,
            node.GetNameForDiagnostic());
    }
}
