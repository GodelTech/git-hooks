using GitHooks.Compilation.DependencyInjection;
using GitHooks.Infrastructure.Yaml.DependencyInjection;
using GitHooks.Validation.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Compilation.Tests;

internal static class IntegrationCompilerFactory
{
    public static PipelineCompiler Create()
    {
        var services = new ServiceCollection();

        _ = services.AddYamlPipelineCompilation();
        _ = services.AddTargetPipelineCompilation();
        _ = services.AddPipelineValidation();

        return services
            .BuildServiceProvider()
            .GetRequiredService<PipelineCompiler>();
    }
}
