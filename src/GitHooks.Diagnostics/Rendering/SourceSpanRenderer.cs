using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics.Rendering;

public static class SourceSpanRenderer
{
    public static string Render(
        SourceSpan span)
    {
        return span.IsUnknown
            ? "<unknown>"
            : $"({Render(span.Start)}-{Render(span.End)})";
    }

    private static string Render(
        SourcePosition position)
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
