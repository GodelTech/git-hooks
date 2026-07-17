using System.Runtime.CompilerServices;

using GitHooks.Diagnostics;
using GitHooks.Diagnostics.Printing;
using GitHooks.Testing.Snapshots;

namespace GitHooks.Testing.Diagnostics;

public static class DiagnosticSnapshotVerifier
{
    public static async Task VerifyAsync(
        DiagnosticBag diagnostics,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        var output = new DiagnosticPrinter().Print(diagnostics.Diagnostics);

        await SnapshotVerifier.VerifyAsync(
            output,
            memberName,
            sourceFilePath);
    }
}
