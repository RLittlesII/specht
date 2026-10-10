---
title: "Decision 0006: A violation line names its identifier"
description: "A printed violation line ends with the violation's identifier in square brackets when it has one and is unchanged when it has none; no rule message is reworded, and the report document does not change"
type: decision
---

# Decision 0006: A violation line names its identifier

**Date:** 2026-10-09
**Decided by:** the repository owner, in session; recorded by spec-author

## The call

A violation line states what it targets:

- **With an identifier:** `<path>(<line>): <severity> SPEC###: <message> [<identifier>]`.
- **With none** - a violation of the file as a whole, or a rule's fault
  (`0001-F1` B-013): the line is unchanged, with no brackets.

The identifier is the one the engine already gives the violation (`0001-F1`
B-010) and the report document already prints (`0001-F3` B-007). No rule
message is reworded. The six verdict fields, `--json` and `--report` do not
change.

## Why

- The owner read
  `src/x/.spec/README.md(213): error SPEC060: spec_status is 'approved' while § 9 still carries a 'Missing' cell`
  and could not tell from it that it targets B-010.
- The engine carries the target on every violation that has one; only the
  printed line dropped it.
- `0001-F1` C-9 and B-004 pin each verdict's message to the golden report,
  which cannot be regenerated (`0001-F1` C-10), so the message cannot carry it.
- A trailing `[...]` is the form MSBuild uses for its project suffix, and this
  repository's CI annotation reads the message to the end of the line, so the
  annotation gains the identifier with no change to it.

## Rejected

**Rewording each rule's message to name its identifier.** One place to read
the target. Rejected: it changes a verdict field `0001-F1` C-9 freezes. Cost of
rejecting: a rule whose message already names its identifier - `SPEC030`,
`SPEC031`, `SPEC041` - prints it twice on the line. The owner accepted that.

**Brackets on every line, empty when there is no identifier.** One line shape.
Rejected: `[]` states a target that does not exist. Cost of rejecting: a reader
of the line handles two endings.

## Affects

- B-001, amended: the line form carries the identifier.
- B-016, added: no identifier, no brackets.
- § 5 row 12.
- `check.feature`: `@B-001` gains a step; `@B-016` is a new `@boundary`
  scenario.
- `0001-F8` B-010: cites B-001 and B-016 for the line and no longer restates
  its form.

## Reversal

None.
