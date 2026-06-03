namespace GitHooks.Domain.Tests.Ast.Mappings.Steps;

public sealed class ScriptStepNodeTests
{
    [Fact]
    public void Env_DefaultsToEmpty()
    {
        var node = TestAstFactory.CreateScriptStepNode();

        Assert.Empty(node.Env);
    }
}
