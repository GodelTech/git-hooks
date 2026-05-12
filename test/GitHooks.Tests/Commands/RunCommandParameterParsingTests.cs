using GitHooks.Commands;

namespace GitHooks.Tests.Commands;

public class RunCommandParameterParsingTests
{
    [Fact]
    public void TryParseParameterOverrides_RepeatableOptionEntriesProvided_ReturnsParsedValues()
    {
        var success = RunCommand.TryParseParameterOverrides(
            ["organization=godeltech", "vmImage=ubuntu-latest"],
            null,
            out var parameterOverrides,
            out var errorMessage
        );

        Assert.True(success);
        Assert.Equal(string.Empty, errorMessage);
        Assert.Equal("godeltech", parameterOverrides["organization"]);
        Assert.Equal("ubuntu-latest", parameterOverrides["vmImage"]);
    }

    [Fact]
    public void TryParseParameterOverrides_CommaSeparatedEntriesProvided_ReturnsParsedValues()
    {
        var success = RunCommand.TryParseParameterOverrides(
            [],
            "organization=godeltech,vmImage=ubuntu-latest",
            out var parameterOverrides,
            out var errorMessage
        );

        Assert.True(success);
        Assert.Equal(string.Empty, errorMessage);
        Assert.Equal("godeltech", parameterOverrides["organization"]);
        Assert.Equal("ubuntu-latest", parameterOverrides["vmImage"]);
    }

    [Fact]
    public void TryParseParameterOverrides_MixedSourcesWithDuplicateParameter_ReturnsFalse()
    {
        var success = RunCommand.TryParseParameterOverrides(
            ["organization=godeltech"],
            "organization=contoso",
            out _,
            out var errorMessage
        );

        Assert.False(success);
        Assert.Equal("Parameter 'organization' is provided more than once in command-line overrides.", errorMessage);
    }

    [Fact]
    public void TryParseParameterOverrides_EntryWithoutSeparator_ReturnsFalse()
    {
        var success = RunCommand.TryParseParameterOverrides(
            ["organization"],
            null,
            out _,
            out var errorMessage
        );

        Assert.False(success);
        Assert.Equal("Option '--parameter' entry 'organization' must use the name=value format.", errorMessage);
    }

    [Fact]
    public void TryParseParameterOverrides_EntryWithEmptyName_ReturnsFalse()
    {
        var success = RunCommand.TryParseParameterOverrides(
            ["=godeltech"],
            null,
            out _,
            out var errorMessage
        );

        Assert.False(success);
        Assert.Equal("Option '--parameter' entry '=godeltech' must specify a parameter name before '='.", errorMessage);
    }
}
