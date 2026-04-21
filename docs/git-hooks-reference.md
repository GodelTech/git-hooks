# Git Hooks Reference

This reference lists all currently documented Git hook names in one table.

Hook behavior details, parameters, and version notes: https://git-scm.com/docs/githooks

## Local hooks by everyday action

Use this quick view to see which hooks run on your local machine for common workflows.

| Action | Local hooks that can run | Notes |
| --- | --- | --- |
| Commit (`git commit`) | `pre-commit` → `prepare-commit-msg` → `commit-msg` → `post-commit` | `prepare-commit-msg` is not skipped by `--no-verify`. |
| Push (`git push`) | `pre-push` | Server-side hooks (`pre-receive`, `update`, `post-receive`, etc.) run on remote, not local. |
| Pull (`git pull`, merge mode) | `pre-merge-commit` → `prepare-commit-msg` → `commit-msg` → `post-merge` | `pre-merge-commit` and `commit-msg` run when a merge commit is created. Fast-forward pulls do not create a merge commit. |
| Pull (`git pull --rebase` or pull.rebase=true) | `pre-rebase` → `post-rewrite` | Rebase-based pulls use rebase hooks instead of merge hooks. |

| Hook | Category | Runs where | Typical trigger |
| --- | --- | --- | --- |
| `applypatch-msg` | Commit and patch workflow | Client | `git am` before commit finalization |
| `pre-applypatch` | Commit and patch workflow | Client | `git am` after patch apply, before commit |
| `post-applypatch` | Commit and patch workflow | Client | `git am` after commit |
| `pre-commit` | Commit and patch workflow | Client | Before `git commit` creates a commit |
| `pre-merge-commit` | Commit and patch workflow | Client | After successful merge, before merge commit |
| `prepare-commit-msg` | Commit and patch workflow | Client | Before commit message editor opens |
| `commit-msg` | Commit and patch workflow | Client | Commit message validation/edit step |
| `post-commit` | Commit and patch workflow | Client | After commit creation |
| `pre-rebase` | Commit and patch workflow | Client | Before `git rebase` |
| `post-rewrite` | Commit and patch workflow | Client | After rewrite operations like amend/rebase |
| `post-checkout` | Checkout and merge workflow | Client | After checkout/switch updates worktree |
| `post-merge` | Checkout and merge workflow | Client | After successful merge/pull merge |
| `pre-push` | Push and receive workflow | Client | Before refs are pushed |
| `pre-receive` | Push and receive workflow | Server | Before processing pushed refs |
| `update` | Push and receive workflow | Server | Per-ref check before each ref update |
| `proc-receive` | Push and receive workflow | Server | Custom receive processing for matched refs |
| `post-receive` | Push and receive workflow | Server | After refs are updated |
| `post-update` | Push and receive workflow | Server | After refs update (ref names available) |
| `reference-transaction` | Push and receive workflow | Server and client internals | During reference transaction state changes |
| `push-to-checkout` | Push and receive workflow | Server | Push updates checked-out branch with `updateInstead` |
| `pre-auto-gc` | Maintenance and integration | Client | Before `git gc --auto` |
| `post-index-change` | Maintenance and integration | Client | After index write operation |
| `sendemail-validate` | Maintenance and integration | Client | During `git send-email` validation |
| `fsmonitor-watchman` | Maintenance and integration | Client | File system monitor integration for status scanning |
| `p4-changelist` | Maintenance and integration | Client | During `git p4 submit` changelist validation |
| `p4-prepare-changelist` | Maintenance and integration | Client | During `git p4 submit` changelist preparation |
| `p4-post-changelist` | Maintenance and integration | Client | After successful `git p4 submit` |
| `p4-pre-submit` | Maintenance and integration | Client | Before `git p4 submit` starts |

## Notes

- Hook availability can vary by Git version and enabled integrations.
- Git executes hook scripts by exact file name from the configured hooks path (`core.hooksPath`).
