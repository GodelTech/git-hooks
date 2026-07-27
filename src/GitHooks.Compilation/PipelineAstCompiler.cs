using GitHooks.Compilation.Expansion;
using GitHooks.Compilation.Parsing;
using GitHooks.Domain.Ast.Mappings;

namespace GitHooks.Compilation;

public sealed class PipelineAstCompiler(
    IPipelineParser parser,
    ITemplateExpander expander)
    : IPipelineAstCompiler
{
    private readonly IPipelineParser _parser
        = parser ?? throw new ArgumentNullException(nameof(parser));

    private readonly ITemplateExpander _expander
        = expander ?? throw new ArgumentNullException(nameof(expander));

    public PipelineNode Compile(
        SourceContent source,
        CompilationContext context)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(context);

        var parsingContext = new ParsingContext(
            source.Text,
            source.Document);

        var root = _parser.Parse(
            parsingContext,
            context.Diagnostics);

        return _expander.Expand(
            root,
            context.Expansion,
            context.Diagnostics);
    }
}
