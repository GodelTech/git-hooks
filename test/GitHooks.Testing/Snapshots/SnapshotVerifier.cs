using System.Runtime.CompilerServices;

namespace GitHooks.Testing.Snapshots;

public static class SnapshotVerifier
{
    public static Task VerifyAsync(
        string output,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(output);

        var testDirectory = Path.GetDirectoryName(sourceFilePath);
        var testClass = Path.GetFileNameWithoutExtension(sourceFilePath);

        if (testDirectory == null)
        {
            throw new InvalidOperationException("Source file path is invalid.");
        }

        return Verify(output)
            .UseDirectory(
                Path.Combine(
                    testDirectory,
                    "Snapshots",
                    testClass))
            .UseFileName(
                memberName);
    }
}
