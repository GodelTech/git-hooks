using GitHooks.Diagnostics.Rendering;
using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Unknown;
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
            AppendExpression("DefaultValue", node.DefaultValue);
            AppendExpressionCollection("Values", node.Values);

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
            VisitNode(node.DisplayName);
            VisitNode(node.Condition);
            VisitNode(node.TimeoutInMinutes);
            VisitNode(node.WorkingDirectory);

            VisitNode(node.Env);

            VisitNodes(node.UnknownFields);
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

            VisitNodes(node.UnknownFields);
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

    public void Visit(VariableExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"Variable({node.Path})",
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
            foreach (var part in node.Parts)
            {
                part.Accept(this);
            }
        }
    }

    public void VisitUnknownNode(UnknownNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        switch (node)
        {
            case UnknownSimpleFieldNode field:
                VisitUnknownSimpleField(field);
                break;

            case UnknownComplexFieldNode field:
                VisitUnknownComplexField(field);
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

    public void Visit<TField>(MappingFieldNode<TField> node)
        where TField : FieldNode
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"MappingField({node.Key})",
            node);

        using (_builder.Indent())
        {
            foreach (var field in node.Fields)
            {
                VisitNode(field);
            }
        }
    }

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

    private void AppendExpression(string fieldName, ExpressionNode? expression)
    {
        if (expression is null)
        {
            return;
        }

        _builder.AppendLine($"{fieldName}:");

        using (_builder.Indent())
        {
            expression.Accept(this);
        }
    }

    private void AppendExpressionCollection(string fieldName, IReadOnlyList<ExpressionNode> expressions)
    {
        if (expressions.Count is 0)
        {
            return;
        }

        _builder.AppendLine($"{fieldName}:");

        using (_builder.Indent())
        {
            foreach (var expression in expressions)
            {
                expression.Accept(this);
            }
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

    private void VisitUnknownSimpleField(UnknownSimpleFieldNode node)
    {
        AppendNodeHeader(
            $"UnknownField({node.Key})",
            node);

        using (_builder.Indent())
        {
            VisitNode(node.Value);
        }
    }

    private void VisitUnknownComplexField(UnknownComplexFieldNode node)
    {
        AppendNodeHeader(
            "UnknownComplexField",
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
