using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.Binding.Parameters;
using GitHooks.Workflow.Application.Binding.Steps;
using GitHooks.Workflow.Application.Binding.UnknownNodes;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Tests.Binding.Steps;

public class TemplateStepParameterBinderTests
{
    [Fact]
    public void Bind_TemplateWithParameterReference_ExpandsTemplate()
    {
        var step = CreateTemplateStep("${{ parameters.templatePath }}");
        var context = CreateContext([("templatePath", "templates/deploy.yaml")]);
        var binder = CreateBinder();

        var result = binder.Bind(step, context);

        Assert.Equal("templates/deploy.yaml", result.Template);
    }

    [Fact]
    public void Bind_SingleParameterWithReference_ExpandsParameter()
    {
        var step = CreateTemplateStep(
            "template.yaml",
            parameters: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["environment"] = "${{ parameters.env }}"
            }
        );
        var context = CreateContext([("env", "production")]);
        var binder = CreateBinder();

        var result = binder.Bind(step, context);

        Assert.Equal("production", result.Parameters["environment"].Value);
    }

    [Fact]
    public void Bind_MultipleParametersWithReferences_ExpandsAll()
    {
        var step = CreateTemplateStep(
            "template.yaml",
            parameters: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["environment"] = "${{ parameters.env }}",
                ["region"] = "${{ parameters.region }}",
                ["version"] = "${{ parameters.version }}"
            }
        );
        var context = CreateContext([
            ("env", "staging"),
            ("region", "us-west"),
            ("version", "1.2.3")
        ]);
        var binder = CreateBinder();

        var result = binder.Bind(step, context);

        Assert.Equal("staging", result.Parameters["environment"].Value);
        Assert.Equal("us-west", result.Parameters["region"].Value);
        Assert.Equal("1.2.3", result.Parameters["version"].Value);
    }

    [Fact]
    public void Bind_TemplateAndParametersWithReferences_ExpandsAll()
    {
        var step = CreateTemplateStep(
            "${{ parameters.templateName }}",
            parameters: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["target"] = "${{ parameters.targetEnv }}"
            }
        );
        var context = CreateContext([
            ("templateName", "deploy-template.yaml"),
            ("targetEnv", "production")
        ]);
        var binder = CreateBinder();

        var result = binder.Bind(step, context);

        Assert.Equal("deploy-template.yaml", result.Template);
        Assert.Equal("production", result.Parameters["target"].Value);
    }

    [Fact]
    public void Bind_NoParameterReferences_LeaveFieldsUnchanged()
    {
        var step = CreateTemplateStep(
            "template.yaml",
            parameters: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["environment"] = "dev"
            }
        );
        var context = CreateContext([]);
        var binder = CreateBinder();

        var result = binder.Bind(step, context);

        Assert.Equal("template.yaml", result.Template);
        Assert.Equal("dev", result.Parameters["environment"].Value);
    }

    [Fact]
    public void Bind_EmptyParameters_PreservesEmpty()
    {
        var step = CreateTemplateStep(
            "template.yaml",
            parameters: new Dictionary<string, string>(StringComparer.Ordinal)
        );
        var context = CreateContext([]);
        var binder = CreateBinder();

        var result = binder.Bind(step, context);

        Assert.Equal("template.yaml", result.Template);
        Assert.Empty(result.Parameters);
    }

    [Fact]
    public void Bind_UnknownFieldsWithParameterReferences_ExpandsValues()
    {
        var unknownSpan = CreateSpan();
        var metaField = new UnknownFieldNode(
            new UnknownScalarNode("meta", unknownSpan),
            new UnknownMappingNode(
                [
                    new UnknownMappingEntryNode(
                        new UnknownScalarNode("stage", unknownSpan),
                        new UnknownScalarNode("${{ parameters.pipelineStage }}", unknownSpan),
                        unknownSpan
                    )
                ],
                unknownSpan
            ),
            unknownSpan
        );

        var step = CreateTemplateStep(
            "template.yaml",
            unknownFields: [metaField]
        );
        var context = CreateContext([("pipelineStage", "build")]);
        var binder = CreateBinder();

        var result = binder.Bind(step, context);

        var field = Assert.Single(result.UnknownFields);
        var mapping = Assert.IsType<UnknownMappingNode>(field.Value);
        var entry = Assert.Single(mapping.Entries);
        var value = Assert.IsType<UnknownScalarNode>(entry.Value);

        Assert.Equal("build", value.Value);
    }

    [Fact]
    public void Bind_PreservesOriginalStepInstance()
    {
        var step = CreateTemplateStep(
            "template.yaml",
            parameters: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["env"] = "${{ parameters.env }}"
            }
        );
        var originalTemplate = step.Template;
        var context = CreateContext([("env", "prod")]);
        var binder = CreateBinder();

        var result = binder.Bind(step, context);

        // Verify original step unchanged
        Assert.Equal(originalTemplate, step.Template);

        // Verify new step has expanded values
        Assert.Equal("prod", result.Parameters["env"].Value);
    }

    private static TemplateStepParameterBinder CreateBinder()
    {
        var unknownNodeBinder = new UnknownNodeParameterBinder();
        return new TemplateStepParameterBinder(unknownNodeBinder);
    }

    private static TemplateStepNode CreateTemplateStep(
        string template,
        IReadOnlyDictionary<string, string>? parameters = null,
        IReadOnlyList<UnknownFieldNode>? unknownFields = null,
        SourceSpan? span = null)
    {
        var effectiveSpan = span ?? CreateSpan();
        var boundParameters = new Dictionary<string, InterpolatedStringNode>(StringComparer.Ordinal);

        if (parameters is not null)
        {
            foreach (var pair in parameters)
            {
                boundParameters[pair.Key] = new InterpolatedStringNode(pair.Value);
            }
        }

        return new TemplateStepNode(
            template,
            boundParameters,
            unknownFields ?? [],
            effectiveSpan
        );
    }

    private static ParameterBindingContext CreateContext(
        IEnumerable<(string Name, string Value)> parameters)
    {
        var declarations = new Dictionary<string, ParameterNode>(StringComparer.Ordinal);
        var parameterValues = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (name, value) in parameters)
        {
            parameterValues[name] = value;
        }

        return new ParameterBindingContext(declarations, parameterValues);
    }

    private static SourceSpan CreateSpan(string sourceName = "test")
    {
        return SourceSpan.Unknown(new SourceRef(sourceName));
    }
}
