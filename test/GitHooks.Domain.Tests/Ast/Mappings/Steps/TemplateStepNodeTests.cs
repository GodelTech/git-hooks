namespace GitHooks.Domain.Tests.Ast.Mappings.Steps;

public sealed class TemplateStepNodeTests
{
    [Fact]
    public void Parameters_DefaultsToEmpty()
    {
        var node = TestAstFactory.CreateTemplateStepNode();

        Assert.Empty(node.Parameters);
    }
}
