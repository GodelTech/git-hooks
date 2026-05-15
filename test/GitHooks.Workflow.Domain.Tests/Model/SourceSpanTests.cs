using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Domain.Tests.Model;

public class SourceSpanTests
{
    [Fact]
    public void Unknown_WithSource_ReturnsSpanWithZeroStartAndEnd()
    {
        var source = new SourceRef("test.yaml");

        var result = SourceSpan.Unknown(source);

        Assert.Equal(source, result.Source);
        Assert.Equal(new SourceLocation(0, 0), result.Start);
        Assert.Equal(new SourceLocation(0, 0), result.End);
    }
}
