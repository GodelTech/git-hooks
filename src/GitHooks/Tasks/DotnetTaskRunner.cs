using System.Diagnostics;
using GitHooks.Pipelines.Models;

namespace GitHooks.Tasks;

public sealed class DotnetTaskRunner : ITaskRunner
{
    public bool CanHandle(string taskName) =>
        string.Equals(taskName, "dotnet", StringComparison.OrdinalIgnoreCase);

    public async Task<int> RunAsync(Step step, string[] hookArgs, CancellationToken cancellationToken = default)
    {
        var inputs = step.Inputs;

        var command = GetInput(inputs, "command")
            ?? throw new InvalidOperationException($"Step '{step.Name ?? step.Task}': inputs.command is required.");

        var workingDirectory = GetInput(inputs, "workingDirectory");

        var arguments = string.IsNullOrEmpty(command)
            ? string.Empty
            : $"{command} {JoinArgs(hookArgs)}".TrimEnd();

        var psi = new ProcessStartInfo("dotnet", arguments)
        {
            UseShellExecute = false
        };

        if (!string.IsNullOrEmpty(workingDirectory))
            psi.WorkingDirectory = workingDirectory;

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start dotnet process.");

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
