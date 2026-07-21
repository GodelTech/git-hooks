using GitHooks.Compilation.Validation;
using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Validation.Rules;
using GitHooks.Validation.Symbols;

namespace GitHooks.Validation;

public sealed class PipelineValidator : IPipelineValidator
{
    private readonly ValidationRuleRegistry _rules
        = new();

    public void Validate(
        PipelineNode root,
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
