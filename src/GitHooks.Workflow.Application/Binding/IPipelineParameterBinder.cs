using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding.Exceptions;

namespace GitHooks.Workflow.Application.Binding;

/// <summary>
/// Validates pipeline parameters and expands supported parameter expressions.
/// </summary>
public interface IPipelineParameterBinder
{
    /// <summary>
    /// Validates declared parameters and expands supported expressions within a pipeline AST.
    /// </summary>
    /// <param name="pipeline">The parsed pipeline AST.</param>
    /// <returns>A new pipeline AST with supported parameter expressions expanded.</returns>
    /// <exception cref="PipelineParameterBindingException">Thrown when a parameter reference cannot be resolved or validated.</exception>
    public PipelineNode Bind(PipelineNode pipeline);
}
