using System.Diagnostics;

using GitHooks.Pipeline.Ast;

namespace GitHooks.Pipeline.Execution;

/// <summary>
/// Executes script steps sequentially via <see cref="Process"/>.
/// </summary>
public sealed class PipelineRunner : IPipelineRunner
{
    /// <inheritdoc/>
    public async Task<PipelineRunResult> RunAsync(PipelineNode pipeline, CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < pipeline.Steps.Count; i++)
        {
            if (pipeline.Steps[i] is not ScriptStepNode scriptStep)
            {
                return PipelineRunResult.Failure(i + 1, $"Step type '{pipeline.Steps[i].GetType().Name}' is not executable.");
            }

            Console.WriteLine($"[PIPELINE] Step {i + 1}: {scriptStep.Script}");
            var result = await ExecuteScriptAsync(scriptStep.Script, cancellationToken);

            if (!result.IsSuccess)
            {
                return PipelineRunResult.Failure(i + 1, result.ErrorMessage);
            }

            if (!string.IsNullOrWhiteSpace(result.Output))
            {
                Console.WriteLine(result.Output);
            }
        }

        return PipelineRunResult.Success();
    }

    private static async Task<ScriptExecutionResult> ExecuteScriptAsync(string script, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        if (OperatingSystem.IsWindows())
        {
            startInfo.FileName = "cmd.exe";
            startInfo.ArgumentList.Add("/c");
            startInfo.ArgumentList.Add(script);
        }
        else
        {
            startInfo.FileName = "/bin/bash";
            startInfo.ArgumentList.Add("-c");
            startInfo.ArgumentList.Add(script);
        }

        using var process = new Process { StartInfo = startInfo };

        if (!process.Start())
        {
            return ScriptExecutionResult.Failure("Failed to start script process.");
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        if (process.ExitCode == 0)
        {
            return ScriptExecutionResult.Success(stdout);
        }

        var message = string.IsNullOrWhiteSpace(stderr)
            ? $"Script failed with exit code {process.ExitCode}."
            : $"Script failed with exit code {process.ExitCode}: {stderr.Trim()}";

        return ScriptExecutionResult.Failure(message);
    }

    private sealed record ScriptExecutionResult(bool IsSuccess, string Output, string ErrorMessage)
    {
        public static ScriptExecutionResult Success(string output)
        {
            return new ScriptExecutionResult(true, output, string.Empty);
        }

        public static ScriptExecutionResult Failure(string errorMessage)
        {
            return new ScriptExecutionResult(false, string.Empty, errorMessage);
        }
    }
}
