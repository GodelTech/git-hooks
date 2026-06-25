using GitHooks.Diagnostics;
using GitHooks.Domain.Syntax;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;
using GitHooks.Testing.Ast;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Pipeline.Steps;

public sealed class StepFieldValidationTests
{
    [Fact]
    public void ValidateScriptStep_ValidFields_DoesNotReportDiagnostic()
    {
        var context = TestParsingContextFactory.CreateEmpty(TestSourceDocument.Default);

        var step = new StepFields
        {
            Script = TestFields.StringKey("script"),
            DisplayName = TestFields.StringKey("displayName"),
            Condition = TestFields.StringKey("condition"),
            TimeoutInMinutes = TestFields.StringKey("timeoutInMinutes"),
            WorkingDirectory = TestFields.StringKey("workingDirectory"),
            Env = TestFields.Mapping(
                "env",
                TestFields.StringKey("KEY"))
        };

        StepFieldValidation.ValidateScriptStep(
            step,
            context);

        DiagnosticAssert.Empty(context.Diagnostics);
    }

    [Theory]
    [InlineData(StepFieldNames.Template)]
    [InlineData(StepFieldNames.Parameters)]
    public void ValidateScriptStep_InvalidField_ReportsDiagnostic(
        string field)
    {
        var document = TestSourceDocument.Default;

        var context = TestParsingContextFactory.CreateEmpty(document);

        var step = new StepFields
        {
            Script = TestFields.StringKey("script")
        };

        SetField(step, field);

        StepFieldValidation.ValidateScriptStep(
            step,
            context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.InvalidStepField,
            TestSourceSpan.Create(document),
            field,
            "script");
    }

    [Fact]
    public void ValidateScriptStep_MultipleInvalidFields_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context = TestParsingContextFactory.CreateEmpty(document);

        var step = new StepFields
        {
            Script = TestFields.StringKey("script"),
            Template = TestFields.StringKey("template"),
            Parameters = TestFields.Mapping(
                "parameters",
                TestFields.StringKey("Configuration"))
        };

        StepFieldValidation.ValidateScriptStep(
            step,
            context);

        Assert.Equal(
            2,
            context.Diagnostics.Count);

        DiagnosticAssert.Matches(
            context.Diagnostics[0],
            DiagnosticDescriptors.InvalidStepField,
            TestSourceSpan.Create(document),
            "template",
            "script");

        DiagnosticAssert.Matches(
            context.Diagnostics[1],
            DiagnosticDescriptors.InvalidStepField,
            TestSourceSpan.Create(document),
            "parameters",
            "script");
    }

    [Fact]
    public void ValidateTemplateStep_ValidFields_DoesNotReportDiagnostic()
    {
        var context = TestParsingContextFactory.CreateEmpty(TestSourceDocument.Default);

        var step = new StepFields
        {
            Template = TestFields.StringKey("template"),
            Parameters = TestFields.Mapping(
                "parameters",
                TestFields.StringKey("Configuration"))
        };

        StepFieldValidation.ValidateTemplateStep(
            step,
            context);

        DiagnosticAssert.Empty(context.Diagnostics);
    }

    [Theory]
    [InlineData(StepFieldNames.Script)]
    [InlineData(StepFieldNames.DisplayName)]
    [InlineData(StepFieldNames.Condition)]
    [InlineData(StepFieldNames.TimeoutInMinutes)]
    [InlineData(StepFieldNames.WorkingDirectory)]
    [InlineData(StepFieldNames.Env)]
    public void ValidateTemplateStep_InvalidField_ReportsDiagnostic(
        string field)
    {
        var document = TestSourceDocument.Default;

        var context = TestParsingContextFactory.CreateEmpty(document);

        var step = new StepFields
        {
            Template = TestFields.StringKey("template")
        };

        SetField(step, field);

        StepFieldValidation.ValidateTemplateStep(
            step,
            context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.InvalidStepField,
            TestSourceSpan.Create(document),
            field,
            "template");
    }

    [Fact]
    public void ValidateTemplateStep_MultipleInvalidFields_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context = TestParsingContextFactory.CreateEmpty(document);

        var step = new StepFields
        {
            Template = TestFields.StringKey("template"),
            DisplayName = TestFields.StringKey("displayName"),
            Env = TestFields.Mapping(
                "env",
                TestFields.StringKey("KEY"))
        };

        StepFieldValidation.ValidateTemplateStep(
            step,
            context);

        Assert.Equal(
            2,
            context.Diagnostics.Count);

        DiagnosticAssert.Matches(
            context.Diagnostics[0],
            DiagnosticDescriptors.InvalidStepField,
            TestSourceSpan.Create(document),
            "displayName",
            "template");

        DiagnosticAssert.Matches(
            context.Diagnostics[1],
            DiagnosticDescriptors.InvalidStepField,
            TestSourceSpan.Create(document),
            "env",
            "template");
    }

    private static void SetField(
        StepFields step,
        string field)
    {
        switch (field)
        {
            case StepFieldNames.Script:
                step.Script = TestFields.StringKey("script");
                break;

            case StepFieldNames.Template:
                step.Template = TestFields.StringKey("template");
                break;

            case StepFieldNames.DisplayName:
                step.DisplayName = TestFields.StringKey("displayName");
                break;

            case StepFieldNames.Condition:
                step.Condition = TestFields.StringKey("condition");
                break;

            case StepFieldNames.TimeoutInMinutes:
                step.TimeoutInMinutes = TestFields.StringKey("timeoutInMinutes");
                break;

            case StepFieldNames.WorkingDirectory:
                step.WorkingDirectory = TestFields.StringKey("workingDirectory");
                break;

            case StepFieldNames.Env:
                step.Env = TestFields.Mapping(
                    "env",
                    TestFields.StringKey("KEY"));
                break;

            case StepFieldNames.Parameters:
                step.Parameters = TestFields.Mapping(
                    "parameters",
                    TestFields.StringKey("Configuration"));
                break;

            default:
                throw new InvalidOperationException(
                    $"Test helper does not support field '{field}'.");
        }
    }
}
