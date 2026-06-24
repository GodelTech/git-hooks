using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Validation.Rules.Parameters;
using GitHooks.Validation.Rules.Pipeline;
using GitHooks.Validation.Rules.Steps;
using GitHooks.Validation.Rules.Variables;

namespace GitHooks.Validation;

internal sealed class ValidationRuleSet
{
    public IReadOnlyList<IValidationRule<PipelineNode>> PipelineRules { get; } =
        [
            new PipelineMustContainStepRule(),
            new ParameterVariableMustBeResolvableRule()
        ];

    public IReadOnlyList<IValidationRule<ParameterNode>> ParameterRules { get; } =
        [
            new ParameterNameIsRequiredRule(),
            new ParameterNameMustBeValidIdentifierRule(),
            new ParameterValuesMustBeSequenceRule(),
            new ParameterDisplayNameMustNotBeEmptyRule()
        ];

    public IReadOnlyList<IValidationRule<ScriptStepNode>> ScriptStepRules { get; } =
        [
            new ScriptFieldMustNotBeEmptyRule()
        ];

    public IReadOnlyList<IValidationRule<TemplateStepNode>> TemplateStepRules { get; } =
        [
            new TemplateFieldMustNotBeEmptyRule()
        ];

    public IReadOnlyList<IValidationRule<InvalidStepNode>> InvalidStepRules { get; } =
        [
            new InvalidStepRule()
        ];

    public IReadOnlyList<IValidationRule<VariableExpressionNode>> VariableRules { get; } =
        [
            new VariableSyntaxMustBeValidRule()
        ];
}
