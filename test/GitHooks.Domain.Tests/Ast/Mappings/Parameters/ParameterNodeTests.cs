namespace GitHooks.Domain.Tests.Ast.Mappings.Parameters;

public sealed class ParameterNodeTests
{
    [Fact]
    public void Values_DefaultsToEmpty()
    {
        var node = TestAstFactory.CreateParameterNode();

        Assert.Empty(node.Values);
    }
}
