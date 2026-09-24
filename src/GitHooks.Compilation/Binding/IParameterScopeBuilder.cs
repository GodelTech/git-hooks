using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings.Parameters;

namespace GitHooks.Compilation.Binding;

/// <summary>
/// Builds a parameter scope (declarations plus resolved values) from a set of parameter
/// declarations. Used both for the root pipeline scope and for per-template scopes created
/// during expansion.
/// </summary>
internal interface IParameterScopeBuilder
{
    /// <summary>
    /// Collects the supplied parameter declarations into a <see cref="ParameterTable"/>.
    /// </summary>
    /// <param name="parameters">The parameter declarations to collect.</param>
    /// <returns>A table containing the collected declarations.</returns>
    public ParameterTable Collect(
        IReadOnlyList<ParameterNode> parameters);

    /// <summary>
    /// Seeds a <see cref="ParameterValueTable"/> with the default values declared by the
    /// supplied declarations.
    /// </summary>
    /// <param name="declarations">The parameter declarations whose defaults are resolved.</param>
    /// <returns>A value table seeded with declared default values.</returns>
    public ParameterValueTable ResolveDefaults(
        ParameterTable declarations);

    /// <summary>
    /// Applies the supplied overrides to the value table, reporting diagnostics for
    /// undeclared or invalid overrides.
    /// </summary>
    /// <param name="declarations">The parameter declarations of the scope.</param>
    /// <param name="values">The value table to update.</param>
    /// <param name="parameterOverrides">Optional overrides in name/value form.</param>
    /// <param name="diagnostics">The diagnostic bag to report invalid overrides to.</param>
    public void ApplyOverrides(
        ParameterTable declarations,
        ParameterValueTable values,
        IReadOnlyDictionary<string, string>? parameterOverrides,
        DiagnosticBag diagnostics);
}
