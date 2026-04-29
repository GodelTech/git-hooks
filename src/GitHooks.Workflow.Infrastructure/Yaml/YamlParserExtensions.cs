using GitHooks.Workflow.Domain.Model;

using YamlDotNet.Core;

namespace GitHooks.Workflow.Infrastructure.Yaml;

internal static class YamlParserExtensions
{
    public static SourceLocation ToSourceLocation(this Mark mark)
    {
        return new SourceLocation(
            Line: mark.Line,
            Column: mark.Column
        );
    }

    public static SourceSpan ToSourceSpan(
        this (Mark Start, Mark End) range,
        SourceRef source)
    {
        return new SourceSpan(
            source,
            range.Start.ToSourceLocation(),
            range.End.ToSourceLocation()
        );
    }
}
