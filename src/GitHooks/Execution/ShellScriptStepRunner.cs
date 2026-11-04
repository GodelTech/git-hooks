using System.Diagnostics;
using System.Text;

using Spectre.Console;

namespace GitHooks.Execution;

/// <summary>
/// Runs script text through PowerShell Core (<c>pwsh</c>) on every platform.
/// </summary>
public sealed class ShellScriptStepRunner(
    IAnsiConsole console)
    : IScriptStepRunner
{
    private static readonly Lazy<string> s_encodedPowerShellLauncher = new(LoadEncodedPowerShellLauncher);

    private readonly IAnsiConsole _console = console;
    private readonly Lock _consoleLock = new();

    /// <inheritdoc/>
    public async Task<int> RunAsync(
        string script,
        string workingDirectory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(script);

        var startInfo = new ProcessStartInfo
        {
            FileName = "pwsh",
            WorkingDirectory = workingDirectory,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardInputEncoding = new UTF8Encoding(false),
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-NonInteractive");
        startInfo.ArgumentList.Add("-OutputFormat");
        startInfo.ArgumentList.Add("Text");
        startInfo.ArgumentList.Add("-EncodedCommand");
        startInfo.ArgumentList.Add(s_encodedPowerShellLauncher.Value);

        using var process = new Process { StartInfo = startInfo };

        cancellationToken.ThrowIfCancellationRequested();
        _ = process.Start();

        try
        {
            await Task.WhenAll(
                WriteScriptAsync(process.StandardInput, script, cancellationToken),
                PumpAsync(process.StandardOutput, cancellationToken),
                PumpAsync(process.StandardError, cancellationToken),
                process.WaitForExitAsync(cancellationToken));
        }
        catch (OperationCanceledException)
        {
            await KillAsync(process);
            throw;
        }

        return process.ExitCode;
    }

    private static string LoadEncodedPowerShellLauncher()
    {
        using var stream = typeof(ShellScriptStepRunner).Assembly.GetManifestResourceStream(
            typeof(ShellScriptStepRunner),
            "PowerShellLauncher.ps1")
            ?? throw new InvalidOperationException("Embedded PowerShell launcher was not found.");
        using var reader = new StreamReader(stream);

        return Convert.ToBase64String(Encoding.Unicode.GetBytes(reader.ReadToEnd()));
    }

    private static async Task WriteScriptAsync(
        StreamWriter writer,
        string script,
        CancellationToken cancellationToken)
    {
        try
        {
            await writer.WriteAsync(script.AsMemory(), cancellationToken);
        }
        finally
        {
            writer.Close();
        }
    }

    private static async Task KillAsync(
        Process process)
    {
        try
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync(CancellationToken.None);
        }
        catch (InvalidOperationException)
        {
            // The process had already exited.
        }
    }

    private async Task PumpAsync(
        StreamReader reader,
        CancellationToken cancellationToken)
    {
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            lock (_consoleLock)
            {
                _console.WriteLine(line);
            }
        }
    }
}
