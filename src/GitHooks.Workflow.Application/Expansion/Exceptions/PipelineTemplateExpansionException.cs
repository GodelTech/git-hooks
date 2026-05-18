using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Expansion.Exceptions;

/// <summary>
/// Represents a template expansion error with source location and include-chain context.
/// </summary>
public sealed class PipelineTemplateExpansionException(
    string message,
    SourceSpan span,
    IReadOnlyList<string>? includeChain = null,
    Exception? innerException = null)
    : Exception(message, innerException)
{
    /// <summary>
    /// Gets the source span associated with the expansion failure.
    /// </summary>
    public SourceSpan Span { get; } = span;

    /// <summary>
    /// Gets the include chain used to reach the failed template, if available.
    /// </summary>
    public IReadOnlyList<string> IncludeChain { get; } = includeChain ?? [];
}
