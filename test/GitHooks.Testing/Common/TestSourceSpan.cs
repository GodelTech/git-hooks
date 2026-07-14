using GitHooks.Domain.Common;

namespace GitHooks.Testing.Common;

public static class TestSourceSpan
{
    public static readonly SourceSpan Unknown
        = SourceSpan.Unknown;

    public static SourceSpan Create(
        SourceDocument document,
        long startLine,
        long startColumn,
        long endLine,
        long endColumn)
    {
        return new SourceSpan(
            document,
            new SourcePosition(
                startLine,
                startColumn),
            new SourcePosition(
                endLine,
                endColumn));
    }

    public static SourceSpan CreateFieldKeySpan(
        SourceDocument document,
        int line,
        string fieldName)
    {
        return Create(
            document,
            line,
            1,
            line,
            $"{fieldName}".Length + 1);
    }

    public static SourceSpan CreateFieldSpan(
        SourceDocument document,
        int line,
        string fieldName,
        string fieldValue)
    {
        return Create(
            document,
            line,
            1,
            line,
            $"{fieldName}: {fieldValue}".Length + 1);
    }

    public static SourceSpan CreateFieldValueSpan(
        SourceDocument document,
        int line,
        string fieldName,
        string fieldValue)
    {
        return Create(
            document,
            line,
            $"{fieldName}: ".Length + 1,
            line,
            $"{fieldName}: {fieldValue}".Length + 1);
    }
}
