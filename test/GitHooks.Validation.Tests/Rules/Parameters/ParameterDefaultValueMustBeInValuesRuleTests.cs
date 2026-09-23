using GitHooks.Diagnostics;
using GitHooks.Testing.Ast.Builders.Mappings.Parameters;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;
using GitHooks.Validation.Rules.Parameters;

namespace GitHooks.Validation.Tests.Rules.Parameters;

public sealed class ParameterDefaultValueMustBeInValuesRuleTests
{
    private readonly ParameterDefaultValueMustBeInValuesRule _rule = new();

    [Fact]
    public void Validate_ValuesIsNull_DoesNotReportDiagnostic()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .WithDefaultValue("debug")
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
            .WithValue("debug")
            .WithValue("release")
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_DefaultValueIsInValues_DoesNotReportDiagnostic()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .WithValue("debug")
            .WithValue("release")
            .WithDefaultValue("release")
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_DefaultValueIsNotInValues_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context = TestValidationContextFactory.Create(out var diagnostics);

        var defaultValueSpan = TestSourceSpan.Create(document, 1, 1, 1, 10);

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .WithValue("debug")
            .WithValue("release")
            .WithDefaultValue(x => x
                .WithKey("defaultValue")
                .WithStringValue(v => v
                    .WithValue("staging"))
                .WithSpan(defaultValueSpan))
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Single(
            diagnostics,
            DiagnosticDescriptors.ParameterDefaultValueMustBeInValues,
            defaultValueSpan,
            "configuration");
    }
}
