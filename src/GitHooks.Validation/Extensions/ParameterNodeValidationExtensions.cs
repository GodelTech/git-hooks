using GitHooks.Domain.Ast.Mappings.Parameters;

namespace GitHooks.Validation.Extensions;

internal static class ParameterNodeValidationExtensions
{
    public static string GetNameForDiagnostic(
        this ParameterNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        return node.TryGetName(out var name)
            ? name
            : "<unknown>";
    }
}
