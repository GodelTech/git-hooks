namespace GitHooks.Infrastructure.Scaffold.Templates;

/// <summary>
/// Placeholder token constants used in hook scaffold templates.
/// Each token is replaced during scaffolding before the file is written to disk.
/// </summary>
/// <remarks>
/// <para>Tokens present in <c>hook.sh</c>: <see cref="HookName"/>, <see cref="HooksPath"/>, <see cref="Version"/>.</para>
/// <para>Tokens present in <c>hook.yaml</c>: <see cref="HookName"/>, <see cref="HookDescription"/>.</para>
/// </remarks>
internal static class TemplateTokens
{
    /// <summary>Replaced with the Git hook name (e.g. <c>pre-commit</c>).</summary>
    internal const string HookName = "{{HOOK_NAME}}";

    /// <summary>Replaced with the relative path to the hooks directory (e.g. <c>.githooks</c>).</summary>
    internal const string HooksPath = "{{HOOKS_PATH}}";

    /// <summary>Replaced with the hook's human-readable description.</summary>
    internal const string HookDescription = "{{HOOK_DESCRIPTION}}";

    /// <summary>Replaced with the current tool version (e.g. <c>1.2.3</c>).</summary>
    internal const string Version = "{{VERSION}}";
}

