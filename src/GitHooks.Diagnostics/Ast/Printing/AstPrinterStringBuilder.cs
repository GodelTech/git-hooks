using System.Text;

namespace GitHooks.Diagnostics.Ast.Printing;

internal sealed class AstPrinterStringBuilder(
    int indentSize = 2)
{
    private readonly string _indentText = new(' ', indentSize);

    private readonly StringBuilder _builder = new();

    private int _indentLevel;

    public void AppendLine(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        AppendIndent();

        _ = _builder.Append(text);
        _ = _builder.Append('\n');
    }

    public IDisposable Indent()
    {
        _indentLevel++;

        return new IndentScope(this);
    }

    public override string ToString()
    {
        return _builder.ToString().TrimEnd();
    }

    private void AppendIndent()
    {
        for (var i = 0; i < _indentLevel; i++)
        {
            _ = _builder.Append(_indentText);
        }
    }

    private void DecreaseIndent()
    {
        if (_indentLevel == 0)
        {
            throw new InvalidOperationException(
                "Cannot decrease indentation below zero."
            );
        }

        _indentLevel--;
    }

    private sealed class IndentScope(AstPrinterStringBuilder builder)
        : IDisposable
    {
        private readonly AstPrinterStringBuilder _builder = builder;

        public void Dispose()
        {
            _builder.DecreaseIndent();
        }
    }
}
