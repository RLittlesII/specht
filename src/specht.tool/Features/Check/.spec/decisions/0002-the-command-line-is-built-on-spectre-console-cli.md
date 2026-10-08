---
title: "Decision 0002: the command line is built on Spectre.Console.Cli"
description: "specht parses its command line with Spectre.Console.Cli, in the shape the spectre-cli skill describes; System.CommandLine and hand-parsed arguments were turned down. Replaces the withdrawn C-1"
type: decision
---

# Decision 0002: the command line is built on Spectre.Console.Cli

**Date:** 2026-10-07
**Decided by:** the repository's chosen stack (README § "Agent skills", `spectre-cli`; AGENTS.md names `src/specht.tool` the Spectre.Console.Cli host); recorded by spec-author after the 2026-10-07 review found it stated as `0001-F2` C-1

## The call

The `specht` command line is parsed by `Spectre.Console.Cli`, in the shape the
`spectre-cli` skill describes: an `AsyncCommand` per command, its `Settings`
nested inside it, `Validate()` for cross-option rules, and an injected
`IAnsiConsole` for every write.

## Why

- The framework was chosen for the tool before this Feature was written; the
  repository carries a technology skill for it and AGENTS.md describes the tool
  project as its host. A specification restating it as a constraint was an
  imported pattern, not a limit on what the command does.
- An injected `IAnsiConsole` is what lets `0001-F2` C-7's command tester capture
  stdout and stderr against a synthetic root.

## Rejected

**`System.CommandLine`.** Workable, but a second CLI framework beside the one
the repository's skills describe. Cost of rejecting: none.

**Hand-parsed `args`.** No generated help, no validation stage, and every
command repeats the parsing. Cost of rejecting: none.

**A settings class in its own file, or a static `AnsiConsole.*` call.** The
first splits one command across two files; the second writes to the real
console, which the command tester cannot capture. Cost of rejecting: none.

## Affects

- `0001-F2` § 4 C-1, withdrawn in place in favour of this record.
- `0001-F2` C-7 relies on the injected console.

## Reversal

None.
