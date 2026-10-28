---
title: "Decision 0001: open questions gate approval and are listed before review"
description: "The owner chose to specify two mechanical consistency checks now, this open-question gate and dangling id references, to park the fuzzier ones as spikes, and to decline two checks; the gate sits under epic 0228 as its candidate D"
type: decision
---

# Decision 0001: open questions gate approval and are listed before review

**Date:** 2026-10-28
**Decided by:** the repository owner (2026-10-28): calls 1 to 4. Call 5 is
spec-author's reading, for the owner to confirm in review.

## The call

1. Two mechanical consistency checks are specified now: this gate and
   `0228-F2`, dangling id references.
2. Three verdicts: a § 11 Blocks cell citing an id that is not a § 3 claim of
   the same specification is an error by default, as `SPEC021` treats a tag;
   an approved specification with a § 11 row whose Resolution is open is an
   error by default, beside `SPEC060` and `SPEC061`; and, while a
   specification is not approved, each open row that blocks a live claim is a
   warning, so a reviewer has the list before a round (B-001, B-006, B-010).
3. Declined: checking a § 12 review note against repository history, which
   breaks "same tree, same report"; and judging whether a Then step is implied
   by another claim's refusal, which is semantic and the reviewer's work.
4. Parked as spikes, not Features: constraint tracing (`0316`), Then-step
   wording lint (`0315`), term variants (`0317`) and a warning baseline or
   suppression (`0318`).
5. The gate is epic `0228`'s candidate D, Feature `0228-F1`, at
   `src/specht/Rules/OpenQuestions/.spec/`. The owner proposed `0001-F17`
   with leave to choose a better epic.

## Why

- A consumer ran ten review rounds on one specification and the check
  reported 0 errors in each; most rounds came from questions answered late and
  from stale cross-references.
- This tree carries 23 open § 11 rows in four approved specifications on
  2026-10-28, and its Blocks cells cite claims, constraints, assumptions and
  questions alike, so "not a claim" needs the narrower reading B-004 gives.
- Epic `0228` already records this need as R6 and candidate D, decided by the
  owner on 2026-10-15, with the constraints a Feature cut from it carries, and
  says no Feature is owned by two epics.

## Rejected

- **`0001-F17` under epic `0001`.** It would leave epic `0228`'s candidate D
  a second owner of one need.
- **Epic `0101`.** It holds style and ordering rules, warnings by default, and
  uniqueness rules; the gate is approval consistency, an error by default.
- **Reporting a constraint, assumption or question id in a Blocks cell as not
  a claim.** It flags 103 citations this tree writes on purpose; the
  constraint and question ids are resolved by `0228-F2` instead.
- **One rule id for the approval error and the readiness warning.** A rule
  page states one default severity (OQ-1).

## Affects

- B-001 to B-017, C-1 to C-6, § 5 rows 1 to 14, OQ-1 to OQ-6.
- Epic `0228`: `children` gains `0228-F1`; its Features section records the
  cut.
- `0001-F5` and `0001-F7`: `blocks` gains `0228/F1`.

## Reversal

None.
