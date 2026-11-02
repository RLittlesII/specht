---
title: "Decision 0003: the scenario, test and status rules enter schema 0.3.0"
description: "SPEC022, SPEC023, SPEC032 and SPEC033 enter schema 0.3.0, which 0001-F7 cuts first, each relocking it while no published package embeds it; SPEC034 and SPEC035 stay in the published 0.2.0 and 0.2.1"
type: decision
---

# Decision 0003: the scenario, test and status rules enter schema 0.3.0

**Date:** 2026-11-01
**Decided by:** the repository owner (2026-11-01), answering a question card; recorded by spec-author

## The call

1. `SPEC022`, `SPEC023`, `SPEC032` and `SPEC033` enter schema `0.3.0`, not
   `0.2.0`. The owner chose the option labelled "0.3.0 via 0321 first
   (Recommended)".
2. `0.3.0` is cut first, by the `0001-F7` change that makes it (`0001-F7`
   decision 0015 call 7), item `0321`, before any of the four rules is
   built. Each rule then adds its own id to `0.3.0`'s vocabulary, with its
   rule page (`0001-F3` B-031), in the change that builds it, and relocks
   `0.3.0` by `0001-F7` decision 0014's procedure (C-17), so `0001-F7` B-014
   holds at every commit.
3. A relock is allowed only while `0.3.0` is unpublished: published means a
   version a published package embeds (`0001-F7` C-1, C-17). On 2026-11-01
   no tag embeds `0.3.0`. This meaning is recorded as the session that put
   the question stated it.
4. `SPEC034` and `SPEC035` stay where they shipped: in `0.2.0` from tag
   `v0.2.4` on and in `0.2.1`, off unless raised (B-020). No published
   version changes (`0001-F7` C-1).

## Why

- A-4 was false. It rested on decision 0001 call 4: `0.2.0` had never
  shipped, since tag `v0.1.90` carried `0.1.0` alone. Tag `v0.1.131` embeds
  a `0.2.0` holding `0.1.0`'s 21 ids; tags `v0.2.4` and `v0.2.35` embed one
  adding `SPEC034` and `SPEC035`; tag `v0.3.1` adds `0.2.1`, with the same 23
  ids.
- `0001-F7` withdrew B-115, which put the four ids in `0.2.0`'s vocabulary,
  on 2026-10-30, when the owner resolved its OQ-58: whichever `0.2.0` is
  published, C-1 and `0001-F7` B-015 freeze it, and it holds none of the
  four. Each was left to enter at least a `0.minor` step above the newest
  published version (`0001-F7` C-11), with no version named. This decision
  names it for this Feature's four.
- `0.3.0` is the next `0.minor` above `0.2.1`, and `0001-F7` decision 0015
  call 7 already makes it, so the four rules spend no version number of
  their own.
- Cutting `0.3.0` first gives each rule a version to join, and one id per
  relock keeps `0001-F7` B-014 true at every commit, as the owner ruled for
  `0.2.0` on 2026-10-15.

## Rejected

The owner was offered these and chose against them; the cost after each is
spec-author's reading:

- **(b) This Feature's first rule item cuts `0.3.0`, ahead of `0001-F7`'s
  cut.** Two changes would each create `0.3.0`, and whichever lands second
  rebuilds its version folders, lock entry and vocabulary on the first's;
  it puts `0001-F7`'s `SPEC060` change and done marker (`0001-F7` B-118 to
  B-124; `0001-F5` B-063, B-064) behind a rule of this Feature, or this
  Feature's rule behind a cut it did not plan.
- **(c) A later `0.minor`, cut after a published package embeds `0001-F7`'s
  `0.3.0`.** The four rules wait for a release and then spend a further
  version number, and a consumer moves its pin twice, once for `SPEC060`'s
  change and once for these rules.
- **Not offered, and ruled out by `0001-F7` C-1:** adding the four ids to
  `0.2.0` or `0.2.1`.

## Affects

- A-4, B-021, § 1, the frontmatter `description`, § 5 row 8, OQ-4; the
  scenarios of B-001 to B-017 that observe the four rules, which now pin
  `0.3.0`; B-021's scenarios.
- Decision 0001 call 4 and its third Rejected bullet, reversed in part (its
  Reversal).
- `0001-F7` B-014, C-1, C-11, C-17, decision 0014 call 1 and its relock
  procedure, decision 0015 call 7; the version `0001-F7` OQ-58's resolution
  left unnamed, named here for these four ids only. No `0001-F7` claim
  changes. `0001-F3` B-031.
- Items `0224` and `0334` to `0338`.
- From the Addendum: `0055-F1` B-026 and B-027, now carried by item `0339`;
  `0334` and `0336`, which wait on it; `0321`'s out-of-scope row; OQ-4's
  Blocks cell; § 5 row 10.

Spec-author's readings, not stated by the owner:

- **B-001 is unchanged.** The column reading is built and already serves
  `SPEC034` and `SPEC035` under `0.2.0` and `0.2.1`; narrowing it to `0.3.0`
  would change built behaviour. A `@B-001` scenario under `0.2.0` observes it
  through the waiver rule.
- **B-021 names only the four rules.** Under `0.1.0`, `SPEC034` and
  `SPEC035` stay silent through B-020 and `0.1.0`'s vocabulary (`0001-F7`
  B-014), and a `0.1.0` manifest that raises either is rejected (`0001-F5`
  B-013).
- **`0.3.0` keeps `SPEC034` and `SPEC035`.** It is cut from `0.2.1`, and
  `0001-F7` decision 0015 removes no id; no claim here depends on it, since
  B-018 to B-020's scenarios pin `0.2.0`.
- **No `0001-F7` claim lists `0.3.0`'s vocabulary.** Each rule item cites its
  entry in its acceptance criteria, through `0001-F7` B-014 and this
  decision, as item `0227` cited the withdrawn B-115. A claim naming the whole
  vocabulary would be false until the last rule lands, the cost `0001-F7`
  decision 0014's Reversal gave for not retargeting B-115 to `0.3.0`. Each
  rule's scenarios pinned to `0.3.0` show its id runs there, and B-021 shows
  it does not below.
- **Decision 0002 call 3 stands.** Each of the four rules runs on this
  repository's tree in the pull request that builds it, and that pull
  request fixes the rows it reports here, because item `0339` pins this tree
  to `0.3.0` first (Addendum). The `tests` and `owners` keys call 3 declares
  are in place.

## Reversal

None.

## Addendum - 2026-11-01

Recorded by spec-author after spec-reviewer round 7 (§ 12). The call above
stands. At commit `6154f805`, the reading "Decision 0002 call 3 under a
`0.2.1` pin" left the four rules silent on this tree until its pin moved
under `0055-F1` B-026 and B-027, which no item claimed and `0321` put out of
scope. That is the option decision 0002 call 3 rejected. The reading above
replaces it.

The owner answered a question card on the finding with the label below,
verbatim:

- "Cut 0339 pin move (Recommended)"

As put to the owner, the option meant: one `0055-F1` item, `0339`,
delivering B-026 and B-027, which moves this repository's pin to `0.3.0` and
drops its `SPEC060` override (`0001-F7` decision 0015 call 8), after `0321`
and before `0334`, delivered by the lane that delivers this Feature's rules.
Decision 0002 call 3 is not reversed.

Spec-author's readings, not stated by the owner:

- `0334` and `0336` wait on `0339`. `0337` and `0338` follow through
  `0334`. `0335` does not wait on it: this repository's manifest sets no
  `matrix.scenarioTitles`, so `SPEC022` reports nothing here at any pin
  (B-010).
- `0339` is delivered in `0321`'s pull request, as a commit after `0321`'s
  commits. From the cut, `0001-F4` B-004's test holds the live manifest's
  `schemaVersion` to the newest embedded manifest's, and so fails until the
  pin moves (`0001-F7` OQ-56).
