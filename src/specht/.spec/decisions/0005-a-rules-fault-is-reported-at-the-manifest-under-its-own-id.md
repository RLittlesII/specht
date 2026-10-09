---
title: "Decision 0005: a rule's fault is reported at the manifest, under its own id"
description: "A rule that throws is isolated: the other rules still report, and the failing rule reports one error under its own id, at the manifest's path with no line and no identifier, naming only the rule and the exception's type; what it yielded before the fault is dropped and it is not retried"
type: decision
---

# Decision 0005: a rule's fault is reported at the manifest, under its own id

**Date:** 2026-10-09
**Decided by:** the owner, for the id and the severity ([ADR-0005](../../../../.spec/adr/0005-typed-injected-stages-not-a-chain.md) (c), 2026-10-09); spec-author, for the file, the line, the identifier and the message, which ADR-0005 (c) assigns to the claim

## The call

1. **A rule that throws does not end the run.** Every other rule is still
   evaluated and reported (B-012).
2. **The failing rule reports one violation in place of its output** (B-013):
   - **rule id:** the failing rule's own `Id` (owner);
   - **severity:** error, fixed; `0001-F5` B-010's re-grade does not apply to
     it (B-015; owner);
   - **file:** the manifest's path relative to the root,
     `.spec/schema/spec-structure.schema.json` under schema version 1 (C-8);
   - **line:** `0`, so the rendered line is `<manifest path>: error SPEC###: <message>`,
     the whole-file form `SpecViolation.ToString()` already gives
     (`src/specht/SpecViolation.cs:25-27`) and `0001-F2` B-001's line shape
     admits;
   - **identifier:** none;
   - **message:** names the failing rule's id and the exception's type name,
     and carries no text from the exception's own message (B-016).
3. **What the rule yielded before it threw is discarded** (B-014), and the rule
   is not retried (C-11).

## Why

- **The manifest as the file.** It is the one file every run that reaches the
  rules is guaranteed to have: a missing or unparseable one exits `2` or `3`
  before any rule runs (`0001-F2` B-006, B-007). It is repository-relative by
  construction (C-4, brief § 9), it is the file that selects which rules run
  (`0001-F5`), and a CI annotation on it lands on a file the consumer owns.
- **Line `0`.** A fault belongs to no line of any file; `0` is the existing
  "the file as a whole is at fault" value, so no rendering changes.
- **No text from the exception's message.** The runtime localises it and it
  can carry an absolute path, so copying it breaks C-3 and C-4. The type name
  is neither localised nor machine-dependent, and tells the author which kind
  of fault to look for.
- **Discard, no retry.** All-or-nothing per rule keeps the report a function of
  the tree, not of how far a fault got (C-3). A retry can only repeat a
  deterministic fault, or hide a transient one behind a second read.

## Rejected

- **A new `SPEC###` id for faults:** B-001 and C-5 forbid it before
  `0001-F7` (owner, ADR-0005 (c)).
- **The fault re-gradable like the rule's findings:** a consumer who lowered
  the rule to warning would turn a crash into exit `0` (owner, ADR-0005 (c)).
- **The root (`.`) or an empty file:** an empty file breaks `0001-F2` B-001's
  line shape, and `.` names no file a CI annotation can land on.
- **The rule's source file:** it is not in the consumer's tree, and its path is
  outside the root.
- **The specification the rule was reading when it threw:** a rule evaluates
  the whole model, so no single specification is known, and naming one would
  depend on how far the fault got.
- **Keeping what the rule yielded before the fault:** the report would vary
  with where in its iteration a rule failed.

## Affects

`B-012` to `B-016` and `C-11` (new); § 5 rows 12 and 13; § 10 (the reading of
`C-9` for a fault); `0001-F2` B-015; `0001-F5` § 5 row 7.

## Reversal

None.
