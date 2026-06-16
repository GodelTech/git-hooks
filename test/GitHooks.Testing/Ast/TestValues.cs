using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Values;
using GitHooks.Domain.Common;

namespace GitHooks.Testing.Ast;

public static class TestValues
{
    public static ScalarNode Scalar(
        string value)
    {
        return new()
        {
            Value = value,
            Span = SourceSpan.Unknown
        };
    }

    public static SequenceNode Sequence(
        IEnumerable<ValueNode> items)
    {
        return new()
        {
            Items = [.. items],
            Span = SourceSpan.Unknown
        };
    }

    public static MappingNode Mapping(
        IEnumerable<FieldNode> fields)
    {
        return new()
        {
            Fields = [.. fields],
            Span = SourceSpan.Unknown
        };
    }
}
