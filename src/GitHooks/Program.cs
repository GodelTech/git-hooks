using GitHooks.CommandLine;
using GitHooks.Commands;
using GitHooks.Handlers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.CommandLine;

using Spectre.Console;

namespace GitHooks;

internal sealed class Program
{
    public static async Task<int> Main(string[] args)
    {
        using var serviceProvider = CreateServiceProvider();

        var rootCommand = new RootCommand("githooks — run and manage YAML-defined git hook pipelines.");

        serviceProvider
            .GetServices<CommandBase>()
            .ToList()
            .ForEach(
                command =>
                {
                    foreach (var option in command.CreateOptions())
                    {
                        command.Options.Add(option);
                    }

                    command.SetAction(command.HandleActionAsync);

                    rootCommand.Subcommands.Add(command);
                }
            );

        return await rootCommand.Parse(args).InvokeAsync();
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        _ = services.AddLogging(
            options =>
            {
                _ = options.ClearProviders();
                _ = options.AddConsole();
            }
        );

        _ = services.AddSingleton(AnsiConsole.Console);

        // add commands:
        _ = services.AddTransient<CommandBase, InstallCommand>();

        // add command services:
        _ = services.AddTransient<IInstallHandler, InstallHandler>();

        // add services:
        _ = services.AddTransient<ICommandLineRunner, CommandLineRunner>();
        _ = services.AddTransient<IGitCommandLine, GitCommandLine>();

        return services.BuildServiceProvider();
    }
}
