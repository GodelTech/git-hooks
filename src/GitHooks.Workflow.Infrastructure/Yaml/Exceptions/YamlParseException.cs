using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.Yaml.Exceptions;

public sealed class YamlParseException(string message, SourceSpan span, Exception? innerException = null)
    : Exception(message, innerException)
{
    public SourceSpan Span { get; } = span;
}

