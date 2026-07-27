using GitHooks.Compilation.Expansion;
using GitHooks.Diagnostics;

namespace GitHooks.Compilation;

public sealed class CompilationContext(
    CompilationOptions? options = null)
{
    public DiagnosticBag Diagnostics { get; }
        = new();

    public CompilationOptions Options { get; }
        = options ?? new CompilationOptions();

    internal ExpansionContext Expansion { get; }
        = new();
}
