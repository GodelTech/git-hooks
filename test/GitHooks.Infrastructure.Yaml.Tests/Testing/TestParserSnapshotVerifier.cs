using System.Runtime.CompilerServices;

using GitHooks.Diagnostics.Ast.Printing;
using GitHooks.Diagnostics.Printing;
using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Testing.Snapshots;

namespace GitHooks.Infrastructure.Yaml.Tests.Testing;

internal static class TestParserSnapshotVerifier
{
    public static async Task VerifyAstAsync(
        string yaml,
        Func<string, SourceDocument, YamlParserResult> parseFunc,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);
        ArgumentNullException.ThrowIfNull(parseFunc);

        var result = parseFunc(yaml, new SourceDocument("test.yaml"));

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
        Func<ParsingContext, AstNode> parseFunc,
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
        Func<ParsingContext, AstNode> parseFunc,
        Action<ParsingContext>? beforeParse = null,
        Action<ParsingContext>? afterParse = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);
        ArgumentNullException.ThrowIfNull(parseFunc);

        var context = TestParserFactory.CreateContext(yaml);

        context.Cursor.StartDocument();

        beforeParse?.Invoke(context);

        var result = parseFunc(context);

        afterParse?.Invoke(context);

        context.Cursor.EndDocument();

        var output = new AstPrinter().Print(result);

        await SnapshotVerifier.VerifyAsync(
            output,
            memberName,
            sourceFilePath);
    }
}
