using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Steps;

namespace GitHooks.Validation.Rules.Steps;

internal sealed class ScriptFieldMustNotBeEmptyRule
    : IValidationRule<ScriptStepNode>
{
    public void Validate(
        ScriptStepNode node,
        ValidationContext context)
    {
        if (!node.Script.Value.TryGetStringValue(out var value))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        context.Report(
            DiagnosticDescriptors.ScriptFieldMustNotBeEmpty,
            node.Script.Span);
    }
}
