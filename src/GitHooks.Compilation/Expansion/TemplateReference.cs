using GitHooks.Domain.Common;

namespace GitHooks.Compilation.Expansion;

public readonly record struct TemplateReference(
    string Path,
    SourceSpan Span);
