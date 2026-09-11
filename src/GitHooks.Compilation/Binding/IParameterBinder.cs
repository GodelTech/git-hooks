using GitHooks.Domain.Ast.Mappings;

namespace GitHooks.Compilation.Binding;

/// <summary>
/// Binds pipeline parameters by collecting declarations, resolving defaults, applying
/// command-line overrides, and substituting resolved values into the AST.
/// </summary>
/// <remarks>
/// Binding is split into two phases so that validation can run in between:
/// <see cref="Prepare"/> establishes the parameter and value tables, then validation runs,
/// then <see cref="Substitute"/> rewrites the AST using the resolved values.
/// </remarks>
public interface IParameterBinder
{
    /// <summary>
    /// Collects parameter declarations, seeds default values, and applies the supplied
    /// overrides, populating <see cref="CompilationContext.Parameters"/> and
    /// <see cref="CompilationContext.Values"/>. Undeclared overrides are reported as diagnostics.
    /// </summary>
    /// <param name="root">The parsed pipeline root.</param>
    /// <param name="parameterOverrides">Optional command-line parameter overrides in name/value form.</param>
    /// <param name="context">The compilation context whose parameter/value tables are populated.</param>
    public void Prepare(
        PipelineNode root,
        IReadOnlyDictionary<string, string>? parameterOverrides,
        CompilationContext context);

    /// <summary>
    /// Substitutes the resolved parameter values from <see cref="CompilationContext.Values"/>
    /// into the AST, producing the bound pipeline.
    /// </summary>
    /// <param name="root">The parsed pipeline root.</param>
    /// <param name="context">The compilation context holding the resolved values.</param>
    /// <returns>The bound pipeline with parameter values substituted.</returns>
    public PipelineNode Substitute(
        PipelineNode root,
        CompilationContext context);
}
