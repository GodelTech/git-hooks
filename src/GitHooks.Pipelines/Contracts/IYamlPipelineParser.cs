namespace GitHooks.Pipelines.Contracts;

/// <summary>
/// Parses YAML pipeline definitions into strongly typed domain models.
/// </summary>
public interface IYamlPipelineParser
{
    /// <summary>
    /// Parses and validates YAML content.
    /// </summary>
    /// <param name="yaml">The YAML content to parse.</param>
    /// <returns>A <see cref="ParseResult"/> containing either a pipeline model or errors.</returns>
    public ParseResult Parse(string yaml);
}
