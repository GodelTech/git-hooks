//using System.CommandLine;
//using System.CommandLine.Invocation;

//namespace GitHooks.Commands;

//public static class InstallCommand
//{
//    private const string DefaultHooksDir = ".githooks";
//    internal const string ManagedMarker  = "# managed by githooks";

//    public static Command Create()
//    {
//        var hooksDirOption = new Option<DirectoryInfo>(
//            "--hooks-dir",
//            () => new DirectoryInfo(DefaultHooksDir),
//            $"Directory containing hook YAML definitions and scripts. Default: {DefaultHooksDir}");

//        var command = new Command("install", "Write hook scripts into .githooks/ and configure git to use that directory.")
//        {
//            hooksDirOption
//        };

//        command.SetHandler(async (context) =>
//        {
//            var hooksDir = context.ParseResult.GetValueForOption(hooksDirOption)!;
//            var ct       = context.GetCancellationToken();

//            if (!hooksDir.Exists)
//            {
//                Console.Error.WriteLine($"error: hooks directory not found: {hooksDir.FullName}");
//                context.ExitCode = 1;
//                return;
//            }

//            var yamlFiles = hooksDir.GetFiles("*.yaml");
//            if (yamlFiles.Length == 0)
//            {
//                Console.WriteLine($"No YAML files found in {hooksDir.FullName}.");
//                return;
//            }

//            foreach (var yaml in yamlFiles)
//            {
//                ct.ThrowIfCancellationRequested();

//                var hookName   = Path.GetFileNameWithoutExtension(yaml.Name);
//                var scriptPath = Path.Combine(hooksDir.FullName, hookName);

//                var script = BuildHookScript(hookName);
//                await File.WriteAllTextAsync(scriptPath, script, ct);

//                if (!OperatingSystem.IsWindows())
//                {
//                    System.Diagnostics.Process.Start("chmod", $"+x \"{scriptPath}\"")?.WaitForExit();
//                }

//                Console.WriteLine($"[installed] {hookName} → {scriptPath}");
//            }

//            // Tell git to use .githooks/ as the hooks directory.
//            var gitConfigResult = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
//                "git", $"config core.hooksPath {hooksDir}") { UseShellExecute = false });
//            gitConfigResult?.WaitForExit();

//            if (gitConfigResult?.ExitCode == 0)
//                Console.WriteLine($"[configured] git config core.hooksPath = {hooksDir}");
//            else
//                Console.Error.WriteLine("[warning] Could not set git config core.hooksPath automatically. Run: git config core.hooksPath .githooks");
//        });

//        return command;
//    }

//    private static string BuildHookScript(string hookName) =>
//        $"""
//        #!/usr/bin/env sh
//        {ManagedMarker}
//        githooks run --hook {hookName} -- "$@"
//        """;
//}
