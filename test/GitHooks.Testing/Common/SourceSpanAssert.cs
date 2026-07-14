using GitHooks.Domain.Common;
using GitHooks.Domain.Common.Rendering;

namespace GitHooks.Testing.Common;

public static class SourceSpanAssert
{
    public static void Equal(
        SourceSpan expected,
        SourceSpan actual)
    {
        if (expected == TestSourceSpan.Unknown)
        {
            Assert.Fail("Expected SourceSpan must not be Unknown.");
        }

        if (actual == TestSourceSpan.Unknown)
        {
            Assert.Fail("Actual SourceSpan must not be Unknown.");
        }

        if (expected != actual)
        {
            Assert.Fail(
                $"""
                SourceSpan mismatch.

                Expected: {SourceSpanRenderer.Render(expected)}
                Actual:   {SourceSpanRenderer.Render(actual)}
                """);
        }
    }
}
