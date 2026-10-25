---
title: "Decision 0006: evidence paths are positional arguments"
description: "Why specht evidence takes its paths as positional arguments, keeps --evidence as an alias, and which callers switch"
type: decision
---

# Decision 0006: evidence paths are positional arguments

**Date:** 2026-10-25
**Decided by:** Repository owner; recorded by spec-author

## The call

1. `specht evidence` accepts zero or more positional evidence paths, files or
   directories, with the semantics `--evidence` has today.
2. `--evidence <path>` stays as an alias with identical semantics. It is not
   deprecated and prints no warning.
3. When both forms are given, the supplied set is the union: positional paths
   first, then `--evidence` paths, each in command-line order, then the existing
   de-duplication of one export selected twice. No error.
4. When neither form is given the behaviour is unchanged: exit 2 and the
   `missingCompilation` finding "No evidence was supplied".
5. Help shows `[<path>...]` in the usage line and documents `--evidence` as an
   alias of the positional paths. `docs/cli.md` matches help (0002-F1).
6. Exit codes and the report JSON are unchanged. `formatVersion` stays 1.

## Why

`specht frontmatter <path>` already takes its subject positionally; evidence
paths are the command's subject. A new claim per behaviour, B-023 to B-027,
keeps B-014 (the report surface) at one assertion; amending it would give one
claim five.

## Callers

- Documentation and examples switch to positional: `docs/cli.md`, the
  TestEvidence § 7 flow and `src/codeanalysis/README.md`.
- The usage lines in `AGENTS.md`, `README.md` and `src/tool/README.md` show both
  forms.
- Acceptance helpers and build targets keep `--evidence`, which proves the alias;
  positional coverage is added beside them.

## What the test must prove

The same evidence supplied positionally, through `--evidence`, and through both
yields the same report; with both, the order of the labels follows positional
then `--evidence` command-line order before de-duplication; with neither, exit 2
and the unchanged finding; help and `docs/cli.md` show `[<path>...]` and name
`--evidence` an alias.

## Reversal

None.
