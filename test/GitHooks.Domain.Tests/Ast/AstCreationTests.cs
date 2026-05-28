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
                    defaultValue: TestAst.StringLiteral(
                        "Release"))
            ],
            steps:
            [
                TestAst.ScriptStep(
                    script: "dotnet test")
            ]);

        Assert.Single(pipeline.Parameters);
        Assert.Single(pipeline.Steps);
    }
}
