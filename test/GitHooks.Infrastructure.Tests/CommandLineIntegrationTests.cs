using GitHooks.Infrastructure.Cli;

namespace GitHooks.Infrastructure.Tests;

public sealed class CommandLineIntegrationTests
{
    [Fact]
    public async Task RunAsync_RealProcess_CapturesSuccessfulOutput()
    {
        var result = await new CommandLine().RunAsync("dotnet", ["--version"], TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(0, result.ExitCode);
        Assert.False(string.IsNullOrWhiteSpace(result.Output));
        Assert.Empty(result.Error);
    }

    [Fact]
    public async Task RunAsync_FailingProcess_CapturesErrorAndExitCode()
    {
        var result = await new CommandLine().RunAsync("dotnet", ["--definitely-invalid-option"], TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.NotEqual(0, result.ExitCode);
        Assert.False(string.IsNullOrWhiteSpace(result.Error + result.Output));
    }

    [Fact]
    public async Task RunAsync_MissingExecutable_ReturnsFailureResult()
    {
        var result = await new CommandLine().RunAsync(
            "githooks-executable-that-does-not-exist",
            [],
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(1, result.ExitCode);
        Assert.NotEmpty(result.Error);
    }

    [Fact]
    public async Task RunAsync_NullArguments_Throws()
    {
        _ = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            new CommandLine().RunAsync("dotnet", null!, TestContext.Current.CancellationToken));
    }
}
