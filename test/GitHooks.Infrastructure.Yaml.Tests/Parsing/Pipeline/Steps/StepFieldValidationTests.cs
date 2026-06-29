using GitHooks.Diagnostics;
using GitHooks.Domain.Common;
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

        var fieldSpan = TestSourceSpan.Create(document, 1, 1, 1, 10);

        SetField(step, field, fieldSpan);

        StepFieldValidation.ValidateScriptStep(
            step,
            context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.InvalidStepField,
            fieldSpan,
            field,
            "script");
    }

    [Fact]
    public void ValidateScriptStep_MultipleInvalidFields_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context = TestParsingContextFactory.CreateEmpty(document);

        var templateSpan = TestSourceSpan.Create(document, 1, 1, 1, 10);
        var parametersSpan = TestSourceSpan.Create(document, 2, 1, 2, 20);

        var step = new StepFields
        {
            Script = StringKeyFieldNodeBuilder.Create("script"),
            Template = new StringKeyFieldNodeBuilder()
                .WithKey("template")
                .WithValue("template.yml")
                .WithSpan(templateSpan)
                .Build(),
            Parameters = new MappingFieldNodeBuilder()
                .WithKey("parameters")
                .WithStringKeyField(x => x
                    .WithKey("Configuration")
                    .WithValue("test"))
                .WithSpan(parametersSpan)
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
            templateSpan,
            "template",
            "script");

        DiagnosticAssert.Matches(
            context.Diagnostics[1],
            DiagnosticDescriptors.InvalidStepField,
            parametersSpan,
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

        var fieldSpan = TestSourceSpan.Create(document, 1, 1, 1, 10);

        SetField(step, field, fieldSpan);

        StepFieldValidation.ValidateTemplateStep(
            step,
            context);

        DiagnosticAssert.Single(
            context.Diagnostics,
            DiagnosticDescriptors.InvalidStepField,
            fieldSpan,
            field,
            "template");
    }

    [Fact]
    public void ValidateTemplateStep_MultipleInvalidFields_ReportsDiagnostic()
    {
        var document = TestSourceDocument.Default;

        var context = TestParsingContextFactory.CreateEmpty(document);

        var displayNameSpan = TestSourceSpan.Create(document, 1, 1, 1, 10);
        var envSpan = TestSourceSpan.Create(document, 2, 1, 2, 20);

        var step = new StepFields
        {
            Template = StringKeyFieldNodeBuilder.Create("template"),
            DisplayName = new StringKeyFieldNodeBuilder()
                .WithKey("displayName")
                .WithValue("Test Display Name")
                .WithSpan(displayNameSpan)
                .Build(),
            Env = new MappingFieldNodeBuilder()
                .WithKey("env")
                .WithStringKeyField(x => x
                    .WithKey("KEY")
                    .WithValue("test"))
                .WithSpan(envSpan)
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
            displayNameSpan,
            "displayName",
            "template");

        DiagnosticAssert.Matches(
            context.Diagnostics[1],
            DiagnosticDescriptors.InvalidStepField,
            envSpan,
            "env",
            "template");
    }

    private static void SetField(
        StepFields step,
        string field,
        SourceSpan span)
    {
        switch (field)
        {
            case StepFieldNames.Script:
                step.Script = new StringKeyFieldNodeBuilder()
                    .WithKey("script")
                    .WithValue("echo Hello World")
                    .WithSpan(span)
                    .Build();
                break;

            case StepFieldNames.Template:
                step.Template = new StringKeyFieldNodeBuilder()
                    .WithKey("template")
                    .WithValue("template.yml")
                    .WithSpan(span)
                    .Build();
                break;

            case StepFieldNames.DisplayName:
                step.DisplayName = new StringKeyFieldNodeBuilder()
                    .WithKey("displayName")
                    .WithValue("Test Display Name")
                    .WithSpan(span)
                    .Build();
                break;

            case StepFieldNames.Condition:
                step.Condition = new StringKeyFieldNodeBuilder()
                    .WithKey("condition")
                    .WithValue("succeeded()")
                    .WithSpan(span)
                    .Build();
                break;

            case StepFieldNames.TimeoutInMinutes:
                step.TimeoutInMinutes = new StringKeyFieldNodeBuilder()
                    .WithKey("timeoutInMinutes")
                    .WithValue("30")
                    .WithSpan(span)
                    .Build();
                break;

            case StepFieldNames.WorkingDirectory:
                step.WorkingDirectory = new StringKeyFieldNodeBuilder()
                    .WithKey("workingDirectory")
                    .WithValue("src/")
                    .WithSpan(span)
                    .Build();
                break;

            case StepFieldNames.Env:
                step.Env = new MappingFieldNodeBuilder()
                    .WithKey("env")
                    .WithStringKeyField(x => x
                        .WithKey("KEY")
                        .WithValue("test"))
                    .WithSpan(span)
                    .Build();
                break;

            case StepFieldNames.Parameters:
                step.Parameters = new MappingFieldNodeBuilder()
                    .WithKey("parameters")
                    .WithStringKeyField(x => x
                        .WithKey("Configuration")
                        .WithValue("test"))
                    .WithSpan(span)
                    .Build();
                break;

            default:
                throw new InvalidOperationException(
                    $"Test helper does not support field '{field}'.");
        }
    }
}
