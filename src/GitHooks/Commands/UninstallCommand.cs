//using System.CommandLine;
//using System.CommandLine.Invocation;

//namespace GitHooks.Commands;

//public static class UninstallCommand
//{
//    private const string DefaultHooksDir = ".githooks";

//    public static Command Create()
//    {
//        var hooksDirOption = new Option<DirectoryInfo>(
//            "--hooks-dir",
//            () => new DirectoryInfo(DefaultHooksDir),
//            $"Directory containing hook scripts. Default: {DefaultHooksDir}");

//        var command = new Command("uninstall", "Remove managed hook scripts from .githooks/ and unset core.hooksPath.")
//        {
//            hooksDirOption
//        };

//        command.SetHandler(async (context) =>
//        {
//            var hooksDir = context.ParseResult.GetValueForOption(hooksDirOption)!;
//            var ct       = context.GetCancellationToken();

//            if (!hooksDir.Exists)
//            {
//                Console.WriteLine("Hooks directory not found. Nothing to uninstall.");
//                return;
//            }

//            var removed = 0;
//            foreach (var file in Directory.EnumerateFiles(hooksDir.FullName))
//            {
//                ct.ThrowIfCancellationRequested();

//                // Skip YAML files — only remove generated shell scripts.
//                if (file.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
//                    continue;

//                string content;
//                try { content = await File.ReadAllTextAsync(file, ct); }
//                catch { continue; }

//                if (!content.Contains(InstallCommand.ManagedMarker, StringComparison.Ordinal))
//                    continue;

//                File.Delete(file);
//                Console.WriteLine($"[removed] {Path.GetFileName(file)}");
//                removed++;
//            }

//            if (removed == 0)
//                Console.WriteLine("No managed hook scripts found.");
//            else
//                Console.WriteLine($"Uninstalled {removed} hook script(s).");

//            // Unset core.hooksPath so git falls back to .git/hooks/.
//            var gitConfigResult = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
//                "git", "config --unset core.hooksPath") { UseShellExecute = false });
//            gitConfigResult?.WaitForExit();

//            if (gitConfigResult?.ExitCode == 0)
//                Console.WriteLine("[configured] Unset git config core.hooksPath.");
//            else
//                Console.Error.WriteLine("[warning] Could not unset git config core.hooksPath. Run: git config --unset core.hooksPath");
//        });

//        return command;
//    }
//}
