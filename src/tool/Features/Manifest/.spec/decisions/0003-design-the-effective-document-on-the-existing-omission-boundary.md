---
title: "Decision 0003: design the effective document on the existing omission boundary"
description: "The architecture follows B-004 and B-007 as written, keeping declaration-sensitive settings absent while OQ-2 remains open; this does not ratify a broader defaulting policy"
type: decision
---

# Decision 0003: design the effective document on the existing omission boundary

**Date:** 2026-10-26
**Decided by:** `spec-author`, agreement audit for the engine design. This is an
exception for proceeding on the existing claims, not an owner answer to OQ-2.

## The call

Proceed with the effective-document architecture against B-003 to B-007 as
written. B-004 supplies only omissions the check's parser actually fills;
B-007 requires the result to preserve the original root's verdict. An omitted
epic grammar therefore stays absent in the output even though the typed model
holds a fallback grammar. The same boundary excludes inventing optional
consumer configuration.

OQ-2 remains open with its existing provenance. No claim, scenario or owner
decision is changed. Broader agreement review is still pending. A later owner
answer that changes this boundary must amend the agreement before code follows
it; this architecture pass is not evidence that the owner selected an answer.

## Why

A blind merge with the embedded manifest adds its epic grammar, turns on an
optional tier and can change a root's verdict. That conflicts with B-007 and
`0001-F5` C-10. The existing B-004 wording already limits the fill to what the
loader supplies, and the OQ-2 row records that boundary. The design can follow
it without choosing a new product behavior.

The parser also accepts declared JSON that `SpecStructure` does not retain.
Keeping the document from the existing parse preserves B-003 without building
a second file reader or a second table of defaults. Rule levels use the
already-delivered rule-entry reader.

## Rejected

**Fill every key from the embedded manifest.** This chooses the unresolved
broader reading and can enable capabilities the consumer omitted. Cost of
rejecting: the effective document does not list every configurable key.

**Serialize `SpecStructure` as the manifest.** Its declaration flags and derived
values are not manifest members, and accepted metadata and analyzer settings
would be lost. Cost of rejecting: the parser must retain an owned JSON document
for inspection callers.

**Treat a design handoff as ratification or completed behavior.** Cost of
rejecting: agreement review remains visible, the delivery item remains open,
and every coverage row stays Missing until executable proofs exist.

## Affects

- `0001-F9` B-003 to B-007 and § 6 to § 8; no claim text changes.
- OQ-2 stays open. OQ-1 and OQ-3 to OQ-5 are outside this engine design.
- No command, default-manifest printing, write operation or ADR-0010 rollout.

## Reversal

None. A future owner decision supersedes the existing boundary by amending the
claims and scenarios.
