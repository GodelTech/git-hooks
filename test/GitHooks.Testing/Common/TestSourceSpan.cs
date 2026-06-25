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

    // todo: remove this as it is temp method
    public static SourceSpan Create(
        SourceDocument document)
    {
        return new SourceSpan(
            document,
            new SourcePosition(
                1,
                1),
            new SourcePosition(
                1,
                1));
    }
}
