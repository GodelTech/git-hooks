using GitHooks.Domain.Syntax;

namespace GitHooks.Domain.Tests.Syntax;

public sealed class StepFieldNamesTests
{
    [Fact]
    public void Constants_AreExpectedValues()
    {
        Assert.Equal("script", StepFieldNames.Script);
        Assert.Equal("template", StepFieldNames.Template);
        Assert.Equal("displayName", StepFieldNames.DisplayName);
        Assert.Equal("condition", StepFieldNames.Condition);
        Assert.Equal("timeoutInMinutes", StepFieldNames.TimeoutInMinutes);
        Assert.Equal("workingDirectory", StepFieldNames.WorkingDirectory);
        Assert.Equal("env", StepFieldNames.Env);
        Assert.Equal("parameters", StepFieldNames.Parameters);
    }

    [Fact]
    public void All_ContainsExpectedFields()
    {
        var expected = new[]
        {
            StepFieldNames.Script,
            StepFieldNames.Template,
            StepFieldNames.DisplayName,
            StepFieldNames.Condition,
            StepFieldNames.TimeoutInMinutes,
            StepFieldNames.WorkingDirectory,
            StepFieldNames.Env,
            StepFieldNames.Parameters
        };

        Assert.Equal(expected.Length, StepFieldNames.All.Count);

        foreach (var fieldName in expected)
        {
            Assert.Contains(fieldName, StepFieldNames.All);
        }
    }

    [Fact]
    public void ScriptStepFields_ContainsExpectedFields()
    {
        var expected = new[]
        {
            StepFieldNames.Script,
            StepFieldNames.DisplayName,
            StepFieldNames.Condition,
            StepFieldNames.TimeoutInMinutes,
            StepFieldNames.WorkingDirectory,
            StepFieldNames.Env
        };

        Assert.Equal(expected.Length, StepFieldNames.ScriptStepFields.Count);

        foreach (var fieldName in expected)
        {
            Assert.Contains(fieldName, StepFieldNames.ScriptStepFields);
        }
    }

    [Fact]
    public void TemplateStepFields_ContainsExpectedFields()
    {
        var expected = new[]
        {
            StepFieldNames.Template,
            StepFieldNames.Parameters
        };

        Assert.Equal(expected.Length, StepFieldNames.TemplateStepFields.Count);

        foreach (var fieldName in expected)
        {
            Assert.Contains(fieldName, StepFieldNames.TemplateStepFields);
        }
    }

    [Fact]
    public void All_IsCaseInsensitive()
    {
        Assert.Contains("SCRIPT", StepFieldNames.All);
        Assert.Contains("TEMPLATE", StepFieldNames.All);
        Assert.Contains("DISPLAYNAME", StepFieldNames.All);
        Assert.Contains("CONDITION", StepFieldNames.All);
        Assert.Contains("TIMEOUTINMINUTES", StepFieldNames.All);
        Assert.Contains("WORKINGDIRECTORY", StepFieldNames.All);
        Assert.Contains("ENV", StepFieldNames.All);
        Assert.Contains("PARAMETERS", StepFieldNames.All);
    }

    [Fact]
    public void ScriptStepFields_IsCaseInsensitive()
    {
        Assert.Contains("SCRIPT", StepFieldNames.ScriptStepFields);
        Assert.Contains("DISPLAYNAME", StepFieldNames.ScriptStepFields);
        Assert.Contains("CONDITION", StepFieldNames.ScriptStepFields);
        Assert.Contains("TIMEOUTINMINUTES", StepFieldNames.ScriptStepFields);
        Assert.Contains("WORKINGDIRECTORY", StepFieldNames.ScriptStepFields);
        Assert.Contains("ENV", StepFieldNames.ScriptStepFields);
    }

    [Fact]
    public void TemplateStepFields_IsCaseInsensitive()
    {
        Assert.Contains("TEMPLATE", StepFieldNames.TemplateStepFields);
        Assert.Contains("PARAMETERS", StepFieldNames.TemplateStepFields);
    }
}
