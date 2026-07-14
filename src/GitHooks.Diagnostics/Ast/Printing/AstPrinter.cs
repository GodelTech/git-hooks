using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Values;
using GitHooks.Domain.Ast.Visitors;
using GitHooks.Domain.Common.Rendering;

namespace GitHooks.Diagnostics.Ast.Printing;

public sealed class AstPrinter
    : AstWalker
{
    private readonly AstPrinterStringBuilder _builder;
    private readonly AstPrinterOptions _options;

    public AstPrinter(
        AstPrinterOptions? options = null)
    {
        _options = options ?? AstPrinterOptions.Default;
        _builder = new AstPrinterStringBuilder(_options.IndentSize);
    }

    public string Print(
        AstNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        _builder.Clear();

        Walk(node);

        return _builder.ToString();
    }

    public override void Visit(
        PipelineNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            "Pipeline",
            node);

        using (_builder.Indent())
        {
            base.Visit(node);
        }
    }

    public override void Visit(
        ParameterNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            "Parameter",
            node);

        using (_builder.Indent())
        {
            base.Visit(node);
        }
    }

    public override void Visit(
        ScriptStepNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            "ScriptStep",
            node);

        using (_builder.Indent())
        {
            base.Visit(node);
        }
    }

    public override void Visit(
        TemplateStepNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            "TemplateStep",
            node);

        using (_builder.Indent())
        {
            base.Visit(node);
        }
    }

    public override void Visit(
        InvalidStepNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            "InvalidStep",
            node);

        using (_builder.Indent())
        {
            base.Visit(node);
        }
    }

    // Fields
    public override void Visit<TValue>(
        StringKeyFieldNode<TValue> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"Field({node.Key})",
            node);

        using (_builder.Indent())
        {
            base.Visit(node);
        }
    }

    public override void Visit<TValue>(
        ComplexKeyFieldNode<TValue> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            "ComplexField",
            node);

        using (_builder.Indent())
        {
            base.Visit(node);
        }
    }

    public override void Visit<TValue>(
        SequenceFieldNode<TValue> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"SequenceField({node.Key})",
            node);

        using (_builder.Indent())
        {
            base.Visit(node);
        }
    }

    public override void Visit<TField>(
        MappingFieldNode<TField> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"MappingField({node.Key})",
            node);

        using (_builder.Indent())
        {
            base.Visit(node);
        }
    }

    // Values
    public override void Visit(
        ScalarNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"Scalar({StringRenderer.RenderQuoted(node.Value)})",
            node);

        base.Visit(node);
    }

    public override void Visit(
        SequenceNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            "Sequence",
            node);

        using (_builder.Indent())
        {
            base.Visit(node);
        }
    }

    public override void Visit(
        MappingNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            "Mapping",
            node);

        using (_builder.Indent())
        {
            base.Visit(node);
        }
    }

    // Expressions
    public override void Visit(
        BooleanLiteralExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"Boolean({node.Value})",
            node);

        base.Visit(node);
    }

    public override void Visit(
        IntegerLiteralExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"Integer({node.Value})",
            node);

        base.Visit(node);
    }

    public override void Visit(
        StringLiteralExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"String({StringRenderer.RenderQuoted(node.Value)})",
            node);

        base.Visit(node);
    }

    public override void Visit(
        InterpolatedStringExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            "InterpolatedString",
            node);

        using (_builder.Indent())
        {
            base.Visit(node);
        }
    }

    public override void Visit(
        ParameterVariableExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"ParameterVariable({node.Name})",
            node);

        base.Visit(node);
    }

    public override void Visit(
        InvalidVariableExpressionNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        AppendNodeHeader(
            $"InvalidVariable({node.Text})",
            node);

        base.Visit(node);
    }

    protected override void WalkUnknownFields(
        PipelineNodeBase node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (node.UnknownFields.Count is 0)
        {
            return;
        }

        _builder.AppendLine("UnknownFields:");

        using (_builder.Indent())
        {
            base.WalkUnknownFields(node);
        }
    }

    protected override void WalkComplexFieldKey<T>(
        ComplexKeyFieldNode<T> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        _builder.AppendLine("Key:");

        using (_builder.Indent())
        {
            base.WalkComplexFieldKey(node);
        }
    }

    protected override void WalkComplexFieldValue<T>(
        ComplexKeyFieldNode<T> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        _builder.AppendLine("Value:");

        using (_builder.Indent())
        {
            base.WalkComplexFieldValue(node);
        }
    }

    private void AppendNodeHeader(
        string text,
        AstNode node)
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
}
