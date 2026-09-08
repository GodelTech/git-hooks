using System.Diagnostics.CodeAnalysis;

using GitHooks.Domain.Ast.Mappings.Parameters;

namespace GitHooks.Compilation.Binding;

internal sealed class ParameterTable
{
    private readonly Dictionary<string, ParameterNode> _parameters =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<ParameterNode> Parameters
        => _parameters.Values;

    public void AddParameter(
        ParameterNode parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        if (!parameter.TryGetName(out var name))
        {
            return;
        }

        // Duplicate parameters are reported during validation.
        _ = _parameters.TryAdd(name, parameter);
    }

    public bool ContainsParameter(
        string name)
    {
        return TryGetParameter(
            name,
            out _);
    }

    public bool TryGetParameter(
        string name,
        [NotNullWhen(true)] out ParameterNode? parameter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return _parameters.TryGetValue(
            name,
            out parameter);
    }
}
