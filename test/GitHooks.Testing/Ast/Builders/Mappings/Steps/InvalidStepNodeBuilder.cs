using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Testing.Ast.Builders.Fields;

namespace GitHooks.Testing.Ast.Builders.Mappings.Steps;

public sealed class InvalidStepNodeBuilder
    : StepNodeBuilder<InvalidStepNodeBuilder, InvalidStepNode>
{
    private readonly List<FieldNode> _fields = [];

    public InvalidStepNodeBuilder WithStringKeyField(
        Action<StringKeyFieldNodeBuilder> configure)
    {
        return WithField(
            Configure(configure).Build());
    }

    public InvalidStepNodeBuilder WithField(
        string key,
        string value)
    {
        return WithStringKeyField(x => x
            .WithKey(key)
            .WithValue(value));
    }

    public override InvalidStepNode Build()
    {
        return new InvalidStepNode()
        {
            Fields = [.. _fields],
            UnknownFields = [.. UnknownFields],
            Span = Span
        };
    }

    private InvalidStepNodeBuilder WithField(
        FieldNode field)
    {
        ArgumentNullException.ThrowIfNull(field);

        _fields.Add(field);

        return Self;
    }
}
