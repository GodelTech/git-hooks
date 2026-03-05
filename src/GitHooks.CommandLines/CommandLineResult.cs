using System.Diagnostics;

namespace GitHooks.CommandLines;

public class CommandLineResult
{
    private CommandLineResult(string output, string error, int exitCode)
    {
        IsSuccess = exitCode == 0;
        Output = output;
        Error = error;
        ExitCode = exitCode;
    }

    public bool IsSuccess { get; }
    public string Output { get; }
    public string Error { get; }
    public int ExitCode { get; }

    public static CommandLineResult FromProcess(Process process)
    {
        return new CommandLineResult(
            process.StandardOutput.ReadToEnd(),
            process.StandardError.ReadToEnd(),
            process.ExitCode
        );
    }

    public static CommandLineResult FromException(Exception ex)
    {
        return new CommandLineResult(string.Empty, ex.Message, 1);
    }
}
