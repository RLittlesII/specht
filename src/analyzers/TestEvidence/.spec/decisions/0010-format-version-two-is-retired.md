---
title: "Decision 0010: Format version 2 is retired"
description: "With the catalog member withdrawn, writers emit formatVersion 1 or 3 only, readers report version 2 as unsupportedContract, catalog is an unknown member at every version, and the number 2 is never reused"
type: decision
---

# Decision 0010: Format version 2 is retired

**Date:** 2026-11-06
**Decided by:** Repository owner, in chat; recorded by spec-author

## The call

Item 0411 withdraws the `catalog` member of the test-evidence artifact
(0201-F7 decision 0004, B-030). The recorder asked: "Item 0411 withdraws the
`catalog` member of the test-evidence artifact. It shipped in v0.3.0 and
v0.4.0 ... What should a newer `specht evidence` (and the analyzer's reader) do
with those older artifacts?" The owner chose the option labelled "Retire
version 2 (Recommended)". The option's wording was proposed by the recorder, not
the owner:

1. Writers emit `formatVersion` 1 or 3 only.
2. Readers report a version-2 artifact as `unsupportedContract`.
3. `catalog` is an unknown member at every version, so an artifact carrying it
   is invalid; objects stay closed.
4. The number 2 is never reused for another contract.

## Why

The option's own reasons, as the recorder wrote them: a consumer on a released
analyzer under native claim input rebuilds to get a version-1 or version-3
artifact; Markdown-path artifacts were never version 2 and are unaffected; the
reconciler never read `catalog`, so no gate result changes.

## Rejected

None recorded; the other options put to the owner are not in this record.

## Affects

§ 7 Envelope (`formatVersion`, `catalog`, the version-2 text) and § 7
"Version three"; B-009; [ADR-0001](../adr/0001-vstest-logger-and-artifact-version-three.md);
0201-F7 B-030, § 7 and ADR-0001 item 10; item 0411.

## Reversal

None.
