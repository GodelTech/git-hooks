using GitHooks.Diagnostics;
using GitHooks.Domain.Ast;

namespace GitHooks.Validation;

public sealed class AstValidator
{
    private readonly ValidationRuleSet _rules = new();

    public void Validate(
        AstNode root,
        DiagnosticBag diagnostics)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(diagnostics);

        var context = new ValidationContext(diagnostics);

        var visitor = new ValidationVisitor(
            _rules,
            context);

        root.Accept(visitor);
    }
}
