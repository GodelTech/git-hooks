using GitHooks.Pipelines.Models;

namespace GitHooks.Pipelines.Tests.Models;

public class StepTests
{
    [Fact]
    public void Step_Initialization()
    {
        // Arrange & Act
        var step = new Step
        {
            Task = "testTask"
        };

        // Assert
        Assert.NotNull(step);
        Assert.Equal("testTask", step.Task);
        Assert.Null(step.Name);
        Assert.Null(step.DisplayName);
        Assert.True(step.Enabled);
        Assert.False(step.ContinueOnError);
        Assert.Null(step.Inputs);
    }
}
