using GitHooks.Commands;

namespace GitHooks.Tests.Commands;

public sealed class RunCommandIntegrationTests : IDisposable
{
    private readonly string _repositoryDirectory = Directory.CreateDirectory(
        Path.Combine(Path.GetTempPath(), "githooks-cli-tests-" + Guid.NewGuid().ToString("N"))).FullName;

    [Fact]
    public async Task Run_WithParameterOverride_ExecutesBoundScriptAndReturnsZero()
    {
        await InitializeRepositoryAsync();
        var pipelinePath = Path.Combine(_repositoryDirectory, "pipeline.yaml");
        var yaml = """
            parameters:
              - name: message
                type: string
                default: default

            steps:
              - script: echo hello-${{ parameters.message }}
            """;
        await File.WriteAllTextAsync(
            pipelinePath,
            yaml,
            TestContext.Current.CancellationToken);

        var (exitCode, output) = await RunCliAsync("run", "--file", pipelinePath, "--parameter", "message=overridden");

        Assert.Equal(0, exitCode);
        Assert.Contains("Compilation completed without errors.", output, StringComparison.Ordinal);
        Assert.Contains("hello-overridden", output, StringComparison.Ordinal);
        Assert.Contains("All steps completed.", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Run_WithMalformedYaml_ReportsErrorAndReturnsOne()
    {
        await InitializeRepositoryAsync();
        var pipelinePath = Path.Combine(_repositoryDirectory, "invalid.yaml");
        await File.WriteAllTextAsync(
            pipelinePath,
            "steps:\n  - script: [",
            TestContext.Current.CancellationToken);

        var (exitCode, output) = await RunCliAsync("run", "--file", pipelinePath);

        Assert.Equal(1, exitCode);
        Assert.Contains("[ERROR]", output, StringComparison.Ordinal);
        Assert.Contains("Compilation completed with errors.", output, StringComparison.Ordinal);
        Assert.DoesNotContain("All steps completed.", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Run_WithDuplicateParameterOption_RejectsArgumentsBeforeExecution()
    {
        var (exitCode, output) = await RunCliAsync(
            "run",
            "--file",
            "pipeline.yaml",
            "--parameter",
            "message=first",
            "--parameter",
            "message=second");

        Assert.Equal(1, exitCode);
        Assert.Contains("provided more than once", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Compilation completed", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Run_WithWhitespaceFileOption_RejectsArgumentsBeforeExecution()
    {
        var (exitCode, output) = await RunCliAsync("run", "--file", " ");

        Assert.Equal(1, exitCode);
        Assert.Contains("--file", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Compilation completed", output, StringComparison.Ordinal);
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
        var (exitCode, _) = await IntegrationProcess.RunAsync(_repositoryDirectory, "git", "init", "-q");
        Assert.Equal(0, exitCode);
    }

    private Task<(int ExitCode, string Output)> RunCliAsync(params string[] arguments)
    {
        return IntegrationProcess.RunAsync(_repositoryDirectory, "dotnet", [typeof(RunCommand).Assembly.Location, .. arguments]);
    }
}
