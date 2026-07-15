using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings;

namespace GitHooks.Validation.Rules.Pipeline;

internal sealed class PipelineMustContainStepRule
    : IValidationRule<PipelineNode>
{
    public void Validate(
        PipelineNode node,
        ValidationContext context)
    {
        if (node.Steps.Count == 0)
        {
            context.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.PipelineMustContainStep,
                    node.Span));
        }
    }
}
