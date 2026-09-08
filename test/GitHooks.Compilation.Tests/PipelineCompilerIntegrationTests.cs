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

    [Fact]
    public void Compile_InvalidOverrideValue_ReportsOverrideDiagnosticAndPreservesDefaultSubstitution()
    {
        var compiler = CreateCompiler();

        var yaml = """
            parameters:
              - name: operatingSystem
                type: string
                default: ubuntu
                values:
                  - windows
                  - ubuntu

            steps:
              - script: ${{ parameters.operatingSystem }}
            """;

        var result = compiler.Compile(
            yaml,
            TestSourceDocument.Default,
            parameterOverrides: new Dictionary<string, string>
            {
                ["operatingSystem"] = "solaris"
            });

        Assert.Contains(
            result.Diagnostics.Diagnostics,
            diagnostic => diagnostic.Descriptor == DiagnosticDescriptors.ParameterOverrideValueMustBeInValues);

        var step = Assert.IsType<Domain.Ast.Mappings.Steps.ScriptStepNode>(result.BoundRoot.Steps[0]);
        var literal = Assert.IsType<Domain.Ast.Expressions.StringLiteralExpressionNode>(step.Script.Value);

        Assert.Equal("ubuntu", literal.Value);
    }

    [Fact]
    public void Compile_BooleanAndIntegerDefaults_AreSubstitutedIntoBoundRoot()
    {
        var compiler = CreateCompiler();

        var yaml = """
            parameters:
              - name: enableDebug
                type: boolean
                default: true

              - name: retries
                type: number
                default: 42

            steps:
              - script: ${{ parameters.enableDebug }}
              - script: ${{ parameters.retries }}
            """;

        var result = compiler.Compile(
            yaml,
            TestSourceDocument.Default);

        Assert.False(result.Diagnostics.HasErrors);

        var firstStep = Assert.IsType<Domain.Ast.Mappings.Steps.ScriptStepNode>(result.BoundRoot.Steps[0]);
        var firstLiteral = Assert.IsType<Domain.Ast.Expressions.StringLiteralExpressionNode>(firstStep.Script.Value);
        Assert.Equal("True", firstLiteral.Value);

        var secondStep = Assert.IsType<Domain.Ast.Mappings.Steps.ScriptStepNode>(result.BoundRoot.Steps[1]);
        var secondLiteral = Assert.IsType<Domain.Ast.Expressions.StringLiteralExpressionNode>(secondStep.Script.Value);
        Assert.Equal("42", secondLiteral.Value);
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
