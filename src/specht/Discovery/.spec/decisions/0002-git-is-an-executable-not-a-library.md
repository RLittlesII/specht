---
title: "Decision 0002: git is an executable, not a library"
description: "Git-backed discovery runs the git executable on PATH as a child process; a git library in the package set was turned down"
type: decision
---

# Decision 0002: git is an executable, not a library

**Date:** 2026-10-07
**Decided by:** spec-author, from brief § 3 (the engine's three dependencies) and brief § 5 "Discovery cost" (`git ls-files`); raised by spec-reviewer finding 10

## The call

Git-backed discovery runs the `git` executable found on `PATH` as a child
process. No git library enters the package set.

## Why

- brief § 3 names the engine's dependencies as exactly three - Markdig,
  YamlDotNet, JsonSchema.Net - pinned to `hooked`'s versions. A fourth is a
  change to that list, not a detail of discovery.
- brief § 5 drafts discovery as `git ls-files`, which is the executable's
  command, with its ignore handling (`--exclude-standard`) exactly as the
  developer's own git applies it.
- Without git on `PATH` the tool still runs, by walking (B-006), so the
  executable is an accelerator, not a prerequisite.

## Rejected

**A managed git library** (for example LibGit2Sharp).

- A fourth dependency, carrying native binaries per platform into a dotnet
  tool package.
- Its ignore-rule evaluation is its own, not the user's git's, so the set it
  returns could differ from what `git status` shows.
- Cost of rejecting: a process start per run, and behaviour that depends on
  the installed git. Taken; OQ-2 records the case where that git fails.

## Affects

- § 2 A-1, resolved by this decision.
- § 4 C-2, reworded to drop the library half.

## Reversal

None.
