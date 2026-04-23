namespace GitHooks.Pipelines;

/// <summary>
/// Parses YAML pipeline definitions into strongly typed models.
/// </summary>
public interface IYamlPipelineParser
{
    /// <summary>
    /// Parses and validates YAML content.
    /// </summary>
    /// <param name="yamlContent">The YAML content to parse.</param>
    /// <returns>A parse result containing either a pipeline model or diagnostics.</returns>
    PipelineParseResult Parse(string yamlContent);
}
