---
title: "Decision 0002: the report file is exempt from never-write and overwrites"
description: "--report <path> is the one file a check run writes, at the path the caller names, replacing any file already there; refusing an existing file and appending to it were turned down"
type: decision
---

# Decision 0002: the report file is exempt from never-write and overwrites

**Date:** 2026-10-07
**Decided by:** the repository owner, answering the spec-reviewer's finding on `0001-F3` B-002 (2026-10-07); recorded by spec-author

## The call

`--report <path>` writes the report document to the path the caller names,
creating the directories above it. It is the one exception to "the tool never
writes into a consumer's tree" for a check run. A file already at that path is
replaced by this run's document, without a prompt and without an error.

## Why

- The report file exists for CI. A pipeline reruns on the same workspace and
  names the same path every time; a run that refused an existing file would
  fail the second time for no reason the tree caused.
- The caller names the path. Writing where the caller asked is the output the
  option exists for, not a write into the specification tree; the rule it is
  exempt from protects documents, and the tool still never writes one.
- The document is a function of the tree, the manifest and the tool version
  (decision 0001, C-3). The file an overwrite replaces carries nothing the new
  run could not reproduce from the same inputs.

## Rejected

**Refuse when the file exists.** The `init` rule: never overwrite.

- Every CI rerun on a persistent workspace needs a delete step first, and a
  forgotten one turns a clean tree into a failed build.
- Cost of rejecting: a mistyped `--report` path that names a file the caller
  cares about is replaced. The caller typed the path. Taken.

**Append to the file.** Accumulate runs in one file.

- The file stops being one JSON document, so it no longer validates against
  the report schema (B-008) and nothing can parse it as a report.
- Cost of rejecting: none stated by any consumer. Taken.

**Write no file; `--json` to stdout only.** Let the caller redirect.

- README § 5 fixes `--report` as part of the command, and a redirect loses the
  line stream that `--report` keeps on stdout for the log.
- Cost of rejecting: one write path in the tool. Taken.

## Affects

- `0001-F3` § 3 B-002 (withdrawn), B-022, B-023, B-024, B-025; § 4 C-7; § 5 row 7.
- `0001-F2` § 3 B-012: no run writes under the root except this file.
- README § 9, `AGENTS.md`, the epic's "never writes" sentence: amended to
  name this exception.

## Reversal

None.
