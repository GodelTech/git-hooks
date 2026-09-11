using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Validation.Rules.Parameters;
using GitHooks.Validation.Rules.Pipeline;
using GitHooks.Validation.Rules.Steps;
using GitHooks.Validation.Rules.Variables;

namespace GitHooks.Validation.Rules;

internal static class ValidationRuleCatalog
{
    public static RuleCollection<PipelineNode> Pipeline { get; } =
        [
            new PipelineMustContainStepRule()
        ];

    public static RuleCollection<ParameterNode> Parameter { get; } =
        [
            new ParameterNameIsRequiredRule(),
            new ParameterNameMustBeValidIdentifierRule(),
            new ParameterNameMustBeUniqueRule(),
            new ParameterValuesMustBeSequenceRule(),
            new ParameterDisplayNameMustNotBeEmptyRule(),
            new ParameterTypeMustBeSupportedRule(),
            new ParameterDefaultValueTypeMismatchRule(),
            new ParameterDefaultValueMustBeInValuesRule()
        ];

    public static RuleCollection<ScriptStepNode> ScriptStep { get; } =
        [
            new ScriptFieldMustNotBeEmptyRule()
        ];

    public static RuleCollection<TemplateStepNode> TemplateStep { get; } =
        [
            new TemplateFieldMustNotBeEmptyRule()
        ];

    public static RuleCollection<InvalidStepNode> InvalidStep { get; } =
        [
            new InvalidStepRule()
        ];

    public static RuleCollection<ParameterVariableExpressionNode> ParameterVariable { get; } =
        [
            new ParameterVariableMustBeResolvableRule()
        ];

    public static void RegisterBuiltInRules(this ValidationRuleRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        registry.Register(Pipeline);
        registry.Register(Parameter);
        registry.Register(ScriptStep);
        registry.Register(TemplateStep);
        registry.Register(InvalidStep);
        registry.Register(ParameterVariable);
    }
}
