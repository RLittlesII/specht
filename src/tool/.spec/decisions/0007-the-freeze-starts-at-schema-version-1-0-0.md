---
title: "Decision 0007: the freeze starts at schema version 1.0.0"
description: "A published schema version is frozen for good from 1.0.0 on; before it a published 0.x.y is never changed in place, a later tool may stop shipping it and a 0.minor step may fail a tree that passed; the freeze is keyed on the schema version, never on the package version, and the first publish no longer waits for 0001-F5 and 0001-F6"
type: decision
---

# Decision 0007: the freeze starts at schema version 1.0.0

**Date:** 2026-10-11
**Decided by:** the repository owner (2026-10-11), three rulings; recorded by spec-author

## The call

The owner asked: "can we make a rule that we will not lock in the schema
version until the 1.0.0 version of the tool is shipping?" Two options were put
on each of two points and the owner ruled "1. recommended 2. recommended". A
third ruling on the order of the first publish followed the same day.

1. **The freeze is keyed on schema version `1.0.0`.** It is not keyed on the
   `specht.tool` package version. C-12 stands whole. The owner cuts schema
   `1.0.0` when they choose; it may coincide with the tool's `1.0.0` and is not
   derived from it.
2. **Before schema `1.0.0`:** a published `0.x.y` is never changed in place; a
   later tool may stop shipping it; and a `0.minor` step may fail a tree that
   passed. **From `1.0.0` on,** C-1 and C-11 hold as they were written: every
   published version is embedded in every later tool, and a change that can
   fail a tree that passed is a major.
3. **The first publish no longer waits for `0001-F5` and `0001-F6`.** Shown
   three options - A publish now, B publish after the manifest lane (items
   `0016`, `0134`, `0018`), C wait as `0055-F6` C-2 said - the owner answered
   "publish now!", option A. It is a ruling on the order of work. It pushes no
   tag: when the first `v*` tag is pushed stays a deliberate act (`0055-F6`
   C-1).

This answers OQ-10. OQ-9, where specht's own schema versions are hosted, stays
open: "a later tool may stop shipping it" speaks of the tool's embedded set
only, and says nothing of where a schema's `$id` is served.

Read by the author from the rulings, not stated by the owner:

- A patch changes no tree's verdict at any version. Ruling 2 loosens the minor
  step only, so before `1.0.0` a new `SPEC###` takes at least a `0.minor` step
  and never a patch (B-066).
- A manifest pinning a published `0.x.y` a later tool has stopped shipping is a
  manifest pinning a version the tool does not ship: exit `3`, naming the
  versions it ships (B-003). The consumer pins forward. No new behaviour is
  claimed for it.
- `0.1.0` is whatever the first published package embeds. Under ruling 3 that
  is the set as it stands when the first tag is pushed, and no longer waits to
  hold what `0001-F5` and `0001-F6` add; which of A-2's parts it then holds is
  OQ-30.

## Why

Stated by the owner: the question quoted above, and the three answers.

The reasons put to the owner with the options they chose, the author's and not
the owner's words:

- Keying the freeze on the package version would derive one version from the
  other, which C-12 rules out.
- Rewriting a published `0.x.y` in place would make one pin give different
  verdicts in different tool releases - the reason decision 0002 gave under
  "Allow additive changes to a shipped version", and what the invariant "the
  same tree gives the same report" forbids.
- A manifest pinning a version the tool does not ship already has an answer
  (B-003), so dropping a `0.x.y` needs no new behaviour.

The author's inference:

- With no freeze at the first publish, the reason decision 0002 and `0055-F6`
  C-2 gave for holding the first tag is gone: what `0001-F5` and `0001-F6` add
  after it is a later `0.x.y`, not a change to a frozen version.

## Rejected

Each cost below is the author's assessment.

**Key the freeze on the `specht.tool` package version `1.0.0`.** The owner's
question as first worded; put to the owner as an option and not chosen. It
amends C-12. Cost of rejecting: schema `1.0.0` is a second, separate act the
owner must take; nothing cuts it when the tool reaches `1.0.0`.

**Rewrite a published `0.x.y` in place.** Put to the owner as an option and
not chosen. Cost of rejecting: every change after the first publish, however
small, is a new version number, and a tool that keeps old `0.x.y` versions
carries each one's files.

**Freeze every published version, `0.x` included.** What C-1 said before this
decision. Cost of rejecting: a consumer pinned to a `0.x.y` can be made to move
its pin by a tool release, and a `0.minor` move can fail its build.

**Publish after the manifest lane (option B), or wait for `0001-F5` and
`0001-F6` (option C).** Shown to the owner and not chosen. Cost of rejecting:
the first published `0.1.0` holds only what has landed at the first tag, so
what those Features add reaches a consumer as a later version it must pin and
`upgrade` to.

## Affects

- `0001-F7` § 1; § 2 A-2; § 3 B-004, B-013, B-015 and B-040 (amended), B-066
  (added); § 4 C-1 and C-11 (amended); § 5 row 13 (amended); § 11 OQ-10
  (resolved), OQ-18 (annotated), OQ-29 and OQ-30 (raised).
- Decision 0002: amended, see its Reversal.
- `0055-F6` § 1, § 4 C-2 and § 5 row 4 (amended). Ruling 3 has no record of its
  own in that Feature: C-2 was derived from decision 0002, and the record that
  reverses its source carries it.
- Items `0084` (no longer waits on `0010` and `0004`), `0082`, `0044` and
  `0169`.
- Epic `0055`'s epic file, on when the first publish happens.
- brief § 2 and § 6; AGENTS.md § Configuration and § Invariants; README.md.
- Not amended, and still worded by the rule this decision changes: the
  Features of epic `0101` name "the major schema version that adds" their
  rules and cite C-11. Before `1.0.0` that step may be a `0.minor`; the number
  is OQ-18, open.

## Reversal

**Redirected 2026-10-11 by the repository owner
([`0055-F6` decision 0004](../../../../.build/Releasing/.spec/decisions/0004-the-first-release-waits-for-a-schema-sourced-at-a-url.md)).**
Ruling 3, "publish now!", is redirected the same day. On learning that
`specht` cannot `init` a repository to a schema sourced at a URL, the owner
said: "Okay, this is a HUGE gap. We can't release without this." The first
`v*` tag now waits until a consumer can initialise a repository to such a
schema and have the check validate with it (`0055-F6` C-8).

What stands: rulings 1 and 2; and of ruling 3, that the first publish does not
wait for `0001-F5` and `0001-F6` (`0055-F6` C-2). "Items `0084` (no longer
waits on `0010` and `0004`)" under Affects still holds; the item now waits on
the upstream source's items instead.

What follows for `0.1.0`, read by the author: it is still the set as it stands
when the first tag is pushed, and that set now holds the key that records an
upstream schema source (OQ-30, OQ-40).

Considered and not chosen: nothing was put to the owner as an alternative.
