using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Common;

namespace GitHooks.Compilation.Binding;

/// <inheritdoc/>
internal sealed class ParameterBinder : IParameterBinder
{
    /// <inheritdoc/>
    public void Prepare(
        PipelineNode root,
        IReadOnlyDictionary<string, string>? parameterOverrides,
        CompilationContext context)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(context);

        context.Parameters = Collect(root.Parameters);
        context.Values = Resolve(context.Parameters);

        ApplyOverrides(
            context.Parameters,
            context.Values,
            parameterOverrides,
            context.Diagnostics);
    }

    /// <inheritdoc/>
    public PipelineNode Substitute(
        PipelineNode root,
        CompilationContext context)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(context);

        return ParameterSubstitutionRewriter.Rewrite(
            root,
            context.Parameters,
            context.Values,
            context.Diagnostics);
    }

    private static ParameterTable Collect(
        IReadOnlyList<ParameterNode> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var table = new ParameterTable();

        foreach (var parameter in parameters)
        {
            table.AddParameter(parameter);
        }

        return table;
    }

    private static ParameterValueTable Resolve(
        ParameterTable declarations)
    {
        ArgumentNullException.ThrowIfNull(declarations);

        var values = new ParameterValueTable();

        foreach (var parameter in declarations.Parameters)
        {
            if (!parameter.TryGetName(out var name))
            {
                continue;
            }

            if (!parameter.TryGetDefaultValue(out var defaultValue))
            {
                continue;
            }

            values.SetValue(name, defaultValue);
        }

        return values;
    }

    private static void ApplyOverrides(
        ParameterTable declarations,
        ParameterValueTable values,
        IReadOnlyDictionary<string, string>? parameterOverrides,
        DiagnosticBag diagnostics)
    {
        ArgumentNullException.ThrowIfNull(declarations);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(diagnostics);

        if (parameterOverrides is null)
        {
            return;
        }

        foreach (var (name, value) in parameterOverrides)
        {
            if (!declarations.TryGetParameter(name, out var declaration))
            {
                diagnostics.Report(
                    Diagnostic.Create(
                        DiagnosticDescriptors.ParameterOverrideNotDeclared,
                        SourceSpan.Unknown,
                        name));

                continue;
            }

            if (!IsOverrideValueValid(declaration, value, diagnostics))
            {
                continue;
            }

            values.SetValue(name, value);
        }
    }

    private static bool IsOverrideValueValid(
        ParameterNode declaration,
        string value,
        DiagnosticBag diagnostics)
    {
        if (declaration.Type is not null &&
            declaration.Type.Value.TryGetStringValue(out var type) &&
            !ParameterNodeExtensions.IsValueValidForType(type, value))
        {
            diagnostics.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.ParameterOverrideValueTypeMismatch,
                    declaration.Span,
                    ParameterNodeExtensions.GetNameForDiagnostic(declaration),
                    type));

            return false;
        }

        if (declaration.Values is not null &&
            declaration.Values.Items.Count > 0 &&
            !ParameterNodeExtensions.ContainsValue(declaration.Values.Items, value))
        {
            diagnostics.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.ParameterOverrideValueMustBeInValues,
                    declaration.Span,
                    ParameterNodeExtensions.GetNameForDiagnostic(declaration)));

            return false;
        }

        return true;
    }
}
