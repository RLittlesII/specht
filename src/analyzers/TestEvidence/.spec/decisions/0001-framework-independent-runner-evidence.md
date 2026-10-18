---
title: "Decision 0001: Framework-independent runner evidence"
description: "Use an extensible common evidence contract across .NET frameworks with real runner identities and deferred runtime cases"
type: decision
---

# Decision 0001: Framework-independent runner evidence

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

The binding and test-evidence model works across .NET test frameworks. It does
not select a closed framework or framework-version allowlist. Collection adapters
translate test-platform/runner discovery and execution metadata into a common
contract; frameworks producing conforming evidence use the same resolution and
reconciliation rules. xUnit facts/theories, NUnit, MSTest and Playwright .NET
are validation cases rather than the permitted framework set.

Framework independence does not fabricate missing metadata. Missing collection
integration or insufficient method/case identity produces unavailable or
unresolved evidence, never empty complete coverage. Concrete integration
compatibility is documented and tested without restricting the common model.

Runtime-only parameterized cases remain unenumerated until the runner supplies
them. Do not invent row identities, cases or case counts. Once supplied, cases
map through runner case IDs and method metadata, not display-name guessing.

Collect artifacts through normal discovery/test workflows. Use versioned JSON
evidence carrying project and target-framework identity, runner case IDs, method
metadata, source/assembly/configuration fingerprints, adapter version and run
identity. The versioned wire schema and concrete adapter implementation remain
technical-design work; these required semantics are agreed.

Analysis and reconciliation consume supplied artifacts without launching tests.
The existing freshness and completeness rules in 0201-F6 decision 0004 apply.
Reqnroll remains optional; its scenario mapping is an additional bridge contract.

## Why

Claim identity and the relation between a method and a test case do not depend
on a framework brand. A common contract permits reuse across runners while
explicit missing-metadata states keep coverage honest. Normal runner workflows
are the authority for runtime-only cases and execution outcomes.

## Rejected

- Limit the core to xUnit versions or selected framework names: unnecessarily
  couples traceability semantics to the examples used to validate it.
- Promise automatic correlation without adequate runner metadata: hides the
  cases where method identity cannot be established.
- Infer runtime cases or identities from display names: guesses data the runner
  has not supplied and can conflate distinct cases.
- Run tests during analysis to fill missing evidence: breaks the explicit offline
  input boundary and mixes execution with evidence checking.

The accepted cost is an extensible collection boundary, a versioned common
artifact and explicit unavailable/unresolved states when required data is absent.

## Affects

0201-F3 B-003, B-004, B-007 through B-009 and OQ-1/OQ-2; 0201-F5's direct
framework-independent chain; epic 0201 OQ-6. The owner accepted the other adapter
and reporting recommendations while broadening framework scope.

## Reversal

None.
