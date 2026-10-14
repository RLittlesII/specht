---
title: "Decision 0003: Make comparison evidence and advisory failures explicit"
description: "Review refinements cover measurement equivalence, unit conversion, invalid evidence and failures throughout advisory reporting"
type: decision
---

# Decision 0003: Make comparison evidence and advisory failures explicit

**Date:** 2026-10-13
**Decided by:** Codex, acting as spec-author during the owner's requested review and decomposition; these refinements preserve decisions 0001 and 0002.

## The call

The review found three gaps in the draft agreement:

- B-005 and B-006 did not explicitly reject unknown workload equivalence or differing confidence levels and measurement settings. The agreement now names those conditions and distinguishes a changed product implementation from a changed measurement workload.
- B-009 and B-013 left unit conversion and invalid timing values implicit. Compatible scales are normalised before arithmetic; invalid evidence and duplicate observations cannot establish a clean result.
- B-018 covered only benchmark collection failures. Parsing, evaluation, rendering and attachment failures also produce an incomplete advisory outcome after publication. They do not change a passed build into a failure, conceal an earlier correctness failure, or fabricate a report link when publication failed.

The publication sequence also has an explicit deterministic tie-breaker: releases
with the same publication timestamp are ordered by immutable release identifier.
This does not replace publication order with version order.

The owner's chosen strict 10% review screens and inclusion of prereleases stand.
Workflow structure remains the implementer's responsibility. In particular,
design must demonstrate a feasible acyclic collection and attachment path that
preserves C-10 and `0055-F6` C-7 before implementation begins. The review does
not grant a new credential or publishing job.

## Why

A comparison can produce a plausible wrong answer from matching names, differing
units or invalid observations. A renderer or upload failure can also bypass an
advisory policy that handles only benchmark exceptions. Explicit outcomes let
tests exercise these cases without running real benchmarks or contacting GitHub.

## Rejected

- **Treat unknown equivalence as comparable:** risks a false regression or a false clean result. Cost of rejection: older releases with insufficient metadata receive an incomplete comparison.
- **Assume all measurements use the same display unit:** silently corrupts ratios. Cost of rejection: the importer must validate dimensions and normalise compatible scales.
- **Handle benchmark exceptions alone:** leaves later reporting stages able to fail the build or disappear. Cost of rejection: orchestration must preserve useful failure evidence throughout the reporting path.
- **Choose a credential or workflow exception during review:** would move an existing release boundary. Cost of rejection: the design item must prove feasibility or route a concrete conflicting requirement back to the owner before coding.

## Affects

`0109-F5` B-002, B-005, B-006, B-009, B-013, B-018, B-020;
C-2, C-5, C-6, C-9 and C-10.
