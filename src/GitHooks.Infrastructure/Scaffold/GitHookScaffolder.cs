using System.Text;

using GitHooks.Infrastructure.Hooks;
using GitHooks.Infrastructure.Scaffold.Templates;

namespace GitHooks.Infrastructure.Scaffold;

/// <summary>
/// Default implementation of <see cref="IGitHookScaffolder"/>.
/// </summary>
/// <param name="bashTemplateProvider">The provider used to load raw Bash hook scaffold templates.</param>
/// <param name="yamlTemplateProvider">The provider used to load raw YAML hook scaffold templates.</param>
public sealed class GitHookScaffolder(
    IBashTemplateProvider bashTemplateProvider,
    IYamlTemplateProvider yamlTemplateProvider) : IGitHookScaffolder
{
    private readonly IBashTemplateProvider _bashTemplateProvider = bashTemplateProvider;
    private readonly IYamlTemplateProvider _yamlTemplateProvider = yamlTemplateProvider;

    /// <inheritdoc/>
    public async Task<GitHookScaffoldResult> CreateScaffoldAsync(string repositoryRootPath, string hooksPath, IReadOnlyCollection<GitHook> hooks, bool overwrite, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(hooksPath))
        {
            return GitHookScaffoldResult.Failure("hooks-path cannot be empty.");
        }

        // Resolve hooksPath relative to the repository root so the tool behaves consistently
        // regardless of which subfolder it is invoked from.
        var resolvedHooksPath = Path.GetFullPath(hooksPath, repositoryRootPath);

        try
        {
            _ = Directory.CreateDirectory(resolvedHooksPath);

            // Load templates once before iterating hooks to avoid redundant I/O per hook.
            var scriptTemplate = await _bashTemplateProvider.GetTemplateAsync(cancellationToken);
            var yamlTemplate = await _yamlTemplateProvider.GetTemplateAsync(cancellationToken);

            var createdHooks = new List<GitHook>(hooks.Count);

            foreach (var hook in hooks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var hookFilePath = Path.Combine(resolvedHooksPath, hook.Name);
                var yamlFilePath = Path.Combine(resolvedHooksPath, $"{hook.Name}.yaml");

                if (File.Exists(hookFilePath) && !overwrite)
                {
                    return GitHookScaffoldResult.Failure($"Scaffold file already exists: {hookFilePath}.");
                }

                if (File.Exists(yamlFilePath) && !overwrite)
                {
                    return GitHookScaffoldResult.Failure($"Scaffold file already exists: {yamlFilePath}.");
                }

                await File.WriteAllTextAsync(hookFilePath, _bashTemplateProvider.ApplyTokens(scriptTemplate, hooksPath, hook), new UTF8Encoding(false), cancellationToken);
                await File.WriteAllTextAsync(yamlFilePath, _yamlTemplateProvider.ApplyTokens(yamlTemplate, hook), new UTF8Encoding(false), cancellationToken);
                createdHooks.Add(hook);
            }

            return GitHookScaffoldResult.Success(createdHooks, resolvedHooksPath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (
            ex is IOException
            or UnauthorizedAccessException
            or ArgumentException
            or NotSupportedException
            or System.Security.SecurityException
        )
        {
            return GitHookScaffoldResult.Failure($"Failed to create hook scaffold files: {ex.Message}");
        }
    }
}

