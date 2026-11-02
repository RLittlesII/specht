---
title: "Lesson 0016: A schema version is published only when a package carries it"
description: "Schema 0.3.0 was taken as shipped because its folders, cut and lock entry existed, so the rule-id renumbering was sent to a new 0.minor version; no package carried 0.3.0, and the change belonged in it, in place"
type: lesson
kind: process
---

# Lesson 0016: A schema version is published only when a package carries it

**Date:** 2026-11-02
**Kind:** process

## Symptom

On 2026-11-02 the owner asked where the `SPEC###` to `SPEC####` rule-id
renumbering of item
[`0169`](../../.issue/0169-renumber-the-rule-ids-by-group.yml) (ADR-0010 track
R) lands. The answer given was that schema versions `0.1.0` to `0.3.0` "have
shipped", so `0169`'s criterion 11 applied and the new ids needed a new
`0.minor` schema version. The evidence was the version folders under
`docs/rules/`: `v0.1.0`, `v0.2.0`, `v0.2.1` and `v0.3.0`.

Schema `0.3.0` had only been cut, in `2e849065` (item
[`0339`](../../.build/.issue/0339-pin-schema-0-3-0.yml), part of
[`0321`](../../src/tool/.issue/0321-schema-0-3-0.yml)), and no package carried
it. The owner confirmed that the newest schema version in a published
`specht.tool` package is `0.2.1`. The renumbering lands in `0.3.0`, rewritten
in place. The owner: "If we update to 0.3.0 in the middle of a release cycle,
but no binary get's deployed ... how have we shipped?!" The owner also said
this pattern recurs: guess a version is shipped, then add a schema version.

## Root cause

"Shipped" was inferred from repository state - the version folders, the cut
commit, the version lock entry, this repository's pin, git tags and GitHub
pre-releases - instead of from what a published package embeds. None of those
puts a version in a consumer's hands. Acted on, the answer would have burned a
schema version no consumer could ever pin, and left `0.3.0` published with
none of the change it was cut to carry.

The word helped. In `0001-F7` and `AGENTS.md`, "the versions the tool ships"
means the versions a tool built from this tree embeds: decision 0013 has the
tool stop shipping `0.1.1`, a version no package carried. The freeze is keyed
on a different word, "published".

## Spec delta

None is made here, and the agreement already says the rule.
[`0001-F7`](../../src/tool/.spec/README.md) C-1 freezes a schema version "once
published in a package", and its Rules Out includes counting an unpublished
pack as shipped. C-17 lets a locked version no published package has carried
be relocked
([decision 0014](../../src/tool/.spec/decisions/0014-a-locked-unpublished-version-may-be-relocked.md),
which relocked `0.2.0` for the same reason). Decision 0007's freeze applies to
published versions too. Changing `0.3.0` in place is a relock under C-17.
Item
[`0341`](../../.issue/0341-a-schema-version-is-published-only-when-a-package-carries-it.yml)
records this lesson and the rule.

Owed, for the owner to rule on: `0001-F7` uses "ships" for the embedded set
and "published" for the frozen set, and `AGENTS.md` says the pin moves "to any
shipped version". This lesson's rule speaks of "published" only. Whether
"shipped" in those texts is defined or replaced is not decided. Nothing in
`0001-F7` was edited for this lesson.

## Claim

None - a process lesson proves nothing about the tool. Review enforces the rule
under § Skill.

## Skill

[`specht-conventions` § ID schemes](../../.agents/skills/specht-conventions/SKILL.md)
now says that a schema version is published only when a `specht.tool` package
embedding it is on a feed consumers install from. Until then the version is
open, and a change to it, a new rule id included, is made in place. It names
the signals that prove nothing and says to ask the owner when it is unclear.
§ "Never add" lists a new schema version for a change that fits an
unpublished one.
