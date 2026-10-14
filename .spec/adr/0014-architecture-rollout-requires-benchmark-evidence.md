---
title: "ADR-0014: architecture rollout requires benchmark evidence"
description: "Gate further ADR-0010 migration on repository measurements and evaluate the source-generated Mediator candidate before choosing a dispatcher"
type: adr
---

# ADR-0014: architecture rollout requires benchmark evidence

## Status

Accepted, 2026-10-14, by the repository owner while reviewing PR #185.
Supersedes ADR-0010's execution timing and handwritten mediator choice; its
remaining target design and staged prerequisites are retained for evaluation.
ADR-0010 gains only a status pointer; its original reasoning remains intact.

## Context

PR #185 introduced `Specht.Cqrs`, `SpecModelReader` and a host factory while
delivering schema list and pin. ADR-0010 assigned that work to later stages,
but its status and contributor instructions omitted the intended performance
gate. Existing benchmark harnesses and findings measure earlier implementations;
they do not establish that this architecture's benefit justifies its cost.

The owner clarified that ADR-0010 was supposed to wait for benchmarks proving
the performance worth the added cost, and named
[martinothamar/Mediator](https://github.com/martinothamar/Mediator) as the
source-generated mediator candidate to evaluate. Its documentation describes
generated dispatch and dependency-injection registration; those capabilities
and its published benchmarks are not evidence of a benefit in this tool.

## Decision

Further engine and host architecture rollout under ADR-0010 waits for a
repository-specific benchmark comparison that the owner judges worth its
complexity and maintenance cost. An accepted target, a benchmark harness, a
green correctness suite or a new feature does not waive that gate.

The evaluation compares the current direct-call implementation with a trial
using martinothamar/Mediator under equivalent workloads and build conditions.
Record the candidate version, source revisions, elapsed time and allocations,
including whole-check and CLI startup costs with dispatch and registration
actually exercised. Isolated handler or library benchmarks alone cannot prove
the end-to-end benefit. No numerical adoption threshold is invented here.

Trial branches may implement enough of the candidate to measure it; that code
does not merge as production architecture before the gate is satisfied and the
follow-up design is agreed. Item `0216` owns the comparison and recommendation.
The candidate is not adopted by this record, and a handwritten intermediary is
not authorized as a substitute. Item `0167` must be redesigned from the evidence
before implementation, including the handler contracts and return types.

PR #185 uses the existing manifest store, parser and catalog through a concrete
schema service. It adds no mediator contracts, dispatcher, model-reader
migration or global host-factory migration. ADR-0007's factory remains item
`0144`; shared slice registration is sufficient for these commands and tests.
Already landed behavior is retained. Independent test-support cleanup remains
separate from production architecture adoption.

## Alternatives not chosen

- Treat ADR-0010's acceptance as immediate implementation permission: bypasses
  the owner's evidence gate and the staged dependencies.
- Keep a local mediator until a generated port arrives: adds unmeasured dispatch
  and a second migration without evaluating the named candidate.
- Install the candidate directly in the feature PR: changes the implementation
  before establishing that its performance is worth its cost.

## Consequences

Schema commands can ship with direct calls. Structural engine/host work waits
on item `0216`; benchmark evaluation can proceed on trial branches. The later
architecture decision cites the actual findings, including an unfavorable or
inconclusive result, rather than treating a prototype as permission to merge.
