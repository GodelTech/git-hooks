using GitHooks.Diagnostics.Ast.Printing;

namespace GitHooks.Diagnostics.Tests.Ast.Printing;

public sealed class AstPrinterStringBuilderTests
{
    private readonly AstPrinterStringBuilder _builder
        = new();

    [Fact]
    public void AppendLine_TextProvided_AppendsText()
    {
        _builder.AppendLine("Hello");

        Assert.Equal(
            "Hello",
            _builder.ToString());
    }

    [Fact]
    public void AppendLine_IndentedTextProvided_AppendsIndentedText()
    {
        using (_builder.Indent())
        {
            _builder.AppendLine("Hello");
        }

        Assert.Equal(
            "  Hello",
            _builder.ToString());
    }

    [Fact]
    public void AppendLine_NestedIndentationProvided_AppendsIndentedText()
    {
        using (_builder.Indent())
        {
            _builder.AppendLine("Level1");

            using (_builder.Indent())
            {
                _builder.AppendLine("Level2");
            }
        }

        Assert.Equal(
            """
              Level1
                Level2
            """,
            _builder.ToString());
    }

    [Fact]
    public void ToString_MultipleLinesProvided_RemovesTrailingNewLine()
    {
        _builder.AppendLine("Line1");
        _builder.AppendLine("Line2");

        Assert.Equal(
            """
            Line1
            Line2
            """,
            _builder.ToString());
    }

    [Fact]
    public void Clear_ContentProvided_RemovesContent()
    {
        _builder.AppendLine("Hello");

        _builder.Clear();

        Assert.Equal(
            string.Empty,
            _builder.ToString());
    }

    [Fact]
    public void Dispose_Twice_Throws()
    {
        var scope = _builder.Indent();

        scope.Dispose();

        var exception =
            Assert.Throws<InvalidOperationException>(
                scope.Dispose);

        Assert.Equal(
            "Cannot decrease indentation below zero.",
            exception.Message);
    }
}
