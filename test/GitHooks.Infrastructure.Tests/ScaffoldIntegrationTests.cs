using GitHooks.Infrastructure.Cli;
using GitHooks.Infrastructure.Git;
using GitHooks.Infrastructure.Hooks;
using GitHooks.Infrastructure.Scaffold;
using GitHooks.Infrastructure.Scaffold.Templates;
using GitHooks.Templates;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Infrastructure.Tests;

public sealed class ScaffoldIntegrationTests : IDisposable
{
    private readonly string _root = Directory.CreateDirectory(
        Path.Combine(Path.GetTempPath(), "githooks-infrastructure-tests-" + Guid.NewGuid().ToString("N"))).FullName;

    [Fact]
    public async Task AddInfrastructure_ResolvesServicesAndCreatesScaffoldFromEmbeddedTemplates()
    {
        using var provider = CreateProvider();
        Assert.IsType<CommandLine>(provider.GetRequiredService<ICommandLine>());
        Assert.IsType<GitCommandLine>(provider.GetRequiredService<IGitCommandLine>());
        Assert.IsType<GitHookCatalog>(provider.GetRequiredService<IGitHookCatalog>());
        Assert.IsType<BashTemplateProvider>(provider.GetRequiredService<IBashTemplateProvider>());
        Assert.IsType<YamlTemplateProvider>(provider.GetRequiredService<IYamlTemplateProvider>());

        var scaffolder = provider.GetRequiredService<IGitHookScaffolder>();
        var hooks = provider.GetRequiredService<IGitHookCatalog>().ResolveHooks(["pre-commit", "pre-push"]);
        var result = await scaffolder.CreateScaffoldAsync(
            _root,
            ".githooks",
            hooks.SupportedHooks,
            overwrite: false,
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(2, result.CreatedHooks.Count);
        Assert.Equal(Path.Combine(_root, ".githooks"), result.ResolvedHooksPath);

        foreach (var hook in hooks.SupportedHooks)
        {
            var scriptPath = Path.Combine(result.ResolvedHooksPath, hook.Name);
            var script = await File.ReadAllTextAsync(scriptPath, TestContext.Current.CancellationToken);
            var yaml = await File.ReadAllTextAsync(Path.Combine(result.ResolvedHooksPath, hook.Name + ".yaml"), TestContext.Current.CancellationToken);
            Assert.Contains(hook.Name, script, StringComparison.Ordinal);
            Assert.Contains(hook.Name, yaml, StringComparison.Ordinal);
            Assert.Contains(".githooks", script, StringComparison.Ordinal);
            Assert.DoesNotContain("{{", script, StringComparison.Ordinal);
            Assert.DoesNotContain("{{", yaml, StringComparison.Ordinal);
            if (!OperatingSystem.IsWindows())
            {
                Assert.True(File.GetUnixFileMode(scriptPath).HasFlag(UnixFileMode.UserExecute));
            }
        }
    }

    [Fact]
    public async Task CreateScaffold_ExistingScriptWithoutOverwrite_PreservesExistingFile()
    {
        using var provider = CreateProvider();
        var hooks = provider.GetRequiredService<IGitHookCatalog>().ResolveHooks(["pre-commit"]).SupportedHooks;
        var directory = Directory.CreateDirectory(Path.Combine(_root, ".githooks"));
        var scriptPath = Path.Combine(directory.FullName, "pre-commit");
        await File.WriteAllTextAsync(scriptPath, "original", TestContext.Current.CancellationToken);

        var result = await provider.GetRequiredService<IGitHookScaffolder>()
            .CreateScaffoldAsync(_root, ".githooks", hooks, overwrite: false, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Contains("already exists", result.Error, StringComparison.Ordinal);
        Assert.Equal("original", await File.ReadAllTextAsync(scriptPath, TestContext.Current.CancellationToken));
        Assert.False(File.Exists(scriptPath + ".yaml"));
    }

    [Fact]
    public async Task CreateScaffold_ExistingYamlWithoutOverwrite_PreservesExistingFile()
    {
        using var provider = CreateProvider();
        var hooks = provider.GetRequiredService<IGitHookCatalog>().ResolveHooks(["pre-commit"]).SupportedHooks;
        var directory = Directory.CreateDirectory(Path.Combine(_root, ".githooks"));
        var yamlPath = Path.Combine(directory.FullName, "pre-commit.yaml");
        await File.WriteAllTextAsync(yamlPath, "original", TestContext.Current.CancellationToken);

        var result = await provider.GetRequiredService<IGitHookScaffolder>()
            .CreateScaffoldAsync(_root, ".githooks", hooks, overwrite: false, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal("original", await File.ReadAllTextAsync(yamlPath, TestContext.Current.CancellationToken));
        Assert.False(File.Exists(Path.Combine(directory.FullName, "pre-commit")));
    }

    [Fact]
    public async Task CreateScaffold_WithOverwrite_ReplacesBothFiles()
    {
        using var provider = CreateProvider();
        var hooks = provider.GetRequiredService<IGitHookCatalog>().ResolveHooks(["pre-commit"]).SupportedHooks;
        var directory = Directory.CreateDirectory(Path.Combine(_root, ".githooks"));
        var scriptPath = Path.Combine(directory.FullName, "pre-commit");
        var yamlPath = scriptPath + ".yaml";
        await File.WriteAllTextAsync(scriptPath, "old script", TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(yamlPath, "old yaml", TestContext.Current.CancellationToken);

        var result = await provider.GetRequiredService<IGitHookScaffolder>()
            .CreateScaffoldAsync(_root, ".githooks", hooks, overwrite: true, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Contains("pre-commit", await File.ReadAllTextAsync(scriptPath, TestContext.Current.CancellationToken), StringComparison.Ordinal);
        Assert.Contains("pre-commit", await File.ReadAllTextAsync(yamlPath, TestContext.Current.CancellationToken), StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateScaffold_WithEmptyPath_ReturnsFailureWithoutCreatingFiles()
    {
        using var provider = CreateProvider();
        var hooks = provider.GetRequiredService<IGitHookCatalog>().ResolveHooks(["pre-commit"]).SupportedHooks;

        var result = await provider.GetRequiredService<IGitHookScaffolder>()
            .CreateScaffoldAsync(_root, " ", hooks, overwrite: false, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Contains("cannot be empty", result.Error, StringComparison.Ordinal);
        Assert.Empty(result.CreatedHooks);
    }

    [Fact]
    public async Task CreateScaffold_WithCancellation_PropagatesCancellation()
    {
        using var provider = CreateProvider();
        var hooks = provider.GetRequiredService<IGitHookCatalog>().ResolveHooks(["pre-commit"]).SupportedHooks;
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            provider.GetRequiredService<IGitHookScaffolder>()
                .CreateScaffoldAsync(_root, ".githooks", hooks, overwrite: false, cancellation.Token));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        _ = services.AddTemplates();
        _ = services.AddInfrastructure();
        return services.BuildServiceProvider();
    }
}
