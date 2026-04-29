namespace GitHooks.Pipeline.Domain;

/// <summary>
/// Represents an error that occurs while parsing, expanding, or executing a pipeline.
/// </summary>
public sealed class PipelineException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The wrapped exception.</param>
    public PipelineException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
