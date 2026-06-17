using GitHooks.Diagnostics.Rendering;
using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Values;
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

            AppendUnknownFields(node);
        }
    }

    public void Visit(ParameterNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"Parameter",
            node);

        using (_builder.Indent())
        {
            VisitNode(node.Name);
            VisitNode(node.DisplayName);
            AppendField("Type", node.Type);
            VisitNode(node.DefaultValue);
            VisitNode(node.Values);

            AppendUnknownFields(node);
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
            VisitNode(node.DisplayName);
            VisitNode(node.Condition);
            VisitNode(node.TimeoutInMinutes);
            VisitNode(node.WorkingDirectory);

            VisitNode(node.Env);

            AppendUnknownFields(node);
        }
    }

    public void Visit(TemplateStepNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            "TemplateStep",
            node);

        using (_builder.Indent())
        {
            VisitNode(node.Template);

            VisitNode(node.Parameters);

            AppendUnknownFields(node);
        }
    }

    public void Visit(InvalidStepNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            "InvalidStep",
            node);

        using (_builder.Indent())
        {
            VisitNodes(node.Fields);

            AppendUnknownFields(node);
        }
    }

    // Fields
    public void Visit<TValue>(StringKeyFieldNode<TValue> node)
        where TValue : AstNode
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"Field({node.Key})",
            node);

        using (_builder.Indent())
        {
            VisitNode(node.Value);
        }
    }

    public void Visit<TValue>(ComplexKeyFieldNode<TValue> node)
        where TValue : AstNode
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            "ComplexField",
            node);

        using (_builder.Indent())
        {
            _builder.AppendLine("Key:");

            using (_builder.Indent())
            {
                VisitNode(node.Key);
            }

            _builder.AppendLine("Value:");

            using (_builder.Indent())
            {
                VisitNode(node.Value);
            }
        }
    }

    public void Visit<TValue>(SequenceFieldNode<TValue> node)
        where TValue : AstNode
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"SequenceField({node.Key})",
            node);

        using (_builder.Indent())
        {
            VisitNodes(node.Items);
        }
    }

    public void Visit<TField>(MappingFieldNode<TField> node)
        where TField : FieldNode
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"MappingField({node.Key})",
            node);

        using (_builder.Indent())
        {
            VisitNodes(node.Fields);
        }
    }

    // Values
    public void Visit(ScalarNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"Scalar({StringRenderer.RenderQuoted(node.Value)})",
            node);
    }

    public void Visit(SequenceNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            "Sequence",
            node);

        using (_builder.Indent())
        {
            VisitNodes(node.Items);
        }
    }

    public void Visit(MappingNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            "Mapping",
            node);

        using (_builder.Indent())
        {
            VisitNodes(node.Fields);
        }
    }

    // Expressions
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

    public void Visit(InterpolatedStringExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            "InterpolatedString",
            node);

        using (_builder.Indent())
        {
            VisitNodes(node.Parts);
        }
    }

    public void Visit(VariableExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"Variable({node.Path})",
            node);
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

    private void AppendField(string name, Enum? value)
    {
        if (value is not null)
        {
            _builder.AppendLine($"{name}({value})");
        }
    }

    private void AppendUnknownFields(PipelineNodeBase node)
    {
        if (node.UnknownFields.Count is 0)
        {
            return;
        }

        _builder.AppendLine($"UnknownFields:");

        using (_builder.Indent())
        {
            VisitNodes(node.UnknownFields);
        }
    }

    private void VisitNode(AstNode? node)
    {
        if (node is null)
        {
            return;
        }

        node.Accept(this);
    }

    private void VisitNodes(IEnumerable<AstNode> nodes)
    {
        foreach (var node in nodes)
        {
            VisitNode(node);
        }
    }
}
