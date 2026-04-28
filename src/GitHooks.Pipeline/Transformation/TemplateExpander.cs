using System.Text.RegularExpressions;

using GitHooks.Pipeline.Ast;
using GitHooks.Pipeline.Parsing;

namespace GitHooks.Pipeline.Transformation;

/// <summary>
/// Rewrites template steps into concrete script steps by recursively loading template files.
/// </summary>
public sealed partial class TemplateExpander(IPipelineParser pipelineParser) : ITemplateExpander
{
    private readonly IPipelineParser _pipelineParser = pipelineParser;

    /// <inheritdoc/>
    public async Task<PipelineNode> ExpandAsync(PipelineNode pipeline, string pipelineFilePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pipelineFilePath))
        {
            throw new PipelineException("Pipeline file path cannot be empty for template expansion.");
        }

        var currentScope = new Dictionary<string, string>(StringComparer.Ordinal);
        var expansionStack = new Stack<string>();
        var expandedSteps = await ExpandStepsAsync(pipeline.Steps, pipelineFilePath, currentScope, expansionStack, cancellationToken);

        return new PipelineNode(pipeline.Start, expandedSteps);
    }

    private async Task<IReadOnlyList<StepNode>> ExpandStepsAsync(
        IReadOnlyList<StepNode> steps,
        string currentFilePath,
        IReadOnlyDictionary<string, string> currentScope,
        Stack<string> expansionStack,
        CancellationToken cancellationToken)
    {
        var expanded = new List<StepNode>();

        foreach (var step in steps)
        {
            switch (step)
            {
                case ScriptStepNode scriptStep:
                    var script = SubstituteParameters(scriptStep.Script, currentScope, currentFilePath);
                    expanded.Add(new ScriptStepNode(scriptStep.Start, script));
                    break;

                case TemplateStepNode templateStep:
                    var resolvedTemplatePath = ResolveTemplatePath(currentFilePath, templateStep.TemplatePath);

                    if (expansionStack.Contains(resolvedTemplatePath, StringComparer.OrdinalIgnoreCase))
                    {
                        var chain = string.Join(" -> ", expansionStack.Reverse().Append(resolvedTemplatePath));
                        throw new PipelineException($"Template expansion cycle detected: {chain}");
                    }

                    if (!File.Exists(resolvedTemplatePath))
                    {
                        throw new PipelineException($"Template file not found: {resolvedTemplatePath}");
                    }

                    var templateContent = await File.ReadAllTextAsync(resolvedTemplatePath, cancellationToken);
                    var templatePipeline = _pipelineParser.Parse(templateContent, resolvedTemplatePath);

                    var nextScope = CreateTemplateScope(currentScope, templateStep.Parameters, currentFilePath);
                    expansionStack.Push(resolvedTemplatePath);

                    try
                    {
                        var nestedExpanded = await ExpandStepsAsync(
                            templatePipeline.Steps,
                            resolvedTemplatePath,
                            nextScope,
                            expansionStack,
                            cancellationToken);

                        expanded.AddRange(nestedExpanded);
                    }
                    finally
                    {
                        _ = expansionStack.Pop();
                    }

                    break;

                default:
                    throw new PipelineException($"Unsupported step node type: {step.GetType().Name}");
            }
        }

        return expanded;
    }

    private static Dictionary<string, string> CreateTemplateScope(
        IReadOnlyDictionary<string, string> parentScope,
        IReadOnlyDictionary<string, string> templateParameters,
        string sourcePath)
    {
        var scope = new Dictionary<string, string>(parentScope, StringComparer.Ordinal);

        foreach (var pair in templateParameters)
        {
            scope[pair.Key] = SubstituteParameters(pair.Value, parentScope, sourcePath);
        }

        return scope;
    }

    private static string ResolveTemplatePath(string currentFilePath, string templatePath)
    {
        if (string.IsNullOrWhiteSpace(templatePath))
        {
            throw new PipelineException("Template path cannot be empty.");
        }

        if (Path.IsPathRooted(templatePath))
        {
            throw new PipelineException($"Absolute template paths are not allowed: {templatePath}");
        }

        var currentDirectory = Path.GetDirectoryName(currentFilePath);

        if (string.IsNullOrWhiteSpace(currentDirectory))
        {
            throw new PipelineException($"Cannot resolve template path because source directory is invalid: {currentFilePath}");
        }

        return Path.GetFullPath(Path.Combine(currentDirectory, templatePath));
    }

    private static string SubstituteParameters(string text, IReadOnlyDictionary<string, string> scope, string sourcePath)
    {
        return PlaceholderRegex().Replace(
            text,
            match =>
            {
                var key = match.Groups[1].Value;

                if (!scope.TryGetValue(key, out var value))
                {
                    throw new PipelineException($"{sourcePath}: missing template parameter '{key}' for placeholder '{match.Value}'.");
                }

                return value;
            });
    }

    [GeneratedRegex("\\$\\{\\{\\s*([a-zA-Z_][a-zA-Z0-9_.-]*)\\s*\\}\\}", RegexOptions.Compiled)]
    private static partial Regex PlaceholderRegex();
}
