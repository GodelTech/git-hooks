using System.Runtime.CompilerServices;

using GitHooks.Diagnostics.Ast.Printing;
using GitHooks.Diagnostics.Printing;
using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Testing.Snapshots;

namespace GitHooks.Infrastructure.Yaml.Tests.Testing;

internal static class TestParserSnapshotVerifier
{
    public static async Task VerifyAstAsync(
        string yaml,
        Func<YamlParserCursor, AstNode> parseFunc,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);

        var cursor = TestParserFactory.CreateCursor(yaml);

        cursor.StartDocument();

        var result = parseFunc(cursor);

        cursor.EndDocument();

        var output = new AstPrinter().Print(result);

        await SnapshotVerifier.VerifyAsync(
            output,
            memberName,
            sourceFilePath);
    }

    public static async Task VerifyAstAsync(
        string yaml,
        Func<string, string, YamlParserResult> parseFunc,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);

        var result = parseFunc(yaml, "test.yaml");

        var output = result.Root is PipelineNode root
            ? new AstPrinter().Print(root)
            : new DiagnosticPrinter().Print(result.Diagnostics);

        await SnapshotVerifier.VerifyAsync(
            output,
            memberName,
            sourceFilePath);
    }
}
