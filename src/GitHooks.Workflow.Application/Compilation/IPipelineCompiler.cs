using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Domain.Model;

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
    /// <param name="source">The source of the pipeline content.</param>
    /// <param name="parameterOverrides">Optional parameter overrides applied during binding.</param>
    /// <returns>A bound pipeline AST.</returns>
    public PipelineNode Compile(
        string content,
        PipelineSource source,
        IReadOnlyDictionary<string, string>? parameterOverrides = null);
}
