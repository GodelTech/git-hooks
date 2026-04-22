# AI Agent Guidelines for git-hooks

This is a .NET CLI tool that configures Git hook execution by managing `core.hooksPath`. AI agents should understand this architecture to be immediately productive.

## Quick Start

**Build**: `dotnet build git-hooks.slnx`
**Test**: `dotnet test git-hooks.slnx --settings .runsettings`
**Pack**: `dotnet pack src/GitHooks/GitHooks.csproj`

## Architecture Overview

```
System.CommandLine (CLI Parsing)
         ↓
Commands (InstallCommand, UninstallCommand)
         ↓
Handlers (InstallHandler, UninstallHandler) ← Business Logic
         ↓
Abstractions (IGitCommandLine, ICommandLine)
         ↓
Infrastructure (Git Process Execution)
```

**Key Design Patterns:**
- **Command Pattern**: `CommandBase` subclasses handle subcommands
- **Handler Pattern**: `IInstallHandler`/`IUninstallHandler` contain business logic
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection for service registration
- **Result Pattern**: `CommandLineResult` encapsulates process outcomes (success, output, error, exit code)

## Project Structure

| Location | Purpose |
|----------|---------|
| `src/GitHooks/Program.cs` | DI setup, CLI command registration, entry point |
| `src/GitHooks/Commands/CommandBase.cs` | Abstract base; defines option parsing & handler delegation |
| `src/GitHooks/Commands/{Install,Uninstall}Command.cs` | Subcommand implementations (`install`, `uninstall`) |
| `src/GitHooks/Handlers/{Install,Uninstall}Handler.cs` | Business logic: Git config validation & updates |
| `src/GitHooks.Infrastructure/` | Reusable CLI abstractions; could be published separately |
| `src/GitHooks.Infrastructure/IGitCommandLine.cs` | Git command wrapper interface |
| `src/GitHooks.Infrastructure/GitConfigScope.cs` | Enum for scope (local, global, system) |

## Core Concepts

### GitConfigScope
```csharp
public enum GitConfigScope { Local, Global, System }
```
- **Local**: Current repository (`.git/config`), requires Git repository context
- **Global**: Current user (`~/.gitconfig`), default scope
- **System**: Machine-wide (`/etc/gitconfig`), usually requires elevated permissions

### CommandLineResult
Encapsulates process execution outcomes with properties: `IsSuccess`, `Output`, `Error`, `ExitCode`

### Command Execution Flow
```
githooks install --scope local --hooks-path .githooks --force
    ↓
InstallCommand.HandleActionAsync(ParseResult)
    ├─ Extracts options: scope, hooksPath, force
    └─ Delegates to _installHandler.HandleAsync(scope, hooksPath, force)
```

## Development Patterns

### Adding a New Command

1. Create subclass in `src/GitHooks/Commands/`:
   ```csharp
   public class MyCommand : CommandBase
   {
       public MyCommand(IMyHandler handler) : base(handler) { }
       protected override void CreateOptions(Command parent) { /* add options */ }
   }
   ```
2. Create handler interface & implementation in `src/GitHooks/Handlers/`
3. Register in `Program.cs`:
   ```csharp
   services.AddTransient<IMyHandler, MyHandler>();
   services.AddTransient<MyCommand>();
   ```
4. Add to RootCommand in `Program.cs`

### Testing Pattern
- Use xUnit v3 with naming convention: `MethodName_Condition_ExpectedResult()`
- Mock `IGitCommandLine` for handler unit tests
- Example:
  ```csharp
  [Fact]
  public async Task HandleAsync_GitNotFound_ReturnsErrorCode()
  {
      // Arrange
      var mockGit = new Mock<IGitCommandLine>();
      mockGit.Setup(g => g.IsAvailableAsync(It.IsAny<CancellationToken>()))
          .ReturnsAsync(false);
      var handler = new InstallHandler(mockGit.Object, mockConsole);

      // Act
      var result = await handler.HandleAsync(GitConfigScope.Global, ".githooks", false);

      // Assert
      Assert.Equal(1, result);
  }
  ```

### Error Handling Pattern
1. Validate preconditions (Git available, inside repo for local scope)
2. Check current state (`GitConfigScope` already set?)
3. Return exit code (0 = success, 1 = failure)
4. Use `Spectre.Console` markup for output:
   ```csharp
   _console.MarkupLine("[red][[ERROR]][/] Git not found in PATH...");
   _console.MarkupLine("[green][[OK]][/] core.hooksPath set to: [blue].githooks[/]");
   ```

### Output Communication
- **Colors via Spectre.Console**: `[red]...[/]`, `[green]...[/]`, `[blue]...[/]`
- **Error prefix**: `[[ERROR]]` for visibility
- **Success prefix**: `[[OK]]` or `[[SUCCESS]]`
- Exit codes: 0 for success, 1 for failure (Unix convention)

## Dependencies & Libraries

| Library | Version | Purpose |
|---------|---------|---------|
| System.CommandLine | 2.0.6 | CLI parsing & command structure |
| Spectre.Console | 0.55.2 | Rich terminal output (colors, markup) |
| Microsoft.Extensions.* | 10.0.6 | DI container, logging |
| xunit.v3 | 3.2.2 | Unit testing framework |

## Code Style & Conventions

Follow [C# Development Guidelines](.github/instructions/csharp.instructions.md):
- **C# 14 features**: Use switch expressions, pattern matching, records where applicable
- **Nullable Reference Types**: Enabled; declare non-nullable by default, check at entry points
- **Naming**: PascalCase public members, camelCase private fields
- **Comments**: XML docs for public APIs
- **Formatting**: File-scoped namespaces, newline before opening braces, use `is null`/`is not null`

## Key Files to Understand

1. **[Program.cs](src/GitHooks/Program.cs)**: Entry point with DI setup and CLI structure
2. **[CommandBase.cs](src/GitHooks/Commands/CommandBase.cs)**: Abstract base showing command pattern
3. **[InstallHandler.cs](src/GitHooks/Handlers/InstallHandler.cs)**: Example of validation → action → output pattern
4. **[GitCommandLine.cs](src/GitHooks.Infrastructure/GitCommandLine.cs)**: Git operation abstraction
5. **[CommandLineResult.cs](src/GitHooks.Infrastructure/CommandLineResult.cs)**: Result object pattern

## Documentation

- [Detailed Git Hooks Reference](docs/git-hooks-reference.md)
- [Hooks Setup Guide](docs/hooks.md)
- [README.md](README.md) – Project overview & usage examples

## Common Tasks

| Task | How |
|------|-----|
| Add a Git operation | Add method to `IGitCommandLine` interface & `GitCommandLine` implementation |
| Extend handler logic | Modify handler's `HandleAsync()` method; consider pre/post conditions |
| Handle new error case | Use `Spectre.Console.MarkupLine()` for user message; return exit code 1 |
| Test a handler | Mock `IGitCommandLine`; verify behavior & output |
| Verify code builds | Run `dotnet build git-hooks.slnx` before committing |
| Verify tests pass | Run `dotnet test git-hooks.slnx --settings .runsettings` |

## Architecture Principles

- **Layered Design**: CLI parsing → Command → Handler → Git abstraction
- **Dependency Inversion**: Depend on `IGitCommandLine`, not concrete Git implementation
- **Fail-Fast Validation**: Check preconditions early; return meaningful exit codes
- **Idempotent Operations**: Setting same value twice should succeed
- **Reusable Abstractions**: `GitHooks.Infrastructure` library can be consumed separately

