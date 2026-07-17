using GitHooks.Compilation.Parsing;
using GitHooks.Domain.Common;

namespace GitHooks.Compilation;

public sealed class PipelineCompiler(
    IPipelineParser parser)
{
    private readonly IPipelineParser _parser
        = parser ?? throw new ArgumentNullException(nameof(parser));

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

        return new CompilationResult(
            root,
            context.Diagnostics);
    }
}
