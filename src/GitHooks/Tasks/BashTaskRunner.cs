using System.Diagnostics;
using GitHooks.Pipelines.Models;

namespace GitHooks.Tasks;

public sealed class BashTaskRunner : ITaskRunner
{
    private static readonly HashSet<string> SupportedTasks = new(StringComparer.OrdinalIgnoreCase)
    {
        "bash", "Bash@3", "Bash@2", "Bash@1"
    };

    public bool CanHandle(string taskName) =>
        SupportedTasks.Contains(taskName) ||
        taskName.StartsWith("bash", StringComparison.OrdinalIgnoreCase);

    public async Task<int> RunAsync(Step step, string[] hookArgs, CancellationToken cancellationToken = default)
    {
        var inputs = step.Inputs;

        var targetType = GetInput(inputs, "targetType") ?? "inline";
        string scriptPath;
        bool isTempFile = false;

        if (string.Equals(targetType, "filePath", StringComparison.OrdinalIgnoreCase))
        {
            scriptPath = GetInput(inputs, "filePath")
                ?? throw new InvalidOperationException($"Step '{step.Name ?? step.Task}': inputs.filePath is required when targetType is 'filePath'.");
        }
        else
        {
            var script = GetInput(inputs, "script") ?? string.Empty;
            scriptPath = Path.GetTempFileName();
            isTempFile = true;

            if (OperatingSystem.IsWindows())
            {
                var cmdPath = scriptPath + ".bat";
                File.Move(scriptPath, cmdPath);
                scriptPath = cmdPath;
                await File.WriteAllTextAsync(scriptPath, script, cancellationToken);
            }
            else
            {
                await File.WriteAllTextAsync(scriptPath, script, cancellationToken);
            }
        }

        try
        {
            return await RunScriptAsync(scriptPath, hookArgs, cancellationToken);
        }
        finally
        {
            if (isTempFile && File.Exists(scriptPath))
                File.Delete(scriptPath);
        }
    }

    private static async Task<int> RunScriptAsync(string scriptPath, string[] hookArgs, CancellationToken cancellationToken)
    {
        ProcessStartInfo psi;

        if (OperatingSystem.IsWindows())
        {
            psi = new ProcessStartInfo("cmd.exe", $"/c \"{scriptPath}\" {JoinArgs(hookArgs)}")
            {
                UseShellExecute = false
            };
        }
        else
        {
            psi = new ProcessStartInfo("bash", $"-e \"{scriptPath}\" {JoinArgs(hookArgs)}")
            {
                UseShellExecute = false
            };
        }

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start process.");

        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
    }

    private static string GetInput(Dictionary<string, object>? inputs, string key)
    {
        if (inputs is null) return null!;
        if (inputs.TryGetValue(key, out var value)) return value?.ToString() ?? string.Empty;
        return null!;
    }

    private static string JoinArgs(string[] args) =>
        string.Join(" ", args.Select(a => a.Contains(' ') ? $"\"{a}\"" : a));
}
