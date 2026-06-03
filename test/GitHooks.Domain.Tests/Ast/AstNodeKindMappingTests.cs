namespace GitHooks.Domain.Tests.Ast;

public sealed class AstNodeKindMappingTests
{
    [Fact]
    public void Nodes_HaveExpectedKind()
    {
        foreach (var (node, expectedKind, _) in TestAstFactory.NodeCases)
        {
            Assert.Equal(expectedKind, node.Kind);
        }
    }
}
