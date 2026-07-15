using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Testing.Ast.Builders.Mappings.Parameters;
using GitHooks.Validation.Extensions;

namespace GitHooks.Validation.Tests.Extensions;

public sealed class ParameterNodeValidationExtensionsTests
{
    [Fact]
    public void GetNameForDiagnostic_NullNode_Throws()
    {
        ParameterNode? parameter = null;

        Assert.Throws<ArgumentNullException>(
            () => parameter!.GetNameForDiagnostic());
    }

    [Fact]
    public void GetNameForDiagnostic_ParameterWithName_ReturnsName()
    {
        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .Build();

        var result = parameter.GetNameForDiagnostic();

        Assert.Equal(
            "configuration",
            result);
    }

    [Fact]
    public void GetNameForDiagnostic_ParameterWithoutName_ReturnsUnknown()
    {
        var parameter = new ParameterNodeBuilder()
            .WithName(string.Empty)
            .Build();

        var result = parameter.GetNameForDiagnostic();

        Assert.Equal(
            "<unknown>",
            result);
    }
}
