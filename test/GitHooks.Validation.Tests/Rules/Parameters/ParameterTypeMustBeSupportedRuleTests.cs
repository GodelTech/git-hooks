using GitHooks.Diagnostics;
using GitHooks.Testing.Ast.Builders.Mappings.Parameters;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;
using GitHooks.Validation.Rules.Parameters;

namespace GitHooks.Validation.Tests.Rules.Parameters;

public sealed class ParameterTypeMustBeSupportedRuleTests
{
    private readonly ParameterTypeMustBeSupportedRule _rule = new();

    [Fact]
    public void Validate_TypeIsNull_DoesNotReportDiagnostic()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("string")]
    [InlineData("boolean")]
    [InlineData("number")]
    [InlineData("BOOLEAN")]
    public void Validate_SupportedType_DoesNotReportDiagnostic(
        string type)
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .WithType(type)
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_UnsupportedType_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context = TestValidationContextFactory.Create(out var diagnostics);

        var typeSpan = TestSourceSpan.Create(document, 1, 1, 1, 10);

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .WithType(x => x
                .WithKey("type")
                .WithStringValue(v => v
                    .WithValue("array"))
                .WithSpan(typeSpan))
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Single(
            diagnostics,
            DiagnosticDescriptors.UnsupportedParameterType,
            typeSpan,
            "array");
    }
}
