using GitHooks.Pipeline.Domain.Model;

using PipelineModel = GitHooks.Pipeline.Domain.Model.PipelineOld;

namespace GitHooks.Pipeline.Parsing;

/// <summary>
/// Parses YAML text into a strongly typed pipeline AST.
/// </summary>
public interface IPipelineParser
{
    /// <summary>
    /// Parses YAML text into a <see cref="PipelineModel"/>.
    /// </summary>
    /// <param name="yamlContent">The YAML content to parse.</param>
    /// <param name="sourceName">A source identifier for diagnostics (typically file path).</param>
    /// <returns>The parsed pipeline AST.</returns>
    public PipelineModel Parse(string yamlContent, string sourceName);
}
