using System.CommandLine;

using GitHooks.Commands;
using GitHooks.Handlers;
using GitHooks.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Spectre.Console;

namespace GitHooks;

internal sealed class Program
{
    public static async Task<int> Main(string[] args)
    {
        using var serviceProvider = CreateServiceProvider();

        var rootCommand = new RootCommand("githooks - run and manage YAML-defined git hook pipelines.");

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
        _ = services.AddTransient<CommandBase, CreateCommand>();
        _ = services.AddTransient<CommandBase, RunCommand>();
        _ = services.AddTransient<CommandBase, UninstallCommand>();

        // add command handlers:
        _ = services.AddTransient<IInstallHandler, InstallHandler>();
        _ = services.AddTransient<ICreateHookHandler, CreateHookHandler>();
        _ = services.AddTransient<IRunHandler, RunHandler>();
        _ = services.AddTransient<IUninstallHandler, UninstallHandler>();

        // add services:
        _ = services.AddGitHooksInfrastructure();

        return services.BuildServiceProvider();
    }
}
