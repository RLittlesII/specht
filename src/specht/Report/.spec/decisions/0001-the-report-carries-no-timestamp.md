---
title: "Decision 0001: the report carries no timestamp"
description: "generatedAtUtc is dropped from the report document so a report is a pure function of the tree, the manifest and the tool version; keeping it and excluding it from comparison was turned down"
type: decision
---

# Decision 0001: the report carries no timestamp

**Date:** 2026-10-07
**Decided by:** the repository owner, in the second requirements session (README § 6, "Report timestamp"; `REQUIREMENTS.md` § Revision log); recorded by spec-author

## The call

The report document carries no `generatedAtUtc` and no other field read from
the clock, the machine or the environment. Two runs on the same tree with the
same manifest and the same tool version produce identical documents.

## Why

- Should-7 asks that `hooked`'s run be unchanged by the extraction. With the
  stamp gone the comparison is byte equality instead of "every field but one",
  and the test that proves it has no exclusion list to maintain.
- The tool is deterministic and offline by design (README § 9). A stamp is the
  one field that contradicted that, and nothing consumed it: `hooked`'s target
  logged the report path, not the time.
- A committed report with a stamp is a diff on every run. The reports are
  gitignored, but the lesson `format.json` taught `hooked` is that a generated
  file with machine-specific content gets committed eventually.

## Rejected

**Keep the stamp and exclude it from comparison.** The `hooked` draft
specification wrote its baseline claim that way ("in every field but
`generatedAtUtc`").

- Every equality test carries an exclusion, and every consumer that diffs two
  reports has to know to strip the field.
- Cost of rejecting: a reader of a report file cannot tell when it was written.
  The file's own modification time says that, and CI's log says it better.
  Taken.

**Move the stamp behind a flag.** Emit it only when asked.

- A second shape for the same document, and a schema with an optional field
  whose presence changes whether two reports compare equal.
- Cost of rejecting: none stated by any consumer. Taken.

## Affects

- `0001-F3` § 3 B-006, § 4 C-3.
- `0001-F1` § 3 B-004: the baseline comparison still names the field, because
  `hooked`'s report at `6afe8ab` carries it; after this Feature the exclusion
  is one-sided.

## Reversal

None.
