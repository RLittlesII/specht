---
title: "Decision 0001: Release comparisons report for review"
description: "Release baseline comparison is automatic advisory reporting, with comparable evidence and explicit gaps, rather than a performance gate"
type: decision
---

# Decision 0001: Release comparisons report for review

**Date:** 2026-10-13
**Decided by:** repository owner for the scope; spec-author drafted the comparison and reporting boundaries for agreement.

## The call

The owner requested: "Compare each release against the previous release, automate the report, and flag meaningful regressions for review without failing the build."

`0109-F5` owns that capability, previously deferred in `0109-F1` OQ-4.
It consumes the harness's machine-readable results and publishes an advisory
comparison after successful release publication. Its review signal appears in
the release run summary and report. It does not create issues or send messages.

The draft elaboration measures both published revisions in the same session on
the same runner, with matching workload definitions, parameters and measurement
conditions. Unmatched or failed evidence is explicit, and cannot produce a clean
judgment. Reports and their raw evidence remain with the release independently
of transient workflow artifacts. Benchmark collection runs without publishing
credentials; publication follows the existing release permission boundary.

Release eligibility and the numeric review screen are recorded separately in
decision 0002, following the owner's answers to OQ-1 and OQ-2 on 2026-10-13.

## Why

The existing harness automates collection, but its files do not automatically
identify release regressions. Existing policy rejects measurement gates because
shared-runner timings vary. Matched collection and explicit evidence support a
review signal without claiming that a timing difference proves a cause or has
statistical significance. Advisory collection failures also need to remain
visible rather than becoming an empty report labelled clean.

## Rejected

- **A performance gate:** contradicts the owner's request and `0109-F1` C-1. Cost of rejection: a flagged regression can ship before its review is complete.
- **Comparing arbitrary historical shared-runner reports:** their environments and workloads can differ. Cost of rejection: the previous revision is measured again, adding runner work and exposing old build or runtime incompatibilities.
- **Skipping missing cases or substituting an older measurable release:** conceals evidence gaps or changes the requested baseline. Cost of rejection: some releases receive an incomplete comparison instead of a clean-looking result.
- **Treating the cold-start empty tool as the previous release:** `0109-F4` uses that baseline for a different question. Cost of rejection: two baseline concepts remain, with their identities stated explicitly.
- **Defining the numeric threshold without the owner:** would turn "meaningful" into an invented product requirement. Cost of rejection: B-011 and B-012 await OQ-1 before implementation.

## Affects

`0109-F5` B-001 to B-024; `0109-F1` § 5 row 2 and OQ-4;
`0055-F6` § 5 row 12; epic `0109`'s Feature list and placement.
