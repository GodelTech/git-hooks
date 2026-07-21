using GitHooks.Compilation.Parsing;
using GitHooks.Compilation.Validation;
using GitHooks.Domain.Common;

namespace GitHooks.Compilation;

public sealed class PipelineCompiler(
    IPipelineParser parser,
    IPipelineValidator validator)
{
    private readonly IPipelineParser _parser
        = parser ?? throw new ArgumentNullException(nameof(parser));

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

        var parsingContext = new ParsingContext(
            text,
            document,
            context.Diagnostics);

        var root = _parser.Parse(parsingContext);

        _validator.Validate(
            root,
            context.Diagnostics);

        return new CompilationResult(
            root,
            context.Diagnostics);
    }
}
