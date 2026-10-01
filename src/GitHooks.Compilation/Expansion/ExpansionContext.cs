using GitHooks.Domain.Common;

namespace GitHooks.Compilation.Expansion;

public sealed class ExpansionContext
{
    /// <summary>
    /// Maximum guard depth used by template expansion.
    /// </summary>
    internal const int MaxDepth = 64;

    /// <summary>
    /// Gets the ordered include chain: the currently active document and template
    /// references, used to preserve and report the include chain in
    /// diagnostics and to detect circular references.
    /// </summary>
    public Stack<TemplateReference> ExpansionStack { get; }
        = new();

    /// <summary>
    /// Gets or sets the document currently being compiled at the root of expansion.
    /// Set by the AST compiler before expansion begins.
    /// </summary>
    internal SourceDocument? RootDocument { get; set; }

    /// <summary>
    /// Returns whether a document with the given identifier is currently
    /// being expanded (i.e. is part of the active include chain), used for
    /// circular reference detection.
    /// </summary>
    /// <param name="documentIdentifier">The document identifier to check.</param>
    /// <returns><see langword="true"/> if the document is part of the active include chain; otherwise, <see langword="false"/>.</returns>
    internal bool IsActive(
        string documentIdentifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentIdentifier);

        foreach (var reference in ExpansionStack)
        {
            if (string.Equals(reference.Path, documentIdentifier, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
