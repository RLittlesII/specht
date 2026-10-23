---
title: "ADR-0001: Native claim source contract"
description: "The manifest keys, element rules and diagnostic identities by which tagged Gherkin becomes the claim catalogue"
type: adr
---

# ADR-0001: Native claim source contract

## Status

Proposed — 2026-10-23, `implementer`, from item 0258, as the design that
[decision 0001](../decisions/0001-claim-candidates-carry-the-encoding-prefix.md)
and OQ-1 to OQ-9 leave open. It depends on
[ADR-0021](../../../../../.spec/adr/0021-gherkin-claim-semantics-live-in-a-netstandard-library.md)
being accepted. Section 7 states the keys and rules in full.

## Context

The resolved questions fix what native input does but not where it is
configured, which Gherkin elements declare, what a case is, or how failures and
findings are named. Item 0259 builds against one answer.

## Decision

1. **Where.** Every key sits under `codeAnalysis.records`. `claimSource` is
   repository-wide and has the single value `gherkin`; claim file sets are a
   list of `{ glob, feature, epic?, record? }`. The candidate prefix, the planned
   tags and the lifecycle tags are separate keys beside them.
2. **Tag lists are exact tags.** Entries are `@`-led strings compared ordinally
   with Gherkin tag names. Only claim tags honour `claim.tagIgnoreCase`.
3. **What declares.** A Feature, a Rule, a Scenario or a Scenario Outline
   declares the claims whose tags it carries. A claim-candidate tag anywhere else
   (an `Examples` block) is a malformed claim tag, not a silent ordinary tag.
4. **What a case is.** One Scenario is one case; an Outline has one case per
   Examples row. Planned state is read from the tags in effect for the case:
   Feature, enclosing Rule, the Scenario or Outline and, for a row, its own
   Examples block. A claim's lifecycle tag is read from the declaring element only.
5. **One set per Feature, one Feature per file.** Two claim file sets naming one
   Feature identity, or one file matched by two sets, is an input failure.
6. **A Feature record supplies lifecycle only.** With no record the Feature is
   `current`, a lifecycle value the resolver treats as eligible.
7. **Failures reuse SPCT0001**, one stated message per cause. **Findings take new
   ids**: SPCT0010 duplicate claim, SPCT0011 malformed claim tag, SPCT0012
   scenario without a claim, each at warning (0201-F6 decision 0001).
8. **Incomplete runs are not SPCT diagnostics.** A run happens after the build
   that reports SPCT diagnostics; the incomplete-run diagnosis is a typed finding
   in the evidence artifact and report (0201-F5 § 7).

## Rejected

- A `claimSource` per file set: OQ-1 resolved to a repository-wide selection.
- A regular expression or a bare name for tag lists: a tag is a tag.
- Treating a claim tag on an `Examples` block as ordinary: an intended claim
  would vanish without a diagnostic.
- Deriving a case's planned state from the declaring element's tags only: an
  `@ignore` on a Rule would not mark the cases that generated tests skip.
- A new SPCT id for each input failure: SPCT0001 already means "analysis
  unavailable" and carries the cause as its message.

## Reversal

None.
