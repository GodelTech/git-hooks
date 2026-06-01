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
        Func<string, string, YamlParserResult> parseFunc,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);
        ArgumentNullException.ThrowIfNull(parseFunc);

        var result = parseFunc(yaml, "test.yaml");

        var output = result.Root is PipelineNode root
            ? new AstPrinter().Print(root)
            : new DiagnosticPrinter().Print(result.Diagnostics);

        await SnapshotVerifier.VerifyAsync(
            output,
            memberName,
            sourceFilePath);
    }

    public static async Task VerifyAstAsync(
        string yaml,
        Func<YamlParserCursor, AstNode> parseFunc,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        await VerifyAstAsync(
            yaml,
            parseFunc,
            beforeParse: null,
            afterParse: null,
            memberName,
            sourceFilePath);
    }

    public static async Task VerifyAstAsync(
        string yaml,
        Func<YamlParserCursor, AstNode> parseFunc,
        Action<YamlParserCursor>? beforeParse = null,
        Action<YamlParserCursor>? afterParse = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);
        ArgumentNullException.ThrowIfNull(parseFunc);

        var cursor = TestParserFactory.CreateCursor(yaml);

        cursor.StartDocument();

        beforeParse?.Invoke(cursor);

        var result = parseFunc(cursor);

        afterParse?.Invoke(cursor);

        cursor.EndDocument();

        var output = new AstPrinter().Print(result);

        await SnapshotVerifier.VerifyAsync(
            output,
            memberName,
            sourceFilePath);
    }
}
