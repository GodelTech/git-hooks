using System.Globalization;

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

    public static bool IsValueValidForType(
        string type,
        string value)
    {
        if (string.Equals(type, "boolean", StringComparison.OrdinalIgnoreCase))
        {
            return bool.TryParse(value, out _);
        }

        if (string.Equals(type, "number", StringComparison.OrdinalIgnoreCase))
        {
            return decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out _);
        }

        return true;
    }

    public static bool ContainsValue(
        IReadOnlyList<ExpressionNode> values,
        string parameterValue)
    {
        ArgumentNullException.ThrowIfNull(values);

        foreach (var candidate in values)
        {
            if (candidate.TryGetStringValue(out var stringValue) &&
                string.Equals(stringValue, parameterValue, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public static string GetNameForDiagnostic(
        this ParameterNode declaration)
    {
        ArgumentNullException.ThrowIfNull(declaration);

        return declaration.TryGetName(out var name)
            ? name
            : "<unknown>";
    }
}
