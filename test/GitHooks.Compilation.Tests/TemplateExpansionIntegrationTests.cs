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

    [Fact]
    public void Compile_TemplateWithLiteralParameterOverride_SubstitutesOverrideValue()
    {
        var templateYaml = """
            parameters:
              - name: configuration
                type: string
                default: Release

            steps:
              - script: ${{ parameters.configuration }}
            """;

        var templatePath = WriteFile("template.yaml", templateYaml);

        var rootYaml = $"""
            steps:
              - template: {Path.GetFileName(templatePath)}
                parameters:
                  configuration: Debug
            """;

        var rootPath = WriteFile("root.yaml", rootYaml);

        var compiler = CreateCompiler();

        var result = compiler.Compile(
            File.ReadAllText(rootPath),
            new SourceDocument(rootPath));

        DiagnosticAssert.Empty(result.Diagnostics);

        var step = Assert.IsType<Domain.Ast.Mappings.Steps.ScriptStepNode>(result.BoundRoot.Steps[0]);
        var literal = Assert.IsType<Domain.Ast.Expressions.StringLiteralExpressionNode>(step.Script.Value);

        Assert.Equal("Debug", literal.Value);
    }

    [Fact]
    public void Compile_TemplateWithoutParameterOverride_UsesTemplateDefaultValue()
    {
        var templateYaml = """
            parameters:
              - name: configuration
                type: string
                default: Release

            steps:
              - script: ${{ parameters.configuration }}
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

        var step = Assert.IsType<Domain.Ast.Mappings.Steps.ScriptStepNode>(result.BoundRoot.Steps[0]);
        var literal = Assert.IsType<Domain.Ast.Expressions.StringLiteralExpressionNode>(step.Script.Value);

        Assert.Equal("Release", literal.Value);
    }

    [Fact]
    public void Compile_TemplateParameterOverrideForwardedFromRootParameter_ResolvesAgainstRootScope()
    {
        var templateYaml = """
            parameters:
              - name: configuration
                type: string
                default: Release

            steps:
              - script: ${{ parameters.configuration }}
            """;

        var templatePath = WriteFile("template.yaml", templateYaml);

        var rootYaml = """
            parameters:
              - name: rootConfiguration
                type: string
                default: Debug

            steps:
              - template: {0}
                parameters:
                  configuration: ${{ parameters.rootConfiguration }}
            """;

        rootYaml = rootYaml.Replace("{0}", Path.GetFileName(templatePath));

        var rootPath = WriteFile("root.yaml", rootYaml);

        var compiler = CreateCompiler();

        var result = compiler.Compile(
            File.ReadAllText(rootPath),
            new SourceDocument(rootPath));

        DiagnosticAssert.Empty(result.Diagnostics);

        var step = Assert.IsType<Domain.Ast.Mappings.Steps.ScriptStepNode>(result.BoundRoot.Steps[0]);
        var literal = Assert.IsType<Domain.Ast.Expressions.StringLiteralExpressionNode>(step.Script.Value);

        Assert.Equal("Debug", literal.Value);
    }

    [Fact]
    public void Compile_NestedTemplatesWithDistinctParameters_BindEachTemplateAgainstItsOwnScope()
    {
        var leafYaml = """
            parameters:
              - name: leafValue
                type: string
                default: leaf-default

            steps:
              - script: ${{ parameters.leafValue }}
            """;

        var leafPath = WriteFile("leaf.yaml", leafYaml);

        var middleYaml = """
            parameters:
              - name: middleValue
                type: string
                default: middle-default

            steps:
              - template: {0}
                parameters:
                  leafValue: from-middle
              - script: ${{ parameters.middleValue }}
            """;

        middleYaml = middleYaml.Replace("{0}", Path.GetFileName(leafPath));

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
        Assert.Equal(2, result.BoundRoot.Steps.Count);

        var leafStep = Assert.IsType<Domain.Ast.Mappings.Steps.ScriptStepNode>(result.BoundRoot.Steps[0]);
        var leafLiteral = Assert.IsType<Domain.Ast.Expressions.StringLiteralExpressionNode>(leafStep.Script.Value);
        Assert.Equal("from-middle", leafLiteral.Value);

        var middleStep = Assert.IsType<Domain.Ast.Mappings.Steps.ScriptStepNode>(result.BoundRoot.Steps[1]);
        var middleLiteral = Assert.IsType<Domain.Ast.Expressions.StringLiteralExpressionNode>(middleStep.Script.Value);
        Assert.Equal("middle-default", middleLiteral.Value);
    }

    [Fact]
    public void Compile_TemplateParameterOverrideNotDeclaredInTemplate_ReportsOverrideNotDeclaredDiagnostic()
    {
        var templateYaml = """
            steps:
              - script: echo template
            """;

        var templatePath = WriteFile("template.yaml", templateYaml);

        var rootYaml = $"""
            steps:
              - template: {Path.GetFileName(templatePath)}
                parameters:
                  unused: value
            """;

        var rootPath = WriteFile("root.yaml", rootYaml);

        var compiler = CreateCompiler();

        var result = compiler.Compile(
            File.ReadAllText(rootPath),
            new SourceDocument(rootPath));

        Assert.True(result.Diagnostics.HasErrors);
        Assert.Contains(
            result.Diagnostics.Diagnostics,
            diagnostic => diagnostic.Descriptor == DiagnosticDescriptors.ParameterOverrideNotDeclared);
        Assert.Single(result.BoundRoot.Steps);
    }

    [Fact]
    public void Compile_TemplateParameterOverrideReferencesUndeclaredParentParameter_ReportsDiagnostic()
    {
        var templateYaml = """
            parameters:
              - name: configuration
                type: string
                default: Release

            steps:
              - script: ${{ parameters.configuration }}
            """;

        var templatePath = WriteFile("template.yaml", templateYaml);

        var rootYaml = """
            steps:
              - template: {0}
                parameters:
                  configuration: ${{ parameters.missing }}
            """;

        rootYaml = rootYaml.Replace("{0}", Path.GetFileName(templatePath));

        var rootPath = WriteFile("root.yaml", rootYaml);

        var compiler = CreateCompiler();

        var result = compiler.Compile(
            File.ReadAllText(rootPath),
            new SourceDocument(rootPath));

        Assert.True(result.Diagnostics.HasErrors);
        Assert.Contains(
            result.Diagnostics.Diagnostics,
            diagnostic => diagnostic.Descriptor == DiagnosticDescriptors.ParameterVariableMustBeResolvable);

        // The override must not be silently accepted as an empty string; the template's own
        // default should remain in effect since the override could not be resolved.
        var step = Assert.IsType<Domain.Ast.Mappings.Steps.ScriptStepNode>(result.BoundRoot.Steps[0]);
        var literal = Assert.IsType<Domain.Ast.Expressions.StringLiteralExpressionNode>(step.Script.Value);

        Assert.Equal("Release", literal.Value);
    }

    [Fact]
    public void Compile_TemplateStepReferencesUndeclaredParameterInOwnBody_ReportsDiagnostic()
    {
        var templateYaml = """
            steps:
              - script: ${{ parameters.doesNotExist }}
            """;

        var templatePath = WriteFile("template.yaml", templateYaml);

        var rootYaml = """
            steps:
              - template: {0}
            """;

        rootYaml = rootYaml.Replace("{0}", Path.GetFileName(templatePath));

        var rootPath = WriteFile("root.yaml", rootYaml);

        var compiler = CreateCompiler();

        var result = compiler.Compile(
            File.ReadAllText(rootPath),
            new SourceDocument(rootPath));

        Assert.True(result.Diagnostics.HasErrors);
        Assert.Contains(
            result.Diagnostics.Diagnostics,
            diagnostic => diagnostic.Descriptor == DiagnosticDescriptors.ParameterVariableMustBeResolvable);
    }

    [Fact]
    public void Compile_CycleThroughNestedTemplates_PreservesIncludeChainOnDiagnostic()
    {
        var rootPath = Path.Combine(_rootDirectory, "root.yaml");
        var middlePath = Path.Combine(_rootDirectory, "middle.yaml");
        var leafPath = Path.Combine(_rootDirectory, "leaf.yaml");

        var rootYaml = $"""
            steps:
              - template: {Path.GetFileName(middlePath)}
            """;

        var middleYaml = $"""
            steps:
              - template: {Path.GetFileName(leafPath)}
            """;

        var leafYaml = $"""
            steps:
              - template: {Path.GetFileName(middlePath)}
            """;

        File.WriteAllText(rootPath, rootYaml);
        File.WriteAllText(middlePath, middleYaml);
        File.WriteAllText(leafPath, leafYaml);

        var compiler = CreateCompiler();

        var result = compiler.Compile(
            File.ReadAllText(rootPath),
            new SourceDocument(rootPath));

        Assert.True(result.Diagnostics.HasErrors);

        var cycleDiagnostic = Assert.Single(
            result.Diagnostics.Diagnostics,
            diagnostic => diagnostic.Descriptor == DiagnosticDescriptors.TemplateIncludeCycleDetected);

        Assert.NotEmpty(cycleDiagnostic.RelatedLocations);
        Assert.Collection(
            cycleDiagnostic.RelatedLocations,
            location => Assert.Contains(Path.GetFileName(rootPath), location.Message, StringComparison.Ordinal),
            location => Assert.Contains(Path.GetFileName(middlePath), location.Message, StringComparison.Ordinal),
            location => Assert.Contains(Path.GetFileName(leafPath), location.Message, StringComparison.Ordinal));
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
