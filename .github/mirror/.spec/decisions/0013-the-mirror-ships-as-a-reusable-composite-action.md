---
title: "Decision 0013: the mirror ships as a reusable composite action"
description: Reuse the existing mirror from other repositories without their installing the build or skills
type: decision
---

# Decision 0013: the mirror ships as a reusable composite action

**Date:** 2026-11-02
**Decided by:** repository owner, requesting reuse across repositories and selecting hosting in specht.

The action lives at `.github/actions/mirror/action.yml`. GitHub downloads the
action repository; the entry point resolves the existing mirror and shared reader
there, while the mirrored tree comes from the caller’s workspace or `root` input.
No parser is copied and no consumer skill installation is required. Decision
0007’s shared-reader requirement and all reconciliation behavior remain in force.

Inputs are `token`, `root`, `repository`, `before`, `after`, `backfill` and
`dry-run`. Apply is the default; dry-run retains the existing read-only plan.
The caller supplies a full-history checkout and a workflow token with contents
read, issues write and pull requests write. Ubuntu runners provide Python 3
and `gh`. The caller owns triggers and concurrency; the action does not checkout
or change the caller’s repository. This repository invokes the same action locally
from its generated workflow after running the Python tests directly. This
supersedes the target-invocation requirement recorded from pull request #281
for the mirror workflow; the NUKE target remains available locally, and NUKE
continues to own workflow generation.

Project boards, new item formats, other GitHub hosts, configurable branch names,
new reconciliation behavior and action release tags are outside this change.
The existing B-064 gap remains a separate item.

## Addendum: consumer process scripts

**Date:** 2026-11-04
**Decided by:** repository owner, requesting configuration for a consumer’s own process script in pull request #691.

The optional `script` input selects an executable path relative to `root` and
replaces the bundled mirror. Its interpreter comes from its shebang; no shell
command string is evaluated. The action starts it in the consumer root with the
same separate mirror CLI arguments and inherited environment, including `GH_TOKEN`
and `GITHUB_STEP_SUMMARY`. The `--root` argument is resolved before changing
directories. The script owns reconciliation, dry-run behavior and
summary output. The bundled mirror remains the default and its claims apply to
that default; custom-script behavior belongs to the consumer. No plugin API,
additional item parser or automatic fallback is introduced. A script’s exit code
is returned; a failure to start exits 2 with a script startup error.

This extends the packaging-only scope above for consumer-owned scripts; it does
not add new formats or reconciliation rules to the bundled mirror.

## Review addendum: explicit script and C# action tests

**Date:** 2026-11-04
**Decided by:** repository owner in review 5776968566 and its follow-up: new coverage belongs in C# tests or an F# script; a Python test gate is not required to consume the action.

The generated mirror workflow passes `script: .github/mirror/mirror_items.py`
explicitly. The existing script becomes executable and keeps `-B` in its
shebang, preserving its existing process and avoiding bytecode writes.
The job invokes the action without running a test suite. Reusable-action tests
live in the existing C# integration test project and run in ordinary CI;
`test_action.py` is replaced. The legacy mirror plan and adapter tests and their
manual `MirrorTest` target stay; this change does not rewrite that engine or
add a Python test runner to CI. This supersedes the direct Python test step
chosen above and amends B-069 and C-13. Omitting `script` still selects the
bundled mirror for consumers.
