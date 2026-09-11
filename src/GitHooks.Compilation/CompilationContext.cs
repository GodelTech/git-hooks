using GitHooks.Compilation.Binding;
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

    internal ParameterTable Parameters { get; set; }
        = new();

    internal ParameterValueTable Values { get; set; }
        = new();
}
