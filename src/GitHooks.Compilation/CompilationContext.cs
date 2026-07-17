using GitHooks.Diagnostics;

namespace GitHooks.Compilation;

public sealed class CompilationContext(
    CompilationOptions? options = null)
{
    public DiagnosticBag Diagnostics { get; }
        = new DiagnosticBag();

    public CompilationOptions Options { get; }
        = options ?? new CompilationOptions();
}
