using GitHooks.Compilation.Binding;
using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Testing.Ast.Builders.Mappings;
using GitHooks.Testing.Ast.Builders.Mappings.Parameters;

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

        var declarations = new ParameterTable();
        declarations.AddParameter(
            new ParameterNodeBuilder()
                .WithName("serverName")
                .Build());

        var values = new ParameterValueTable();
        values.SetValue("serverName", "localhost");

        var diagnostics = new DiagnosticBag();

        var result = ParameterSubstitutionRewriter.Rewrite(
            pipeline,
            declarations,
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
    public void Rewrite_DeclaredParameterWithoutValue_ReportsDiagnostic()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithScriptStep(x => x
                .WithScript(s => s
                    .WithKey("script")
                    .WithInterpolatedStringValue(v => v
                        .WithParameterVariablePart("missingParam"))))
            .Build();

        var declarations = new ParameterTable();
        declarations.AddParameter(
            new ParameterNodeBuilder()
                .WithName("missingParam")
                .Build());

        var values = new ParameterValueTable();
        var diagnostics = new DiagnosticBag();

        ParameterSubstitutionRewriter.Rewrite(
            pipeline,
            declarations,
            values,
            diagnostics);

        var diagnostic = Assert.Single(diagnostics.Diagnostics);

        Assert.Equal(
            DiagnosticDescriptors.ParameterValueNotResolvable,
            diagnostic.Descriptor);
    }

    [Fact]
    public void Rewrite_UndeclaredParameter_DoesNotReportDiagnostic()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithScriptStep(x => x
                .WithScript(s => s
                    .WithKey("script")
                    .WithInterpolatedStringValue(v => v
                        .WithParameterVariablePart("doesNotExist"))))
            .Build();

        var declarations = new ParameterTable();
        var values = new ParameterValueTable();
        var diagnostics = new DiagnosticBag();

        ParameterSubstitutionRewriter.Rewrite(
            pipeline,
            declarations,
            values,
            diagnostics);

        Assert.Empty(diagnostics.Diagnostics);
    }

    [Fact]
    public void Rewrite_ScriptStepDisplayNameWithResolvedParameter_SubstitutesValue()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithScriptStep(x => x
                .WithScript("echo hi")
                .WithDisplayName(d => d
                    .WithKey("displayName")
                    .WithInterpolatedStringValue(v => v
                        .WithParameterVariablePart("stepName"))))
            .Build();

        var declarations = new ParameterTable();
        declarations.AddParameter(
            new ParameterNodeBuilder()
                .WithName("stepName")
                .Build());

        var values = new ParameterValueTable();
        values.SetValue("stepName", "Build");

        var diagnostics = new DiagnosticBag();

        var result = ParameterSubstitutionRewriter.Rewrite(
            pipeline,
            declarations,
            values,
            diagnostics);

        var script = Assert.IsType<ScriptStepNode>(
            result.Steps[0]);

        var literal = Assert.IsType<StringLiteralExpressionNode>(script.DisplayName!.Value);

        Assert.Equal(
            "Build",
            literal.Value);

        Assert.Empty(diagnostics.Diagnostics);
    }

    [Fact]
    public void Rewrite_ScriptStepEnvWithResolvedParameter_SubstitutesValue()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithScriptStep(x => x
                .WithScript("echo hi")
                .WithEnvironmentVariable(e => e
                    .WithKey("API_KEY")
                    .WithInterpolatedStringValue(v => v
                        .WithParameterVariablePart("apiKey"))))
            .Build();

        var declarations = new ParameterTable();
        declarations.AddParameter(
            new ParameterNodeBuilder()
                .WithName("apiKey")
                .Build());

        var values = new ParameterValueTable();
        values.SetValue("apiKey", "secret");

        var diagnostics = new DiagnosticBag();

        var result = ParameterSubstitutionRewriter.Rewrite(
            pipeline,
            declarations,
            values,
            diagnostics);

        var script = Assert.IsType<ScriptStepNode>(
            result.Steps[0]);

        var envField = Assert.Single(script.Env!.Fields);

        var literal = Assert.IsType<StringLiteralExpressionNode>(envField.Value);

        Assert.Equal(
            "secret",
            literal.Value);

        Assert.Empty(diagnostics.Diagnostics);
    }

    [Fact]
    public void Rewrite_TemplateStepParametersWithResolvedParameter_SubstitutesValue()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithTemplateStep(x => x
                .WithTemplate("shared.yaml")
                .WithParameter(p => p
                    .WithKey("configuration")
                    .WithInterpolatedStringValue(v => v
                        .WithParameterVariablePart("buildConfiguration"))))
            .Build();

        var declarations = new ParameterTable();
        declarations.AddParameter(
            new ParameterNodeBuilder()
                .WithName("buildConfiguration")
                .Build());

        var values = new ParameterValueTable();
        values.SetValue("buildConfiguration", "Release");

        var diagnostics = new DiagnosticBag();

        var result = ParameterSubstitutionRewriter.Rewrite(
            pipeline,
            declarations,
            values,
            diagnostics);

        var template = Assert.IsType<TemplateStepNode>(
            result.Steps[0]);

        var parameterField = Assert.Single(template.Parameters!.Fields);

        var literal = Assert.IsType<StringLiteralExpressionNode>(parameterField.Value);

        Assert.Equal(
            "Release",
            literal.Value);

        Assert.Empty(diagnostics.Diagnostics);
    }

    [Fact]
    public void Rewrite_NoParameterVariables_ReturnsSameInstance()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithScriptStep(x => x
                .WithScript("echo hi"))
            .Build();

        var declarations = new ParameterTable();
        var values = new ParameterValueTable();
        var diagnostics = new DiagnosticBag();

        var result = ParameterSubstitutionRewriter.Rewrite(
            pipeline,
            declarations,
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
                new ParameterTable(),
                new ParameterValueTable(),
                new DiagnosticBag()));
    }
}
