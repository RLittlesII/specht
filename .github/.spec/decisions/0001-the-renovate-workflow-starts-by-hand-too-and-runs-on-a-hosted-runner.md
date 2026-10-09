---
title: "Decision 0001: the Renovate workflow starts by hand too, and runs on a hosted runner"
description: "The Renovate workflow has a manual trigger beside its schedule, opens its pull requests with the repository secret RENOVATE_TOKEN, and runs on GitHub's hosted ubuntu-latest runner; self-hosted means self-hosted Renovate, not a self-hosted runner"
type: decision
---

# Decision 0001: the Renovate workflow starts by hand too, and runs on a hosted runner

**Date:** 2026-10-09
**Decided by:** the owner, while item 0073 was planned

## The call

1. **A manual trigger beside the schedule.** The Renovate workflow can also be
   started by hand. It is a convenience, not a claim: no B-id states it, and
   B-015 still claims only the scheduled run.
2. **The token secret is `RENOVATE_TOKEN`.** The repository secret C-8 requires
   for opening pull requests is named `RENOVATE_TOKEN`.
3. **The run uses GitHub's hosted runner.** The job runs on `ubuntu-latest`,
   and `renovatebot/github-action` runs the `ghcr.io/renovatebot/renovate`
   container on that runner. "Self-hosted" in A-2 and OQ-6 means a self-hosted
   Renovate, as opposed to the hosted Renovate app; it never meant a
   self-hosted runner.

## Why

- **The manual trigger.** Without it, the first observation of B-015 and B-016
  after the workflow merges waits for the next Monday's schedule (A-1). A run
  started by hand executes the same job with the same committed configuration
  (C-2), so it changes nothing a claim states.
- **The secret's name.** The workflow reads the secret by name, and `0055-F8`
  B-007 lists every secret a workflow reads by name, so the name is fixed once
  here rather than chosen by whoever writes the workflow.
- **The hosted runner.** The owner asked where the Renovate container runs and
  kept the design. GitHub's `ubuntu-latest` image has Docker preinstalled (its
  image manifest lists Docker Client and Server 28.0.4), which is all
  `renovatebot/github-action` needs to start the container. No machine of the
  owner's is involved.

## Rejected

- **A claim for the manual trigger:** a test would prove only that GitHub
  honours its own trigger, and B-015 already proves the run itself.
- **A self-hosted runner:** it would make the owner keep a machine registered,
  patched and online for one weekly job, for nothing the hosted runner lacks.
- **Leaving the secret unnamed until the workflow is written:** `0055-F8`'s
  checklist would then follow the code instead of the specification.

## Affects

`A-2` (amended); `C-8` (amended); § 5 rows 6 and 7 (new); `0055-F8` B-007,
which lists the secret.

## Reversal

None.
