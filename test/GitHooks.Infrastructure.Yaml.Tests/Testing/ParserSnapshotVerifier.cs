using System.Runtime.CompilerServices;

using GitHooks.Compilation.Parsing;
using GitHooks.Diagnostics;
using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Testing.Common;
using GitHooks.Testing.Diagnostics;
using GitHooks.Testing.Diagnostics.Ast;

namespace GitHooks.Infrastructure.Yaml.Tests.Testing;

internal static class ParserSnapshotVerifier
{
    public static async Task VerifyAstAsync(
        string yaml,
        Func<ParsingContext, PipelineNode> parseFunc,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);
        ArgumentNullException.ThrowIfNull(parseFunc);

        var context = new ParsingContext(
            yaml,
            new SourceDocument("test.yaml"),
            new DiagnosticBag());

        var result = parseFunc(context);

        if (context.Diagnostics.Count > 0)
        {
            await DiagnosticSnapshotVerifier.VerifyAsync(
                context.Diagnostics,
                memberName,
                sourceFilePath);

            return;
        }

        await VerifyAstAsync(
            result,
            memberName,
            sourceFilePath);
    }

    public static async Task VerifyAstAsync(
        string yaml,
        Func<YamlParserContext, AstNode> parseFunc,
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
        Func<YamlParserContext, AstNode> parseFunc,
        Action<YamlParserContext>? beforeParse = null,
        Action<YamlParserContext>? afterParse = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);
        ArgumentNullException.ThrowIfNull(parseFunc);

        var context = TestYamlParserContextFactory.Create(
            yaml,
            TestSourceDocument.Default);

        context.Cursor.StartDocument();

        beforeParse?.Invoke(context);

        var result = parseFunc(context);

        afterParse?.Invoke(context);

        context.Cursor.EndDocument();

        Assert.Empty(context.Diagnostics.Diagnostics);

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
