using Spectre.Console;

namespace GitHooks.Execution.Tests;

public sealed class ShellScriptStepRunnerTests
{
    [Fact]
    public async Task RunAsync_WithEcho_ReturnsZeroAndStreamsOutput()
    {
        var (exitCode, output) = await RunAsync("echo hello-from-step", TestContext.Current.CancellationToken);

        Assert.Equal(0, exitCode);
        Assert.Contains("hello-from-step", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_WithFailingScript_ReturnsItsExitCode()
    {
        var (exitCode, _) = await RunAsync("exit 7", TestContext.Current.CancellationToken);

        Assert.Equal(7, exitCode);
    }

    [Fact]
    public async Task RunAsync_WithMultiLineScript_RunsEveryLine()
    {
        var (exitCode, output) = await RunAsync("echo first\necho second", TestContext.Current.CancellationToken);

        Assert.Equal(0, exitCode);
        Assert.Contains("first", output, StringComparison.Ordinal);
        Assert.Contains("second", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_WhenEarlierLineFails_StopsAndFails()
    {
        var (exitCode, output) = await RunAsync("exit 3\necho should-not-run", TestContext.Current.CancellationToken);

        Assert.Equal(3, exitCode);
        Assert.DoesNotContain("should-not-run", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_WhenRedirectionFails_ReturnsNonZero()
    {
        var target = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing", "f.txt");

        var (exitCode, _) = await RunAsync($"echo x > \"{target}\"", TestContext.Current.CancellationToken);

        Assert.NotEqual(0, exitCode);
    }

    [Fact]
    public async Task RunAsync_WithMarkupLikeOutput_WritesItVerbatim()
    {
        var (_, output) = await RunAsync("echo [red]not-markup[/]", TestContext.Current.CancellationToken);

        Assert.Contains("[red]not-markup[/]", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_WithReadingScript_DoesNotHangOnStdin()
    {
        var script = OperatingSystem.IsWindows() ? "set /p x=" : "read x";

        _ = await RunAsync(script, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task RunAsync_WhenCancelled_ThrowsOperationCanceled()
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.CancelAfter(TimeSpan.FromMilliseconds(300));
        var script = OperatingSystem.IsWindows() ? "ping -n 30 127.0.0.1 > nul" : "sleep 30";

        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => RunAsync(script, cts.Token));
    }

    private static async Task<(int ExitCode, string Output)> RunAsync(
        string script,
        CancellationToken cancellationToken)
    {
        var writer = new StringWriter();
        var console = AnsiConsole.Create(
            new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.No,
                ColorSystem = ColorSystemSupport.NoColors,
                Interactive = InteractionSupport.No,
                Out = new AnsiConsoleOutput(writer),
            });

        console.Profile.Width = 1000;

        var runner = new ShellScriptStepRunner(console);
        var exitCode = await runner.RunAsync(script, Path.GetTempPath(), cancellationToken);

        return (exitCode, writer.ToString());
    }
}
