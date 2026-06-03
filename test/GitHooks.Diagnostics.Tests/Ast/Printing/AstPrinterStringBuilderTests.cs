using GitHooks.Diagnostics.Ast.Printing;

namespace GitHooks.Diagnostics.Tests.Ast.Printing;

public sealed class AstPrinterStringBuilderTests
{
    [Fact]
    public void AppendLine_TextProvided_AppendsText()
    {
        var builder = new AstPrinterStringBuilder();

        builder.AppendLine("Hello");

        Assert.Equal(
            "Hello",
            builder.ToString());
    }

    [Fact]
    public void AppendLine_IndentedTextProvided_AppendsIndentedText()
    {
        var builder = new AstPrinterStringBuilder();

        using (builder.Indent())
        {
            builder.AppendLine("Hello");
        }

        Assert.Equal(
            "  Hello",
            builder.ToString());
    }

    [Fact]
    public void AppendLine_NestedIndentationProvided_AppendsIndentedText()
    {
        var builder = new AstPrinterStringBuilder();

        using (builder.Indent())
        {
            builder.AppendLine("Level1");

            using (builder.Indent())
            {
                builder.AppendLine("Level2");
            }
        }

        Assert.Equal(
            """
              Level1
                Level2
            """,
            builder.ToString());
    }

    [Fact]
    public void ToString_MultipleLinesProvided_RemovesTrailingNewLine()
    {
        var builder = new AstPrinterStringBuilder();

        builder.AppendLine("Line1");
        builder.AppendLine("Line2");

        Assert.Equal(
            """
            Line1
            Line2
            """,
            builder.ToString());
    }

    [Fact]
    public void Clear_ContentProvided_RemovesContent()
    {
        var builder = new AstPrinterStringBuilder();

        builder.AppendLine("Hello");

        builder.Clear();

        Assert.Equal(
            string.Empty,
            builder.ToString());
    }

    [Fact]
    public void Dispose_Twice_Throws()
    {
        var builder = new AstPrinterStringBuilder();

        var scope = builder.Indent();

        scope.Dispose();

        var exception =
            Assert.Throws<InvalidOperationException>(
                scope.Dispose);

        Assert.Equal(
            "Cannot decrease indentation below zero.",
            exception.Message);
    }
}
