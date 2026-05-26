using GitHooks.Diagnostics.Rendering;
using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Unknown;
using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Diagnostics.Ast.Printing;

public sealed class AstPrinter
    : IAstCommandVisitor
{
    private readonly AstPrinterStringBuilder _builder;
    private readonly AstPrinterOptions _options;

    public AstPrinter(AstPrinterOptions? options = null)
    {
        _options = options ?? AstPrinterOptions.Default;
        _builder = new AstPrinterStringBuilder(_options.IndentSize);
    }

    public string Print(AstNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        _builder.Clear();

        VisitNode(node);

        return _builder.ToString();
    }

    public void Visit(PipelineNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            "Pipeline",
            node);

        using (_builder.Indent())
        {
            VisitNodes(node.Parameters);
            VisitNodes(node.Steps);
            VisitNodes(node.UnknownFields);
        }
    }

    public void Visit(ParameterNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"Parameter({node.Name})",
            node);

        using (_builder.Indent())
        {
            AppendField("DisplayName", node.DisplayName);
            AppendField("Type", node.Type);
            AppendField("DefaultValue", node.DefaultValue);
            AppendField("Values", node.Values);

            VisitNodes(node.UnknownFields);
        }
    }

    public void Visit(ScriptStepNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            "ScriptStep",
            node);

        using (_builder.Indent())
        {
            VisitNode(node.Script);

            VisitNodes(node.UnknownFields);
        }
    }

    public void Visit(BooleanLiteralExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"Boolean({node.Value})",
            node);
    }

    public void Visit(IntegerLiteralExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"Integer({node.Value})",
            node);
    }

    public void Visit(StringLiteralExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"String({StringRenderer.RenderQuoted(node.Value)})",
            node);
    }

    public void VisitUnknownNode(UnknownNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        switch (node)
        {
            case UnknownFieldNode field:
                VisitUnknownField(field);
                break;

            case UnknownScalarNode scalar:
                VisitUnknownScalar(scalar);
                break;

            case UnknownSequenceNode sequence:
                VisitUnknownSequence(sequence);
                break;

            case UnknownMappingNode mapping:
                VisitUnknownMapping(mapping);
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported unknown node '{node.GetType().Name}'.");
        }
    }

    private void AppendNodeHeader(string text, AstNode node)
    {
        var output = text;

        if (_options.IncludeNodeKinds)
        {
            output += $" [{node.Kind}]";
        }

        if (_options.IncludeSourceSpans)
        {
            output += $" @ {SourceSpanRenderer.Render(node.Span)}";
        }

        _builder.AppendLine(output);
    }

    private void AppendField(string name, string? value)
    {
        if (value is not null)
        {
            _builder.AppendLine($"{name}({StringRenderer.RenderQuoted(value)})");
        }
    }

    private void AppendField(string name, Enum? value)
    {
        if (value is not null)
        {
            _builder.AppendLine($"{name}({value})");
        }
    }

    private void AppendField(string name, IReadOnlyList<string> values)
    {
        if (values.Count == 0)
        {
            return;
        }

        _builder.AppendLine(name);

        using (_builder.Indent())
        {
            foreach (var value in values)
            {
                _builder.AppendLine(StringRenderer.RenderQuoted(value));
            }
        }
    }

    private void VisitNode(AstNode node)
    {
        node.Accept(this);
    }

    private void VisitNodes(IEnumerable<AstNode> nodes)
    {
        foreach (var node in nodes)
        {
            VisitNode(node);
        }
    }

    private void VisitUnknownField(UnknownFieldNode node)
    {
        AppendNodeHeader(
            $"UnknownField({node.Key})",
            node);

        using (_builder.Indent())
        {
            VisitNode(node.Value);
        }
    }

    private void VisitUnknownScalar(UnknownScalarNode node)
    {
        AppendNodeHeader(
            $"UnknownScalar({StringRenderer.RenderQuoted(node.Value)})",
            node);
    }

    private void VisitUnknownSequence(UnknownSequenceNode node)
    {
        AppendNodeHeader(
            "UnknownSequence",
            node);

        using (_builder.Indent())
        {
            VisitNodes(node.Items);
        }
    }

    private void VisitUnknownMapping(UnknownMappingNode node)
    {
        AppendNodeHeader(
            "UnknownMapping",
            node);

        using (_builder.Indent())
        {
            VisitNodes(node.Fields);
        }
    }
}
