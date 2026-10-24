---
title: "Decision 0001: Claim candidates carry a configured prefix"
description: "A Gherkin tag is a claim candidate only when its identifier begins with the configured candidate prefix"
type: decision
---

# Decision 0001: Claim candidates carry a configured prefix

**Date:** 2026-10-23
**Decided by:** Repository owner

## The call

The claim tag encoding stays `@{claim}`: a claim's identifier is the whole tag
text after `@`, so `@REQ-FIX-001` declares `REQ-FIX-001`. A candidate prefix,
for example `REQ-`, is configured beside the encoding and begins every claim
identifier. A tag whose text begins with the prefix is a claim candidate and is
checked against the claim identifier grammar: an identifier the grammar accepts
declares a claim; one it rejects draws the malformed-claim diagnostic. A tag
without the prefix is an ordinary tag and is ignored. Under native Gherkin
claim input, a configuration with no candidate prefix is an input failure.

## Why

With a bare `@{claim}` encoding, ordinary tags such as `@visual`, `@ignore`,
`@boundary` and `@issue-42` match the tag form and fail the grammar, so a
grammar check alone would reject valid Gherkin. Filtering on form and grammar
together, as the existing claim-tag parser does, silently drops an intended
claim that is malformed. A prefix that is part of the identifier separates
intent from validity, keeps the identifiers the consumer's relationships
already name, and adds no second grammar. Without a prefix no tag can be told
apart from an ordinary one.

## Rejected

- Treat every tag matching the form as a candidate: rejects ordinary tags.
- Filter on form and grammar together: hides a malformed intended claim.
- Put the prefix in the encoding, as `@REQ-{claim}`: the captured identifier
  becomes `FIX-001` and no longer matches the relationships that name
  `REQ-FIX-001`.
- A separate candidate pattern or allowlist of ordinary tags: a competing
  grammar beside the configured encoding (C-1).

## Affects

B-004, B-013, B-025, B-027; OQ-8 and OQ-9.

## Reversal

Superseded 2026-10-23 by the repository owner in
[decision 0002](0002-a-claim-tag-is-the-existing-claim-tag-form.md): there is no
candidate prefix, and a tag declares a claim exactly when it matches the
existing claim tag form and the claim identifier grammar. The reasoning above is
kept as it was at the time.
