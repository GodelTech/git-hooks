//using System.CommandLine;
//using System.CommandLine.Invocation;

//namespace GitHooks.Commands;

//public static class ListCommand
//{
//    private const string DefaultHooksDir = ".githooks";

//    public static Command Create()
//    {
//        var hooksDirOption = new Option<DirectoryInfo>(
//            "--hooks-dir",
//            () => new DirectoryInfo(DefaultHooksDir),
//            $"Directory containing hook YAML definitions and scripts. Default: {DefaultHooksDir}");

//        var command = new Command("list", "List available hook definitions and their installation status.")
//        {
//            hooksDirOption
//        };

//        command.SetHandler((context) =>
//        {
//            var hooksDir = context.ParseResult.GetValueForOption(hooksDirOption)!;

//            if (!hooksDir.Exists)
//            {
//                Console.Error.WriteLine($"error: hooks directory not found: {hooksDir.FullName}");
//                context.ExitCode = 1;
//                return Task.CompletedTask;
//            }

//            // A hook is "installed" when a generated script (containing the marker) exists
//            // next to its YAML file inside .githooks/.
//            var installedHooks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
//            foreach (var file in Directory.EnumerateFiles(hooksDir.FullName))
//            {
//                if (file.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
//                    continue;
//                try
//                {
//                    var content = File.ReadAllText(file);
//                    if (content.Contains(InstallCommand.ManagedMarker, StringComparison.Ordinal))
//                        installedHooks.Add(Path.GetFileName(file));
//                }
//                catch { /* skip unreadable files */ }
//            }

//            var yamlFiles = hooksDir.GetFiles("*.yaml");
//            if (yamlFiles.Length == 0)
//            {
//                Console.WriteLine($"No hook definitions found in {hooksDir.FullName}.");
//                return Task.CompletedTask;
//            }

//            Console.WriteLine($"{"Hook",-20} {"YAML",-40} Status");
//            Console.WriteLine(new string('-', 70));

//            foreach (var yaml in yamlFiles.OrderBy(f => f.Name))
//            {
//                var hookName = Path.GetFileNameWithoutExtension(yaml.Name);
//                var status = installedHooks.Contains(hookName) ? "installed" : "not installed";
//                Console.WriteLine($"{hookName,-20} {yaml.Name,-40} {status}");
//            }

//            return Task.CompletedTask;
//        });

//        return command;
//    }
//}
