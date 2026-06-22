using System.Runtime.CompilerServices;

using GitHooks.Diagnostics.Printing;
using GitHooks.Testing.Snapshots;

namespace GitHooks.Diagnostics.Tests.Printing;

internal static class DiagnosticPrinterTestHelper
{
    public static Task VerifyDiagnosticAsync(
        IReadOnlyList<Diagnostic> diagnostics,
        DiagnosticPrinterOptions? options = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        var printer = new DiagnosticPrinter(options);

        var output = printer.Print(diagnostics);

        return SnapshotVerifier.VerifyAsync(
            output,
            memberName,
            sourceFilePath);
    }
}
