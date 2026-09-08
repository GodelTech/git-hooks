using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Validation.Extensions;

namespace GitHooks.Validation.Rules.Parameters;

internal sealed class ParameterDefaultValueMustBeInValuesRule
    : IValidationRule<ParameterNode>
{
    public void Validate(
        ParameterNode node,
        ValidationContext context)
    {
        if (node.Values is null ||
            node.Values.Items.Count == 0 ||
            node.DefaultValue is null)
        {
            return;
        }

        if (!node.DefaultValue.Value.TryGetStringValue(out var defaultValue))
        {
            return;
        }

        if (ContainsValue(node.Values.Items, defaultValue))
        {
            return;
        }

        context.Report(
            Diagnostic.Create(
                DiagnosticDescriptors.ParameterDefaultValueMustBeInValues,
                node.DefaultValue.Span,
                node.GetNameForDiagnostic()));
    }

    private static bool ContainsValue(
        IReadOnlyList<ExpressionNode> values,
        string defaultValue)
    {
        foreach (var value in values)
        {
            if (value.TryGetStringValue(out var stringValue) &&
                string.Equals(stringValue, defaultValue, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
