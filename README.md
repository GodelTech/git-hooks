# git-hooks

Dotnet tool to configure Git hook execution by managing `core.hooksPath`.

## What this tool does today

This project currently supports three operations:

- `install`: sets `core.hooksPath` for a selected Git config scope.
- `create`: creates hook script files in the selected hooks directory.
- `uninstall`: unsets `core.hooksPath` for a selected Git config scope.

The tool can generate hook file stubs and configure where Git looks for them.

## Common Git hooks you can use

After setting `core.hooksPath`, place standard Git hook files in that directory. Common examples:

- `pre-commit`
- `commit-msg`
- `pre-push`
- `post-commit`

For a full hook list, see `docs/git-hooks-reference.md` and Git docs: https://git-scm.com/docs/githooks

## Quick start

### Install globally (default scope)

```bash
githooks install
```

### Create default hook files

```bash
githooks create
```

### Install for current repository

```bash
githooks install --scope local --hooks-path .githooks
```

### Overwrite existing value

```bash
githooks install --scope global --hooks-path .githooks --force
```

### Remove configuration

```bash
githooks uninstall
githooks uninstall --scope local
```

## Scope behavior

- `local`: current repository only (`.git/config`), must be inside a Git repository.
- `global`: current user (`~/.gitconfig`), default.
- `system`: machine-wide, usually requires elevated permissions.

## Troubleshooting

- If Git is not in PATH, install Git and retry.
- If `--scope local` fails, run inside a Git repository.
- If hooks do not run, confirm `core.hooksPath` points to the folder containing your hook files.
- If a path is already configured, use `--force` to replace it.

## Roadmap

YAML-defined hook pipelines are planned, but are not available as an executable command in the current implementation.

## Detailed docs

See [docs/hooks.md](docs/hooks.md) for a fuller guide, including setup patterns for single-repo and organization-wide usage.
