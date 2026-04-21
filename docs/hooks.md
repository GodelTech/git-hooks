# Git Hooks Guide

This guide explains what hooks exist in Git and how this tool helps you use them.

## Tool responsibility

`githooks` manages hook setup and Git configuration:

- `create` generates hook script files in a hooks directory
- `install` sets `core.hooksPath`
- `uninstall` removes `core.hooksPath`

## What hooks exist

Git supports many hooks across commit, merge, push, and server workflows.

For a full, tabular reference of all supported hook names and where they run, see [git-hooks-reference.md](git-hooks-reference.md).

Common examples:

- `pre-commit`
- `commit-msg`
- `pre-push`
- `post-commit`

Git executes hooks by file name from the configured hooks directory. To use a hook, create a file with that exact hook name in your hooks path.

Some hooks are command- or integration-specific and may not apply to every workflow.

Reference: https://git-scm.com/docs/githooks

## Commands supported by this tool

## Install

```bash
githooks install
githooks install --scope local --hooks-path .githooks
githooks install --scope global --hooks-path .githooks --force
```

Behavior:

- Default scope is `global`
- Default hooks path is `.githooks`
- If a value already exists, install fails unless `--force` is provided

## Create

```bash
githooks create
githooks create --hooks pre-commit,commit-msg,pre-push
githooks create --hooks pre-commit --hooks commit-msg --hooks pre-push
githooks create --hooks-path .githooks --force
```

Behavior:

- Default scope is `global`
- Default hooks path is `.githooks`
- Default hook set is `pre-commit`, `commit-msg`, `pre-push`, `prepare-commit-msg`, and `post-commit`
- If a hook file already exists, create fails unless `--force` is provided
- Generated hook files resolve `{hook-name}.yaml` from the repository root so they do not depend on the current working directory

## Uninstall

```bash
githooks uninstall
githooks uninstall --scope local
githooks uninstall --scope system
```

Behavior:

- Default scope is `global`
- If no value is set, command exits successfully with "nothing to uninstall"

## Scope selection

- `local`: affects only current repository (`.git/config`)
- `global`: affects current user (`~/.gitconfig`)
- `system`: affects all users on machine (requires elevation in many environments)

Use `local` for repository-specific hooks and `global` for personal defaults.

## Setup patterns

## Single repository workflow

1. Create hook files in the repository hook directory.
2. Run install for local scope.
3. Customize generated hook scripts as needed.

Example:

```bash
githooks create --scope local --hooks-path .githooks
githooks install --scope local --hooks-path .githooks
```

## Organization-wide workflow

1. Define a shared hook standard (which hooks and rules).
2. Choose a common path policy for all developers.
3. Install using global scope for developer machines.
4. Keep hook scripts versioned and distributed consistently.

Example:

```bash
githooks install --scope global --hooks-path .githooks
```

## Troubleshooting

- Git not found: install Git and ensure `git --version` works in shell.
- Local scope error: run command from inside a Git repository.
- Hooks not executing: verify `core.hooksPath` points to the directory containing actual hook files.
- Existing path collision: rerun install with `--force`.
- Permission issues for system scope: run with elevated permissions.

## Planned capabilities

YAML-defined hook pipeline execution is planned. Current `run` behavior is informational: it prints the startup path, repository root, YAML path relative to the repository root, and the YAML file contents.
