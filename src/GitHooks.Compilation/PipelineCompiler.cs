using GitHooks.Compilation.Binding;
using GitHooks.Compilation.Validation;
using GitHooks.Domain.Common;

namespace GitHooks.Compilation;

public sealed class PipelineCompiler(
    IPipelineAstCompiler astCompiler,
    IPipelineValidator validator,
    IParameterBinder parameterBinder)
{
    private readonly IPipelineAstCompiler _astCompiler
        = astCompiler ?? throw new ArgumentNullException(nameof(astCompiler));

    private readonly IPipelineValidator _validator
        = validator ?? throw new ArgumentNullException(nameof(validator));

    private readonly IParameterBinder _parameterBinder
        = parameterBinder ?? throw new ArgumentNullException(nameof(parameterBinder));

    public CompilationResult Compile(
        string text,
        SourceDocument document,
        CompilationContext? context = null,
        IReadOnlyDictionary<string, string>? parameterOverrides = null)
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

        _parameterBinder.Prepare(
            root,
            parameterOverrides,
            context);

        _validator.Validate(
            root,
            context.Diagnostics);

        var boundRoot = _parameterBinder.Substitute(
            root,
            context);

        return new CompilationResult(
            root,
            boundRoot,
            context.Diagnostics);
    }
}
