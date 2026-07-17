using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Binding.Exceptions;

/// <summary>
/// Represents a semantic pipeline-parameter validation or substitution error.
/// </summary>
public sealed class PipelineParameterBindingException(string message, SourceSpan span)
    : Exception(message)
{
    /// <summary>
    /// Gets the source span associated with the semantic error.
    /// </summary>
    public SourceSpan Span { get; }
        = span;
}
