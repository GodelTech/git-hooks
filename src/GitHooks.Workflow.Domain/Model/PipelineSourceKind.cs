namespace GitHooks.Workflow.Domain.Model;

/// <summary>
/// Defines the backing location kind for a pipeline source.
/// </summary>
public enum PipelineSourceKind
{
    /// <summary>
    /// Source content is loaded from a local file.
    /// </summary>
    LocalFile,

    /// <summary>
    /// Source content is loaded from a remote repository.
    /// </summary>
    RemoteRepository,
}
