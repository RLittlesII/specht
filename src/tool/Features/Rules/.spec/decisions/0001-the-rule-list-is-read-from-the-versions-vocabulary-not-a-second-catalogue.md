---
title: "Decision 0001: the rule list is read from the version's vocabulary, not a second catalogue"
description: "specht rules lists the pinned version's rule ids by reading that version's vocabulary and its rule pages at run time; no index file is generated or kept, so 0001-F3 section 5 row 10 and C-6 stand"
type: decision
---

# Decision 0001: the rule list is read from the version's vocabulary, not a second catalogue

**Date:** 2026-10-11
**Decided by:** the repository owner, who asked for `specht rules` "printed to the console or returned as json"; the reconciliation with `0001-F3` is recorded by `spec-author`

## The call

`specht rules` prints one entry per rule id of the pinned version. Each entry
is read when the command runs: the id from the version's vocabulary, the
family and the default severity from that rule's page (`0001-F3` B-031,
B-032), the effective level from the loaded manifest (`0001-F5` B-044).

Nothing is generated and nothing is kept: no index file under `docs/`, no
table of ids in the command, no list in a skill.

`0001-F3` § 5 row 10 excludes "a generated index of the rule pages" because
"the version's vocabulary is the index; a second list can drift from it".
This command is that vocabulary printed, so the row stands and is amended
only to point here. `0001-F3` C-6 stands unchanged: a rule's text lives once,
and this listing prints none of it.

## Why

- The owner asked for the list as a command's output, not as a file.
- A listing built from the vocabulary cannot name a rule the version lacks or
  miss one it holds; `0001-F3` B-031 already ties one page to each id.
- The family and default severity already have one home, the rule page. B-007
  holds the listing to it.

## Rejected

**A generated index page beside the rule pages.** It is the artifact
`0001-F3` § 5 row 10 rules out. Cost of rejecting: a reader of `docs/` has no
list without running the tool.

**A catalogue in the command: id, family and severity as literals.** Cost of
rejecting: the command depends on the rule pages being embedded
(`0001-F3` A-4) and readable as data.

**Families and severities from brief § 4.** The brief summarises and is not
versioned with the schema (`0001-F3` C-6). Cost of rejecting: none recorded.

## Affects

- `0001-F10` § 3 B-001 to B-003, B-007; § 4 C-2.
- `0001-F3` § 5 row 10: amended to point at `0001-F10`; its exclusion of a
  generated index stands.
- `0001-F3` C-6: unchanged.

## Reversal

None.
