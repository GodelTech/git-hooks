using GitHooks.Domain.Ast.Mappings;
using GitHooks.Validation.Rules.Pipeline;

namespace GitHooks.Validation;

internal sealed class ValidationRuleSet
{
    public IReadOnlyList<IValidationRule<PipelineNode>> PipelineRules { get; } =
        [
            new PipelineMustContainStepRule()
        ];
}
