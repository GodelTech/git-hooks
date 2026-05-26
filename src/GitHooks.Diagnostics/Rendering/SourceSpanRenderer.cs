using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics.Rendering;

public static class SourceSpanRenderer
{
    public static string Render(SourceSpan span)
    {
        return span.IsUnknown
            ? "<unknown>"
            : $"({RenderPosition(span.Start)}-{RenderPosition(span.End)})";
    }

    private static string RenderPosition(SourcePosition position)
    {
        if (position.IsUnknown)
        {
            return "<unknown>";
        }

        var line = position.HasKnownLine
            ? position.Line.ToString()
            : "?";

        var column = position.HasKnownColumn
            ? position.Column.ToString()
            : "?";

        return $"{line}:{column}";
    }
}
