using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Common;

namespace GitHooks.Testing.Ast.Builders;

public sealed class ScriptStepNodeBuilder
    : MappingNodeBuilder<ScriptStepNodeBuilder>
{
    private readonly List<StringKeyFieldNode<ExpressionNode>> _env = [];

    private StringKeyFieldNode<ExpressionNode>? _script;
    private StringKeyFieldNode<ExpressionNode>? _displayName;
    private StringKeyFieldNode<ExpressionNode>? _condition;
    private StringKeyFieldNode<ExpressionNode>? _timeoutInMinutes;
    private StringKeyFieldNode<ExpressionNode>? _workingDirectory;

    public ScriptStepNodeBuilder()
    {
        WithScript("dotnet test");
    }

    public ScriptStepNodeBuilder WithScript(
        string script = "dotnet test")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(script);

        _script = TestFields.StringKey(
            "script",
            script);

        return this;
    }

    public ScriptStepNodeBuilder WithDisplayName(
        string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        _displayName = TestFields.StringKey(
            "displayName",
            displayName);

        return this;
    }

    public ScriptStepNodeBuilder WithCondition(
        string condition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(condition);

        _condition = TestFields.StringKey(
            "condition",
            condition);

        return this;
    }

    public ScriptStepNodeBuilder WithTimeoutInMinutes(
        int timeoutInMinutes)
    {
        _timeoutInMinutes = TestFields.StringKey(
            "timeoutInMinutes",
            TestExpressions.Integer(timeoutInMinutes));

        return this;
    }

    public ScriptStepNodeBuilder WithWorkingDirectory(
        string workingDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);

        _workingDirectory = TestFields.StringKey(
            "workingDirectory",
            workingDirectory);

        return this;
    }

    public ScriptStepNodeBuilder WithEnvironmentVariable(
        string name,
        ExpressionNode value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);

        _env.Add(
            new StringKeyFieldNode<ExpressionNode>
            {
                Key = name,
                Value = value,
                Span = SourceSpan.Unknown
            });

        return this;
    }

    public ScriptStepNodeBuilder WithEnvironmentVariable(
        string name,
        string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        WithEnvironmentVariable(
            name,
            TestExpressions.String(value));

        return this;
    }

    public ScriptStepNode Build()
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
}
