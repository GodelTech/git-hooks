namespace GitHooks.Services;

public interface IInstallService
{
    Task<ServiceResult> RunAsync(string hooksPath, bool force, CancellationToken cancellationToken = default);
}
