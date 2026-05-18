using System.Text.RegularExpressions;

namespace GitHooks.Templates;

/// <summary>
/// Default unresolved-token validator for scaffold templates.
/// </summary>
public sealed partial class TemplateTokenValidator
    : ITemplateTokenValidator
{
    /// <inheritdoc/>
    public void ValidateNoUnresolvedTokens(string content)
    {
        var match = UnresolvedTokenRegex().Match(content);

        if (match.Success)
        {
            throw new InvalidOperationException(
                $"Template token '{match.Value}' was not resolved. " +
                "Ensure all tokens present in the template have a corresponding substitution.");
        }
    }

    // Matches any remaining {{UPPER_SNAKE_CASE}} placeholder after token substitution.
    [GeneratedRegex(@"\{\{[A-Z_]+\}\}")]
    private static partial Regex UnresolvedTokenRegex();
}
