using GitHooks.Compilation.Parsing;
using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings;

namespace GitHooks.Compilation.Expansion;

internal sealed class TemplateCompiler(
    IPipelineParser parser)
    : ITemplateCompiler
{
    private readonly IPipelineParser _parser
        = parser ?? throw new ArgumentNullException(nameof(parser));

    public PipelineNode Compile(
        SourceContent source,
        DiagnosticBag diagnostics)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(diagnostics);

        var parsingContext = new ParsingContext(
            source.Text,
            source.Document);

        return _parser.Parse(
            parsingContext,
            diagnostics);
    }
}
