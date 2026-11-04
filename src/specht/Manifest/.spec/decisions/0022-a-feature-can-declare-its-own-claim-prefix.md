---
title: "Decision 0022: A Feature can declare its own claim prefix"
description: "A Markdown Feature can use its own literal prefix with the shipped numbered suffix by declaring one frontmatter field."
type: decision
---

# A Feature can declare its own claim prefix

**Date:** 2026-11-03

**Decision owner:** repository owner (OQ-29 and OQ-30)

## Decision

Under a schema supporting B-076 to B-080, a Feature can declare
`claim_prefix: CI-AC-` in its spec frontmatter. The declared prefix selects the claim grammar even when
the manifest supplies a custom `identifiers.claim` regex. CI claims begin with `CI-AC-` followed by three ASCII digits
and an optional lowercase ASCII letter, as in `CI-AC-001a`. A sibling that
declares `claim_prefix: RELEASE-AC-` uses `RELEASE-AC-001a` for the same
suffix. A Feature that omits the key uses the manifest’s effective `identifiers.claim` regex for the complete ID.

The complete authored ID identifies the claim in its claims section, matrix
and companion tags. Existing tag form and case settings still govern the
reading of tags. In the claims section the prefix is literal and case-sensitive.

## Alternatives considered

Requiring the manifest's full-ID regex to enumerate every Feature prefix
would make adding a Feature a two-file operation. The owner chose to keep the
usual numbered suffix. Applying a custom full-ID regex as a second check
would reject valid prefixed IDs or impose a second suffix format. On OQ-30
the owner confirmed that the Feature declaration takes precedence.

## Scope

The manifest role `keys.feature.claimPrefix` defaults to `claim_prefix`
(B-004, C-8). The change targets the open schema version under A-7. Native
Gherkin declarations have no Markdown Feature frontmatter (`0001-F16`).
