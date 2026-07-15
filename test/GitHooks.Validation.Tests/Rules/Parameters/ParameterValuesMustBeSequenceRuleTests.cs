using GitHooks.Diagnostics;
using GitHooks.Testing.Ast.Builders.Mappings.Parameters;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;
using GitHooks.Validation.Rules.Parameters;

namespace GitHooks.Validation.Tests.Rules.Parameters;

public sealed class ParameterValuesMustBeSequenceRuleTests
{
    private readonly ParameterValuesMustBeSequenceRule _rule = new();

    [Fact]
    public void Validate_ValuesIsNull_DoesNotReportDiagnostic()
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

    [Fact]
    public void Validate_ValuesHasItems_DoesNotReportDiagnostic()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .WithValue("option1")
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_ValuesIsEmptySequence_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context = TestValidationContextFactory.Create(out var diagnostics);

        var valuesSpan = TestSourceSpan.Create(document, 1, 1, 1, 1);

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .WithValues(x => x
                .WithSpan(valuesSpan))
            .Build();

        _rule.Validate(
            parameter,
            context);

        DiagnosticAssert.Single(
            diagnostics,
            DiagnosticDescriptors.ParameterValuesMustBeSequence,
            valuesSpan,
            "configuration");
    }
}
