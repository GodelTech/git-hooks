using GitHooks.Domain.Syntax;

namespace GitHooks.Domain.Tests.Syntax;

public sealed class PipelineFieldNamesTests
{
    [Fact]
    public void Constants_AreExpectedValues()
    {
        Assert.Equal("parameters", PipelineFieldNames.Parameters);
        Assert.Equal("steps", PipelineFieldNames.Steps);
    }

    [Fact]
    public void All_ContainsExpectedFields()
    {
        var expected = new[]
        {
            PipelineFieldNames.Parameters,
            PipelineFieldNames.Steps
        };

        Assert.Equal(expected.Length, PipelineFieldNames.All.Count);

        foreach (var fieldName in expected)
        {
            Assert.Contains(fieldName, PipelineFieldNames.All);
        }
    }

    [Fact]
    public void All_IsCaseInsensitive()
    {
        Assert.Contains("PARAMETERS", PipelineFieldNames.All);
        Assert.Contains("STEPS", PipelineFieldNames.All);
    }
}
