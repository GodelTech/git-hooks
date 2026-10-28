using GitHooks.Commands;

namespace GitHooks.Tests.Commands;

public sealed class HookLifecycleIntegrationTests : IDisposable
{
    private readonly string _repositoryDirectory = Directory.CreateDirectory(
        Path.Combine(Path.GetTempPath(), "githooks-lifecycle-tests-" + Guid.NewGuid().ToString("N"))).FullName;

    [Fact]
    public async Task Create_WithSelectedHooks_CreatesFilesAndHonorsForce()
    {
        await InitializeRepositoryAsync();

        var (firstExitCode, firstOutput) = await RunCliAsync(
            "create", "--scope", "local", "--hooks-path", ".githooks", "--hooks", "pre-commit,pre-push");

        Assert.Equal(0, firstExitCode);
        Assert.Contains("Created 2 hook file(s)", firstOutput, StringComparison.Ordinal);
        var hooksDirectory = Path.Combine(_repositoryDirectory, ".githooks");
        Assert.True(File.Exists(Path.Combine(hooksDirectory, "pre-commit")));
        Assert.True(File.Exists(Path.Combine(hooksDirectory, "pre-commit.yaml")));
        Assert.True(File.Exists(Path.Combine(hooksDirectory, "pre-push")));
        Assert.True(File.Exists(Path.Combine(hooksDirectory, "pre-push.yaml")));

        var (runExitCode, runOutput) = await RunCliAsync(
            "run", "--file", Path.Combine(hooksDirectory, "pre-commit.yaml"));
        Assert.True(runExitCode == 0, runOutput);
        Assert.Contains("Running pre-commit hook", runOutput, StringComparison.Ordinal);

        var scriptPath = Path.Combine(hooksDirectory, "pre-commit");
        await File.WriteAllTextAsync(scriptPath, "original", TestContext.Current.CancellationToken);

        var (secondExitCode, secondOutput) = await RunCliAsync(
            "create", "--scope", "local", "--hooks-path", ".githooks", "--hooks", "pre-commit");

        Assert.Equal(1, secondExitCode);
        Assert.Contains("already exists", secondOutput, StringComparison.Ordinal);
        Assert.Equal("original", await File.ReadAllTextAsync(scriptPath, TestContext.Current.CancellationToken));

        var (forceExitCode, _) = await RunCliAsync(
            "create", "--scope", "local", "--hooks-path", ".githooks", "--hooks", "pre-commit", "--force");

        Assert.Equal(0, forceExitCode);
        Assert.Contains("pre-commit", await File.ReadAllTextAsync(scriptPath, TestContext.Current.CancellationToken), StringComparison.Ordinal);
        if (!OperatingSystem.IsWindows())
        {
            Assert.True(File.GetUnixFileMode(scriptPath).HasFlag(UnixFileMode.UserExecute));
        }
    }

    [Fact]
    public async Task Create_WithoutOptions_UsesDefaultHookNamesAndDirectory()
    {
        await InitializeRepositoryAsync();

        var (exitCode, output) = await RunCliAsync("create");

        Assert.Equal(0, exitCode);
        Assert.Contains("Created 5 hook file(s)", output, StringComparison.Ordinal);
        var hooksDirectory = Path.Combine(_repositoryDirectory, ".githooks");
        foreach (var hookName in new[] { "pre-commit", "prepare-commit-msg", "commit-msg", "post-commit", "pre-push" })
        {
            Assert.True(File.Exists(Path.Combine(hooksDirectory, hookName)));
            Assert.True(File.Exists(Path.Combine(hooksDirectory, hookName + ".yaml")));
        }
    }

    [Fact]
    public async Task CreatedHook_IsInvokedByGit()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        await InitializeRepositoryAsync();
        var (createExitCode, createOutput) = await RunCliAsync(
            "create", "--scope", "local", "--hooks-path", ".githooks", "--hooks", "pre-commit");
        Assert.True(createExitCode == 0, createOutput);

        var hookPath = Path.Combine(_repositoryDirectory, ".githooks", "pre-commit");
        Assert.True(File.GetUnixFileMode(hookPath).HasFlag(UnixFileMode.UserExecute));

        // The generated script calls the installed tool. A temporary stand-in proves Git invokes it
        // without depending on a machine-wide installation or writing outside this repository.
        var binDirectory = Directory.CreateDirectory(Path.Combine(_repositoryDirectory, "bin"));
        var markerPath = Path.Combine(_repositoryDirectory, "hook-invoked");
        var toolPath = Path.Combine(binDirectory.FullName, "githooks");
        await File.WriteAllTextAsync(toolPath, "#!/bin/sh\nprintf '%s\\n' \"$*\" > \"" + markerPath + "\"\n", TestContext.Current.CancellationToken);
        File.SetUnixFileMode(toolPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);

        string[] commitArguments =
        [
            "-c", "core.hooksPath=.githooks", "-c", "user.name=Test", "-c", "user.email=test@example.invalid",
            "commit", "--allow-empty", "-m", "test"
        ];
        var (commitExitCode, commitOutput) = await RunGitAsync(commitArguments);
        Assert.True(commitExitCode == 0, commitOutput);
        Assert.Equal("run --file .githooks/pre-commit.yaml", (await File.ReadAllTextAsync(markerPath, TestContext.Current.CancellationToken)).Trim());
    }

    [Fact]
    public async Task Create_WithEmptyHookList_UsesDefaultHooks()
    {
        await InitializeRepositoryAsync();

        var (exitCode, output) = await RunCliAsync("create", "--hooks", ",");

        Assert.Equal(0, exitCode);
        Assert.Contains("Created 5 hook file(s)", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Create_WithUnknownHook_ReportsErrorWithoutWritingFiles()
    {
        await InitializeRepositoryAsync();

        var (exitCode, output) = await RunCliAsync(
            "create", "--scope", "local", "--hooks-path", ".githooks", "--hooks", "unknown-hook");

        Assert.Equal(1, exitCode);
        Assert.Contains("Unsupported hook name", output, StringComparison.Ordinal);
        Assert.False(Directory.Exists(Path.Combine(_repositoryDirectory, ".githooks")));
    }

    [Fact]
    public async Task InstallAndUninstall_LocalScope_HandlesExistingSettingAndForce()
    {
        await InitializeRepositoryAsync();

        var (installExitCode, installOutput) = await RunCliAsync(
            "install", "--scope", "local", "--hooks-path", ".githooks");
        Assert.Equal(0, installExitCode);
        Assert.Contains("successfully set", installOutput, StringComparison.Ordinal);
        Assert.Equal(".githooks", (await RunGitAsync("config", "--local", "--get", "core.hooksPath")).Output.Trim());

        var (repeatExitCode, repeatOutput) = await RunCliAsync(
            "install", "--scope", "local", "--hooks-path", ".githooks");
        Assert.Equal(0, repeatExitCode);
        Assert.Contains("already set", repeatOutput, StringComparison.Ordinal);

        var (conflictExitCode, conflictOutput) = await RunCliAsync(
            "install", "--scope", "local", "--hooks-path", "other-hooks");
        Assert.Equal(1, conflictExitCode);
        Assert.Contains("already set", conflictOutput, StringComparison.Ordinal);
        Assert.Equal(".githooks", (await RunGitAsync("config", "--local", "--get", "core.hooksPath")).Output.Trim());

        var (forceExitCode, _) = await RunCliAsync(
            "install", "--scope", "local", "--hooks-path", "other-hooks", "--force");
        Assert.Equal(0, forceExitCode);
        Assert.Equal("other-hooks", (await RunGitAsync("config", "--local", "--get", "core.hooksPath")).Output.Trim());

        var (uninstallExitCode, uninstallOutput) = await RunCliAsync("uninstall", "--scope", "local");
        Assert.Equal(0, uninstallExitCode);
        Assert.Contains("has been removed", uninstallOutput, StringComparison.Ordinal);
        Assert.Equal(1, (await RunGitAsync("config", "--local", "--get", "core.hooksPath")).ExitCode);

        var (repeatUninstallExitCode, repeatUninstallOutput) = await RunCliAsync("uninstall", "--scope", "local");
        Assert.Equal(0, repeatUninstallExitCode);
        Assert.Contains("Nothing to uninstall", repeatUninstallOutput, StringComparison.Ordinal);
    }

    public void Dispose()
    {
        if (Directory.Exists(_repositoryDirectory))
        {
            Directory.Delete(_repositoryDirectory, recursive: true);
        }
    }

    private async Task InitializeRepositoryAsync()
    {
        var (exitCode, _) = await RunGitAsync("init", "-q");
        Assert.Equal(0, exitCode);
    }

    private Task<(int ExitCode, string Output)> RunCliAsync(params string[] arguments)
    {
        return IntegrationProcess.RunAsync(_repositoryDirectory, "dotnet", [typeof(CreateCommand).Assembly.Location, .. arguments]);
    }

    private Task<(int ExitCode, string Output)> RunGitAsync(params string[] arguments)
    {
        return IntegrationProcess.RunAsync(_repositoryDirectory, "git", arguments);
    }
}
