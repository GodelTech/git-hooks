using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Steps;

namespace GitHooks.Validation.Rules.Steps;

internal sealed class TemplateFieldMustNotBeEmptyRule
    : IValidationRule<TemplateStepNode>
{
    public void Validate(
        TemplateStepNode node,
        ValidationContext context)
    {
        if (!node.Template.Value.TryGetStringValue(out var value))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        context.Report(
            Diagnostic.Create(
                DiagnosticDescriptors.TemplateFieldMustNotBeEmpty,
                node.Template.Span));
    }
}
