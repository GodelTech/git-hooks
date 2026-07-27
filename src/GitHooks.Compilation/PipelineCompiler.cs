using GitHooks.Compilation.Validation;
using GitHooks.Domain.Common;

namespace GitHooks.Compilation;

public sealed class PipelineCompiler(
    IPipelineAstCompiler astCompiler,
    IPipelineValidator validator)
{
    private readonly IPipelineAstCompiler _astCompiler
        = astCompiler ?? throw new ArgumentNullException(nameof(astCompiler));

    private readonly IPipelineValidator _validator
        = validator ?? throw new ArgumentNullException(nameof(validator));

    public CompilationResult Compile(
        string text,
        SourceDocument document,
        CompilationContext? context = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentNullException.ThrowIfNull(document);

        context ??= new CompilationContext();

        var source = new SourceContent(
            text,
            document);

        var root = _astCompiler.Compile(
            source,
            context);

        _validator.Validate(
            root,
            context.Diagnostics);

        return new CompilationResult(
            root,
            context.Diagnostics);
    }
}
