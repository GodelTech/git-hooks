using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Parameters;

namespace GitHooks.Validation.Rules.Parameters;

internal sealed class ParameterTypeMustBeSupportedRule
    : IValidationRule<ParameterNode>
{
    private static readonly string[] s_supportedTypes =
        ["string", "boolean", "number"];

    public void Validate(
        ParameterNode node,
        ValidationContext context)
    {
        if (node.Type is null)
        {
            return;
        }

        if (!node.Type.Value.TryGetStringValue(out var value))
        {
            return;
        }

        if (Array.Exists(
                s_supportedTypes,
                supportedType => string.Equals(
                    supportedType,
                    value,
                    StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        context.Report(
            Diagnostic.Create(
                DiagnosticDescriptors.UnsupportedParameterType,
                node.Type.Span,
                value));
    }
}
