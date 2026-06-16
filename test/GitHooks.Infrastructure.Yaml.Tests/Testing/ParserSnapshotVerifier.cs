using System.Runtime.CompilerServices;

using GitHooks.Domain.Ast;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Testing.Diagnostics;
using GitHooks.Testing.Diagnostics.Ast;

namespace GitHooks.Infrastructure.Yaml.Tests.Testing;

internal static class ParserSnapshotVerifier
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

        if (result.Root is null)
        {
            await DiagnosticSnapshotVerifier.VerifyAsync(
                result.Diagnostics,
                memberName,
                sourceFilePath);

            return;
        }

        Assert.Empty(result.Diagnostics);

        await VerifyAstAsync(
            result.Root,
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

        Assert.Empty(context.Diagnostics);

        await VerifyAstAsync(
            result,
            memberName,
            sourceFilePath);
    }

    public static async Task VerifyAstAsync(
        AstNode node,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        await AstSnapshotVerifier.VerifyAsync(
            node,
            memberName,
            sourceFilePath);
    }
}
