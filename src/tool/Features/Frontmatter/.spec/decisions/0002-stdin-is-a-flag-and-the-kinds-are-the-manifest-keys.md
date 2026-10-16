---
title: "Decision 0002: the pipe is --stdin, and the kinds are the manifest keys"
description: "--kind takes feature, task and epic, the manifest's frontmatterSchemas keys; an issue reuses task; no --json and a summary of the two counts; a missing option or a conflicting pair exits 2 with the usage line; the pipe is the flag --stdin, since the command-line library rejects a lone -"
type: decision
---

# Decision 0002: the pipe is --stdin, and the kinds are the manifest keys

**Date:** 2026-10-15
**Decided by:** the repository owner (2026-10-15); recorded by spec-author

## The call

1. `--kind` takes `feature`, `task` and `epic`, the manifest's
   `frontmatterSchemas` keys (`0001-F5` decision 0003). The draft's
   `feature-spec` named a retired file name. A-2, B-003 and B-020 name the
   three. This sets the kind names.
2. A GitHub issue reuses the `task` kind: no `issue` kind, no new frontmatter
   schema, no new schema version. This resolves OQ-1.
3. The defaults stand. No `--json` for this command, so § 5 row 5 stands and
   `0001-F3` owns the report document. The summary is the error count and the
   warning count alone, B-013 as written; the check's other summary lines -
   the specification count per layout, the item count, the rule ids
   evaluated - are not printed for a document that has no layout. This
   resolves OQ-2 and OQ-3.
4. When `--kind` is absent, or neither a document nor `--stdin` is given, or
   both are given, the command writes the usage line on stderr, writes
   nothing on stdout, and exits `2`, the missing-input constant of `0001-F2`
   C-2. The command-line library's `-1` validation result never surfaces.
   B-026 claims it. This resolves OQ-4.
5. The pipe is the flag `--stdin`, not a lone `-` as the document argument,
   and the whole document is read from standard input at once. A path and
   `--stdin` together, or neither, is call 4's usage failure. The command line
   is `specht frontmatter --kind <kind> (<path> | --stdin) [--root <dir>]`
   (A-1). B-002 and B-012 amended; § 2 row 2, the agent, withdrawn. This
   resolves OQ-5, raised the same day.

## Why

Recorded by spec-author from the session:

- The manifest's `frontmatterSchemas` keys are the one name a consumer already
  writes for a kind. A schema file name moves with the schema version
  (`0001-F5` decision 0003 and its reversal); `feature-spec` was one that had.
- An issue is a task-shaped record. A kind of its own is a new frontmatter
  schema, so a new schema version (brief § 4), for a document the `task`
  schema already describes.
- A missing option or a conflicting pair is a missing input, the meaning `2`
  carries for the check; it is not a thing named and not found, which is `4`
  (`0001-F2` decision 0003).
- Spectre.Console.Cli's tokenizer rejects a lone `-` before any command runs:
  `Error: Option does not have a name.`, exit 255, and a `--` separator does
  not help. Verified on the restored tool. So `-` cannot mark standard input
  under the chosen command-line library, and a flag can.
- The tool's callers are a person or a CI step; an agent reaches the tool
  through a protocol of its own, not a pipe. The pipe stays for the CI
  persona, read whole, and the agent persona is withdrawn.

## Rejected

The owner was offered these and chose against them; the cost after each is
spec-author's reading:

- **`feature-spec` as a kind name, or the schema file names
  (`feature.schema.json`).** `feature-spec` is a retired file name, and a file
  name moves with the schema version. Cost of rejecting: a caller who types
  the old name is told the three kinds under B-020 and exits `4`.
- **An `issue` kind with a schema of its own.** Cost of rejecting: an issue
  body is checked as a `task`, so a key an issue needs and a task does not has
  no schema here; adding one later is a new schema version (brief § 4).
- **`--json`, and the full check summary.** Cost of rejecting: a CI step that
  branches on the report document uses the check, not this command, and a
  caller reads the two count lines; a later `--json` is a new claim and
  `0001-F3`'s report for one document.
- **Exit `4`, or the command-line library's `-1`, for the usage failure.**
  `4` means a named thing was not found (`0001-F2` decision 0003), and `-1`
  is not one of `0001-F2` C-2's constants (C-6). Cost of rejecting: a caller
  cannot tell a missing option from a missing root or manifest by the exit
  code alone; stderr says which.
- **A lone `-` as the document argument.** Impossible under the chosen
  command-line library (Why). Cost of rejecting:
  `specht frontmatter --kind task -` is refused by the command-line library
  before the command runs, with its own message and exit code, not B-026's;
  a caller who knows the shell convention types `--stdin` instead.
- **A path only, with no pipe.** Cost of rejecting: the command keeps two
  inputs and the conflict B-026 handles, so the CI persona keeps its pipe.

## Affects

- A-1, A-2.
- B-002, B-003, B-012, B-013, B-020; B-026 added; B-027, spec-author's
  reading, not an owner call.
- § 2 row 2, the agent, withdrawn.
- § 5 row 1, which names `--stdin`; row 5, whose reason now reads decided
  against; row 8, added.
- OQ-1 to OQ-4 resolved; OQ-5 raised and resolved.
- C-6 unchanged: `2` and `4` are `0001-F2` C-2's constants, with the meanings
  the check gives them.

## Reversal

None.
