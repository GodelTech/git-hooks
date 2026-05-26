using GitHooks.Domain.Common;

namespace GitHooks.Infrastructure.Yaml.Parsing.Exceptions;

internal sealed class YamlPipelineParsingException(string message, SourceSpan span)
    : Exception(message)
{
    public SourceSpan Span { get; } = span;
}
