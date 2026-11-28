---
title: "Decision 0004: an onboard failure writes its reason on stderr, and an unwritable file exits 4"
description: "specht onboard writes an input failure's reason on stderr and nothing on stdout, a usage line for an absent --output, and exits 4 with the reason on stderr when the file cannot be written"
type: decision
---

# Decision 0004: an `onboard` failure writes its reason on stderr, and an unwritable file exits `4`

**Date:** 2026-11-27
**Decided by:** the repository owner, answering "Do you accept reading (a) for OQ-3?" with "Accept (a)" (2026-11-27); recorded by `spec-author`

## The call

The owner accepted OQ-3's reading (a), as put to them:

1. On an input failure, the reason goes to stderr and nothing goes to stdout.
2. An absent `--output` prints a usage line on stderr.
3. A file the command cannot write prints the reason on stderr and exits `4`,
   as `specht evidence` does for a report file it cannot write.

Derived by `spec-author`, not the owner's call: an absent `--output` also
writes nothing on stdout. Call 1 covers input failures, and B-007 treats an
absent `--output` as one (decision 0003). B-007 is unchanged; exit `4` for an
unwritable file is a case its mapping does not name, which B-010 states.

## Why

The owner accepted the reading and gave no reason. The reading's own: a failed
run leaves stdout empty, so nothing downstream reads a partial report, and
exit `4` matches the tool's code for a caller-named report it cannot write.

## Rejected

- **Reading (b), the terminal report printed before an unwritable file is
  reported.** spec-author's reading of the cost: a failed run would leave a
  report on stdout that the file does not hold, which B-003 requires be the
  same.

## Affects

- `0001-F15` OQ-3, resolved; B-008 to B-010, added with their scenarios and
  § 9 rows; items `0384`, `0385` (B-008, B-009) and `0386` (B-010).

## Reversal

None.
