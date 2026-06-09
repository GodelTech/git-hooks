using GitHooks.Diagnostics;
using GitHooks.Domain.Ast;

namespace GitHooks.Validation;

public sealed class AstValidator
{
    private readonly ValidationRuleSet _rules = new();

    public DiagnosticBag Validate(AstNode root)
    {
        ArgumentNullException.ThrowIfNull(root);

        var diagnostics = new DiagnosticBag();

        var visitor = new ValidationVisitor(
            _rules,
            diagnostics);

        root.Accept(visitor);

        return diagnostics;
    }
}
