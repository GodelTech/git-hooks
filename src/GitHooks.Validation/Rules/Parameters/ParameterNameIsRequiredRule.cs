using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Parameters;

namespace GitHooks.Validation.Rules.Parameters;

internal sealed class ParameterNameIsRequiredRule
    : IValidationRule<ParameterNode>
{
    public void Validate(
        ParameterNode node,
        ValidationContext context)
    {
        if (!node.Name.Value.TryGetStringValue(out var value) ||
            string.IsNullOrWhiteSpace(value))
        {
            context.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.ParameterNameIsRequired,
                    node.Span));
        }
    }
}
