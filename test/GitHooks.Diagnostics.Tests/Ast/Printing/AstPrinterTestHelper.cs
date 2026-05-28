using System.Runtime.CompilerServices;

using GitHooks.Diagnostics.Ast.Printing;
using GitHooks.Domain.Ast;
using GitHooks.Testing.Snapshots;

namespace GitHooks.Diagnostics.Tests.Ast.Printing;

internal static class AstPrinterTestHelper
{
    public static Task VerifyAstAsync(
        AstNode node,
        AstPrinterOptions? options = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentNullException.ThrowIfNull(node);

        var printer = new AstPrinter(options);

        var output = printer.Print(node);

        return SnapshotVerifier.VerifyAsync(
            output,
            memberName,
            sourceFilePath);
    }
}
