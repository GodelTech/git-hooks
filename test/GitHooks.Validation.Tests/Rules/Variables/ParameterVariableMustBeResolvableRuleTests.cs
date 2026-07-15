using GitHooks.Diagnostics;
using GitHooks.Testing.Ast.Builders.Expressions;
using GitHooks.Testing.Ast.Builders.Mappings.Parameters;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;
using GitHooks.Validation.Rules.Variables;

namespace GitHooks.Validation.Tests.Rules.Variables;

public sealed class ParameterVariableMustBeResolvableRuleTests
{
    private readonly ParameterVariableMustBeResolvableRule _rule = new();

    [Fact]
    public void Validate_ReferencedParameterExists_DoesNotReportDiagnostic()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .Build();

        context.Symbols.AddParameter(parameter);

        var variable = new ParameterVariableExpressionNodeBuilder()
            .WithName("configuration")
            .Build();

        _rule.Validate(
            variable,
            context);

        DiagnosticAssert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_ReferencedParameterDoesNotExist_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context = TestValidationContextFactory.Create(out var diagnostics);

        var span = TestSourceSpan.Create(document, 1, 1, 1, 20);

        var variable = new ParameterVariableExpressionNodeBuilder()
            .WithName("missing")
            .WithSpan(span)
            .Build();

        _rule.Validate(
            variable,
            context);

        DiagnosticAssert.Single(
            diagnostics,
            DiagnosticDescriptors.ParameterVariableMustBeResolvable,
            span,
            "missing");
    }
}
