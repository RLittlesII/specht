---
title: "Decision 0002: A shipped Reqnroll scenario producer"
description: "specht ships a production producer that turns a consumer's Reqnroll scenarios and outline rows into scenario records through the existing bridge"
type: decision
---

# Decision 0002: A shipped Reqnroll scenario producer

**Date:** 2026-10-25
**Decided by:** Repository owner, on the consumer request #256 readiness review

## The call

The owner's words: "Gap 3 needs a production Reqnroll producer feeding
ScenarioEvidenceBridge.Map." specht ships a producer that turns a selected
consumer project's Reqnroll scenarios and Scenario Outline rows, as its normal
discovery and test run report them, into scenario metadata, and maps that
metadata through the existing bridge into the evidence artifact's scenario
records. Each row's planned flag comes from the consumer's configured planned or
ignored tags (0201-F7 B-016, B-026).

The producer stays optional and outside the binding model (C-3). Consumer bridges
still produce scenario records through the common contract (B-008); this call
adds a producer and takes none away.

Not decided here: which metadata the producer reads, where it runs, how a project
selects it, which runners it covers and which Reqnroll versions are tested (OQ-4
to OQ-6).

## Why

The readiness review on 2026-10-25 found that the bridge has no production
caller: only acceptance steps drive it, with synthetic metadata. A real Reqnroll
consumer therefore gets no scenario records, so per-row scenario links and
planned rows never reach the evidence artifact, and 0201-F5 B-018 and B-019 take
planned state from scenario records only (0201-F5 OQ-3). The request names that
missing production bridge as gap 3 and rules out fabricated runner evidence.

## Rejected

- Leave every producer to the consumer, as decision 0001 alone allows: the
  requesting consumer would have to build the bridge the request asks this
  repository to ship, and planned rows would stay unproven end to end.
- Put Reqnroll knowledge in the binding model so the analyzer reads scenarios
  itself: imposes the scenario framework on direct tests, which C-3 rules out.

## Affects

B-012 to B-017, § 2 row 2, § 5 rows 5 and 6, OQ-2's note, OQ-4 to OQ-6. Decision
0001 stands. § 7's statement that a producer is outside this repository is the
implementer's to update.

## Reversal

None.
