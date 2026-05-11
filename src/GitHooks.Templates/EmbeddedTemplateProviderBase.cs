namespace GitHooks.Templates;

/// <summary>
/// Base implementation of <see cref="ITemplateProvider"/> that loads template text from an embedded resource.
/// </summary>
public abstract class EmbeddedTemplateProviderBase : ITemplateProvider
{
    private readonly string _resourceName;
    private readonly Lazy<string> _template;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddedTemplateProviderBase"/> class.
    /// </summary>
    /// <param name="resourceName">The fully qualified embedded resource name containing the template text.</param>
    protected EmbeddedTemplateProviderBase(string resourceName)
    {
        _resourceName = resourceName;
        _template = new Lazy<string>(LoadEmbeddedResource);
    }

    /// <summary>
    /// Retrieves the embedded template content.
    /// </summary>
    /// <param name="cancellationToken">A token used to observe cancellation requests.</param>
    /// <returns>A task containing the normalized template content.</returns>
    public ValueTask<string> GetTemplateAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(_template.Value);
    }

    /// <summary>
    /// Loads template text from the configured embedded resource and normalizes line endings to LF.
    /// </summary>
    /// <returns>The template content loaded from the assembly manifest resource stream.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the configured embedded resource cannot be found.
    /// </exception>
    protected string LoadEmbeddedResource()
    {
        var assembly = GetType().Assembly;

        using var stream = assembly.GetManifestResourceStream(_resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded template '{_resourceName}' not found. " +
                $"Ensure the file exists and is marked as EmbeddedResource in the project.");

        using var reader = new StreamReader(stream);

        // Normalise to LF so generated bash scripts are valid on Linux/macOS
        // regardless of the line endings committed to the repository.
        return reader.ReadToEnd().ReplaceLineEndings("\n");
    }
}

