using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings;

namespace GitHooks.Validation.Rules.Pipeline;

internal sealed class PipelineMustContainStepRule
    : IValidationRule<PipelineNode>
{
    public void Validate(
        PipelineNode node,
        DiagnosticBag diagnostics)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(diagnostics);

        if (node.Steps.Count == 0)
        {
            diagnostics.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.PipelineMustContainStep,
                    node.Span));
        }
    }
}
