---
title: "Decision 0001: Scenario scope and bridge ownership"
description: "Use Feature defaults and complete scenario overrides while reusing consumer bridges through common evidence metadata"
type: decision
---

# Decision 0001: Scenario scope and bridge ownership

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Gherkin Feature-level scope supplies the default. An explicit Scenario or Outline
scope replaces that default completely, without merging components. Local claim
tags resolve within the resulting effective scope under 0201-F1's identity and
consumer grammar rules. An omitted optional epic in an override is not inherited.

Scenario-to-case mapping uses generator/runner metadata through 0201-F3's common
runner-evidence contract. Reuse the consumer's claim bridge through that contract;
its convention analyzers remain consumer-owned. Reqnroll integration is optional.
Shared step methods, scenario titles and display names do not establish case
identity. Missing required metadata retains the existing unresolved-link behavior.

This Gherkin scope policy does not weaken the explicit member identity required
for checked CSharp documentation bindings. A consumer can use direct test
bindings without any scenario bridge. Exact scope/tag encoding, producer metadata
extraction and concrete integration compatibility remain technical-design work.

## Why

Feature defaults avoid repeated scope on every scenario; complete overrides
prevent hidden combinations of unrelated owner components. The shared evidence
contract lets existing consumer metadata producers participate without transferring
ownership of unrelated analyzer rules or making Reqnroll mandatory.

## Rejected

- Merge missing Scenario/Outline fields with Feature scope: permits hidden scope
  changes and makes an explicit override incomplete in meaning.
- Infer scenario-to-case identity from titles or shared steps: conflates distinct
  scenarios and depends on mutable text rather than generator/runner identity.
- Replace consumer convention analyzers with a new generic convention layer:
  expands ownership beyond traceability and duplicates consumer responsibilities.
- Require Reqnroll for the common model: blocks framework-independent direct tests.

The accepted cost is explicit complete overrides and a metadata bridge boundary,
with unresolved links when the required producer data is absent.

## Affects

0201-F4 B-001, B-006 through B-008 and OQ-1/OQ-2; epic 0201 OQ-4, OQ-6's scenario
mapping policy and OQ-10. Exact encoding/extraction and compatibility tests remain
technical design; checked target kinds are owned by 0201-F1 decision 0005.

## Reversal

None.
