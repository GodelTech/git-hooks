using GitHooks.Domain.Common;

namespace GitHooks.Diagnostics.Printing;

public static class DiagnosticLocationRenderer
{
    public static string Render(
        SourceSpan span)
    {
        var location = RenderLocation(span);

        if (!span.HasDocument)
        {
            return location;
        }

        return $"{span.Document!.Name}({location})";
    }

    private static string RenderLocation(
        SourceSpan span)
    {
        if (!span.Start.HasKnownLine ||
            !span.Start.HasKnownColumn ||
            !span.End.HasKnownLine ||
            !span.End.HasKnownColumn)
        {
            return "<unknown>";
        }

        if (span.Start == span.End)
        {
            return $"{span.Start.Line},{span.Start.Column}";
        }

        return $"{span.Start.Line},{span.Start.Column},{span.End.Line},{span.End.Column}";
    }
}
