using GitHooks.Commands;

namespace GitHooks.Tests.Commands;

public class RunCommandParameterParsingTests
{
    [Fact]
    public void TryParseParameterOverrides_RepeatableOptionEntriesProvided_ReturnsParsedValues()
    {
        var success = RunCommand.TryParseParameterOverrides(
            ["serverName=TEST-SERVER", "operatingSystem=ubuntu"],
            null,
            out var parameterOverrides,
            out var errorMessage
        );

        Assert.True(success);
        Assert.Equal(string.Empty, errorMessage);
        Assert.Equal("TEST-SERVER", parameterOverrides["serverName"]);
        Assert.Equal("ubuntu", parameterOverrides["operatingSystem"]);
    }

    [Fact]
    public void TryParseParameterOverrides_CommaSeparatedEntriesProvided_ReturnsParsedValues()
    {
        var success = RunCommand.TryParseParameterOverrides(
            [],
            "serverName=TEST-SERVER,operatingSystem=ubuntu",
            out var parameterOverrides,
            out var errorMessage
        );

        Assert.True(success);
        Assert.Equal(string.Empty, errorMessage);
        Assert.Equal("TEST-SERVER", parameterOverrides["serverName"]);
        Assert.Equal("ubuntu", parameterOverrides["operatingSystem"]);
    }

    [Fact]
    public void TryParseParameterOverrides_MixedSourcesWithDuplicateParameter_ReturnsFalse()
    {
        var success = RunCommand.TryParseParameterOverrides(
            ["serverName=TEST-SERVER"],
            "serverName=contoso",
            out _,
            out var errorMessage
        );

        Assert.False(success);
        Assert.Equal("Parameter 'serverName' is provided more than once in command-line overrides.", errorMessage);
    }

    [Fact]
    public void TryParseParameterOverrides_EntryWithoutSeparator_ReturnsFalse()
    {
        var success = RunCommand.TryParseParameterOverrides(
            ["serverName"],
            null,
            out _,
            out var errorMessage
        );

        Assert.False(success);
        Assert.Equal("Option '--parameter' entry 'serverName' must use the name=value format.", errorMessage);
    }

    [Fact]
    public void TryParseParameterOverrides_EntryWithEmptyName_ReturnsFalse()
    {
        var success = RunCommand.TryParseParameterOverrides(
            ["=contoso"],
            null,
            out _,
            out var errorMessage
        );

        Assert.False(success);
        Assert.Equal("Option '--parameter' entry '=contoso' must specify a parameter name before '='.", errorMessage);
    }
}
