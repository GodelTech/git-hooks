using GitHooks.Diagnostics.Ast.Printing;
using GitHooks.Testing.Ast;

namespace GitHooks.Diagnostics.Tests.Ast.Printing;

public sealed class AstPrinterTests
{
    [Fact]
    public void Pipeline_ParametersAndStepsProvided_CreatesPipeline()
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
            ]
        );

        var printer = new AstPrinter();

        var result = printer.Print(pipeline);

        Assert.Equal(
            """
            Pipeline
              Parameter(configuration)
                String("Release")
              ScriptStep
                String("dotnet test")
            """,
            result
        );
    }

    [Fact]
    public void Print_StringContainsEscapedCharacters_ReturnsEscapedString()
    {
        var step = TestAst.Script(
            script: "echo \"Hello\"\nexit 0");

        var printer = new AstPrinter();

        var result = printer.Print(step);

        Assert.Equal(
            """
            ScriptStep
              String("echo \"Hello\"\nexit 0")
            """,
            result
        );
    }
}
