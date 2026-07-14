namespace GitHooks.Domain.Common.Rendering;

public static class StringRenderer
{
    public static string RenderQuoted(
        string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return $"\"{Escape(value)}\"";
    }

    private static string Escape(
        string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal);
    }
}
