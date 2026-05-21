using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Visitors;
using GitHooks.Domain.Common;

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

    private static string Quote(string value)
    {
        return $"\"{Escape(value)}\"";
    }

    private static string Escape(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    private static string RenderSpan(
        SourceSpan span)
    {
        return span.IsUnknown
            ? "<unknown>"
            : $"({RenderPosition(span.Start)}-{RenderPosition(span.End)})";
    }

    private static string RenderPosition(
        SourcePosition position)
    {
        if (position.IsUnknown)
        {
            return "<unknown>";
        }

        var line = position.HasKnownLine
            ? position.Line.ToString()
            : "?";

        var column = position.HasKnownColumn
            ? position.Column.ToString()
            : "?";

        return $"{line}:{column}";
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
            output += $" @ {RenderSpan(node.Span)}";
        }

        _builder.AppendLine(output);
    }
}
