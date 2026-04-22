using System.ComponentModel;
using System.Diagnostics;

namespace GitHooks.Infrastructure;

/// <summary>
/// Default implementation of <see cref="ICommandLine"/> that runs external processes.
/// </summary>
public sealed class CommandLine : ICommandLine
{
    /// <inheritdoc/>
    /// <remarks>
    /// Each element of <paramref name="arguments"/> is added to <see cref="ProcessStartInfo.ArgumentList"/>
    /// so the OS quotes values correctly and no shell interpretation occurs.
    /// Stdout and stderr are read concurrently with <see cref="Process.WaitForExitAsync"/> to
    /// prevent deadlocks when the process writes enough output to fill the pipe buffer.
    /// </remarks>
    public async Task<CommandLineResult> RunAsync(string fileName, IEnumerable<string> arguments, CancellationToken cancellationToken = default)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        foreach (var arg in arguments)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        try
        {
            _ = process.Start();

            // Read both streams concurrently before awaiting exit to avoid deadlock
            // when the process fills the pipe buffer before terminating.
            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            return CommandLineResult.FromProcess(stdout, stderr, process.ExitCode);
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
