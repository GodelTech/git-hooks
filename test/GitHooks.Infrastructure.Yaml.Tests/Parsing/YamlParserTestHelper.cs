using System.Runtime.CompilerServices;

using GitHooks.Diagnostics.Ast.Printing;
using GitHooks.Diagnostics.Printing;
using GitHooks.Domain.Ast.Mappings;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing;

internal static class YamlParserTestHelper
{
    private static readonly AstPrinter s_astPrinter = new();
    private static readonly DiagnosticPrinter s_diagnosticPrinter = new();

    public static async Task VerifyAstAsync(
        string yaml,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);

        var parser = TestParserFactory.Create();

        var result = parser.Parse(yaml, "test.yaml");

        var output = result.Root is PipelineNode root
            ? s_astPrinter.Print(root)
            : s_diagnosticPrinter.Print(result.Diagnostics);

        var testClass = Path.GetFileNameWithoutExtension(sourceFilePath);

        await Verify(output)
            .UseDirectory(
                Path.Combine(
                    "Snapshots",
                    testClass))
            .UseFileName(memberName);
    }
}
