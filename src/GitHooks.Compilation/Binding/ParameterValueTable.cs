using System.Diagnostics.CodeAnalysis;

namespace GitHooks.Compilation.Binding;

internal sealed class ParameterValueTable
{
    private readonly Dictionary<string, string> _values =
        new(StringComparer.OrdinalIgnoreCase);

    public void SetValue(
        string name,
        string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);

        _values[name] = value;
    }

    public bool ContainsValue(
        string name)
    {
        return TryGetValue(
            name,
            out _);
    }

    public bool TryGetValue(
        string name,
        [NotNullWhen(true)] out string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return _values.TryGetValue(
            name,
            out value);
    }
}
