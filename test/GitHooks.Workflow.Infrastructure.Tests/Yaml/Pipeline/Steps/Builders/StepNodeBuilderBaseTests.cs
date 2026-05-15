using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Domain.Model;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Builders;

namespace GitHooks.Workflow.Infrastructure.Tests.Yaml.Pipeline.Steps.Builders;

public class StepNodeBuilderBaseTests
{
    [Fact]
    public void Build_WithNoViolatingFields_CallsCreateAndReturnsStepNode()
    {
        // Arrange
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var fields = new StepFields
        {
            Script = new InterpolatedStringNode("echo hello")
        };
        var builder = new TestStepNodeBuilder();

        // Act
        var result = builder.Build(fields, [], span);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ScriptStepNode>(result);
    }

    [Fact]
    public void Build_WithViolatingFields_ThrowsYamlParseException()
    {
        // Arrange
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var fields = new StepFields
        {
            Script = new InterpolatedStringNode("echo hello"),
            DisplayName = "My Step"
        };
        var builder = new TestStepNodeBuilder();

        // Act & Assert
        var exception = Assert.Throws<YamlParseException>(() => builder.Build(fields, [], span));
        Assert.Contains("cannot contain fields", exception.Message);
        Assert.Contains("displayName", exception.Message);
    }

    [Fact]
    public void Build_WithMultipleViolatingFields_IncludesAllInErrorMessage()
    {
        // Arrange
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var fields = new StepFields
        {
            Script = new InterpolatedStringNode("echo hello"),
            DisplayName = "My Step",
            TimeoutInMinutes = 10
        };
        var builder = new TestStepNodeBuilder();

        // Act & Assert
        var exception = Assert.Throws<YamlParseException>(() => builder.Build(fields, [], span));
        Assert.Contains("displayName", exception.Message);
        Assert.Contains("timeoutInMinutes", exception.Message);
    }

    [Fact]
    public void Build_WithEnvViolation_IncludesEnvInErrorMessage()
    {
        // Arrange
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var env = new Dictionary<string, InterpolatedStringNode>
        {
            ["KEY"] = new InterpolatedStringNode("value")
        };
        var fields = new StepFields
        {
            Script = new InterpolatedStringNode("echo hello"),
            Env = env
        };
        var builder = new TestStepNodeBuilder();

        // Act & Assert
        var exception = Assert.Throws<YamlParseException>(() => builder.Build(fields, [], span));
        Assert.Contains("env", exception.Message);
    }

    [Fact]
    public void Build_WithParametersViolation_IncludesParametersInErrorMessage()
    {
        // Arrange
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var parameters = new Dictionary<string, InterpolatedStringNode>
        {
            ["param"] = new InterpolatedStringNode("value")
        };
        var fields = new StepFields
        {
            Script = new InterpolatedStringNode("echo hello"),
            Parameters = parameters
        };
        var builder = new TestStepNodeBuilder();

        // Act & Assert
        var exception = Assert.Throws<YamlParseException>(() => builder.Build(fields, [], span));
        Assert.Contains("parameters", exception.Message);
    }

    [Fact]
    public void Build_WithScriptViolation_IncludesScriptInErrorMessage()
    {
        // Arrange
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var builder = new TestStepNodeBuilderNoScript();
        var fields = new StepFields
        {
            Script = new InterpolatedStringNode("echo hello")
        };

        // Act & Assert
        var exception = Assert.Throws<YamlParseException>(() => builder.Build(fields, [], span));
        Assert.Contains("script", exception.Message);
    }

    [Fact]
    public void Build_WithTemplateViolation_IncludesTemplateInErrorMessage()
    {
        // Arrange
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var fields = new StepFields
        {
            Script = new InterpolatedStringNode("echo hello"),
            Template = "my-template"
        };
        var builder = new TestStepNodeBuilder();

        // Act & Assert
        var exception = Assert.Throws<YamlParseException>(() => builder.Build(fields, [], span));
        Assert.Contains("template", exception.Message);
    }

    [Fact]
    public void Build_WithConditionViolation_IncludesConditionInErrorMessage()
    {
        // Arrange
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var fields = new StepFields
        {
            Script = new InterpolatedStringNode("echo hello"),
            Condition = new RawExpressionNode("succeeded()")
        };
        var builder = new TestStepNodeBuilder();

        // Act & Assert
        var exception = Assert.Throws<YamlParseException>(() => builder.Build(fields, [], span));
        Assert.Contains("condition", exception.Message);
    }

    [Fact]
    public void Build_WithWorkingDirectoryViolation_IncludesWorkingDirectoryInErrorMessage()
    {
        // Arrange
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var fields = new StepFields
        {
            Script = new InterpolatedStringNode("echo hello"),
            WorkingDirectory = new InterpolatedStringNode("/src")
        };
        var builder = new TestStepNodeBuilder();

        // Act & Assert
        var exception = Assert.Throws<YamlParseException>(() => builder.Build(fields, [], span));
        Assert.Contains("workingDirectory", exception.Message);
    }

    [Fact]
    public void Build_WithScriptViolationAllowed_ReturnsSuccessfully()
    {
        // Arrange
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var fields = new StepFields
        {
            Script = new InterpolatedStringNode("echo hello")
        };
        var builder = new TestStepNodeBuilder();

        // Act
        var result = builder.Build(fields, [], span);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ScriptStepNode>(result);
    }

    [Fact]
    public void Build_WithAllFieldsViolating_IncludesAllFieldNamesInErrorMessage()
    {
        // Arrange
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var parameters = new Dictionary<string, InterpolatedStringNode>
        {
            ["param"] = new InterpolatedStringNode("value")
        };
        var env = new Dictionary<string, InterpolatedStringNode>
        {
            ["KEY"] = new InterpolatedStringNode("value")
        };
        var fields = new StepFields
        {
            Script = new InterpolatedStringNode("echo hello"),
            Template = "template",
            DisplayName = "Display",
            Condition = new RawExpressionNode("succeeded()"),
            TimeoutInMinutes = 5,
            WorkingDirectory = new InterpolatedStringNode("/src"),
            Env = env,
            Parameters = parameters
        };
        var builder = new TestStepNodeBuilder();

        // Act & Assert
        var exception = Assert.Throws<YamlParseException>(() => builder.Build(fields, [], span));
        var errorMessage = exception.Message;
        Assert.Contains("template", errorMessage);
        Assert.Contains("displayName", errorMessage);
        Assert.Contains("condition", errorMessage);
        Assert.Contains("timeoutInMinutes", errorMessage);
        Assert.Contains("workingDirectory", errorMessage);
        Assert.Contains("env", errorMessage);
        Assert.Contains("parameters", errorMessage);
    }

    /// <summary>
    /// Test implementation of <see cref="StepNodeBuilderBase"/> that allows only the 'script' field.
    /// </summary>
    private sealed class TestStepNodeBuilder : StepNodeBuilderBase
    {
        private static readonly IReadOnlySet<string> s_allowedFields = new HashSet<string>
        {
            "script"
        };

        protected override string StepType => "test";

        public override bool CanBuild(StepFields fields)
        {
            return fields.Script is not null;
        }

        protected override StepNode Create(StepFields fields, IReadOnlyList<UnknownFieldNode> unknownFields, SourceSpan span)
        {
            return new ScriptStepNode(
                fields.Script ?? throw new InvalidOperationException("Script is required"),
                unknownFields,
                span
            );
        }

        protected override IReadOnlySet<string> GetAllowedFields()
        {
            return s_allowedFields;
        }
    }

    /// <summary>
    /// Test implementation that doesn't allow the 'script' field to test violations.
    /// </summary>
    private sealed class TestStepNodeBuilderNoScript : StepNodeBuilderBase
    {
        private static readonly IReadOnlySet<string> s_allowedFields = new HashSet<string>();

        protected override string StepType => "test";

        public override bool CanBuild(StepFields fields)
        {
            return false;
        }

        protected override StepNode Create(StepFields fields, IReadOnlyList<UnknownFieldNode> unknownFields, SourceSpan span)
        {
            throw new InvalidOperationException("Should not be called");
        }

        protected override IReadOnlySet<string> GetAllowedFields()
        {
            return s_allowedFields;
        }
    }
}
