---
title: "Decision 0006: evidence paths are positional arguments"
description: "Why specht evidence takes its paths as positional arguments, why the --evidence alias was then removed, and which callers switch"
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

**2026-10-25**, repository owner: "So what I don't like about `--evidence` we
are calling it on `specht evidence`. Would you have `dotnet test --test`?",
then "drop it".

Points 2 and 3 are reversed. `--evidence` is removed with no alias, no
deprecation period and no warning; passing it is an unknown option like any
other, the CLI parser's usage error. Evidence paths are positional only:
repeated paths are supplied in command-line order, then the existing
de-duplication. Point 5 now reads: help shows `[<path>...]` and no evidence
option. Points 1, 4 and 6 stand.

The owner then stated the rule, 2026-10-25: "Doesn't _have_ to be positional,
it just can't repeat the dern command verb/noun". The governing rule is that a
command's option never repeats the command's own name, verb or noun. Positional
paths are the chosen way to satisfy it for `specht evidence`, because they match
`specht frontmatter <path>`.

Considered and not chosen:

- A differently named option, such as `--input`: it satisfies the rule too; the
  positional form was chosen for consistency with `specht frontmatter`.
- Keeping `--evidence` as an alias: it repeats the command name, the objection
  the owner raised.
- A deprecation period with a warning: the owner chose removal outright.

B-024 is withdrawn; B-023, B-025, B-026 and B-027 are amended to the positional
form.

The Callers bullets on the usage lines and on acceptance helpers and build
targets, and the "What the test must prove" paragraph, are superseded. In their
place: the usage lines in `AGENTS.md`, `README.md` and `src/tool/README.md` show
the positional form only; acceptance helpers, build targets and tests name
evidence positionally. The tests prove that repeated positional paths are
supplied in command-line order, then de-duplicated; that no evidence path gives
exit 2 with the unchanged finding; and that help and `docs/cli.md` show
`[<path>...]` and no evidence option.
