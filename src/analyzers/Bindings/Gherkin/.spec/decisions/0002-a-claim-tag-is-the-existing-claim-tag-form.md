---
title: "Decision 0002: A claim tag is the existing claim tag form"
description: "No candidate prefix; a Gherkin tag declares a claim exactly when it matches claim.tag and its captured text matches identifiers.claim"
type: decision
---

# Decision 0002: A claim tag is the existing claim tag form

**Date:** 2026-10-23
**Decided by:** Repository owner, in
[review 5635389907](https://github.com/RLittlesII/specht/pull/260#pullrequestreview-5635389907)
and the chat that followed it

## The call

There is no `claimTagPrefix` key. A Gherkin tag declares a claim exactly when it
matches the consumer's existing `claim.tag` form, honouring
`claim.tagIgnoreCase`, and the text the `{claim}` placeholder captures matches
`identifiers.claim`. That is the reading the engine already gives companion
tags (`0001-F5` decision 0013). The claim identifier is the captured text,
exactly as authored. Every other tag is ordinary: it declares nothing and draws
no diagnostic. A tag that matches both form and grammar but sits on an
`Examples` block, where no claim can be declared, still draws the
malformed-claim diagnostic.

This supersedes [decision 0001](0001-claim-candidates-carry-the-encoding-prefix.md).

## Why

The candidate prefix restated the leading literal of `identifiers.claim`: a
consumer whose grammar is `^REQ-FIX-[0-9]{3}$` had to configure `REQ-` a second
time. A duplicated manifest option can disagree with the grammar it copies, and
one claim-tag reading shared with the engine keeps C-1 true without a new key.

## Rejected

- Keep the candidate prefix (decision 0001): a second manifest option restating
  the grammar.
- Derive a prefix from the grammar's leading literal: an inference from a
  regular expression, and a second reading of the grammar.

The accepted cost: an intended claim that is mistyped is no longer reported.
Against `^REQ-FIX-[0-9]{3}$`, `@REQ-FIX-1` fails the grammar, so it is an
ordinary tag and draws no diagnostic. Such a scenario is still reported if
nothing else declares its claim (B-015), and a relationship naming the intended
identifier still fails to resolve (B-018).

## Affects

B-004 and B-025 amended; B-013 and B-027 withdrawn; B-028 added; C-1; OQ-8 and
OQ-9 annotated; the `.feature` Background and the B-013, B-025 and B-027
scenarios.

## Reversal

None.
