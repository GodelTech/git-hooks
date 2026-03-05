using System.Diagnostics;

namespace GitHooks.CommandLines;

public abstract class CommandLineBase
{
    protected async Task<CommandLineResult> RunAsync(string fileName, string arguments, CancellationToken cancellationToken = default)
    {
        var process = new Process
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
            process.Start();
            await process.WaitForExitAsync(cancellationToken);

            return CommandLineResult.FromProcess(process);
        }
        catch (Exception ex)
        {
            return CommandLineResult.FromException(ex);
        }
    }
}
