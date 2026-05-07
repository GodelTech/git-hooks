using GitHooks.Workflow.Domain.Model;
using GitHooks.Workflow.Infrastructure.Yaml;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml;

public class YamlReaderTests
{
    [Fact]
    public void Create_ValidYaml_InitializesSuccessfully()
    {
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        var streamStart = reader.Read<StreamStart>();

        Assert.NotNull(streamStart);
    }

    [Fact]
    public void Read_WithMatchingType_ConsumesAndReturnsEvent()
    {
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        _ = reader.Read<StreamStart>();
        var documentStart = reader.Read<DocumentStart>();

        Assert.NotNull(documentStart);
    }

    [Fact]
    public void Read_WithMismatchType_ThrowsYamlParseException()
    {
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        var exception = Assert.Throws<YamlParseException>(reader.Read<MappingStart>);

        Assert.Equal("Expected MappingStart, got StreamStart", exception.Message);
        Assert.Equal(new SourceRef("pipeline.yml"), exception.Span.Source);
    }

    [Fact]
    public void Read_AtEof_ThrowsEndOfStreamException()
    {
        var reader = YamlReader.Create("key: value", "pipeline.yml");
        ConsumeToEnd(reader);

        _ = Assert.Throws<EndOfStreamException>(reader.Read<StreamEnd>);
    }

    [Fact]
    public void Require_WithMatchingType_ConsumesEvent()
    {
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        reader.Require<StreamStart>();
        var isDocumentStart = reader.Is<DocumentStart>();

        Assert.True(isDocumentStart);
    }

    [Fact]
    public void Require_WithMismatchType_ThrowsYamlParseException()
    {
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        var exception = Assert.Throws<YamlParseException>(reader.Require<MappingStart>);

        Assert.Equal("Expected MappingStart, got StreamStart", exception.Message);
        Assert.Equal(new SourceRef("pipeline.yml"), exception.Span.Source);
    }

    [Fact]
    public void Is_WithMatchingType_ReturnsTrueWithoutConsuming()
    {
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        var firstCheck = reader.Is<StreamStart>();
        var streamStart = reader.Read<StreamStart>();

        Assert.True(firstCheck);
        Assert.NotNull(streamStart);
    }

    [Fact]
    public void Is_WithMismatchType_ReturnsFalseWithoutConsuming()
    {
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        var firstCheck = reader.Is<MappingStart>();
        var streamStart = reader.Read<StreamStart>();

        Assert.False(firstCheck);
        Assert.NotNull(streamStart);
    }

    [Fact]
    public void Is_AtEof_ThrowsEndOfStreamException()
    {
        var reader = YamlReader.Create("key: value", "pipeline.yml");
        ConsumeToEnd(reader);

        _ = Assert.Throws<EndOfStreamException>(() => reader.Is<StreamEnd>());
    }

    [Fact]
    public void Peek_WithMatchingType_ReturnsEventWithoutConsuming()
    {
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        var peeked = reader.Peek<StreamStart>();
        var consumed = reader.Read<StreamStart>();

        Assert.NotNull(peeked);
        Assert.NotNull(consumed);
    }

    [Fact]
    public void Peek_WithMismatchType_ThrowsYamlParseException()
    {
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        var exception = Assert.Throws<YamlParseException>(reader.Peek<MappingStart>);

        Assert.Equal("Expected MappingStart, got StreamStart", exception.Message);
        Assert.Equal(new SourceRef("pipeline.yml"), exception.Span.Source);
    }

    [Fact]
    public void SkipNode_WithScalar_SkipsScalarNode()
    {
        var reader = YamlReader.Create("value", "pipeline.yml");
        ReadEnvelopeStart(reader);

        reader.SkipNode();
        var documentEnd = reader.Read<DocumentEnd>();

        Assert.NotNull(documentEnd);
    }

    [Fact]
    public void SkipNode_WithMapping_SkipsMappingNode()
    {
        var reader = YamlReader.Create(
            """
            key1: value1
            key2: value2
            """,
            "pipeline.yml"
        );
        ReadEnvelopeStart(reader);

        reader.SkipNode();
        var documentEnd = reader.Read<DocumentEnd>();

        Assert.NotNull(documentEnd);
    }

    [Fact]
    public void SkipNode_WithSequence_SkipsSequenceNode()
    {
        var reader = YamlReader.Create(
            """
            - one
            - two
            """,
            "pipeline.yml"
        );
        ReadEnvelopeStart(reader);

        reader.SkipNode();
        var documentEnd = reader.Read<DocumentEnd>();

        Assert.NotNull(documentEnd);
    }

    [Fact]
    public void SkipNode_WithNestedStructures_SkipsWholeNode()
    {
        var reader = YamlReader.Create(
            """
            root:
              nested:
                - first
                - second
              map:
                key: value
            """,
            "pipeline.yml"
        );
        ReadEnvelopeStart(reader);

        reader.SkipNode();
        var documentEnd = reader.Read<DocumentEnd>();

        Assert.NotNull(documentEnd);
    }

    [Fact]
    public void SkipNode_AtEof_ThrowsYamlParseException()
    {
        var reader = YamlReader.Create("value", "pipeline.yml");
        ConsumeToEnd(reader);

        var exception = Assert.Throws<YamlParseException>(reader.SkipNode);

        Assert.Equal("Unexpected token while skipping node, got EOF", exception.Message);
        Assert.Equal(new SourceRef("pipeline.yml"), exception.Span.Source);
    }

    [Fact]
    public void CurrentSpan_AfterReadingScalar_ReturnsSourceFromReader()
    {
        var reader = YamlReader.Create("value", "pipeline.yml");
        ReadEnvelopeStart(reader);

        _ = reader.Read<Scalar>();
        var span = reader.CurrentSpan();

        Assert.Equal(new SourceRef("pipeline.yml"), span.Source);
        Assert.NotEqual(new SourceLocation(0, 0), span.Start);
        Assert.NotEqual(new SourceLocation(0, 0), span.End);
    }

    [Fact]
    public void CurrentSpan_AtExhaustedParser_ReturnsUnknownSpan()
    {
        var reader = YamlReader.Create("value", "pipeline.yml");
        ConsumeToEnd(reader);

        var span = reader.CurrentSpan();

        Assert.Equal(new SourceRef("pipeline.yml"), span.Source);
        Assert.Equal(new SourceLocation(0, 0), span.Start);
        Assert.Equal(new SourceLocation(0, 0), span.End);
    }

    [Fact]
    public void SpanOf_WithMappingStartAndEnd_ReturnsRangeWithSameSource()
    {
        var reader = YamlReader.Create("key: value", "pipeline.yml");
        ReadEnvelopeStart(reader);

        var mappingStart = reader.Read<MappingStart>();
        _ = reader.Read<Scalar>();
        _ = reader.Read<Scalar>();
        var mappingEnd = reader.Read<MappingEnd>();

        var span = reader.SpanOf(mappingStart, mappingEnd);

        Assert.Equal(new SourceRef("pipeline.yml"), span.Source);
        Assert.NotEqual(new SourceLocation(0, 0), span.Start);
        Assert.NotEqual(new SourceLocation(0, 0), span.End);
    }

    private static void ReadEnvelopeStart(YamlReader reader)
    {
        reader.Require<StreamStart>();
        reader.Require<DocumentStart>();
    }

    private static void ConsumeToEnd(YamlReader reader)
    {
        while (!reader.Is<StreamEnd>())
        {
            _ = reader.Read<ParsingEvent>();
        }

        reader.Require<StreamEnd>();
    }
}
