using GitHooks.Domain.Common;

namespace GitHooks.Testing.Common;

public static class SourceSpanAssert
{
    public static void Equal(
        SourceSpan expected,
        SourceSpan actual)
    {
        Assert.False(
            expected == TestSourceSpan.Unknown,
            "Expected span must be specified explicitly.");

        Assert.False(
            actual == TestSourceSpan.Unknown,
            "Actual span should not be Unknown.");

        Assert.Equal(
            expected.Document,
            actual.Document);

        Assert.Equal(
            expected.Start,
            actual.Start);

        Assert.Equal(
            expected.End,
            actual.End);
    }
}
