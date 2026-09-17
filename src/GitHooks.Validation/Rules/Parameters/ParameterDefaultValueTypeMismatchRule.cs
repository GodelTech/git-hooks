using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Parameters;

namespace GitHooks.Validation.Rules.Parameters;

internal sealed class ParameterDefaultValueTypeMismatchRule
    : IValidationRule<ParameterNode>
{
    public void Validate(
        ParameterNode node,
        ValidationContext context)
    {
        if (node.Type is null ||
            node.DefaultValue is null)
        {
            return;
        }

        if (!node.Type.Value.TryGetStringValue(out var type))
        {
            return;
        }

        if (!node.DefaultValue.Value.TryGetStringValue(out var defaultValue))
        {
            return;
        }

        if (ParameterNodeExtensions.IsValueValidForType(type, defaultValue))
        {
            return;
        }

        context.Report(
            Diagnostic.Create(
                DiagnosticDescriptors.ParameterDefaultValueTypeMismatch,
                node.DefaultValue.Span,
                node.GetNameForDiagnostic(),
                type));
    }
}
