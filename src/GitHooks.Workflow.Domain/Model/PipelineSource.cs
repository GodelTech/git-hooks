namespace GitHooks.Workflow.Domain.Model;

/// <summary>
/// Represents the canonical source identifier for pipeline YAML content.
/// </summary>
public readonly record struct PipelineSource(PipelineSourceKind Kind, string Identifier)
{
    /// <summary>
    /// Creates a source that points to a local file.
    /// </summary>
    /// <param name="fullPath">Canonical full path to the file.</param>
    /// <returns>A local-file pipeline source.</returns>
    public static PipelineSource LocalFile(string fullPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullPath);

        return new PipelineSource(PipelineSourceKind.LocalFile, fullPath);
    }

    /// <summary>
    /// Creates a source that points to a remote repository location.
    /// </summary>
    /// <param name="identifier">Canonical remote source identifier.</param>
    /// <returns>A remote-repository pipeline source.</returns>
    public static PipelineSource RemoteRepository(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

        return new PipelineSource(PipelineSourceKind.RemoteRepository, identifier);
    }

    /// <summary>
    /// Returns the source identifier used in diagnostics and include chains.
    /// </summary>
    /// <returns>The source identifier.</returns>
    public override string ToString()
    {
        return Identifier;
    }
}
