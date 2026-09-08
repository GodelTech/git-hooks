using GitHooks.Domain.Ast.Expressions;

namespace GitHooks.Domain.Ast.Mappings.Parameters;

public static class ParameterNodeExtensions
{
    public static bool TryGetName(
        this ParameterNode node,
        out string name)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (node.Name.Value.TryGetStringValue(out name) &&
            !string.IsNullOrWhiteSpace(name))
        {
            return true;
        }

        name = string.Empty;
        return false;
    }

    public static bool TryGetDefaultValue(
        this ParameterNode node,
        out string defaultValue)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (node.DefaultValue is not null &&
            node.DefaultValue.Value.TryGetStringValue(out defaultValue))
        {
            return true;
        }

        defaultValue = string.Empty;
        return false;
    }
}
