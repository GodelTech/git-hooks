using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Parsing.Exceptions;

public sealed class PipelineParsingException(string message, SourceSpan span, Exception? innerException = null)
    : Exception(message, innerException)
{
    public SourceSpan Span { get; } = span;
}
