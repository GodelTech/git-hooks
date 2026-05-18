using System.Reflection;

namespace GitHooks.Templates;

/// <summary>
/// Resolves the tool version from assembly metadata.
/// </summary>
public sealed class ToolVersionResolver
    : IToolVersionResolver
{
    private readonly Lazy<string> _version;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolVersionResolver"/> class.
    /// </summary>
    public ToolVersionResolver()
    {
        _version = new Lazy<string>(ResolveVersionCore);
    }

    /// <inheritdoc/>
    public string ResolveVersion()
    {
        return _version.Value;
    }

    /// <summary>
    /// Resolves the version once from informational metadata with sensible fallbacks.
    /// </summary>
    /// <returns>The resolved version value.</returns>
    private static string ResolveVersionCore()
    {
        return typeof(ToolVersionResolver).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? typeof(ToolVersionResolver).Assembly.GetName().Version?.ToString()
            ?? "unknown";
    }
}
