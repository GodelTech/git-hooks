using System.Text.RegularExpressions;

using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;

namespace GitHooks.Validation.Rules.Variables;

internal sealed partial class VariableMustBeResolvableParameterRule
    : IValidationRule<VariableExpressionNode>
{
    public void Validate(
        VariableExpressionNode node,
        ValidationContext context)
    {
        var match = ParameterPathPattern().Match(node.Path);

        if (match.Success)
        {
            var parameterName = match.Groups[1].Value;

            if (!context.Symbols.ContainsParameter(parameterName))
            {
                context.Report(
                    DiagnosticDescriptors.VariableMustBeResolvableParameter,
                    node.Span,
                    parameterName);
            }
        }
    }

    // todo: AST node must already solve this, so this regex should not be needed. The AST should have a VariableReference that has the parameter name as a property.
    [GeneratedRegex(@"^parameters\.([a-zA-Z_][a-zA-Z0-9_]*)$")]
    private static partial Regex ParameterPathPattern();
}
