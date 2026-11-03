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
