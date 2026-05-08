using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Domain.Model;
using GitHooks.Workflow.Infrastructure.Yaml;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml;

public class YamlReaderTests
{
    [Fact]
    public void Create_ValidYaml_InitializesSuccessfully()
    {
        // Arrange
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        // Act
        var streamStart = reader.Read<StreamStart>();

        // Assert
        Assert.NotNull(streamStart);
    }

    [Fact]
    public void Read_WithMatchingType_ConsumesAndReturnsEvent()
    {
        // Arrange
        var reader = YamlReader.Create("key: value", "pipeline.yml");
        _ = reader.Read<StreamStart>();

        // Act
        var documentStart = reader.Read<DocumentStart>();

        // Assert
        Assert.NotNull(documentStart);
    }

    [Fact]
    public void Read_WithMismatchType_ThrowsYamlParseException()
    {
        // Arrange
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        // Act
        var exception = Assert.Throws<YamlParseException>(reader.Read<MappingStart>);

        // Assert
        Assert.Equal("Expected MappingStart, got StreamStart", exception.Message);
        Assert.Equal(new SourceRef("pipeline.yml"), exception.Span.Source);
    }

    [Fact]
    public void Read_AtEof_ThrowsEndOfStreamException()
    {
        // Arrange
        var reader = YamlReader.Create("key: value", "pipeline.yml");
        ConsumeToEnd(reader);

        // Act & Assert
        _ = Assert.Throws<EndOfStreamException>(reader.Read<StreamEnd>);
    }

    [Fact]
    public void Require_WithMatchingType_ConsumesEvent()
    {
        // Arrange
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        // Act
        reader.Require<StreamStart>();
        var isDocumentStart = reader.Is<DocumentStart>();

        // Assert
        Assert.True(isDocumentStart);
    }

    [Fact]
    public void Require_WithMismatchType_ThrowsYamlParseException()
    {
        // Arrange
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        // Act
        var exception = Assert.Throws<YamlParseException>(reader.Require<MappingStart>);

        // Assert
        Assert.Equal("Expected MappingStart, got StreamStart", exception.Message);
        Assert.Equal(new SourceRef("pipeline.yml"), exception.Span.Source);
    }

    [Fact]
    public void Is_WithMatchingType_ReturnsTrueWithoutConsuming()
    {
        // Arrange
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        // Act
        var firstCheck = reader.Is<StreamStart>();
        var streamStart = reader.Read<StreamStart>();

        // Assert
        Assert.True(firstCheck);
        Assert.NotNull(streamStart);
    }

    [Fact]
    public void Is_WithMismatchType_ReturnsFalseWithoutConsuming()
    {
        // Arrange
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        // Act
        var firstCheck = reader.Is<MappingStart>();
        var streamStart = reader.Read<StreamStart>();

        // Assert
        Assert.False(firstCheck);
        Assert.NotNull(streamStart);
    }

    [Fact]
    public void Is_AtEof_ThrowsEndOfStreamException()
    {
        // Arrange
        var reader = YamlReader.Create("key: value", "pipeline.yml");
        ConsumeToEnd(reader);

        // Act & Assert
        _ = Assert.Throws<EndOfStreamException>(() => reader.Is<StreamEnd>());
    }

    [Fact]
    public void Peek_WithMatchingType_ReturnsEventWithoutConsuming()
    {
        // Arrange
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        // Act
        var peeked = reader.Peek<StreamStart>();
        var consumed = reader.Read<StreamStart>();

        // Assert
        Assert.NotNull(peeked);
        Assert.NotNull(consumed);
    }

    [Fact]
    public void Peek_WithMismatchType_ThrowsYamlParseException()
    {
        // Arrange
        var reader = YamlReader.Create("key: value", "pipeline.yml");

        // Act
        var exception = Assert.Throws<YamlParseException>(reader.Peek<MappingStart>);

        // Assert
        Assert.Equal("Expected MappingStart, got StreamStart", exception.Message);
        Assert.Equal(new SourceRef("pipeline.yml"), exception.Span.Source);
    }

    [Fact]
    public void ReadUnknownNode_WithScalar_ReturnsUnknownScalarNode()
    {
        // Arrange
        var reader = YamlReader.Create("value", "pipeline.yml");
        ReadEnvelopeStart(reader);

        // Act
        var node = reader.ReadUnknownNode();

        // Assert
        var scalar = Assert.IsType<UnknownScalarNode>(node);

        Assert.Equal("value", scalar.Value);
    }

    [Fact]
    public void ReadUnknownNode_WithNestedMappingAndSequence_ReturnsCompleteTree()
    {
        // Arrange
        var reader = YamlReader.Create(
            """
            root:
              - one
              - two
            """,
            "pipeline.yml"
        );
        ReadEnvelopeStart(reader);

        // Act
        var node = reader.ReadUnknownNode();

        // Assert
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
        // Arrange
        var reader = YamlReader.Create(
            """
            ? [a, b]
            : 42
            """,
            "pipeline.yml"
        );
        ReadEnvelopeStart(reader);

        // Act
        var node = reader.ReadUnknownNode();

        // Assert
        var mapping = Assert.IsType<UnknownMappingNode>(node);
        Assert.Single(mapping.Entries);

        _ = Assert.IsType<UnknownSequenceNode>(mapping.Entries[0].Key);

        var value = Assert.IsType<UnknownScalarNode>(mapping.Entries[0].Value);

        Assert.Equal("42", value.Value);
    }

    [Fact]
    public void ReadUnknownNode_WithAlias_ReturnsUnknownReferenceNode()
    {
        // Arrange
        var reader = YamlReader.Create(
            """
            first: &anchor value
            second: *anchor
            """,
            "pipeline.yml"
        );
        ReadEnvelopeStart(reader);

        // Act
        var node = reader.ReadUnknownNode();

        // Assert
        var mapping = Assert.IsType<UnknownMappingNode>(node);
        Assert.Equal(2, mapping.Entries.Count);

        var reference = Assert.IsType<UnknownReferenceNode>(mapping.Entries[1].Value);

        Assert.Equal("anchor", reference.Value);
    }

    [Fact]
    public void ReadUnknownNode_WithDocumentEnd_ThrowsYamlParseException()
    {
        // Arrange
        var reader = YamlReader.Create("value", "pipeline.yml");
        ReadEnvelopeStart(reader);
        _ = reader.ReadUnknownNode();

        // Act
        var exception = Assert.Throws<YamlParseException>(reader.ReadUnknownNode);

        // Assert
        Assert.Contains("Unsupported token while reading unknown node", exception.Message);
        Assert.Contains(nameof(DocumentEnd), exception.Message);
    }

    [Fact]
    public void ReadUnknownNode_AtExhaustedParser_ThrowsYamlParseException()
    {
        // Arrange
        var reader = YamlReader.Create("value", "pipeline.yml");
        ConsumeToEnd(reader);

        // Act
        var exception = Assert.Throws<YamlParseException>(reader.ReadUnknownNode);

        // Assert
        Assert.Equal("Unexpected token while reading unknown node, got EOF", exception.Message);
    }

    [Fact]
    public void ReadUnknownField_WithScalarKey_CreatesFieldNode()
    {
        // Arrange
        var reader = YamlReader.Create("key: value", "pipeline.yml");
        ReadEnvelopeStart(reader);
        reader.Require<MappingStart>();

        var key = reader.Read<Scalar>();

        // Act
        var field = reader.ReadUnknownField(key);

        // Assert
        var parsedKey = Assert.IsType<UnknownScalarNode>(field.Key);
        var parsedValue = Assert.IsType<UnknownScalarNode>(field.Value);

        Assert.Equal("key", parsedKey.Value);
        Assert.Equal("value", parsedValue.Value);
    }

    [Fact]
    public void ReadUnknownField_WithUnknownNodeKey_CreatesFieldNode()
    {
        // Arrange
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

        // Act
        var field = reader.ReadUnknownField(key);

        // Assert
        _ = Assert.IsType<UnknownSequenceNode>(field.Key);

        var value = Assert.IsType<UnknownScalarNode>(field.Value);

        Assert.Equal("custom", value.Value);
    }

    [Fact]
    public void CurrentSpan_AfterReadingScalar_ReturnsSourceFromReader()
    {
        // Arrange
        var reader = YamlReader.Create("value", "pipeline.yml");
        ReadEnvelopeStart(reader);
        _ = reader.Read<Scalar>();

        // Act
        var span = reader.CurrentSpan();

        // Assert
        Assert.Equal(new SourceRef("pipeline.yml"), span.Source);
        Assert.NotEqual(new SourceLocation(0, 0), span.Start);
        Assert.NotEqual(new SourceLocation(0, 0), span.End);
    }

    [Fact]
    public void CurrentSpan_AtExhaustedParser_ReturnsUnknownSpan()
    {
        // Arrange
        var reader = YamlReader.Create("value", "pipeline.yml");
        ConsumeToEnd(reader);

        // Act
        var span = reader.CurrentSpan();

        // Assert
        Assert.Equal(new SourceRef("pipeline.yml"), span.Source);
        Assert.Equal(new SourceLocation(0, 0), span.Start);
        Assert.Equal(new SourceLocation(0, 0), span.End);
    }

    [Fact]
    public void SpanOf_WithMappingStartAndEnd_ReturnsRangeWithSameSource()
    {
        // Arrange
        var reader = YamlReader.Create("key: value", "pipeline.yml");
        ReadEnvelopeStart(reader);

        var mappingStart = reader.Read<MappingStart>();
        _ = reader.Read<Scalar>();
        _ = reader.Read<Scalar>();
        var mappingEnd = reader.Read<MappingEnd>();

        // Act
        var span = reader.SpanOf(mappingStart, mappingEnd);

        // Assert
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
