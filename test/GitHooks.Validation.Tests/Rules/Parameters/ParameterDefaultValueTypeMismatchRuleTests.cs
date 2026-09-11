using GitHooks.Diagnostics;
using GitHooks.Testing.Ast.Builders.Mappings.Parameters;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;
using GitHooks.Validation.Rules.Parameters;

namespace GitHooks.Validation.Tests.Rules.Parameters;

public sealed class ParameterDefaultValueTypeMismatchRuleTests
{
    private readonly ParameterDefaultValueTypeMismatchRule _rule = new();

    [Fact]
    public void Validate_TypeIsNull_DoesNotReportDiagnostic()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .WithDefaultValue("not-a-bool")
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_DefaultValueIsNull_DoesNotReportDiagnostic()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .WithType("boolean")
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("boolean", "true")]
    [InlineData("boolean", "false")]
    [InlineData("number", "42")]
    [InlineData("number", "-1.5")]
    [InlineData("string", "anything")]
    public void Validate_DefaultValueMatchesType_DoesNotReportDiagnostic(
        string type,
        string defaultValue)
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .WithType(type)
            .WithDefaultValue(defaultValue)
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("boolean", "not-a-bool")]
    [InlineData("number", "not-a-number")]
    public void Validate_DefaultValueDoesNotMatchType_ReportsDiagnostic(
        string type,
        string defaultValue)
    {
        var document = TestSourceDocument.Default;

        var context = TestValidationContextFactory.Create(out var diagnostics);

        var defaultValueSpan = TestSourceSpan.Create(document, 1, 1, 1, 10);

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .WithType(type)
            .WithDefaultValue(x => x
                .WithKey("defaultValue")
                .WithStringValue(v => v
                    .WithValue(defaultValue))
                .WithSpan(defaultValueSpan))
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Single(
            diagnostics,
            DiagnosticDescriptors.ParameterDefaultValueTypeMismatch,
            defaultValueSpan,
            "configuration",
            type);
    }
}
