using GitHooks.Domain.Common;

namespace GitHooks.Compilation;

public sealed record SourceContent(
    string Text,
    SourceDocument Document);
