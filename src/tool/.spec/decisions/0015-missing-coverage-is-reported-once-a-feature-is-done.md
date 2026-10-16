---
title: "Decision 0015: Missing coverage is reported once a Feature is done"
description: "From schema 0.3.0 SPEC060 reports a Missing row in a specification whose delivery status is the manifest's done marker, whatever its spec_status; 0.1.0 and 0.2.0 keep the approved axis, SPEC061 is unchanged, and scoping the rule per claim is not built"
type: decision
---

# Decision 0015: Missing coverage is reported once a Feature is done

**Date:** 2026-10-16
**Decided by:** the repository owner (2026-10-16): the delivery axis, the done
value read from the manifest, `SPEC061` unchanged, a new fixed message naming
the done status, and per-claim scoping not built. Recorded by spec-author, who
made the calls marked as spec-author's reading below.

## The call

1. From schema `0.3.0`, `SPEC060` reports each § 9 row carrying the missing
   marker in a specification whose delivery `status` is the done marker,
   whatever its `spec_status`. Under any other delivery status it reports
   nothing. The approved `spec_status` no longer turns it on (B-118, B-119).
2. The done value is read from the manifest's `markers`, under a new name,
   `done`, whose default is `done`, known from schema `0.3.0` (`0001-F5`
   B-063, B-064). The key is spec-author's reading: `markers` already holds
   one entry per text `SPEC060` and `SPEC061` compare (`0001-F5` B-003,
   decision 0006), and `0.2.0` grew it the same way (`0001-F5` B-058).
3. `SPEC061` is unchanged: it reports a § 12 row that is not approved in a
   specification whose `spec_status` is approved (B-122).
4. The message is the fixed text `status is 'done' while § 9 still carries a
'Missing' cell` (B-121). That it names the done status is the owner's; the
   words are spec-author's, in the shape of the message they replace.
5. `SPEC060` keeps its id and its own severity, error (B-123). No rule id is
   added. `0001-F5` B-044 is unchanged: a consumer that sets no level gets
   error.
6. Schema `0.1.0` and `0.2.0` keep the approved axis (B-120), so `0001-F1`
   B-004's golden report, the `0.1.0` verdict on the baseline tree, is
   unchanged (`0001-F1` C-9, C-10).
7. The change is schema `0.3.0`, not a relock of `0.2.0`. Spec-author's
   reading of decision 0014 call 1 against the release record: the release
   run for tag `v0.1.131` (2026-10-15) pushed the `specht.tool` package
   `0.1.131` to the package feed, and that package embeds schema `0.2.0`,
   locked then at digest `5d087a05…`. A package has carried `0.2.0`, so call 1
   allows no relock. The owner is asked to confirm (OQ-60).
8. This repository stops lowering `SPEC060` in the same change that moves its
   pin to `0.3.0`: its manifest then sets no level for `SPEC060`, and a
   `Missing` row fails `Specht` only on a Feature whose status is done
   (`0055-F1` B-026, B-027, C-6). Spec-author's reading of the owner's
   finding that the override hides the defect. No Feature here is done with a
   `Missing` row on 2026-10-16, so the move reports nothing new.
9. Scoping `SPEC060` per claim - a row reported only once the item delivering
   that claim is done - is not built (§ 5 row 27). The consumer item schema
   has no `claims` field. OQ-59 records it, with the default "not until items
   carry claims".
10. `0.3.0`'s feature template says, in its § 9 guidance, that a missing row
    does not block approval and that `SPEC060` reports it once status is done
    (B-124). The wording is spec-author's.

## Why

- This repository approves a specification before its items are cut and its
  tests written (AGENTS.md "The artifact chain"), so `Missing` rows are
  expected at approval. AGENTS.md § `specht` and the spec-reviewer contract
  both say a `Missing` row blocks an item reaching done, not a specification
  reaching approved. A rule keyed on approval fails every honest approval.
- The override (`0055-F1` C-6) hid that conflict here. A consumer without it
  had its reviewer blocked from approving.
- Done is the point where a `Missing` row is a defect: the Feature says it is
  delivered while a claim has no test.
- Under C-1 a published version never changes, and under C-11 a `0.minor`
  step may fail a tree that passed. This step also passes trees that failed.

## Rejected

- **Keep the approved axis and let each repository lower the rule.** Cost: the
  shipped default contradicts the process the tool ships with, every consumer
  must find the override, and the conflict stays hidden.
- **Report on approved or done.** It still blocks an honest approval.
- **Scope the rule per claim, through the item that delivers it.** It needs
  items that carry claim ids, and the consumer item schema has none. Cost of
  rejecting it: until a Feature is done, `SPEC060` cannot tell an untested
  claim whose item is finished from one still in flight.
- **Relock `0.2.0` under decision 0014.** A package has carried it (call 7).
- **Change `0.1.0` or `0.2.0` in place.** C-1.
- **Hard-code `done`.** brief § 5 moves every literal a rule compares into the
  manifest; `missing` and `approved` are already there.
- **A new rule id for the delivery axis.** It retires `SPEC060` or leaves it
  checking the wrong axis, and breaks every manifest setting and `--explain`
  call that names `SPEC060`.

## Affects

- Added: B-118 to B-124 with their scenarios and § 9 rows; § 5 row 27; OQ-59
  and OQ-60.
- `0001-F5`: B-063 and B-064 added; B-022 and B-023 amended to list B-064.
- `0001-F3`: B-043 added.
- `0055-F1`: B-024 amended to pins below `0.3.0`; B-026 and B-027 added; C-6
  and C-7 amended.
- `0055-F2` C-4 and `0055-F6` C-6 amended: a run or a release fails for a
  `Missing` row only on a Feature whose status is done.
- brief § 4, the `SPEC060` row.
- Not specified here, for the pull request that ships `0.3.0`: the
  `docs/rules/v0.3.0/SPEC060.md` page (`0001-F3` B-031, B-032),
  `docs/what-it-checks.md`, `docs/setup.md`, the `specht-conventions` skill's
  `references/specs.md`, and this repository's manifest and live templates
  (`0001-F4` B-004).
- `0001-F1` B-004 holds only while the baseline tree pins below `0.3.0`. The
  test tree copies this repository's live manifest, so the change that moves
  the pin must pin the baseline tree's manifest itself.

## Reversal

None.
