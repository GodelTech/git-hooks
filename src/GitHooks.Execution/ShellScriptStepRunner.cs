using System.Diagnostics;
using System.Globalization;
using System.Text;

using Spectre.Console;

namespace GitHooks.Execution;

/// <summary>
/// Runs script text through the platform shell (<c>cmd.exe</c> on Windows, <c>sh</c> elsewhere).
/// </summary>
public sealed class ShellScriptStepRunner(
    IAnsiConsole console)
    : IScriptStepRunner
{
    private readonly IAnsiConsole _console = console;

    /// <inheritdoc/>
    public async Task<int> RunAsync(
        string script,
        string workingDirectory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(script);

        var isWindows = OperatingSystem.IsWindows();
        var scriptPath = Path.Combine(Path.GetTempPath(), $"githooks-{Guid.NewGuid():N}{(isWindows ? ".cmd" : ".sh")}");

        await File.WriteAllTextAsync(
            scriptPath,
            isWindows ? BuildBatchScript(script) : BuildShellScript(script),
            new UTF8Encoding(false),
            cancellationToken);

        try
        {
            var outputEncoding = GetOutputEncoding(isWindows);

            var startInfo = new ProcessStartInfo
            {
                FileName = isWindows ? "cmd.exe" : "sh",
                WorkingDirectory = workingDirectory,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = outputEncoding,
                StandardErrorEncoding = outputEncoding,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            if (isWindows)
            {
                startInfo.ArgumentList.Add("/d");
                startInfo.ArgumentList.Add("/c");
            }

            startInfo.ArgumentList.Add(scriptPath);

            using var process = new Process { StartInfo = startInfo };

            _ = process.Start();

            process.StandardInput.Close();

            try
            {
                await Task.WhenAll(
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
        finally
        {
            TryDelete(scriptPath);
        }
    }

    // cmd.exe batch parsing is unreliable with LF-only files.
    private static string BuildBatchScript(
        string script)
    {
        var lines = script.ReplaceLineEndings("\n").Split('\n');

        var failFast = !lines.Any(
            line => line.Contains('(', StringComparison.Ordinal)
                || line.Contains(')', StringComparison.Ordinal)
                || line.Contains('^', StringComparison.Ordinal)
                || line.TrimStart().StartsWith(':'));

        var builder = new StringBuilder("@echo off\r\n");

        foreach (var line in lines)
        {
            _ = builder.Append(line);

            if (!failFast || string.IsNullOrWhiteSpace(line))
            {
                _ = builder.Append("\r\n");
                continue;
            }

            _ = builder
                .Append(" || call set \"GH_CODE=%%errorlevel%%\"\r\n")
                .Append("@if defined GH_CODE if \"%GH_CODE%\"==\"0\" exit /b 1\r\n")
                .Append("@if defined GH_CODE exit /b %GH_CODE%\r\n");
        }

        return builder.ToString();
    }

    private static string BuildShellScript(
        string script)
    {
        return $"set -e\n{script.ReplaceLineEndings("\n")}\n";
    }

    private static Encoding GetOutputEncoding(
        bool isWindows)
    {
        if (!isWindows)
        {
            return new UTF8Encoding(false);
        }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        try
        {
            return Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException)
        {
            return new UTF8Encoding(false);
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

    private static void TryDelete(
        string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Best-effort cleanup must never replace the step's result or a cancellation.
        }
    }

    private async Task PumpAsync(
        StreamReader reader,
        CancellationToken cancellationToken)
    {
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            lock (_console)
            {
                _console.WriteLine(line);
            }
        }
    }
}
