using GitHooks.Compilation.DependencyInjection;
using GitHooks.Diagnostics;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.DependencyInjection;
using GitHooks.Testing.Diagnostics;
using GitHooks.Validation.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Compilation.Tests;

/// <summary>
/// Verifies template loading and expansion behavior end-to-end through the
/// DI-wired compilation pipeline, using real files on disk.
/// </summary>
public sealed class TemplateExpansionIntegrationTests : IDisposable
{
    private readonly string _rootDirectory
        = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "gh-template-tests-" + Guid.NewGuid().ToString("N"))).FullName;

    public void Dispose()
    {
        if (Directory.Exists(_rootDirectory))
        {
            Directory.Delete(_rootDirectory, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Compile_LocalTemplate_InlinesStepsIntoRootPipeline()
    {
        var templateYaml = """
            steps:
              - script: dotnet build
                displayName: Build
            """;

        var templatePath = WriteFile("template.yaml", templateYaml);

        var rootYaml = $"""
            steps:
              - template: {Path.GetFileName(templatePath)}
            """;

        var rootPath = WriteFile("root.yaml", rootYaml);

        var compiler = CreateCompiler();

        var result = compiler.Compile(
            File.ReadAllText(rootPath),
            new SourceDocument(rootPath));

        DiagnosticAssert.Empty(result.Diagnostics);
        Assert.Single(result.Root.Steps);
    }

    [Fact]
    public void Compile_NestedTemplates_ExpandsRecursively()
    {
        var leafYaml = """
            steps:
              - script: echo leaf
            """;

        var leafPath = WriteFile("leaf.yaml", leafYaml);

        var middleYaml = $"""
            steps:
              - template: {Path.GetFileName(leafPath)}
              - script: echo middle
            """;

        var middlePath = WriteFile("middle.yaml", middleYaml);

        var rootYaml = $"""
            steps:
              - template: {Path.GetFileName(middlePath)}
            """;

        var rootPath = WriteFile("root.yaml", rootYaml);

        var compiler = CreateCompiler();

        var result = compiler.Compile(
            File.ReadAllText(rootPath),
            new SourceDocument(rootPath));

        DiagnosticAssert.Empty(result.Diagnostics);
        Assert.Equal(2, result.Root.Steps.Count);
    }

    [Fact]
    public void Compile_MissingTemplate_ReportsTemplateNotFoundDiagnostic()
    {
        var rootYaml = """
            steps:
              - template: does-not-exist.yaml
            """;

        var rootPath = WriteFile("root.yaml", rootYaml);

        var compiler = CreateCompiler();

        var result = compiler.Compile(
            File.ReadAllText(rootPath),
            new SourceDocument(rootPath));

        Assert.True(result.Diagnostics.HasErrors);
        Assert.Contains(
            result.Diagnostics.Diagnostics,
            diagnostic => diagnostic.Descriptor == DiagnosticDescriptors.TemplateNotFound);
    }

    [Fact]
    public void Compile_CircularTemplateReferences_ReportsCycleDiagnostic()
    {
        var aPath = Path.Combine(_rootDirectory, "a.yaml");
        var bPath = Path.Combine(_rootDirectory, "b.yaml");

        var aYaml = $"""
            steps:
              - template: {Path.GetFileName(bPath)}
            """;

        var bYaml = $"""
            steps:
              - template: {Path.GetFileName(aPath)}
            """;

        File.WriteAllText(aPath, aYaml);
        File.WriteAllText(bPath, bYaml);

        var compiler = CreateCompiler();

        var result = compiler.Compile(
            File.ReadAllText(aPath),
            new SourceDocument(aPath));

        Assert.True(result.Diagnostics.HasErrors);
        Assert.Contains(
            result.Diagnostics.Diagnostics,
            diagnostic => diagnostic.Descriptor == DiagnosticDescriptors.TemplateIncludeCycleDetected);
    }

    [Fact]
    public void Compile_RemoteTemplateReference_ReportsRemoteTemplatesNotSupportedDiagnostic()
    {
        var rootYaml = """
            steps:
              - template: repo@refs/heads/main/template.yaml
            """;

        var rootPath = WriteFile("root.yaml", rootYaml);

        var compiler = CreateCompiler();

        var result = compiler.Compile(
            File.ReadAllText(rootPath),
            new SourceDocument(rootPath));

        Assert.True(result.Diagnostics.HasErrors);
        Assert.Contains(
            result.Diagnostics.Diagnostics,
            diagnostic => diagnostic.Descriptor == DiagnosticDescriptors.RemoteTemplatesNotSupported);
    }

    private static PipelineCompiler CreateCompiler()
    {
        var services = new ServiceCollection();

        _ = services.AddYamlPipelineCompilation();
        _ = services.AddTargetPipelineCompilation();
        _ = services.AddPipelineValidation();

        return services
            .BuildServiceProvider()
            .GetRequiredService<PipelineCompiler>();
    }

    private string WriteFile(
        string fileName,
        string contents)
    {
        var path = Path.Combine(_rootDirectory, fileName);
        File.WriteAllText(path, contents);
        return path;
    }
}
