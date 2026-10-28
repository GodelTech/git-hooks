using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Testing.Common;

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
        var compiler = IntegrationCompilerFactory.Create();

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
        var compiler = IntegrationCompilerFactory.Create();

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
        var compiler = IntegrationCompilerFactory.Create();

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
    public void Compile_UndeclaredParameterReference_ReportsSingleDiagnostic()
    {
        var compiler = IntegrationCompilerFactory.Create();

        var yaml = """
            steps:
              - script: echo ${{ parameters.doesNotExist }}
            """;

        var result = compiler.Compile(
            yaml,
            TestSourceDocument.Default);

        var diagnostic = Assert.Single(result.Diagnostics.Diagnostics);

        Assert.Equal(
            DiagnosticDescriptors.ParameterVariableMustBeResolvable,
            diagnostic.Descriptor);
    }

    [Fact]
    public void Compile_InvalidOverrideValue_ReportsOverrideDiagnosticAndPreservesDefaultSubstitution()
    {
        var compiler = IntegrationCompilerFactory.Create();

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
        var compiler = IntegrationCompilerFactory.Create();

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
        Assert.Equal("true", firstLiteral.Value);

        var secondStep = Assert.IsType<Domain.Ast.Mappings.Steps.ScriptStepNode>(result.BoundRoot.Steps[1]);
        var secondLiteral = Assert.IsType<Domain.Ast.Expressions.StringLiteralExpressionNode>(secondStep.Script.Value);
        Assert.Equal("42", secondLiteral.Value);
    }

    [Fact]
    public void Compile_ParameterOverride_BindsInterpolatedScriptWithoutChangingRoot()
    {
        var compiler = IntegrationCompilerFactory.Create();
        var yaml = """
            parameters:
              - name: configuration
                type: string
                default: Release

            steps:
              - script: echo ${{ parameters.configuration }}
            """;

        var result = compiler.Compile(
            yaml,
            TestSourceDocument.Default,
            parameterOverrides: new Dictionary<string, string>
            {
                ["configuration"] = "Debug"
            });

        Assert.False(result.HasErrors);
        Assert.NotSame(result.Root, result.BoundRoot);

        var original = Assert.IsType<ScriptStepNode>(Assert.Single(result.Root.Steps));
        var expression = Assert.IsType<InterpolatedStringExpressionNode>(original.Script.Value);
        Assert.Contains(expression.Parts, part => part is ParameterVariableExpressionNode);

        var bound = Assert.IsType<ScriptStepNode>(Assert.Single(result.BoundRoot.Steps));
        var literal = Assert.IsType<StringLiteralExpressionNode>(bound.Script.Value);
        Assert.Equal("echo Debug", literal.Value);
    }

    [Fact]
    public void Compile_UndeclaredParameterReference_PreservesDiagnosticLocation()
    {
        var compiler = IntegrationCompilerFactory.Create();
        var document = TestSourceDocument.Default;
        var yaml = """
            steps:
              - script: echo ${{ parameters.missing }}
            """;

        var result = compiler.Compile(yaml, document);

        var diagnostic = Assert.Single(result.Diagnostics.Diagnostics);
        Assert.Equal(DiagnosticDescriptors.ParameterVariableMustBeResolvable, diagnostic.Descriptor);
        Assert.Equal(document, diagnostic.Span.Document);
        Assert.False(diagnostic.Span.HasUnknownPosition);
        Assert.Equal(2, diagnostic.Span.Start.Line);
    }

    [Fact]
    public void Compile_SeparateContexts_DoNotLeakDiagnosticsOrParameterValues()
    {
        var compiler = IntegrationCompilerFactory.Create();
        var yaml = """
            parameters:
              - name: configuration
                type: string
                default: Release

            steps:
              - script: ${{ parameters.configuration }}
            """;

        var first = compiler.Compile(
            yaml,
            TestSourceDocument.Default,
            parameterOverrides: new Dictionary<string, string>
            {
                ["configuration"] = "Debug",
                ["unknown"] = "value"
            });
        var second = compiler.Compile(yaml, TestSourceDocument.Default);

        Assert.True(first.HasErrors);
        Assert.False(second.HasErrors);
        Assert.Empty(second.Diagnostics.Diagnostics);

        var firstStep = Assert.IsType<ScriptStepNode>(Assert.Single(first.BoundRoot.Steps));
        var firstLiteral = Assert.IsType<StringLiteralExpressionNode>(firstStep.Script.Value);
        Assert.Equal("Debug", firstLiteral.Value);

        var secondStep = Assert.IsType<ScriptStepNode>(Assert.Single(second.BoundRoot.Steps));
        var secondLiteral = Assert.IsType<StringLiteralExpressionNode>(secondStep.Script.Value);
        Assert.Equal("Release", secondLiteral.Value);
    }
}
