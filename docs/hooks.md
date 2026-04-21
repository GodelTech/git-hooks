# Git Hooks Guide

This guide explains what hooks exist in Git and how this tool helps you use them.

## Tool responsibility

`githooks` manages Git configuration only:

- `install` sets `core.hooksPath`
- `uninstall` removes `core.hooksPath`

It does not generate or maintain hook script files.

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

1. Run install for local scope.
2. Create the hooks directory if needed.
3. Add hook files (`pre-commit`, `commit-msg`, etc.) to that directory.

Example:

```bash
githooks install --scope local --hooks-path .githooks
mkdir -p .githooks
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

YAML-defined hook pipeline execution is planned, but no `run` command is available in the current implementation.
