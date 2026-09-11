using GitHooks.Compilation.Binding;
using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Testing.Ast.Builders.Mappings;

namespace GitHooks.Compilation.Tests.Binding;

public sealed class ParameterSubstitutionRewriterTests
{
    [Fact]
    public void Rewrite_ScriptFieldWithResolvedParameter_SubstitutesValue()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithScriptStep(x => x
                .WithScript(s => s
                    .WithKey("script")
                    .WithInterpolatedStringValue(v => v
                        .WithParameterVariablePart("serverName"))))
            .Build();

        var values = new ParameterValueTable();
        values.SetValue("serverName", "localhost");

        var diagnostics = new DiagnosticBag();

        var result = ParameterSubstitutionRewriter.Rewrite(
            pipeline,
            values,
            diagnostics);

        var script = Assert.IsType<ScriptStepNode>(
            result.Steps[0]);

        var literal = Assert.IsType<StringLiteralExpressionNode>(script.Script.Value);

        Assert.Equal(
            "localhost",
            literal.Value);

        Assert.Empty(diagnostics.Diagnostics);
    }

    [Fact]
    public void Rewrite_UnresolvedParameter_ReportsDiagnostic()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithScriptStep(x => x
                .WithScript(s => s
                    .WithKey("script")
                    .WithInterpolatedStringValue(v => v
                        .WithParameterVariablePart("missingParam"))))
            .Build();

        var values = new ParameterValueTable();
        var diagnostics = new DiagnosticBag();

        ParameterSubstitutionRewriter.Rewrite(
            pipeline,
            values,
            diagnostics);

        var diagnostic = Assert.Single(diagnostics.Diagnostics);

        Assert.Equal(
            DiagnosticDescriptors.ParameterValueNotResolvable,
            diagnostic.Descriptor);
    }

    [Fact]
    public void Rewrite_NoParameterVariables_ReturnsSameInstance()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithScriptStep(x => x
                .WithScript("echo hi"))
            .Build();

        var values = new ParameterValueTable();
        var diagnostics = new DiagnosticBag();

        var result = ParameterSubstitutionRewriter.Rewrite(
            pipeline,
            values,
            diagnostics);

        Assert.Same(
            pipeline,
            result);
    }

    [Fact]
    public void Rewrite_NullRoot_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => ParameterSubstitutionRewriter.Rewrite(
                null!,
                new ParameterValueTable(),
                new DiagnosticBag()));
    }
}
