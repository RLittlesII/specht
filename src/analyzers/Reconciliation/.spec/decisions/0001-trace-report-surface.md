---
title: "Decision 0001: Trace report surface"
description: "Expose specht trace with human and separately versioned JSON output while consuming existing test-workflow evidence"
type: decision
---

# Decision 0001: Trace report surface

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Expose reconciliation through `specht trace`, with a human summary and a separate
versioned JSON report contract. Preserve the existing repository checker's report
contract. The new command consumes evidence collected by normal discovery/test
workflows and does not launch tests to complete missing inputs.

Reconcile through 0201-F3's common framework-independent runner-evidence contract.
Binding validity, discovery, required-link completeness, exemptions, input
completeness, freshness and execution outcomes retain their distinct meanings.
Existing offline, deterministic and repository-relative output constraints apply.

Detailed report fields, command options, exit-code mapping and artifact packaging
remain technical-design work. This decision does not introduce a new engine/host
architecture or waive existing architecture gates.

## Why

A dedicated surface exposes code traceability without changing the schema/checker
report consumers already use. Separately versioned reports and runner artifacts
permit evidence reconciliation without invoking test execution during checking.

## Rejected

- Extend the checker report implicitly with trace-only fields: changes an existing
  consumer contract and mixes independent workflows.
- Hide reconciliation inside an IDE-only view: prevents an explicit offline
  command/report workflow for supplied cross-project evidence.
- Launch tests during reconciliation: mixes checking with execution and hides
  absent evidence by performing additional work.

The accepted cost is a dedicated command and report schema, with explicit supplied
artifacts and completeness states.

## Affects

0201-F5 B-001, B-014 through B-016 and OQ-2; epic 0201 OQ-11. Concrete report
schema, options and exit codes remain downstream technical design.

## Reversal

None.
