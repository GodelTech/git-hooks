using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using GitHooks.Infrastructure.Hooks;
using GitHooks.Infrastructure.Scaffold.Templates;

namespace GitHooks.Infrastructure.Scaffold;

/// <summary>
/// Default implementation of <see cref="IGitHookScaffolder"/>.
/// </summary>
/// <param name="bashTemplateProvider">The provider used to load raw Bash hook scaffold templates.</param>
/// <param name="yamlTemplateProvider">The provider used to load raw YAML hook scaffold templates.</param>
public sealed partial class GitHookScaffolder(
    IBashTemplateProvider bashTemplateProvider,
    IYamlTemplateProvider yamlTemplateProvider) : IGitHookScaffolder
{
    // Resolved once at startup from the infrastructure assembly's informational version attribute.
    // Falls back to the assembly version string, then "unknown" if neither is available.
    private static readonly string _toolVersion =
        typeof(GitHookScaffolder).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? typeof(GitHookScaffolder).Assembly.GetName().Version?.ToString()
        ?? "unknown";

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

                // ApplyScriptTokens receives the original (relative) hooksPath so the generated
                // bash script's --file argument stays relative to the repository root, which is
                // the working directory when Git invokes the hook.
                await File.WriteAllTextAsync(hookFilePath, ApplyScriptTokens(scriptTemplate, hooksPath, hook), new UTF8Encoding(false), cancellationToken);
                await File.WriteAllTextAsync(yamlFilePath, ApplyYamlTokens(yamlTemplate, hook), new UTF8Encoding(false), cancellationToken);
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

    /// <summary>
    /// Applies bash script tokens to the raw template and validates that no placeholder is left unresolved.
    /// </summary>
    private static string ApplyScriptTokens(string template, string hooksPath, GitHook hook)
    {
        var result = template
            .Replace(TemplateTokens.HookName, hook.Name)
            .Replace(TemplateTokens.HookDescription, hook.Description)
            .Replace(TemplateTokens.HooksPath, hooksPath)
            .Replace(TemplateTokens.Version, _toolVersion);

        ValidateNoUnresolvedTokens(result);

        return result;
    }

    /// <summary>
    /// Applies YAML tokens to the raw template and validates that no placeholder is left unresolved.
    /// </summary>
    private static string ApplyYamlTokens(string template, GitHook hook)
    {
        var result = template
            .Replace(TemplateTokens.HookName, hook.Name)
            .Replace(TemplateTokens.HookDescription, hook.Description)
            .Replace(TemplateTokens.Version, _toolVersion);

        ValidateNoUnresolvedTokens(result);

        return result;
    }

    /// <summary>
    /// Throws if the substituted content still contains any <c>{{TOKEN}}</c> placeholder,
    /// indicating a token was defined in a template but not handled in code.
    /// </summary>
    private static void ValidateNoUnresolvedTokens(string content)
    {
        var match = UnresolvedTokenRegex().Match(content);

        if (match.Success)
        {
            throw new InvalidOperationException(
                $"Template token '{match.Value}' was not resolved. " +
                "Ensure all tokens present in the template have a corresponding substitution.");
        }
    }

    // Matches any remaining {{UPPER_SNAKE_CASE}} placeholder after token substitution.
    [GeneratedRegex(@"\{\{[A-Z_]+\}\}")]
    private static partial Regex UnresolvedTokenRegex();
}
