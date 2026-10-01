using GitHooks.Domain.Ast.Mappings;

namespace GitHooks.Compilation.Binding;

/// <inheritdoc/>
internal sealed class ParameterBinder(
    IParameterScopeBuilder scopeBuilder)
    : IParameterBinder
{
    private readonly IParameterScopeBuilder _scopeBuilder
        = scopeBuilder ?? throw new ArgumentNullException(nameof(scopeBuilder));

    /// <inheritdoc/>
    public void Prepare(
        PipelineNode root,
        IReadOnlyDictionary<string, string>? parameterOverrides,
        CompilationContext context)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(context);

        context.Parameters = _scopeBuilder.Collect(root.Parameters);
        context.Values = _scopeBuilder.ResolveDefaults(context.Parameters);

        _scopeBuilder.ApplyOverrides(
            context.Parameters,
            context.Values,
            parameterOverrides,
            context.Diagnostics);
    }

    /// <inheritdoc/>
    public PipelineNode Substitute(
        PipelineNode root,
        CompilationContext context)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(context);

        return ParameterSubstitutionRewriter.Rewrite(
            root,
            context.Parameters,
            context.Values,
            context.Diagnostics);
    }
}
