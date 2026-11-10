using GitHooks.Handlers;
using GitHooks.Infrastructure.Cli;
using GitHooks.Infrastructure.Git;
using GitHooks.Infrastructure.Hooks;
using GitHooks.Infrastructure.Scaffold;

using Spectre.Console;

namespace GitHooks.Tests.Handlers;

public sealed class HookHandlerFailureTests
{
    [Fact]
    public async Task Create_GitUnavailable_StopsBeforeScaffolding()
    {
        var git = new FakeGitCommandLine { IsAvailable = false };
        var scaffolder = new FakeScaffolder();
        var (console, writer) = CreateConsole();
        var handler = new CreateHookHandler(git, GitHookCatalog.Default, scaffolder, console);

        var exitCode = await handler.HandleAsync(GitConfigScope.Local, ".githooks", ["pre-commit"], false, TestContext.Current.CancellationToken);

        Assert.Equal(1, exitCode);
        Assert.Contains("Git not found", writer.ToString(), StringComparison.Ordinal);
        Assert.False(scaffolder.WasCalled);
    }

    [Fact]
    public async Task Create_OutsideRepository_StopsBeforeScaffolding()
    {
        var git = new FakeGitCommandLine { IsInsideRepository = false };
        var scaffolder = new FakeScaffolder();
        var (console, writer) = CreateConsole();
        var handler = new CreateHookHandler(git, GitHookCatalog.Default, scaffolder, console);

        var exitCode = await handler.HandleAsync(GitConfigScope.Local, ".githooks", ["pre-commit"], false, TestContext.Current.CancellationToken);

        Assert.Equal(1, exitCode);
        Assert.Contains("Not inside a git repository", writer.ToString(), StringComparison.Ordinal);
        Assert.False(scaffolder.WasCalled);
    }

    [Theory]
    [InlineData(false, "fatal: missing repository")]
    [InlineData(true, "empty repository root path")]
    public async Task Create_InvalidRepositoryRoot_StopsBeforeScaffolding(bool success, string expectedMessage)
    {
        var git = new FakeGitCommandLine
        {
            RepositoryRoot = success
                ? CommandLineResult.FromProcess(" ", string.Empty, 0)
                : CommandLineResult.FromProcess(string.Empty, "fatal: missing repository", 1)
        };
        var scaffolder = new FakeScaffolder();
        var (console, writer) = CreateConsole();
        var handler = new CreateHookHandler(git, GitHookCatalog.Default, scaffolder, console);

        var exitCode = await handler.HandleAsync(GitConfigScope.Local, ".githooks", ["pre-commit"], false, TestContext.Current.CancellationToken);

        Assert.Equal(1, exitCode);
        Assert.Contains(expectedMessage, writer.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.False(scaffolder.WasCalled);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Create_ScaffoldFailure_ReturnsOneAndMentionsForceOnlyWhenAvailable(bool force)
    {
        var scaffolder = new FakeScaffolder { Result = GitHookScaffoldResult.Failure("cannot write") };
        var (console, writer) = CreateConsole();
        var handler = new CreateHookHandler(new FakeGitCommandLine(), GitHookCatalog.Default, scaffolder, console);

        var exitCode = await handler.HandleAsync(GitConfigScope.Local, ".githooks", ["pre-commit"], force, TestContext.Current.CancellationToken);

        Assert.Equal(1, exitCode);
        Assert.True(scaffolder.WasCalled);
        Assert.Contains("cannot write", writer.ToString(), StringComparison.Ordinal);
        Assert.Equal(!force, writer.ToString().Contains("--force", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Install_SetFailure_ReportsGitError()
    {
        var git = new FakeGitCommandLine { SetResult = CommandLineResult.FromProcess(string.Empty, "permission denied", 1) };
        var (console, writer) = CreateConsole();
        var handler = new InstallHandler(git, console);

        var exitCode = await handler.HandleAsync(GitConfigScope.Local, ".githooks", false, TestContext.Current.CancellationToken);

        Assert.Equal(1, exitCode);
        Assert.Contains("permission denied", writer.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Uninstall_UnsetFailure_ReportsGitError()
    {
        var git = new FakeGitCommandLine
        {
            ExistingHooksPath = CommandLineResult.FromProcess(".githooks", string.Empty, 0),
            UnsetResult = CommandLineResult.FromProcess(string.Empty, "permission denied", 1)
        };
        var (console, writer) = CreateConsole();
        var handler = new UninstallHandler(git, console);

        var exitCode = await handler.HandleAsync(GitConfigScope.Local, TestContext.Current.CancellationToken);

        Assert.Equal(1, exitCode);
        Assert.Contains("permission denied", writer.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task InstallAndUninstall_Preconditions_ReturnOne(bool isAvailable, bool isInsideRepository)
    {
        var git = new FakeGitCommandLine { IsAvailable = isAvailable, IsInsideRepository = isInsideRepository };
        var (console, _) = CreateConsole();

        var installExitCode = await new InstallHandler(git, console)
            .HandleAsync(GitConfigScope.Local, ".githooks", false, TestContext.Current.CancellationToken);
        var uninstallExitCode = await new UninstallHandler(git, console)
            .HandleAsync(GitConfigScope.Local, TestContext.Current.CancellationToken);

        Assert.Equal(1, installExitCode);
        Assert.Equal(1, uninstallExitCode);
    }

    private static (IAnsiConsole Console, StringWriter Writer) CreateConsole()
    {
        var writer = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Interactive = InteractionSupport.No,
            Out = new AnsiConsoleOutput(writer)
        });
        console.Profile.Width = 1000;
        return (console, writer);
    }

    private sealed class FakeScaffolder : IGitHookScaffolder
    {
        public bool WasCalled { get; private set; }

        public GitHookScaffoldResult Result { get; init; } = GitHookScaffoldResult.Success([], ".githooks");

        public Task<GitHookScaffoldResult> CreateScaffoldAsync(
            string repositoryRootPath,
            string hooksPath,
            IReadOnlyCollection<GitHook> hooks,
            bool overwrite,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(Result);
        }
    }

    private sealed class FakeGitCommandLine : IGitCommandLine
    {
        public bool IsAvailable { get; init; } = true;

        public bool IsInsideRepository { get; init; } = true;

        public CommandLineResult RepositoryRoot { get; init; } = CommandLineResult.FromProcess("C:/repo", string.Empty, 0);

        public CommandLineResult ExistingHooksPath { get; init; } = CommandLineResult.FromProcess(string.Empty, string.Empty, 1);

        public CommandLineResult SetResult { get; init; } = CommandLineResult.FromProcess(string.Empty, string.Empty, 0);

        public CommandLineResult UnsetResult { get; init; } = CommandLineResult.FromProcess(string.Empty, string.Empty, 0);

        public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(IsAvailable);
        }

        public Task<bool> IsInsideGitRepositoryAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(IsInsideRepository);
        }

        public Task<CommandLineResult> GetRepositoryRootPathAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RepositoryRoot);
        }

        public Task<CommandLineResult> GetCoreHooksPathAsync(GitConfigScope scope, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExistingHooksPath);
        }

        public Task<CommandLineResult> SetCoreHooksPathAsync(GitConfigScope scope, string value, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SetResult);
        }

        public Task<CommandLineResult> UnsetCoreHooksPathAsync(GitConfigScope scope, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(UnsetResult);
        }
    }
}
