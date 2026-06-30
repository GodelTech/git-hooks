using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Validation.Symbols;

internal sealed class SymbolCollector(
    ValidationContext context)
    : AstWalker
{
    private readonly ValidationContext _context
        = context ?? throw new ArgumentNullException(nameof(context));

    public void Collect(
        AstNode root)
    {
        Walk(root);
    }

    public override void Visit(
        ParameterNode node)
    {
        _context.Symbols.AddParameter(node);

        base.Visit(node);
    }
}
