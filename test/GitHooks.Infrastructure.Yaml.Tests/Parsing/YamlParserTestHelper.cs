using System.Runtime.CompilerServices;

using GitHooks.Diagnostics.Ast.Printing;
using GitHooks.Domain.Ast.Mappings;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing;

internal static class YamlParserTestHelper
{
    public static async Task VerifyAst(
        string yaml,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);

        var parser = TestParserFactory.Create();

        var result = parser.Parse(yaml, "test.yaml");

        var root = Assert.IsType<PipelineNode>(result.Root);

        var output = new AstPrinter().Print(root);

        var testClass = Path.GetFileNameWithoutExtension(sourceFilePath);

        await Verify(output)
            .UseDirectory(
                Path.Combine(
                    "Snapshots",
                    testClass))
            .UseFileName(memberName);
    }
}
