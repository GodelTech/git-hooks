using System.Runtime.CompilerServices;

using GitHooks.Diagnostics.Ast.Printing;
using GitHooks.Domain.Ast;
using GitHooks.Testing.Snapshots;

namespace GitHooks.Testing.Diagnostics.Ast;

public static class AstSnapshotVerifier
{
    public static async Task VerifyAsync(
        AstNode node,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentNullException.ThrowIfNull(node);

        var output = new AstPrinter().Print(node);

        await SnapshotVerifier.VerifyAsync(
            output,
            memberName,
            sourceFilePath);
    }
}
