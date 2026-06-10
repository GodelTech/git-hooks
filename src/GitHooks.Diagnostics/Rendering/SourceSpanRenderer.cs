using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics.Rendering;

public static class SourceSpanRenderer
{
    public static string Render(SourceSpan span)
    {
        if (!span.HasDocument && span.HasUnknownPosition)
        {
            return "<unknown>";
        }

        var location = span.HasUnknownPosition
            ? "<unknown>"
            : $"{SourcePositionRenderer.Render(span.Start)}-{SourcePositionRenderer.Render(span.End)}";

        return span.HasDocument
            ? $"{span.Document!.Name}({location})"
            : $"({location})";
    }
}
