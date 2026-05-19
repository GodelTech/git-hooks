using GitHooks.Workflow.Application.Ast;

namespace GitHooks.Workflow.Application.Compilation;

/// <summary>
/// Compiles pipeline content into a bound pipeline AST.
/// </summary>
public interface IPipelineCompiler
{
    /// <summary>
    /// Parses the supplied pipeline content and binds parameter values.
    /// </summary>
    /// <param name="content">Pipeline YAML content.</param>
    /// <param name="sourceName">Name or path of the source used for diagnostics.</param>
    /// <param name="parameterOverrides">Optional parameter overrides applied during binding.</param>
    /// <returns>A bound pipeline AST.</returns>
    public PipelineNode Compile(
        string content,
        string sourceName,
        IReadOnlyDictionary<string, string>? parameterOverrides = null);
}
