using System.Globalization;

using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics.Rendering;

public static class SourcePositionRenderer
{
    public static string Render(
        SourcePosition position)
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
