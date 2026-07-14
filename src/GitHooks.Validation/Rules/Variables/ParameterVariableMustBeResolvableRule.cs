using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;

namespace GitHooks.Validation.Rules.Variables;

internal sealed partial class ParameterVariableMustBeResolvableRule
    : IValidationRule<ParameterVariableExpressionNode>
{
    public void Validate(
        ParameterVariableExpressionNode node,
        ValidationContext context)
    {
        if (!context.Symbols.ContainsParameter(node.Name))
        {
            context.Report(
                DiagnosticDescriptors.ParameterVariableMustBeResolvable,
                node.Span,
                node.Name);
        }
    }
}
