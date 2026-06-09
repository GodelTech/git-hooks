using System.Globalization;

using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics.Rendering;

// TODO: start using SourceDocument name in rendering
public static class SourceSpanRenderer
{
    public static string Render(SourceSpan span)
    {
        return span.HasUnknownPosition
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
            ? position.Line.ToString(CultureInfo.InvariantCulture)
            : "?";

        var column = position.HasKnownColumn
            ? position.Column.ToString(CultureInfo.InvariantCulture)
            : "?";

        return $"{line}:{column}";
    }
}
