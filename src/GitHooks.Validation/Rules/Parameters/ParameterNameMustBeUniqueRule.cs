using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Validation.Extensions;

namespace GitHooks.Validation.Rules.Parameters;

internal sealed class ParameterNameMustBeUniqueRule
    : IValidationRule<ParameterNode>
{
    public void Validate(
        ParameterNode node,
        ValidationContext context)
    {
        if (!node.TryGetName(out var name))
        {
            return;
        }

        if (!context.Symbols.TryGetParameter(
                name,
                out var existing))
        {
            return;
        }

        if (ReferenceEquals(
                existing,
                node))
        {
            return;
        }

        context.Report(
            Diagnostic.Create(
                DiagnosticDescriptors.ParameterNameMustBeUnique,
                node.Name.Span,
                [
                    DiagnosticLocation.Create(
                        existing.Name.Span,
                        DiagnosticLocationMessages.PreviousDeclaration)
                ],
                node.GetNameForDiagnostic()));
    }
}
