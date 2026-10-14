---
name: spec-and-traceability
description: The specification model — a specification starts before its work items, one record holds content and another holds delivery state, and every claim traces through a scenario to a test. Use when authoring, amending, or reviewing a specification, a claim, or a work item.
---

# Specification and traceability

**Project rules.** This skill is portable. Its companion names the paths, the id
schemes, the section owners and the tracker —
[`specht-conventions`](../specht-conventions/SKILL.md). Read both.

## Where a specification starts

A Feature begins with its **specification**, authored with no work item in
existence. The specification is the agreement; items are cut from its claims
afterwards, once a stakeholder has agreed to it. That ordering is what lets a
specification be argued with before anyone has spent a day building from it.

A bug, a spike or a chore begins at the item instead, and may produce a
specification delta afterwards. A refactor begins at the item too, and produces
none: it delivers no claim and cites the constraints it preserves.

## Two records, one authority each

| Record            | Authority over                                                      |
| ----------------- | ------------------------------------------------------------------- |
| The specification | content — goal, needs, claims, constraints, scope, design, coverage |
| The work item     | delivery — who has it, what state it is in, when it closed          |

Neither restates the other. The item names its specification; **the
specification never names an item** beyond the Tasks list it cuts, because a
specification outlives any number of items.

Where one field is genuinely needed in both places, exactly one of them is the
source and the other is **mirrored** — never hand-edited on the mirror side. The
companion says which fields those are.

## Filling the blank

Guidance lives in the template. A copy carries content only: delete the
instruction comments, and never leave a `{{placeholder}}` or an empty table row
behind. A placeholder row in a coverage table is worse than an absent one — it
reads as a fact.

## One answer, one section

A derived answer is stored once. If § 9 can be read off § 3 and the test files,
then § 9 is where it lives and no other section re-summarises it. Two copies of a
derived fact drift silently, and the reader cannot tell which is stale.

## A declaration belongs to the file that compiles

Before code exists, a specification may state a signature to agree on it. Once
the code exists, **the compiler is the authority** — the specification references
the declaring file instead of restating the signature. A restated signature is
guaranteed to go stale and will be believed anyway.

## Claims and traceability

- A **claim** is one falsifiable statement, numbered, scoped to its Feature.
- Claim ids are **permanent**. Never reused, never renumbered — not to close a
  gap, not to tidy a sequence, and not during a migration between layouts.
  Renumbering breaks every citation in every past commit, review, issue and
  sibling specification.
- A withdrawn claim is **marked** withdrawn, not deleted. Its number stays
  retired.
- Each scenario carries the tag of the claim it documents. The tag is the anchor;
  a scenario title is a human-reading courtesy and a stale one is a cleanup, not
  a gate failure.
- The **traceability matrix is the gate**: every claim appears there exactly once.
  A row whose test is missing blocks the item reaching done. It does not block the
  specification being agreed — those are different questions.

## An imported rule is a decision, not a constraint

An external rule the project chose to adopt — a vendor's requirement, a
platform's convention, an upstream skill's pattern — is a **decision record**
naming what it costs and what was accepted, not a constraint row. Constraints are
what the problem imposes; decisions are what the project chose. Filing a choice as
a constraint hides that it could be revisited.

## Numbers from independent sequences collide

Several id schemes coexist, each with its own sequence. Always carry the scheme
prefix in prose: a bare "12" is unresolvable when claims, constraints, open
questions and tasks all have a twelfth member.

## Section ownership

Each section has exactly one owning role. A role that needs another's section
changed **escalates**; it does not write there. Filling someone else's section
removes the only signal that the preceding stage is incomplete. The ownership map
is written in one place — the companion — and nowhere else.

## Records beside a specification

Three kinds of record sit beside a specification, and the distinction is what they
are about:

| Record   | About                                           |
| -------- | ----------------------------------------------- |
| decision | the product — what will and will not be built   |
| ADR      | the code's structure — a durable technical rule |
| lesson   | an incident and what it changed                 |

Location is chosen by **blast radius**, not by who made the call: a record binding
one Feature sits beside that Feature's specification; a record binding the whole
repository sits at the repository root, numbered repo-wide.

An accepted ADR is immutable. Only its status changes; a new ADR supersedes it
entirely. A decision that is later reversed keeps its original reasoning and gains
an appended reversal — including **what was considered and not chosen**, so a
later reader cannot rediscover the rejected option as a gap to fill.

## Identifiers are claimed after rebasing

Claim an id against trunk, not against your branch. Two branches that both take
"the next number" collide on merge, and the loser must renumber — which is the one
thing ids may never do. If a collision does land, the newer change renumbers its
own **new** ids; an id already cited anywhere stays.

Trunk is not the only claimant: a change still in review holds the ids it wrote.
Count those before taking a number; the companion says how one is reserved.

## Never add

- Template guidance, or a `{{placeholder}}`, left in a copy.
- A placeholder row in a coverage or traceability table.
- A renumbered or reused claim, constraint, question or task id.
- A second copy of a derived answer, or a section mirroring another.
- A signature restated in prose after the code exists.
- A green coverage signal for a test that does not exist.
- A hand-kept index of specifications, decisions or lessons.
- An item that a specification names as its owner.
