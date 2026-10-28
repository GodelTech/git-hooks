using System.Diagnostics;

namespace GitHooks.Tests.Commands;

internal static class IntegrationProcess
{
    public static async Task<(int ExitCode, string Output)> RunAsync(
        string workingDirectory,
        string executable,
        params string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        // Keep each test repository independent of the developer's Git settings.
        startInfo.Environment["GIT_CONFIG_GLOBAL"] = Path.Combine(workingDirectory, "global.gitconfig");
        startInfo.Environment["GIT_CONFIG_NOSYSTEM"] = "1";
        var binDirectory = Path.Combine(workingDirectory, "bin");
        if (Directory.Exists(binDirectory))
        {
            startInfo.Environment["PATH"] = binDirectory + Path.PathSeparator + startInfo.Environment["PATH"];
        }

        foreach (var name in new[]
        {
            "GIT_DIR", "GIT_WORK_TREE", "GIT_INDEX_FILE", "GIT_COMMON_DIR", "GIT_OBJECT_DIRECTORY",
            "GIT_ALTERNATE_OBJECT_DIRECTORIES", "GIT_TEMPLATE_DIR", "GIT_CONFIG_COUNT", "GIT_CONFIG_PARAMETERS"
        })
        {
            startInfo.Environment.Remove(name);
        }

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Failed to start {executable}.");
        try
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            return (process.ExitCode, await outputTask + await errorTask);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(CancellationToken.None);
            }
        }
    }
}
