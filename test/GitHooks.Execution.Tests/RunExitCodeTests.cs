namespace GitHooks.Execution.Tests;

public sealed class RunExitCodeTests
{
    [Fact]
    public void Constants_DefineStableProcessExitCodes()
    {
        Assert.Equal(0, RunExitCode.Success);
        Assert.Equal(1, RunExitCode.CompilationFailed);
        Assert.Equal(2, RunExitCode.StepFailed);
        Assert.Equal(3, RunExitCode.ExecutionError);
    }
}
