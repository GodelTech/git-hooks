namespace GitHooks.Services;

public class ServiceResult
{
    private ServiceResult(int exitCode, string error = default)
    {
        IsSuccess = exitCode == 0;
        Error = error;
        ExitCode = exitCode;
    }

    public bool IsSuccess { get; }
    public string Error { get; }
    public int ExitCode { get; }

    public static ServiceResult Success()
    {
        return new ServiceResult(0);
    }

    public static ServiceResult Failure(string error = default)
    {
        return new ServiceResult(1, error);
    }
}
