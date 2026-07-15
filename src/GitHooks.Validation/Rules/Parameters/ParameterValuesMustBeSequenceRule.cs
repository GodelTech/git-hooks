using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Validation.Extensions;

namespace GitHooks.Validation.Rules.Parameters;

internal sealed class ParameterValuesMustBeSequenceRule
    : IValidationRule<ParameterNode>
{
    public void Validate(
        ParameterNode node,
        ValidationContext context)
    {
        if (node.Values is null)
        {
            return;
        }

        if (node.Values.Items.Count == 0)
        {
            context.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.ParameterValuesMustBeSequence,
                    node.Values.Span,
                    node.GetNameForDiagnostic()));
        }
    }
}
