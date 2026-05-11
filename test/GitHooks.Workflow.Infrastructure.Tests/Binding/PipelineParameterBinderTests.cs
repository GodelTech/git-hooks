using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.DependencyInjection;
using GitHooks.Workflow.Infrastructure.DependencyInjection;
using GitHooks.Workflow.Infrastructure.Yaml;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline;

using Microsoft.Extensions.DependencyInjection;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Tests.Binding;

public class PipelineParameterBinderTests
{
    [Fact]
    public void Bind_RootParameterReferencedInKnownFields_ExpandsValues()
    {
        var pipeline = ParseRoot(
            """
            parameters:
              - name: organization
                type: string
                default: godeltech
            steps:
              - script: echo ${{ parameters.organization }}
                displayName: Deploy ${{ parameters.organization }}
                workingDirectory: ./artifacts/${{ parameters.organization }}
                env:
                  ORG: ${{ parameters.organization }}
            """
        );

        var result = CreateBinder().Bind(pipeline);

        var step = Assert.IsType<ScriptStepNode>(Assert.Single(result.Steps));

        Assert.Equal("echo godeltech", step.Script.Value);
        Assert.Equal("Deploy godeltech", step.DisplayName);
        Assert.Equal("./artifacts/godeltech", step.WorkingDirectory?.Value);
        Assert.Equal("godeltech", step.Env["ORG"].Value);
    }

    [Fact]
    public void Bind_RootParameterReferencedInUnknownFields_ExpandsValues()
    {
        var pipeline = ParseRoot(
            """
            parameters:
              - name: organization
                type: string
                default: godeltech
            steps:
              - script: echo ok
                inputs:
                  target: ${{ parameters.organization }}
            """
        );

        var result = CreateBinder().Bind(pipeline);

        var step = Assert.IsType<ScriptStepNode>(Assert.Single(result.Steps));
        var field = Assert.Single(step.UnknownFields);
        var mapping = Assert.IsType<UnknownMappingNode>(field.Value);
        var entry = Assert.Single(mapping.Entries);
        var value = Assert.IsType<UnknownScalarNode>(entry.Value);

        Assert.Equal("godeltech", value.Value);
    }

    [Fact]
    public void Bind_RootParameterReferencedInTemplateCall_ExpandsValues()
    {
        var pipeline = ParseRoot(
            """
            parameters:
              - name: organization
                type: string
                default: godeltech
            steps:
              - template: deploy-template.yaml
                parameters:
                  organization: ${{ parameters.organization }}
            """
        );

        var result = CreateBinder().Bind(pipeline);

        var step = Assert.IsType<TemplateStepNode>(Assert.Single(result.Steps));

        Assert.Equal("godeltech", step.Parameters["organization"].Value);
    }

    [Fact]
    public void Bind_ParameterValueOutsideAllowedValues_ThrowsException()
    {
        var pipeline = ParseRoot(
            """
            parameters:
              - name: vmImage
                type: string
                default: linux-latest
                values:
                  - ubuntu-latest
                  - windows-latest
            steps:
              - script: echo ok
            """
        );

        var exception = Assert.Throws<PipelineParameterBindingException>(() => CreateBinder().Bind(pipeline));

        Assert.Equal("Parameter 'vmImage' value 'linux-latest' is not in the allowed values list.", exception.Message);
    }

    [Fact]
    public void Bind_ReferencedParameterWithoutValue_ThrowsException()
    {
        var pipeline = ParseRoot(
            """
            parameters:
              - name: organization
                type: string
            steps:
              - script: echo ${{ parameters.organization }}
            """
        );

        var exception = Assert.Throws<PipelineParameterBindingException>(() => CreateBinder().Bind(pipeline));

        Assert.Equal("Parameter 'organization' does not have a value.", exception.Message);
    }

    [Fact]
    public void Bind_UnsupportedExpressionNamespace_ThrowsException()
    {
        var pipeline = ParseRoot(
            """
            steps:
              - script: echo ${{ variables.organization }}
            """
        );

        var exception = Assert.Throws<PipelineParameterBindingException>(() => CreateBinder().Bind(pipeline));

        Assert.Equal("Unsupported expression 'variables.organization'. Only 'parameters.<name>' is currently supported.", exception.Message);
    }

    private static IPipelineParameterBinder CreateBinder()
    {
        var services = new ServiceCollection();
        services.AddYamlPipelineParsing();
        _ = services.AddPipelineParameterBinding();

        return services.BuildServiceProvider().GetRequiredService<IPipelineParameterBinder>();
    }

    private static PipelineNode ParseRoot(string yamlRoot)
    {
        var reader = YamlReader.Create(yamlRoot, "pipeline.yml");

        reader.Read<StreamStart>();
        reader.Read<DocumentStart>();

        var services = new ServiceCollection();
        services.AddYamlPipelineParsing();

        var provider = services.BuildServiceProvider();
        var parser = provider.GetRequiredService<PipelineRootParser>();

        var pipeline = parser.Parse(reader);

        reader.Read<DocumentEnd>();
        reader.Read<StreamEnd>();

        return pipeline;
    }
}
