using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.Expansion;

/// <summary>
/// Resolves a template path relative to the source pipeline file.
/// </summary>
internal interface ITemplatePathResolver
{
    /// <summary>
    /// Resolves and validates the full path of a template file relative to the source pipeline file.
    /// Absolute template paths are used as-is; relative paths are combined with the source directory.
    /// </summary>
    /// <param name="sourceFilePath">Absolute path of the YAML file referencing the template.</param>
    /// <param name="templatePath">The raw template path as declared in the YAML step.</param>
    /// <param name="span">Source span used for error location reporting.</param>
    /// <returns>The fully resolved, absolute path of the template file.</returns>
    public string Resolve(string sourceFilePath, string templatePath, SourceSpan span);
}
