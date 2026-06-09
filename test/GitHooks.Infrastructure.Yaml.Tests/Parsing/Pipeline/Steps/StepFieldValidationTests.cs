using GitHooks.Domain.Syntax;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;

using YamlDotNet.Core;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing.Pipeline.Steps;

public sealed class StepFieldValidationTests
{
    [Fact]
    public void ValidateScriptStep_ValidFields_DoesNotThrow()
    {
        var step = new StepFields
        {
            Script = TestParserFactory.CreateExpression(),
            DisplayName = TestParserFactory.CreateExpression(),
            Condition = TestParserFactory.CreateExpression(),
            TimeoutInMinutes = TestParserFactory.CreateExpression(),
            WorkingDirectory = TestParserFactory.CreateExpression()
        };

        step.Env["KEY"] = TestParserFactory.CreateExpression();

        StepFieldValidation.ValidateScriptStep(
            step,
            TestParserFactory.CreateDummyCursor());
    }

    [Theory]
    [InlineData(StepFieldNames.Template)]
    [InlineData(StepFieldNames.Parameters)]
    public void ValidateScriptStep_InvalidField_Throws(string field)
    {
        var step = new StepFields
        {
            Script = TestParserFactory.CreateExpression()
        };

        SetField(step, field);

        var exception =
            Assert.Throws<YamlException>(
                () => StepFieldValidation.ValidateScriptStep(
                    step,
                    TestParserFactory.CreateDummyCursor()));

        Assert.StartsWith(
            $"Script step contains invalid field(s): {field}",
            exception.Message);
    }

    [Fact]
    public void ValidateScriptStep_MultipleInvalidFields_Throws()
    {
        var step = new StepFields
        {
            Script = TestParserFactory.CreateExpression(),
            Template = TestParserFactory.CreateExpression()
        };

        step.Parameters["KEY"] = TestParserFactory.CreateExpression();

        var exception =
            Assert.Throws<YamlException>(
                () => StepFieldValidation.ValidateScriptStep(
                    step,
                    TestParserFactory.CreateDummyCursor()));

        Assert.StartsWith(
            "Script step contains invalid field(s): template, parameters",
            exception.Message);
    }

    [Fact]
    public void ValidateTemplateStep_ValidFields_DoesNotThrow()
    {
        var step = new StepFields
        {
            Template = TestParserFactory.CreateExpression()
        };

        step.Parameters["Configuration"] = TestParserFactory.CreateExpression();

        StepFieldValidation.ValidateTemplateStep(
            step,
            TestParserFactory.CreateDummyCursor());
    }

    [Theory]
    [InlineData(StepFieldNames.Script)]
    [InlineData(StepFieldNames.DisplayName)]
    [InlineData(StepFieldNames.Condition)]
    [InlineData(StepFieldNames.TimeoutInMinutes)]
    [InlineData(StepFieldNames.WorkingDirectory)]
    [InlineData(StepFieldNames.Env)]
    public void ValidateTemplateStep_InvalidField_Throws(string field)
    {
        var step = new StepFields
        {
            Template = TestParserFactory.CreateExpression(),
        };

        SetField(step, field);

        var exception =
            Assert.Throws<YamlException>(
                () => StepFieldValidation.ValidateTemplateStep(
                    step,
                    TestParserFactory.CreateDummyCursor()));

        Assert.StartsWith(
            $"Template step contains invalid field(s): {field}",
            exception.Message);
    }

    [Fact]
    public void ValidateTemplateStep_MultipleInvalidFields_Throws()
    {
        var step = new StepFields
        {
            Template = TestParserFactory.CreateExpression(),
            DisplayName = TestParserFactory.CreateExpression(),
        };

        step.Env["KEY"] = TestParserFactory.CreateExpression();

        var exception =
            Assert.Throws<YamlException>(
                () => StepFieldValidation.ValidateTemplateStep(
                    step,
                    TestParserFactory.CreateDummyCursor()));

        Assert.StartsWith(
            "Template step contains invalid field(s): displayName, env",
            exception.Message);
    }

    private static void SetField(
        StepFields step,
        string field)
    {
        switch (field)
        {
            case StepFieldNames.Template:
                step.Template = TestParserFactory.CreateExpression();
                break;

            case StepFieldNames.DisplayName:
                step.DisplayName = TestParserFactory.CreateExpression();
                break;

            case StepFieldNames.Condition:
                step.Condition = TestParserFactory.CreateExpression();
                break;

            case StepFieldNames.TimeoutInMinutes:
                step.TimeoutInMinutes = TestParserFactory.CreateExpression();
                break;

            case StepFieldNames.WorkingDirectory:
                step.WorkingDirectory = TestParserFactory.CreateExpression();
                break;

            case StepFieldNames.Env:
                step.Env["KEY"] = TestParserFactory.CreateExpression();
                break;

            case StepFieldNames.Parameters:
                step.Parameters["KEY"] = TestParserFactory.CreateExpression();
                break;

            case StepFieldNames.Script:
                step.Script = TestParserFactory.CreateExpression();
                break;

            default:
                throw new InvalidOperationException(
                    $"Test helper does not support field '{field}'.");
        }
    }
}
