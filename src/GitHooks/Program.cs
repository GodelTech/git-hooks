using System.CommandLine;
using GitHooks.CommandLines;
using GitHooks.Commands;
using GitHooks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GitHooks;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        using var serviceProvider = CreateServiceProvider();

        var rootCommand = new RootCommand("githooks — run and manage YAML-defined git hook pipelines.");

        serviceProvider
            .GetServices<Command>()
            .ToList()
            .ForEach(rootCommand.Subcommands.Add);

        return await rootCommand.Parse(args).InvokeAsync();
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging(
            options =>
            {
                options.ClearProviders();
                options.AddConsole();
            }
        );

        services.AddSingleton(AnsiConsole.Console);

        // add commands:
        services.AddTransient<Command, TempCommand>();

        // add command services:
        services.AddTransient<IInstallService, InstallService>();

        // add services:
        services.AddTransient<IGitCommandLine, GitCommandLine>();

        return services.BuildServiceProvider();
    }
}
