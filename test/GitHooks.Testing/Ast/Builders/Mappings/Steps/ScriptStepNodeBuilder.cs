using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Common;
using GitHooks.Testing.Ast.Builders.Fields;

namespace GitHooks.Testing.Ast.Builders.Mappings.Steps;

public sealed class ScriptStepNodeBuilder
    : StepNodeBuilder<ScriptStepNodeBuilder, ScriptStepNode>
{
    private readonly List<StringKeyFieldNode<ExpressionNode>> _env = [];

    private StringKeyFieldNode<ExpressionNode>? _script;
    private StringKeyFieldNode<ExpressionNode>? _displayName;
    private StringKeyFieldNode<ExpressionNode>? _condition;
    private StringKeyFieldNode<ExpressionNode>? _timeoutInMinutes;
    private StringKeyFieldNode<ExpressionNode>? _workingDirectory;

    public ScriptStepNodeBuilder WithScript(
        Action<StringKeyFieldNodeBuilder> configure)
    {
        _script = Configure(configure).Build();

        return Self;
    }

    public ScriptStepNodeBuilder WithScript(
        string script)
    {
        return WithScript(x => x
            .WithKey("script")
            .WithStringValue(v => v.WithValue(script)));
    }

    public ScriptStepNodeBuilder WithDisplayName(
        Action<StringKeyFieldNodeBuilder> configure)
    {
        _displayName = Configure(configure).Build();

        return Self;
    }

    public ScriptStepNodeBuilder WithDisplayName(
        string displayName)
    {
        return WithDisplayName(x => x
            .WithKey("displayName")
            .WithStringValue(v => v.WithValue(displayName)));
    }

    public ScriptStepNodeBuilder WithCondition(
        Action<StringKeyFieldNodeBuilder> configure)
    {
        _condition = Configure(configure).Build();

        return Self;
    }

    public ScriptStepNodeBuilder WithCondition(
        string condition)
    {
        return WithCondition(x => x
            .WithKey("condition")
            .WithStringValue(v => v.WithValue(condition)));
    }

    public ScriptStepNodeBuilder WithTimeoutInMinutes(
        Action<StringKeyFieldNodeBuilder> configure)
    {
        _timeoutInMinutes = Configure(configure).Build();

        return Self;
    }

    public ScriptStepNodeBuilder WithTimeoutInMinutes(
        int timeoutInMinutes)
    {
        return WithTimeoutInMinutes(x => x
            .WithKey("timeoutInMinutes")
            .WithIntegerValue(v => v.WithValue(timeoutInMinutes)));
    }

    public ScriptStepNodeBuilder WithWorkingDirectory(
        Action<StringKeyFieldNodeBuilder> configure)
    {
        _workingDirectory = Configure(configure).Build();

        return Self;
    }

    public ScriptStepNodeBuilder WithWorkingDirectory(
        string workingDirectory)
    {
        return WithWorkingDirectory(x => x
            .WithKey("workingDirectory")
            .WithStringValue(v => v.WithValue(workingDirectory)));
    }

    public ScriptStepNodeBuilder WithEnvironmentVariable(
        Action<StringKeyFieldNodeBuilder> configure)
    {
        return WithEnvironmentVariable(
            Configure(configure).Build());
    }

    public ScriptStepNodeBuilder WithEnvironmentVariable(
        string key,
        string value)
    {
        return WithEnvironmentVariable(x => x
            .WithKey(key)
            .WithStringValue(v => v.WithValue(value)));
    }

    public override ScriptStepNode Build()
    {
        if (_script is null)
        {
            throw new InvalidOperationException(
                "Script is required to build a ScriptStepNode.");
        }

        return new ScriptStepNode
        {
            Script = _script,
            DisplayName = _displayName,
            Condition = _condition,
            TimeoutInMinutes = _timeoutInMinutes,
            WorkingDirectory = _workingDirectory,
            Env = _env.Count == 0
                ? null
                : new MappingFieldNode<StringKeyFieldNode<ExpressionNode>>
                {
                    Key = "env",
                    Fields = [.. _env],
                    Span = SourceSpan.Unknown
                },
            UnknownFields = [.. UnknownFields],
            Span = Span
        };
    }

    private ScriptStepNodeBuilder WithEnvironmentVariable(
        StringKeyFieldNode<ExpressionNode> env)
    {
        _env.Add(env);

        return Self;
    }
}
