using GitHooks.Execution;

using Spectre.Console;

namespace GitHooks.Tests.Execution;

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
    public async Task RunAsync_WithLargeScript_ExecutesWithoutCommandLineLengthLimits()
    {
        var script = $"# {new string('x', 100_000)}\nWrite-Output 'large-script-ran'";

        var (exitCode, output) = await RunAsync(script, TestContext.Current.CancellationToken);

        Assert.Equal(0, exitCode);
        Assert.Contains("large-script-ran", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_WhenEarlierLineFails_StopsAndFails()
    {
        var (exitCode, output) = await RunAsync("exit 3\necho should-not-run", TestContext.Current.CancellationToken);

        Assert.Equal(3, exitCode);
        Assert.DoesNotContain("should-not-run", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_WhenUnknownCommandPrecedesParentheses_StopsAndFails()
    {
        var (exitCode, output) = await RunAsync(
            "some-unknown-command-xyz\necho Deploying (staging)",
            TestContext.Current.CancellationToken);

        Assert.NotEqual(0, exitCode);
        Assert.DoesNotContain("Deploying (staging)", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_WithQuotedParentheses_PrintsOutput()
    {
        var (exitCode, output) = await RunAsync("echo 'Deploying (staging)'", TestContext.Current.CancellationToken);

        Assert.Equal(0, exitCode);
        Assert.Contains("Deploying (staging)", output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("if ($true) { Write-Output 'inside-block' }", "inside-block")]
    [InlineData("$message = 'Hello!'; Write-Output $message", "Hello!")]
    [InlineData("Write-Output 'one & two | 100%'", "one & two | 100%")]
    [InlineData("Write-Output 'Zażółć'", "Zażółć")]
    [InlineData("[Console]::Error.WriteLine('stderr-message')", "stderr-message")]
    [InlineData("[pscustomobject]@{ Message = 'formatted-object' }", "formatted-object")]
    public async Task RunAsync_WithPowerShellSyntax_ExecutesAndStreamsOutput(string script, string expected)
    {
        var (exitCode, output) = await RunAsync(script, TestContext.Current.CancellationToken);

        Assert.Equal(0, exitCode);
        Assert.Contains(expected, output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_WithParenthesisInComment_ChecksFollowingCommands()
    {
        var (exitCode, output) = await RunAsync(
            "# (\nsome-unknown-command-xyz\necho should-not-run",
            TestContext.Current.CancellationToken);

        Assert.NotEqual(0, exitCode);
        Assert.DoesNotContain("should-not-run", output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("param([string] $message = 'hello-from-param')\nWrite-Output $message", "hello-from-param")]
    [InlineData("using namespace System.Text\nWrite-Output ([StringBuilder]::new('hello-from-using').ToString())", "hello-from-using")]
    public async Task RunAsync_WithScriptDeclarations_ExecutesUnmodifiedScript(string script, string expected)
    {
        var (exitCode, output) = await RunAsync(script, TestContext.Current.CancellationToken);

        Assert.Equal(0, exitCode);
        Assert.Contains(expected, output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_WhenUnknownCommandFails_StopsBeforeNextCommand()
    {
        var (exitCode, output) = await RunAsync(
            "some-unknown-command-xyz\necho should-not-run",
            TestContext.Current.CancellationToken);

        Assert.NotEqual(0, exitCode);
        Assert.DoesNotContain("should-not-run", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_WhenNativeCommandFails_StopsAndReturnsItsExitCode()
    {
        var (exitCode, output) = await RunAsync(
            "pwsh -NoProfile -NonInteractive -Command 'exit 7'\necho should-not-run",
            TestContext.Current.CancellationToken);

        Assert.Equal(7, exitCode);
        Assert.DoesNotContain("should-not-run", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_WhenPowerShellCommandWritesError_StopsAndFails()
    {
        var (exitCode, output) = await RunAsync(
            "Write-Error 'step-error'\necho should-not-run",
            TestContext.Current.CancellationToken);

        Assert.NotEqual(0, exitCode);
        Assert.Contains("step-error", output, StringComparison.Ordinal);
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
        var (exitCode, _) = await RunAsync("Read-Host 'input'", TestContext.Current.CancellationToken);

        Assert.NotEqual(0, exitCode);
    }

    [Fact]
    public async Task RunAsync_WhenCancelled_ThrowsOperationCanceled()
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.CancelAfter(TimeSpan.FromMilliseconds(300));
        var script = "Start-Sleep -Seconds 30";

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
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
