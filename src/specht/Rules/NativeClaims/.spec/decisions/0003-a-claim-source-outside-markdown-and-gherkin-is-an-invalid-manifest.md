---
title: "Decision 0003: a claim source outside markdown and gherkin is an invalid manifest"
description: "The owner's selection of November 13 2026, Exit 3: under a manifest pinned to schema 0.4.0 or later, a claim.source other than markdown or gherkin exits 3 naming claim.source, which B-031 states; the wording, the precedent and the scope are spec-author's reading"
type: decision
---

# Decision 0003: a claim source outside markdown and gherkin is an invalid manifest

**Date:** 2026-11-13
**Decided by:** the repository owner, selecting an option relayed through the coordinating session; the wording, the precedent and the scope below are spec-author's reading; recorded by spec-author

## The call

The owner selected **"Exit 3"**: a `claim.source` other than `markdown` or
`gherkin` is an invalid manifest, and the run exits `3`.

Spec-author's reading, not the owner's words:

1. **The gap.** No claim stated what `claim.source` of `"yaml"`, `"Gherkin"`
   or a number does. § 7 rejected it with the fault
   `claim.source must be 'markdown' or 'gherkin'` and kept the row out of the
   build until a claim stated it.
2. **The claim.** B-031: under a pin of `0.4.0` or later, any value but the
   text `markdown` or `gherkin` exits `3` before any rule runs, naming
   `claim.source`. The match is exact, so `"Gherkin"` is refused, and a value
   that is not text is refused the same way. That is how `schemaSource` and
   `pinPolicy` refuse a value outside theirs (`0001-F7` B-056), and an omitted
   `claim.source` is not such a value: it reads as `markdown` (OQ-9, B-002).
3. **The scope.** Below `0.4.0` the member is unread and changes nothing
   (B-028; [decision 0002](0002-claim-members-go-unread-below-schema-0-4-0.md)),
   so B-031 names the pin. B-031's scenario declares no `claim.files`, so the
   fault B-029 gives a set beside a source other than `gherkin` does not also
   report.

## Why

The owner's reason for the selection is not in this record. Spec-author's
reading of what it gives: a misspelt or miscased source fails the run instead
of checking the tree as Markdown in silence, the failure § 7 names.

## Rejected

The options offered beside "Exit 3" are not in this record. Spec-author's
reading of the alternative § 7 names: reading an unknown value as `markdown`,
which would check a tree its consumer meant as Gherkin against claims tables it
does not have, with no message saying why.

## Affects

B-031 and its scenario in `native-claims.feature`; § 9's B-031 row; § 10;
Tasks; item `0389`'s claims, acceptance criteria and decisions; § 7's fault
table, whose `claim.source` row names no claim, for the implementer.

## Reversal

None.
