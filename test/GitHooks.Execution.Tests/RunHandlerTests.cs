using GitHooks.Compilation;
using GitHooks.Compilation.DependencyInjection;
using GitHooks.Infrastructure.Cli;
using GitHooks.Infrastructure.Git;
using GitHooks.Infrastructure.Yaml.DependencyInjection;
using GitHooks.Validation.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console;

namespace GitHooks.Execution.Tests;

public sealed class RunHandlerTests : IDisposable
{
    private const string ParameterizedPipeline = """
        parameters:
          - name: os
            type: string
            default: ubuntu
            values:
              - windows
              - ubuntu

        steps:
          - script: echo ${{ parameters.os }}
        """;

    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public RunHandlerTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task HandleAsync_WithUnavailableGit_ReturnsCompilationFailed()
    {
        var git = new FakeGitCommandLine { IsAvailable = false };

        var (exitCode, output) = await RunAsync("pipeline.yaml", git);

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Contains("Git not found in PATH", output, StringComparison.Ordinal);
        AssertCompilerNotReached(output);
    }

    [Fact]
    public async Task HandleAsync_OutsideGitRepository_ReturnsCompilationFailed()
    {
        var git = new FakeGitCommandLine { IsInsideRepository = false };

        var (exitCode, output) = await RunAsync("pipeline.yaml", git);

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Contains("Not inside a git repository", output, StringComparison.Ordinal);
        AssertCompilerNotReached(output);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryRootCannotBeResolved_ReturnsCompilationFailed()
    {
        var git = new FakeGitCommandLine
        {
            RepositoryRootPathResult = CommandLineResult.FromProcess(string.Empty, "fatal: boom", 128),
        };

        var (exitCode, output) = await RunAsync("pipeline.yaml", git);

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Contains("Failed to resolve repository root path: fatal: boom", output, StringComparison.Ordinal);
        AssertCompilerNotReached(output);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryRootIsEmpty_ReturnsCompilationFailed()
    {
        var git = new FakeGitCommandLine
        {
            RepositoryRootPathResult = CommandLineResult.FromProcess("  ", string.Empty, 0),
        };

        var (exitCode, output) = await RunAsync("pipeline.yaml", git);

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Contains("Git returned an empty repository root path", output, StringComparison.Ordinal);
        AssertCompilerNotReached(output);
    }

    [Fact]
    public async Task HandleAsync_WithMissingFile_ReturnsCompilationFailed()
    {
        var (exitCode, output) = await RunAsync(Path.Combine(_tempDir, "missing.yaml"));

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Contains("YAML file not found", output, StringComparison.Ordinal);
        AssertCompilerNotReached(output);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  \n\t\n")]
    public async Task HandleAsync_WithEmptyFile_ReturnsCompilationFailed(string content)
    {
        var path = WriteTempFile("pipeline.yaml", content);

        var (exitCode, output) = await RunAsync(path);

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Contains("YAML file is empty", output, StringComparison.Ordinal);
        AssertCompilerNotReached(output);
    }

    [Fact]
    public async Task HandleAsync_WhenFileCannotBeRead_ReturnsCompilationFailed()
    {
        var path = WriteTempFile("pipeline.yaml", "steps:\n  - script: echo hi\n");

        using var exclusiveLock = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        var (exitCode, output) = await RunAsync(path);

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Contains("Failed to read YAML file", output, StringComparison.Ordinal);
        AssertCompilerNotReached(output);
    }

    [Fact]
    public async Task HandleAsync_WithValidPipeline_ReturnsSuccess()
    {
        var pipelineYaml = """
            steps:
              - script: echo hello
            """;

        var path = WriteTempFile("pipeline.yaml", pipelineYaml);

        var (exitCode, output) = await RunAsync(path);

        Assert.Equal(RunExitCode.Success, exitCode);
        Assert.Contains("[SUCCESS]", output, StringComparison.Ordinal);
        Assert.DoesNotContain("[ERROR]", output, StringComparison.Ordinal);
        Assert.DoesNotContain("[FAILED]", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithValidPipeline_ExecutesSubstitutedScripts()
    {
        var yaml = """
            parameters:
              - name: serverName
                type: string
                default: TEST-SERVER
            steps:
              - script: echo one
              - script: echo Hello ${{ parameters.serverName }}
            """;
        var path = WriteTempFile("pipeline.yaml", yaml);
        var runner = new FakeScriptStepRunner();

        var (exitCode, _) = await RunAsync(path, runner: runner);

        Assert.Equal(RunExitCode.Success, exitCode);
        Assert.Equal(["echo one", "echo Hello TEST-SERVER"], runner.Scripts);
    }

    [Fact]
    public async Task HandleAsync_WithParameterOverride_ExecutesOverriddenValue()
    {
        var yaml = """
            parameters:
              - name: serverName
                type: string
                default: TEST-SERVER
            steps:
              - script: echo Hello ${{ parameters.serverName }}
            """;
        var path = WriteTempFile("pipeline.yaml", yaml);
        var runner = new FakeScriptStepRunner();

        _ = await RunAsync(path, overrides: Overrides(("serverName", "some server name")), runner: runner);

        Assert.Equal(["echo Hello some server name"], runner.Scripts);
    }

    [Fact]
    public async Task HandleAsync_WithCompilationErrors_DoesNotExecuteScripts()
    {
        var path = WriteTempFile("pipeline.yaml", "steps: []");
        var runner = new FakeScriptStepRunner();

        var (exitCode, _) = await RunAsync(path, runner: runner);

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Empty(runner.Scripts);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(42)]
    public async Task HandleAsync_WhenStepFails_StopsAndReturnsStepFailed(int stepExitCode)
    {
        var yaml = """
            steps:
              - script: first
              - script: second
            """;
        var path = WriteTempFile("pipeline.yaml", yaml);
        var runner = new FakeScriptStepRunner { ExitCode = stepExitCode };

        var (exitCode, output) = await RunAsync(path, runner: runner);

        Assert.Equal(RunExitCode.StepFailed, exitCode);
        Assert.Equal(["first"], runner.Scripts);
        Assert.Contains("[FAILED]", output, StringComparison.Ordinal);
        Assert.Contains($"Step 1/2 exited with code {stepExitCode}.", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithDisplayName_PrintsItAsStepTitle()
    {
        var yaml = """
            steps:
              - script: echo body
                displayName: My Title
            """;
        var path = WriteTempFile("pipeline.yaml", yaml);

        var (_, output) = await RunAsync(path);

        Assert.Contains("My Title", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithUnsupportedStepField_WarnsAndStillRuns()
    {
        var yaml = """
            steps:
              - script: echo body
                condition: false
            """;
        var path = WriteTempFile("pipeline.yaml", yaml);
        var runner = new FakeScriptStepRunner();

        var (exitCode, output) = await RunAsync(path, runner: runner);

        Assert.Equal(RunExitCode.Success, exitCode);
        Assert.Contains("[WARNING]", output, StringComparison.Ordinal);
        Assert.Contains("condition", output, StringComparison.Ordinal);
        Assert.Single(runner.Scripts);
    }

    [Theory]
    [InlineData(typeof(IOException))]
    [InlineData(typeof(InvalidOperationException))]
    [InlineData(typeof(System.ComponentModel.Win32Exception))]
    [InlineData(typeof(UnauthorizedAccessException))]
    public async Task HandleAsync_WhenRunnerThrows_ReturnsExecutionError(Type exceptionType)
    {
        var yaml = """
            steps:
              - script: echo body
              - script: echo skipped
            """;
        var path = WriteTempFile("pipeline.yaml", yaml);

        var runner = new FakeScriptStepRunner { Exception = (Exception)Activator.CreateInstance(exceptionType, "boom")! };
        var (exitCode, output) = await RunAsync(path, runner: runner);

        Assert.Equal(RunExitCode.ExecutionError, exitCode);
        Assert.Equal(["echo body"], runner.Scripts);
        Assert.Contains("Step 1/2 could not be run: boom", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithMalformedYaml_ReturnsCompilationFailedAndReportsLocation()
    {
        var pipelineYaml = """
            steps:
              - script: test
                invalid: [
            """;

        var path = WriteTempFile("pipeline.yaml", pipelineYaml);

        var (exitCode, output) = await RunAsync(path);

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Contains("[ERROR]", output, StringComparison.Ordinal);
        Assert.Contains("Invalid YAML", output, StringComparison.Ordinal);
        Assert.Contains($"{path}:", output, StringComparison.Ordinal);
        Assert.Contains("[FAILED]", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithPipelineWithoutSteps_ReturnsCompilationFailedAndPrintsDiagnostic()
    {
        var path = WriteTempFile("pipeline.yaml", "steps: []\n");

        var (exitCode, output) = await RunAsync(path);

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Contains("[ERROR]", output, StringComparison.Ordinal);
        Assert.Contains("Compilation completed with errors.", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithParameterDefault_ReturnsSuccess()
    {
        var pipelineYaml = """
            parameters:
              - name: configuration
                type: string
                default: Release

            steps:
              - script: dotnet build -c ${{ parameters.configuration }}
            """;

        var path = WriteTempFile("pipeline.yaml", pipelineYaml);

        var (exitCode, output) = await RunAsync(path);

        Assert.Equal(RunExitCode.Success, exitCode);
        Assert.DoesNotContain("[ERROR]", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithDefaultOutsideAllowedValues_ReturnsCompilationFailed()
    {
        var pipelineYaml = """
            parameters:
              - name: os
                type: string
                default: macos
                values:
                  - windows
                  - ubuntu

            steps:
              - script: echo ${{ parameters.os }}
            """;

        var path = WriteTempFile("pipeline.yaml", pipelineYaml);

        var (exitCode, output) = await RunAsync(path);

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Contains("default value is not in the allowed values list", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithValidParameterOverride_ReturnsSuccess()
    {
        var path = WriteTempFile("pipeline.yaml", ParameterizedPipeline);

        var (exitCode, output) = await RunAsync(path, overrides: Overrides(("os", "windows")));

        Assert.Equal(RunExitCode.Success, exitCode);
        Assert.DoesNotContain("[ERROR]", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithOverrideOutsideAllowedValues_ReturnsCompilationFailed()
    {
        var path = WriteTempFile("pipeline.yaml", ParameterizedPipeline);

        var (exitCode, output) = await RunAsync(path, overrides: Overrides(("os", "macos")));

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Contains("override value is not in the allowed values list", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithOverrideForUndeclaredParameter_ReturnsCompilationFailed()
    {
        var path = WriteTempFile("pipeline.yaml", ParameterizedPipeline);

        var (exitCode, output) = await RunAsync(path, overrides: Overrides(("unknown", "value")));

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Contains("Parameter override 'unknown' does not match any declared parameter.", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithBlankOverrideName_ReturnsCompilationFailedWithoutThrowing()
    {
        var path = WriteTempFile("pipeline.yaml", ParameterizedPipeline);

        var (exitCode, output) = await RunAsync(path, overrides: Overrides((" ", "value")));

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Contains("[ERROR] Invalid compilation input", output, StringComparison.Ordinal);
        AssertCompilerNotReached(output);
    }

    [Fact]
    public async Task HandleAsync_WithUndeclaredParameterReference_ReturnsCompilationFailedAndReportsLocation()
    {
        var pipelineYaml = """
            steps:
              - script: echo ${{ parameters.doesNotExist }}
            """;

        var path = WriteTempFile("pipeline.yaml", pipelineYaml);

        var (exitCode, output) = await RunAsync(path);

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Contains("Parameter 'doesNotExist' referenced but not defined.", output, StringComparison.Ordinal);
        Assert.Contains($"{path}:2:", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithMissingTemplate_ReturnsCompilationFailedAndReportsPath()
    {
        var pipelineYaml = """
            steps:
              - template: missing-template.yaml
            """;

        var path = WriteTempFile("pipeline.yaml", pipelineYaml);

        var (exitCode, output) = await RunAsync(path);

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Contains("Template file not found", output, StringComparison.Ordinal);
        Assert.Contains("missing-template.yaml", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithTemplateNextToPipeline_ResolvesRelativeToPipelineFile()
    {
        var templateYaml = """
            steps:
              - script: echo from template
            """;

        _ = WriteTempFile("template.yaml", templateYaml);

        var pipelineYaml = """
            steps:
              - template: template.yaml
            """;

        var path = WriteTempFile("pipeline.yaml", pipelineYaml);

        var (exitCode, output) = await RunAsync(path);

        Assert.Equal(RunExitCode.Success, exitCode);
        Assert.DoesNotContain("[ERROR]", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithNestedTemplates_ReturnsSuccess()
    {
        var leafYaml = """
            steps:
              - script: echo leaf
            """;

        _ = WriteTempFile("leaf.yaml", leafYaml);

        var middleYaml = """
            steps:
              - template: leaf.yaml
              - script: echo middle
            """;

        _ = WriteTempFile("middle.yaml", middleYaml);

        var pipelineYaml = """
            steps:
              - template: middle.yaml
            """;

        var path = WriteTempFile("pipeline.yaml", pipelineYaml);

        var (exitCode, output) = await RunAsync(path);

        Assert.Equal(RunExitCode.Success, exitCode);
        Assert.DoesNotContain("[ERROR]", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithMissingNestedTemplate_ReturnsCompilationFailed()
    {
        var middleYaml = """
            steps:
              - template: leaf-missing.yaml
            """;

        _ = WriteTempFile("middle.yaml", middleYaml);

        var pipelineYaml = """
            steps:
              - template: middle.yaml
            """;

        var path = WriteTempFile("pipeline.yaml", pipelineYaml);

        var (exitCode, output) = await RunAsync(path);

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Contains("Template file not found", output, StringComparison.Ordinal);
        Assert.Contains("leaf-missing.yaml", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithTemplateIncludeCycle_ReturnsCompilationFailed()
    {
        var aYaml = """
            steps:
              - template: b.yaml
            """;

        _ = WriteTempFile("a.yaml", aYaml);

        var bYaml = """
            steps:
              - template: a.yaml
            """;

        _ = WriteTempFile("b.yaml", bYaml);

        var pipelineYaml = """
            steps:
              - template: a.yaml
            """;

        var path = WriteTempFile("pipeline.yaml", pipelineYaml);

        var (exitCode, output) = await RunAsync(path);

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Contains("Template include cycle detected", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithMultipleErrors_PrintsAllDiagnostics()
    {
        var pipelineYaml = """
            steps:
              - script: echo ${{ parameters.first }}
              - script: echo ${{ parameters.second }}
            """;

        var path = WriteTempFile("pipeline.yaml", pipelineYaml);

        var (exitCode, output) = await RunAsync(path);

        Assert.Equal(RunExitCode.CompilationFailed, exitCode);
        Assert.Contains("Parameter 'first' referenced but not defined.", output, StringComparison.Ordinal);
        Assert.Contains("Parameter 'second' referenced but not defined.", output, StringComparison.Ordinal);
        Assert.Contains("Compilation completed with errors.", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WithWarningOnly_PrintsWarningAndReturnsSuccess()
    {
        var pipelineYaml = """
            parameters:
              - name: configuration
                type: string
                default: Release
                displayName: ""

            steps:
              - script: echo ${{ parameters.configuration }}
            """;

        var path = WriteTempFile("pipeline.yaml", pipelineYaml);

        var (exitCode, output) = await RunAsync(path);

        Assert.Equal(RunExitCode.Success, exitCode);
        Assert.Contains("[WARNING]", output, StringComparison.Ordinal);
        Assert.Contains("[SUCCESS]", output, StringComparison.Ordinal);
        Assert.DoesNotContain("[ERROR]", output, StringComparison.Ordinal);
    }

    private static void AssertCompilerNotReached(string output)
    {
        Assert.DoesNotContain("[SUCCESS]", output, StringComparison.Ordinal);
        Assert.DoesNotContain("[FAILED]", output, StringComparison.Ordinal);
    }

    private static Dictionary<string, string> Overrides(params (string Name, string Value)[] values)
    {
        return values.ToDictionary(
            value => value.Name,
            value => value.Value,
            StringComparer.Ordinal);
    }

    private static PipelineCompiler CreateCompiler()
    {
        var services = new ServiceCollection();

        _ = services.AddYamlPipelineCompilation();
        _ = services.AddTargetPipelineCompilation();
        _ = services.AddPipelineValidation();

        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<PipelineCompiler>();
    }

    private static IAnsiConsole CreateConsole(StringWriter writer)
    {
        var console = AnsiConsole.Create(
            new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.No,
                ColorSystem = ColorSystemSupport.NoColors,
                Interactive = InteractionSupport.No,
                Out = new AnsiConsoleOutput(writer),
            });

        // Prevent line wrapping from splitting messages and long paths that tests assert on.
        console.Profile.Width = 1000;

        return console;
    }

    private static async Task<(int ExitCode, string Output)> RunAsync(
        string filePath,
        FakeGitCommandLine? git = null,
        IReadOnlyDictionary<string, string>? overrides = null,
        FakeScriptStepRunner? runner = null)
    {
        var writer = new StringWriter();
        var handler = new RunHandler(git ?? new FakeGitCommandLine(), CreateCompiler(), CreateConsole(writer), runner ?? new FakeScriptStepRunner());

        var exitCode = await handler.HandleAsync(
            filePath,
            overrides,
            TestContext.Current.CancellationToken);

        return (exitCode, writer.ToString());
    }

    private string WriteTempFile(string fileName, string content)
    {
        var filePath = Path.Combine(_tempDir, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }

    private sealed class FakeScriptStepRunner : IScriptStepRunner
    {
        public List<string> Scripts { get; } = [];

        public int ExitCode { get; init; }

        public Exception? Exception { get; init; }

        public Task<int> RunAsync(string script, string workingDirectory, CancellationToken cancellationToken = default)
        {
            Scripts.Add(script);

            if (Exception is not null)
            {
                throw Exception;
            }

            return Task.FromResult(ExitCode);
        }
    }

    private sealed class FakeGitCommandLine : IGitCommandLine
    {
        public bool IsAvailable { get; init; }
            = true;

        public bool IsInsideRepository { get; init; }
            = true;

        public CommandLineResult RepositoryRootPathResult { get; init; }
            = CommandLineResult.FromProcess("C:/repo", string.Empty, 0);

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
            return Task.FromResult(RepositoryRootPathResult);
        }

        public Task<CommandLineResult> GetCoreHooksPathAsync(GitConfigScope scope, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<CommandLineResult> SetCoreHooksPathAsync(GitConfigScope scope, string value, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<CommandLineResult> UnsetCoreHooksPathAsync(GitConfigScope scope, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
