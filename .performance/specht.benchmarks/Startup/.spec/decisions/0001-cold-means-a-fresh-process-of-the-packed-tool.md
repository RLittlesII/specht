---
title: "Decision 0001: cold means a fresh process of the packed tool"
description: "Cold start is the packed tool, installed into a temporary local tool manifest, started as a new process for each measured check of the one-specification tree; first-run-after-boot timing, the build output's executable and a no-op invocation were turned down"
type: decision
---

# Decision 0001: cold means a fresh process of the packed tool

**Date:** 2026-10-09
**Decided by:** spec-author, reading the owner's scope "the packed tool's cold
start" (2026-10-09); the owner may reverse it

## The call

- **Cold** is a fresh process per measured invocation (B-003, C-6): the
  runtime starts, loads the tool's assemblies, JITs what it runs, does the
  check and exits. The operating system's file cache is whatever the previous
  iteration left.
- **The packed tool** is the `.nupkg` the build packed, installed from the
  local package output into a local tool manifest inside the benchmark's
  temporary directory, and run through it as a consumer's hook does (B-001,
  C-3, C-4).
- **The work** is one check of the one-specification generated tree (B-002),
  the smallest input on which the check exits `0`, so the engine's share of
  the number is as small as a real invocation can make it.

## Why

- A fresh process is what every call site pays and what the library's default
  toolchain can repeat; a reboot-cold measurement cannot be repeated inside
  one run, so it has no statistics.
- The packed tool through a tool manifest is what a consumer runs; the build
  output's executable skips the tool host's resolution, which is part of the
  cost a hook pays.
- A real check, not a no-op, keeps the measured path one a user takes.

## Rejected

**First run after a reboot or a cache flush.** The cost a user pays once a
day; unrepeatable, and flushing the cache needs privileges a CI runner does
not grant. Cost of rejecting: the number understates the very first run.

**The build output's executable.** Cheaper to set up, and not what a consumer
runs. Cost of rejecting: the benchmark needs `Pack` first (`0109-F1` B-013).

**`--help` or `--explain`** as the measured invocation. Isolates the host, but
measures a command no call site runs. Cost of rejecting: the finding includes a
small check; OQ-2 asks whether a host baseline is wanted beside it.

## Affects

- `0109-F4` B-001 to B-003; C-3, C-4, C-6.
- `0109-F1` B-013.

## Reversal

None.
