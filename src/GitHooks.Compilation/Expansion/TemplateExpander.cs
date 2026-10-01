using GitHooks.Compilation.Binding;
using GitHooks.Compilation.Validation;
using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Common;

namespace GitHooks.Compilation.Expansion;

internal sealed class TemplateExpander(
    ITemplateLoader loader,
    ITemplateCompiler templateCompiler,
    IParameterScopeBuilder scopeBuilder,
    IPipelineValidator validator)
    : ITemplateExpander
{
    private readonly ITemplateLoader _loader
        = loader ?? throw new ArgumentNullException(nameof(loader));

    private readonly ITemplateCompiler _templateCompiler
        = templateCompiler ?? throw new ArgumentNullException(nameof(templateCompiler));

    private readonly IParameterScopeBuilder _scopeBuilder
        = scopeBuilder ?? throw new ArgumentNullException(nameof(scopeBuilder));

    private readonly IPipelineValidator _validator
        = validator ?? throw new ArgumentNullException(nameof(validator));

    public PipelineNode Expand(
        PipelineNode pipeline,
        ExpansionContext context,
        DiagnosticBag diagnostics)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(diagnostics);

        var rootDocument = context.RootDocument
            ?? new SourceDocument("<unknown>");

        var rootReference = new TemplateReference(
            rootDocument.Name,
            pipeline.Span);

        context.ExpansionStack.Push(rootReference);

        var rootDeclarations = _scopeBuilder.Collect(pipeline.Parameters);
        var rootValues = _scopeBuilder.ResolveDefaults(rootDeclarations);

        try
        {
            return ExpandPipeline(
                pipeline,
                rootDocument,
                context,
                diagnostics,
                rootDeclarations,
                rootValues);
        }
        finally
        {
            _ = context.ExpansionStack.Pop();
        }
    }

    private static bool TryResolveTemplatePath(
        TemplateStepNode templateStep,
        SourceDocument currentDocument,
        DiagnosticBag diagnostics,
        out string resolvedPath)
    {
        resolvedPath = string.Empty;

        if (!templateStep.Template.Value.TryGetStringValue(out var templateReference))
        {
            diagnostics.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.TemplateReferenceMustBeLiteral,
                    templateStep.Span));

            return false;
        }

        if (string.IsNullOrWhiteSpace(templateReference))
        {
            diagnostics.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.TemplateReferenceMustNotBeEmpty,
                    templateStep.Span));

            return false;
        }

        if (templateReference.Contains('@'))
        {
            diagnostics.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.RemoteTemplatesNotSupported,
                    templateStep.Span,
                    templateReference));

            return false;
        }

        var sourceDirectory = Path.GetDirectoryName(currentDocument.Name);

        var combinedPath = Path.IsPathRooted(templateReference)
            ? templateReference
            : Path.Combine(sourceDirectory ?? string.Empty, templateReference);

        var fullPath = Path.GetFullPath(combinedPath);

        if (!File.Exists(fullPath))
        {
            diagnostics.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.TemplateNotFound,
                    templateStep.Span,
                    fullPath));

            return false;
        }

        resolvedPath = fullPath;
        return true;
    }

    private static List<DiagnosticLocation> BuildIncludeChainLocations(
        ExpansionContext context)
    {
        var locations = new List<DiagnosticLocation>();

        foreach (var reference in context.ExpansionStack.Reverse())
        {
            locations.Add(
                DiagnosticLocation.Create(
                    reference.Span,
                    $"Included from '{reference.Path}'."));
        }

        return locations;
    }

    private static Dictionary<string, string>? ResolveTemplateParameterOverrides(
        TemplateStepNode templateStep,
        ParameterTable scopeDeclarations,
        ParameterValueTable scopeValues,
        DiagnosticBag diagnostics)
    {
        if (templateStep.Parameters is null)
        {
            return null;
        }

        var overrides = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in templateStep.Parameters.Fields)
        {
            if (ParameterSubstitutionRewriter.TryResolveExpressionValue(
                field.Value,
                scopeDeclarations,
                scopeValues,
                diagnostics,
                out var value))
            {
                overrides[field.Key] = value;
            }
        }

        return overrides;
    }

    private PipelineNode ExpandPipeline(
        PipelineNode pipeline,
        SourceDocument currentDocument,
        ExpansionContext context,
        DiagnosticBag diagnostics,
        ParameterTable scopeDeclarations,
        ParameterValueTable scopeValues)
    {
        List<StepNode>? expandedSteps = null;

        for (var i = 0; i < pipeline.Steps.Count; i++)
        {
            var step = pipeline.Steps[i];

            if (step is not TemplateStepNode templateStep)
            {
                expandedSteps?.Add(step);
                continue;
            }

            expandedSteps ??= [.. pipeline.Steps.Take(i)];

            var expanded = ExpandTemplateStep(
                templateStep,
                currentDocument,
                context,
                diagnostics,
                scopeDeclarations,
                scopeValues);

            expandedSteps.AddRange(expanded);
        }

        if (expandedSteps is null)
        {
            return pipeline;
        }

        return new PipelineNode
        {
            Parameters = pipeline.Parameters,
            Steps = expandedSteps,
            UnknownFields = pipeline.UnknownFields,
            Span = pipeline.Span
        };
    }

    private IReadOnlyList<StepNode> ExpandTemplateStep(
        TemplateStepNode templateStep,
        SourceDocument currentDocument,
        ExpansionContext context,
        DiagnosticBag diagnostics,
        ParameterTable scopeDeclarations,
        ParameterValueTable scopeValues)
    {
        if (!TryResolveTemplatePath(templateStep, currentDocument, diagnostics, out var resolvedPath))
        {
            return [];
        }

        if (context.ExpansionStack.Count >= ExpansionContext.MaxDepth)
        {
            diagnostics.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.TemplateExpansionDepthExceeded,
                    templateStep.Span,
                    ExpansionContext.MaxDepth));

            return [];
        }

        if (context.IsActive(resolvedPath))
        {
            diagnostics.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.TemplateIncludeCycleDetected,
                    templateStep.Span,
                    BuildIncludeChainLocations(context),
                    resolvedPath));

            return [];
        }

        var reference = new TemplateReference(
            resolvedPath,
            templateStep.Span);

        var source = _loader.Load(reference);
        var templateDocument = source.Document;

        context.ExpansionStack.Push(reference);

        try
        {
            var childPipeline = _templateCompiler.Compile(
                source,
                diagnostics);

            _validator.Validate(
                childPipeline,
                diagnostics);

            var childDeclarations = _scopeBuilder.Collect(childPipeline.Parameters);
            var childValues = _scopeBuilder.ResolveDefaults(childDeclarations);

            var childOverrides = ResolveTemplateParameterOverrides(
                templateStep,
                scopeDeclarations,
                scopeValues,
                diagnostics);

            _scopeBuilder.ApplyOverrides(
                childDeclarations,
                childValues,
                childOverrides,
                diagnostics);

            var expandedChild = ExpandPipeline(
                childPipeline,
                templateDocument,
                context,
                diagnostics,
                childDeclarations,
                childValues);

            var boundSteps = ParameterSubstitutionRewriter.RewriteStepsForScope(
                expandedChild.Steps,
                childDeclarations,
                childValues,
                diagnostics);

            return boundSteps;
        }
        finally
        {
            _ = context.ExpansionStack.Pop();
        }
    }
}
