//using System.CommandLine;
//using System.CommandLine.Invocation;
//using GitHooks.Pipelines;
//using GitHooks.Tasks;

//namespace GitHooks.Commands;

//public static class RunCommand
//{
//    private const string DefaultHooksDir = ".githooks";

//    public static Command Create()
//    {
//        var fileOption = new Option<FileInfo?>(
//            "--file",
//            "Explicit path to the pipeline YAML file.");

//        var hookOption = new Option<string?>(
//            "--hook",
//            "Hook name to look up in --hooks-dir (e.g. commit-msg resolves to .githooks/commit-msg.yaml).");

//        var hooksDirOption = new Option<DirectoryInfo>(
//            "--hooks-dir",
//            () => new DirectoryInfo(DefaultHooksDir),
//            $"Directory containing hook YAML definitions. Default: {DefaultHooksDir}");

//        var argsArgument = new Argument<string[]>(
//            "args",
//            () => [],
//            "Arguments forwarded from git to the hook (everything after --).")
//        {
//            Arity = ArgumentArity.ZeroOrMore
//        };

//        var command = new Command("run", "Execute a git hook pipeline.")
//        {
//            fileOption,
//            hookOption,
//            hooksDirOption,
//            argsArgument
//        };

//        command.SetHandler(async (context) =>
//        {
//            var file       = context.ParseResult.GetValueForOption(fileOption);
//            var hook       = context.ParseResult.GetValueForOption(hookOption);
//            var hooksDir   = context.ParseResult.GetValueForOption(hooksDirOption)!;
//            var hookArgs   = context.ParseResult.GetValueForArgument(argsArgument);
//            var ct         = context.GetCancellationToken();

//            var yamlPath = ResolveYamlPath(file, hook, hooksDir);
//            if (yamlPath is null)
//            {
//                Console.Error.WriteLine("error: specify --file <path> or --hook <name>.");
//                context.ExitCode = 1;
//                return;
//            }

//            if (!File.Exists(yamlPath))
//            {
//                Console.Error.WriteLine($"error: pipeline file not found: {yamlPath}");
//                context.ExitCode = 1;
//                return;
//            }

//            var yaml = await File.ReadAllTextAsync(yamlPath, ct);
//            var parser = new PipelineParser();
//            var result = parser.Parse(yaml);

//            if (!result.IsSuccess)
//            {
//                Console.Error.WriteLine($"error: failed to parse '{yamlPath}':");
//                foreach (var err in result.Errors)
//                    Console.Error.WriteLine($"  {err}");
//                context.ExitCode = 1;
//                return;
//            }

//            var pipeline = result.Value;
//            if (pipeline is null || pipeline.Steps.Count == 0)
//            {
//                Console.WriteLine("No steps to execute.");
//                return;
//            }

//            var registry = new TaskRunnerRegistry();
//            int exitCode = 0;

//            foreach (var step in pipeline.Steps)
//            {
//                if (!step.Enabled)
//                {
//                    Console.WriteLine($"[skip] {step.DisplayName ?? step.Name ?? step.Task} (disabled)");
//                    continue;
//                }

//                Console.WriteLine($"[run]  {step.DisplayName ?? step.Name ?? step.Task}");

//                int stepCode = await registry.RunAsync(step, hookArgs, ct);

//                if (stepCode != 0)
//                {
//                    Console.Error.WriteLine($"[fail] step exited with code {stepCode}.");
//                    if (!step.ContinueOnError)
//                    {
//                        context.ExitCode = stepCode;
//                        return;
//                    }
//                    exitCode = stepCode;
//                }
//                else
//                {
//                    Console.WriteLine($"[ok]   {step.DisplayName ?? step.Name ?? step.Task}");
//                }
//            }

//            context.ExitCode = exitCode;
//        });

//        return command;
//    }

//    private static string? ResolveYamlPath(FileInfo? file, string? hook, DirectoryInfo hooksDir)
//    {
//        if (file is not null) return file.FullName;
//        if (hook is not null) return Path.Combine(hooksDir.FullName, $"{hook}.yaml");
//        return null;
//    }
//}
