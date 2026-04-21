using System.ComponentModel;
using System.Diagnostics;

namespace GitHooks.CommandLine;

/// <summary>
/// Default implementation of <see cref="ICommandLineRunner"/> that runs external processes.
/// </summary>
public class CommandLineRunner : ICommandLineRunner
{
    /// <inheritdoc/>
    public async Task<CommandLineResult> RunAsync(string fileName, string arguments, CancellationToken cancellationToken = default)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        try
        {
            _ = process.Start();
            await process.WaitForExitAsync(cancellationToken);

            return CommandLineResult.FromProcess(process);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (
            ex is Win32Exception
            or InvalidOperationException
            or ObjectDisposedException
        )
        {
            return CommandLineResult.FromException(ex);
        }
    }
}
