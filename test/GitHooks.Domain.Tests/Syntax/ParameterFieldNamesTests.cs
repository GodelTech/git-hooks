using GitHooks.Domain.Syntax;

namespace GitHooks.Domain.Tests.Syntax;

public sealed class ParameterFieldNamesTests
{
    [Fact]
    public void Constants_AreExpectedValues()
    {
        Assert.Equal("name", ParameterFieldNames.Name);
        Assert.Equal("displayName", ParameterFieldNames.DisplayName);
        Assert.Equal("type", ParameterFieldNames.Type);
        Assert.Equal("default", ParameterFieldNames.DefaultValue);
        Assert.Equal("values", ParameterFieldNames.Values);
    }

    [Fact]
    public void All_ContainsExpectedFields()
    {
        var expected = new[]
        {
            ParameterFieldNames.Name,
            ParameterFieldNames.DisplayName,
            ParameterFieldNames.Type,
            ParameterFieldNames.DefaultValue,
            ParameterFieldNames.Values
        };

        Assert.Equal(expected.Length, ParameterFieldNames.All.Count);

        foreach (var fieldName in expected)
        {
            Assert.Contains(fieldName, ParameterFieldNames.All);
        }
    }

    [Fact]
    public void All_IsCaseInsensitive()
    {
        Assert.Contains("NAME", ParameterFieldNames.All);
        Assert.Contains("DISPLAYNAME", ParameterFieldNames.All);
        Assert.Contains("TYPE", ParameterFieldNames.All);
        Assert.Contains("DEFAULT", ParameterFieldNames.All);
        Assert.Contains("VALUES", ParameterFieldNames.All);
    }
}
