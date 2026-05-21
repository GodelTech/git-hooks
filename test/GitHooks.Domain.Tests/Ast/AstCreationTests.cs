using GitHooks.Testing.Ast;

namespace GitHooks.Domain.Tests.Ast;

public sealed class AstCreationTests
{
    [Fact]
    public void Should_Create_Pipeline_Ast()
    {
        var pipeline = TestAst.Pipeline(
            parameters:
            [
                TestAst.Parameter(
                    name: "configuration",
                    value: "Release")
            ],
            steps:
            [
                TestAst.Script(
                    script: "dotnet test")
            ]);

        Assert.Single(pipeline.Parameters);
        Assert.Single(pipeline.Steps);
    }
}
