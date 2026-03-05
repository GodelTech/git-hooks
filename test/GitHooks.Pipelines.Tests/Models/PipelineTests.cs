using GitHooks.Pipelines.Models;

namespace GitHooks.Pipelines.Tests.Models;

public class PipelineTests
{
    [Fact]
    public void Pipeline_Initialization()
    {
        // Arrange & Act
        var pipeline = new Pipeline();

        // Assert
        Assert.NotNull(pipeline);
        Assert.Empty(pipeline.Steps);
    }
}
