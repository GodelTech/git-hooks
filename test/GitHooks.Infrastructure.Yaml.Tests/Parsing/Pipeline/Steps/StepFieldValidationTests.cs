using GitHooks.Diagnostics;
using GitHooks.Domain.Syntax;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;
using GitHooks.Testing.Ast.Builders.Fields;
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
            Script = StringKeyFieldNodeBuilder.Create("script"),
            DisplayName = StringKeyFieldNodeBuilder.Create("displayName"),
            Condition = StringKeyFieldNodeBuilder.Create("condition"),
            TimeoutInMinutes = StringKeyFieldNodeBuilder.Create("timeoutInMinutes"),
            WorkingDirectory = StringKeyFieldNodeBuilder.Create("workingDirectory"),
            Env = new MappingFieldNodeBuilder()
                .WithKey("env")
                .WithStringKeyField(x => x
                    .WithKey("KEY")
                    .WithValue("test"))
                .Build()
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
            Script = StringKeyFieldNodeBuilder.Create("script")
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
            Script = StringKeyFieldNodeBuilder.Create("script"),
            Template = StringKeyFieldNodeBuilder.Create("template"),
            Parameters = new MappingFieldNodeBuilder()
                .WithKey("parameters")
                .WithStringKeyField(x => x
                    .WithKey("Configuration")
                    .WithValue("test"))
                .Build()
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
            Template = StringKeyFieldNodeBuilder.Create("template"),
            Parameters = new MappingFieldNodeBuilder()
                .WithKey("parameters")
                .WithStringKeyField(x => x
                    .WithKey("Configuration")
                    .WithValue("test"))
                .Build()
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
            Template = StringKeyFieldNodeBuilder.Create("template")
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
            Template = StringKeyFieldNodeBuilder.Create("template"),
            DisplayName = StringKeyFieldNodeBuilder.Create("displayName"),
            Env = new MappingFieldNodeBuilder()
                .WithKey("env")
                .WithStringKeyField(x => x
                    .WithKey("KEY")
                    .WithValue("test"))
                .Build()
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
                step.Script = StringKeyFieldNodeBuilder.Create("script");
                break;

            case StepFieldNames.Template:
                step.Template = StringKeyFieldNodeBuilder.Create("template");
                break;

            case StepFieldNames.DisplayName:
                step.DisplayName = StringKeyFieldNodeBuilder.Create("displayName");
                break;

            case StepFieldNames.Condition:
                step.Condition = StringKeyFieldNodeBuilder.Create("condition");
                break;

            case StepFieldNames.TimeoutInMinutes:
                step.TimeoutInMinutes = StringKeyFieldNodeBuilder.Create("timeoutInMinutes");
                break;

            case StepFieldNames.WorkingDirectory:
                step.WorkingDirectory = StringKeyFieldNodeBuilder.Create("workingDirectory");
                break;

            case StepFieldNames.Env:
                step.Env = new MappingFieldNodeBuilder()
                    .WithKey("env")
                    .WithStringKeyField(x => x
                        .WithKey("KEY")
                        .WithValue("test"))
                    .Build();
                break;

            case StepFieldNames.Parameters:
                step.Parameters = new MappingFieldNodeBuilder()
                    .WithKey("parameters")
                    .WithStringKeyField(x => x
                        .WithKey("Configuration")
                        .WithValue("test"))
                    .Build();
                break;

            default:
                throw new InvalidOperationException(
                    $"Test helper does not support field '{field}'.");
        }
    }
}
