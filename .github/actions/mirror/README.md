---
title: Work-item mirror action
description: Reuse the one-way work-item, epic and milestone mirror in another repository
type: reference
---

# Work-item mirror

Mirrors local `**/.issue/<four-digit-id>-*.yml` work items and
`epics/*/epic.md` as GitHub issues, and versioned sections in `.issue/.goal`
as milestones. It preserves the existing mirror’s labels, parent links,
delivery comments, markers and rerun behavior. Local files own delivery state;
GitHub edits are overwritten and never imported.

Use an Ubuntu runner with Python 3, git and the GitHub CLI (`gh`) available.
GitHub-hosted `ubuntu-latest` supplies these. The action ships its mirror and
shared reader from specht; the consumer needs no .NET build, Python packages
or skill installation. The bundled script’s work-item format follows
[the item schema](../../../.spec/templates/item.yml). Project boards and
alternative item formats are outside the bundled script’s scope. Select a
consumer script to run your own process.

## Consumer workflow

Save this as `.github/workflows/mirror.yml` in the consumer. Replace `<commit-sha>`
with the specht commit containing the action. A branch reference such as `main`
also works; a commit pin keeps the implementation fixed. No separate action
release tag is introduced by this change.

```yaml
name: Work-item mirror
on:
  push:
    branches: [main]
    paths: ["**/.issue/**", "epics/**/epic.md", ".github/workflows/mirror.yml"]
  workflow_dispatch:
    inputs:
      backfill:
        description: Post every missing delivery comment
        type: boolean
        default: false
permissions:
  contents: read
  issues: write
  pull-requests: write
concurrency:
  group: work-item-mirror
  cancel-in-progress: false
jobs:
  mirror:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      - uses: RLittlesII/specht/.github/actions/mirror@<commit-sha>
        with:
          token: ${{ github.token }}
          before: ${{ github.event.before }}
          after: ${{ github.event.after }}
          backfill: ${{ inputs.backfill || false }}
```

The caller owns checkout, permissions, triggers and concurrency. Mirror only
trusted `main` state; fetch full history so delivery attribution is available.
The existing renderer names `main` in provenance. Serialize runs to avoid
concurrent writes. GitHub may replace a pending run; manual backfill posts
missing delivery comments after that happens.

## Inputs

| Input        | Default                    | Meaning                                                                             |
| ------------ | -------------------------- | ----------------------------------------------------------------------------------- |
| `token`      | `${{ github.token }}`      | Workflow token for the receiving repository, with the permissions above.            |
| `root`       | `${{ github.workspace }}`  | Consumer checkout root; pass its path for a checkout in a subdirectory.             |
| `script`     | empty                      | Executable path relative to `root`; replaces the bundled mirror.                    |
| `repository` | `${{ github.repository }}` | Receiving `owner/repository`; ordinarily leave this at the caller’s repository.     |
| `before`     | empty                      | Commit before the push; a zero SHA is accepted for a new branch.                    |
| `after`      | empty                      | Commit after the push; empty means no pushed commits and no push delivery comments. |
| `backfill`   | `false`                    | Post every missing delivery comment; accepts `true` or `false`.                     |
| `dry-run`    | `false`                    | Read GitHub and print planned writes without applying them.                         |

For a first inspection of the bundled mirror, add `dry-run: 'true'`. Dry-run still needs GitHub read
access. Both bundled modes append the mirror’s counts and warnings to the job summary.
Failures from git or `gh` fail the step; rerunning reconciles partial progress.
The bundled mirror writes no files in the consumer checkout.

## Consumer process script

Set `script` to an executable file in your checkout. The path is relative to
`root`, and the script runs from that root. A shebang selects its interpreter,
so shell, Python and other executables work. Pass a file path, including any
spaces, rather than a shell command or inline script.

```yaml
- uses: RLittlesII/specht/.github/actions/mirror@<commit-sha>
  with:
    script: .github/scripts/mirror.sh
    before: ${{ github.event.before }}
    after: ${{ github.event.after }}
    backfill: ${{ inputs.backfill || false }}
```

For example, a wrapper can delegate to your existing process:

```sh
#!/bin/sh
exec python3 .github/scripts/our_mirror.py "$@"
```

Commit the executable bit with `git update-index --chmod=+x .github/scripts/mirror.sh`.
The script receives `--apply` or `--dry-run`, `--root <root>`,
`--repo <owner/repository>`, optional `--before <commit>` and `--after <commit>`,
and `--backfill` when enabled, as separate arguments. For a custom script,
`--root` is resolved before changing directories, so a relative root input
still identifies the checkout. The workflow environment
is inherited, including `GH_TOKEN`, the `MIRROR_*` inputs and
`GITHUB_STEP_SUMMARY`. Install any additional runtime your script needs before
calling the action.

The selected script owns its reconciliation, dry-run handling and summary;
the bundled mirror does not run alongside it or after a failure. Its exit code
becomes the action’s exit code. A missing or unstartable script fails with code 2.
Include your script’s path in the caller’s push filter so changes trigger a run.

When calling the action from this repository, use `./.github/actions/mirror`
after checkout. The generated mirror workflow passes
`script: .github/mirror/mirror_items.py` explicitly, selecting this repository’s
existing executable mirror. Its shebang preserves Python’s `-B` option.
The action and mirror job run no test suite; action coverage runs as C#
integration tests in the ordinary build.

The action follows GitHub’s
[composite action metadata contract](https://docs.github.com/en/actions/reference/workflows-and-actions/metadata-syntax).
