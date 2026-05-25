using GitHooks.Diagnostics.Rendering;
using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings;
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

        node.Accept(this);

        return _builder.ToString();
    }

    public void Visit(PipelineNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            node,
            "Pipeline");

        using (_builder.Indent())
        {
            foreach (var parameter in node.Parameters)
            {
                parameter.Accept(this);
            }

            foreach (var step in node.Steps)
            {
                step.Accept(this);
            }

            foreach (var unknownField in node.UnknownFields)
            {
                unknownField.Accept(this);
            }
        }
    }

    public void Visit(ParameterNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            node,
            $"Parameter({node.Name})");

        using (_builder.Indent())
        {
            node.Value.Accept(this);

            foreach (var unknownField in node.UnknownFields)
            {
                unknownField.Accept(this);
            }
        }
    }

    public void Visit(ScriptStepNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            node,
            "ScriptStep");

        using (_builder.Indent())
        {
            node.Script.Accept(this);

            foreach (var unknownField in node.UnknownFields)
            {
                unknownField.Accept(this);
            }
        }
    }

    public void Visit(BooleanLiteralExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            node,
            $"Boolean({node.Value})");
    }

    public void Visit(IntegerLiteralExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            node,
            $"Integer({node.Value})");
    }

    public void Visit(StringLiteralExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            node,
            $"String({Quote(node.Value)})");
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

    private static string Quote(string value)
    {
        return $"\"{Escape(value)}\"";
    }

    private static string Escape(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal);
    }

    private void AppendNodeHeader(
        AstNode node,
        string text)
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

    private void VisitUnknownField(UnknownFieldNode node)
    {
        AppendNodeHeader(
            node,
            $"UnknownField({node.Key})");

        using (_builder.Indent())
        {
            node.Value.Accept(this);
        }
    }

    private void VisitUnknownScalar(UnknownScalarNode node)
    {
        AppendNodeHeader(
            node,
            $"UnknownScalar({Quote(node.Value)})");
    }

    private void VisitUnknownSequence(UnknownSequenceNode node)
    {
        AppendNodeHeader(
            node,
            "UnknownSequence");

        using (_builder.Indent())
        {
            foreach (var item in node.Items)
            {
                item.Accept(this);
            }
        }
    }

    private void VisitUnknownMapping(UnknownMappingNode node)
    {
        AppendNodeHeader(
            node,
            "UnknownMapping");

        using (_builder.Indent())
        {
            foreach (var field in node.Fields)
            {
                field.Accept(this);
            }
        }
    }
}
