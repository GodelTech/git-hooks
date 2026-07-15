using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings.Steps;

namespace GitHooks.Validation.Rules.Steps;

internal sealed class InvalidStepRule
    : IValidationRule<InvalidStepNode>
{
    public void Validate(
        InvalidStepNode node,
        ValidationContext context)
    {
        context.Report(
            Diagnostic.Create(
                DiagnosticDescriptors.InvalidStep,
                node.Span));
    }
}
