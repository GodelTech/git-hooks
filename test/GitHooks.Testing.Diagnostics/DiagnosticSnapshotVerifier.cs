using System.Runtime.CompilerServices;

using GitHooks.Diagnostics;
using GitHooks.Diagnostics.Printing;
using GitHooks.Testing.Snapshots;

namespace GitHooks.Testing.Diagnostics;

public static class DiagnosticSnapshotVerifier
{
    public static async Task VerifyAsync(
        IReadOnlyList<Diagnostic> diagnostics,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        var output = new DiagnosticPrinter().Print(diagnostics);

        await SnapshotVerifier.VerifyAsync(
            output,
            memberName,
            sourceFilePath);
    }
}
