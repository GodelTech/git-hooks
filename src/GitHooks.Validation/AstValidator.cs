using GitHooks.Diagnostics;
using GitHooks.Domain.Ast;
using GitHooks.Validation.Rules;
using GitHooks.Validation.Symbols;

namespace GitHooks.Validation;

public sealed class AstValidator
{
    private readonly ValidationRuleRegistry _rules
        = new();

    public void Validate(
        AstNode root,
        DiagnosticBag diagnostics)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(diagnostics);

        var context = new ValidationContext(diagnostics);

        var collector = new SymbolCollector(context);

        collector.Collect(root);

        var validator = new ValidationVisitor(
            _rules,
            context);

        validator.Validate(root);
    }
}
