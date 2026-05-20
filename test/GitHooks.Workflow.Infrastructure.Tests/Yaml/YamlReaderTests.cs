using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Application.Parsing.Exceptions;
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
    public void Read_WithMismatchType_ThrowsPipelineParsingException()
    {
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        var exception = Assert.Throws<PipelineParsingException>(reader.Read<MappingStart>);

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
    public void Require_WithMismatchType_ThrowsPipelineParsingException()
    {
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        var exception = Assert.Throws<PipelineParsingException>(reader.Require<MappingStart>);

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
    public void Peek_WithMismatchType_ThrowsPipelineParsingException()
    {
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        var exception = Assert.Throws<PipelineParsingException>(reader.Peek<MappingStart>);

        Assert.Equal("Expected MappingStart, got StreamStart", exception.Message);
        Assert.Equal(new SourceRef("pipeline.yml"), exception.Span.Source);
    }

    [Fact]
    public void ReadUnknownNode_WithScalar_ReturnsUnknownScalarNode()
    {
        var reader = YamlReader.Create("value", "pipeline.yml");
        ReadEnvelopeStart(reader);

        var node = reader.ReadUnknownNode();

        var scalar = Assert.IsType<UnknownScalarNode>(node);

        Assert.Equal("value", scalar.Value);
    }

    [Fact]
    public void ReadUnknownNode_WithNestedMappingAndSequence_ReturnsCompleteTree()
    {
        var reader = YamlReader.Create(
            """
            root:
              - one
              - two
            """,
            "pipeline.yml"
        );
        ReadEnvelopeStart(reader);

        var node = reader.ReadUnknownNode();

        var mapping = Assert.IsType<UnknownMappingNode>(node);
        Assert.Single(mapping.Entries);

        var entry = mapping.Entries[0];
        var key = Assert.IsType<UnknownScalarNode>(entry.Key);
        var value = Assert.IsType<UnknownSequenceNode>(entry.Value);

        Assert.Equal("root", key.Value);
        Assert.Equal(2, value.Items.Count);
    }

    [Fact]
    public void ReadUnknownNode_WithMappingWithNonScalarKey_ReturnsMappingWithSequenceKey()
    {
        var reader = YamlReader.Create(
            """
            ? [a, b]
            : 42
            """,
            "pipeline.yml"
        );
        ReadEnvelopeStart(reader);

        var node = reader.ReadUnknownNode();

        var mapping = Assert.IsType<UnknownMappingNode>(node);
        Assert.Single(mapping.Entries);

        _ = Assert.IsType<UnknownSequenceNode>(mapping.Entries[0].Key);

        var value = Assert.IsType<UnknownScalarNode>(mapping.Entries[0].Value);

        Assert.Equal("42", value.Value);
    }

    [Fact]
    public void ReadUnknownNode_WithAlias_ReturnsUnknownReferenceNode()
    {
        var reader = YamlReader.Create(
            """
            first: &anchor value
            second: *anchor
            """,
            "pipeline.yml"
        );
        ReadEnvelopeStart(reader);

        var node = reader.ReadUnknownNode();

        var mapping = Assert.IsType<UnknownMappingNode>(node);
        Assert.Equal(2, mapping.Entries.Count);

        var reference = Assert.IsType<UnknownReferenceNode>(mapping.Entries[1].Value);

        Assert.Equal("anchor", reference.Value);
    }

    [Fact]
    public void ReadUnknownNode_WithDocumentEnd_ThrowsPipelineParsingException()
    {
        var reader = YamlReader.Create("value", "pipeline.yml");
        ReadEnvelopeStart(reader);
        _ = reader.ReadUnknownNode();

        var exception = Assert.Throws<PipelineParsingException>(reader.ReadUnknownNode);

        Assert.Contains("Unsupported token while reading unknown node", exception.Message);
        Assert.Contains(nameof(DocumentEnd), exception.Message);
    }

    [Fact]
    public void ReadUnknownNode_AtExhaustedParser_ThrowsPipelineParsingException()
    {
        var reader = YamlReader.Create("value", "pipeline.yml");
        ConsumeToEnd(reader);

        var exception = Assert.Throws<PipelineParsingException>(reader.ReadUnknownNode);

        Assert.Equal("Unexpected token while reading unknown node, got EOF", exception.Message);
    }

    [Fact]
    public void ReadUnknownField_WithScalarKey_CreatesFieldNode()
    {
        var reader = YamlReader.Create("key: value", "pipeline.yml");
        ReadEnvelopeStart(reader);
        reader.Require<MappingStart>();

        var key = reader.Read<Scalar>();

        var field = reader.ReadUnknownField(key);

        var parsedKey = Assert.IsType<UnknownScalarNode>(field.Key);
        var parsedValue = Assert.IsType<UnknownScalarNode>(field.Value);

        Assert.Equal("key", parsedKey.Value);
        Assert.Equal("value", parsedValue.Value);
    }

    [Fact]
    public void ReadUnknownField_WithUnknownNodeKey_CreatesFieldNode()
    {
        var reader = YamlReader.Create(
            """
            ? [a, b]
            : custom
            """,
            "pipeline.yml"
        );
        ReadEnvelopeStart(reader);
        reader.Require<MappingStart>();

        var key = reader.ReadUnknownNode();

        var field = reader.ReadUnknownField(key);

        _ = Assert.IsType<UnknownSequenceNode>(field.Key);

        var value = Assert.IsType<UnknownScalarNode>(field.Value);

        Assert.Equal("custom", value.Value);
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
