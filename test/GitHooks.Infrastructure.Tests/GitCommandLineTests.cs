using GitHooks.Infrastructure.Cli;
using GitHooks.Infrastructure.Git;

namespace GitHooks.Infrastructure.Tests;

public sealed class GitCommandLineTests
{
    [Fact]
    public async Task RepositoryQueries_PassExpectedArgumentsAndUseExitStatus()
    {
        var commandLine = new RecordingCommandLine();
        var git = new GitCommandLine(commandLine);

        Assert.True(await git.IsAvailableAsync(TestContext.Current.CancellationToken));
        Assert.True(await git.IsInsideGitRepositoryAsync(TestContext.Current.CancellationToken));
        Assert.True((await git.GetRepositoryRootPathAsync(TestContext.Current.CancellationToken)).IsSuccess);

        Assert.Collection(
            commandLine.Calls,
            call => Assert.Equal(["--version"], call.Arguments),
            call => Assert.Equal(["rev-parse", "--is-inside-work-tree"], call.Arguments),
            call => Assert.Equal(["rev-parse", "--show-toplevel"], call.Arguments));

        commandLine.Result = CommandLineResult.FromProcess(string.Empty, "failure", 1);
        Assert.False(await git.IsAvailableAsync(TestContext.Current.CancellationToken));
        Assert.False(await git.IsInsideGitRepositoryAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(GitConfigScope.Local, "--local")]
    [InlineData(GitConfigScope.Global, "--global")]
    [InlineData(GitConfigScope.System, "--system")]
    public async Task HooksPathOperations_PassScopeAndValue(GitConfigScope scope, string scopeArgument)
    {
        var commandLine = new RecordingCommandLine();
        var git = new GitCommandLine(commandLine);

        _ = await git.GetCoreHooksPathAsync(scope, TestContext.Current.CancellationToken);
        _ = await git.SetCoreHooksPathAsync(scope, "hooks with spaces", TestContext.Current.CancellationToken);
        _ = await git.UnsetCoreHooksPathAsync(scope, TestContext.Current.CancellationToken);

        Assert.Collection(
            commandLine.Calls,
            call => Assert.Equal(["config", scopeArgument, "--get", "core.hooksPath"], call.Arguments),
            call => Assert.Equal(["config", scopeArgument, "core.hooksPath", "hooks with spaces"], call.Arguments),
            call => Assert.Equal(["config", scopeArgument, "--unset", "core.hooksPath"], call.Arguments));
    }

    private sealed class RecordingCommandLine : ICommandLine
    {
        public List<(string Executable, string[] Arguments)> Calls { get; } = [];

        public CommandLineResult Result { get; set; } = CommandLineResult.FromProcess("ok", string.Empty, 0);

        public Task<CommandLineResult> RunAsync(
            string fileName,
            IEnumerable<string> arguments,
            CancellationToken cancellationToken = default)
        {
            Calls.Add((fileName, [.. arguments]));
            Assert.Equal("git", fileName);
            Assert.Equal(TestContext.Current.CancellationToken, cancellationToken);
            return Task.FromResult(Result);
        }
    }
}
