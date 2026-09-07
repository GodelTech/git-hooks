using GitHooks.Compilation.DependencyInjection;
using GitHooks.Diagnostics;
using GitHooks.Infrastructure.Yaml.DependencyInjection;
using GitHooks.Testing.Common;
using GitHooks.Validation.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Compilation.Tests;

/// <summary>
/// Verifies the full compilation flow (YAML -> Parser -> Domain AST -> Template Expansion
/// -> Validation) using the real, DI-wired implementations rather than mocks.
/// </summary>
public sealed class PipelineCompilerIntegrationTests
{
    [Fact]
    public void Compile_ValidPipeline_ProducesAstWithoutErrors()
    {
        var compiler = CreateCompiler();

        var yaml = """
            parameters:
              - name: configuration
                type: string
                default: Release

            steps:
              - script: dotnet restore

              - script: dotnet build
                displayName: Build
            """;

        var result = compiler.Compile(
            yaml,
            TestSourceDocument.Default);

        Assert.False(result.Diagnostics.HasErrors);
        Assert.Single(result.Root.Parameters);
        Assert.Equal(2, result.Root.Steps.Count);
    }

    [Fact]
    public void Compile_MalformedYaml_ReportsInvalidYamlDiagnostic()
    {
        var compiler = CreateCompiler();

        var yaml = """
            steps:
              - script: test
                invalid: [
            """;

        var result = compiler.Compile(
            yaml,
            TestSourceDocument.Default);

        Assert.True(result.Diagnostics.HasErrors);
        Assert.Contains(
            result.Diagnostics.Diagnostics,
            diagnostic => diagnostic.Descriptor == DiagnosticDescriptors.InvalidYaml);
        Assert.Empty(result.Root.Steps);
    }

    [Fact]
    public void Compile_PipelineWithoutSteps_ReportsValidationDiagnostic()
    {
        var compiler = CreateCompiler();

        var yaml = """
            parameters:
              - name: configuration
                type: string
                default: Release
            """;

        var result = compiler.Compile(
            yaml,
            TestSourceDocument.Default);

        Assert.True(result.Diagnostics.HasErrors);
        Assert.Contains(
            result.Diagnostics.Diagnostics,
            diagnostic => diagnostic.Descriptor == DiagnosticDescriptors.PipelineMustContainStep);
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
}
