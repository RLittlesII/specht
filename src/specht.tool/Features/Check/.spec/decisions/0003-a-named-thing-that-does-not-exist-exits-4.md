---
title: "Decision 0003: a named thing that does not exist exits 4"
description: "The exit-code set grows a fifth constant, 4 NotFound, for a command-line argument that names something the tool does not have - first `--explain` with a rule id outside the pinned vocabulary; an unshipped pinned version stays 3"
type: decision
---

# Decision 0003: a named thing that does not exist exits 4

**Date:** 2026-10-08
**Decided by:** the repository owner, answering `0001-F3` OQ-1; recorded at their request

## The call

The exit codes are `0` clean, `1` violations, `2` missing root or manifest,
`3` invalid manifest, and **`4` not found**: an argument the user typed names
something the tool does not have. Its first user is `specht --explain` with a
rule id outside the pinned version's vocabulary (`0001-F3` B-020). `4` joins
the constants `0001-F2` C-2 requires, returned and never thrown.

`4` is for what the **command line** names. A version pinned in the
**manifest** that the tool does not ship is invalid configuration and stays
`3`, for the check (`0001-F7` B-003), for `upgrade` (`0001-F7` B-021) and for
`init` (`0001-F4` B-013), so one manifest gets one answer from every command.

## Why

- An unknown rule id is neither a missing root or manifest (`2`) nor a
  manifest the tool rejects (`3`); folding it into either makes the code
  ambiguous to a script branching on it.

## Rejected

**`2`, missing input** — `0001-F3` A-2's proposed default. Keeps four codes,
but a script can no longer tell "no manifest" from "no such rule". Cost of
rejecting: every call site and brief § 5 learn a fifth code.

**`3`, invalid configuration.** Blurs a typo on the command line with a bad
manifest. Cost of rejecting: none.

## Affects

- `0001-F2` C-2: the constant set gains `4`.
- `0001-F3` B-020, A-2 and OQ-1; B-030 keeps `3` for an unshipped pin under `--explain`.
- `0001-F7` B-021 and OQ-5, and `0001-F4` B-013 and OQ-1 (c): an unshipped pin stays `3`.
- brief § 5, AGENTS.md § CLI and the `dotnet-tool` skill's exit-code line.

## Reversal

None.
