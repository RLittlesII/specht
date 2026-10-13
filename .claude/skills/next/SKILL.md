---
name: next
description: Choose the next Feature to drive to completion by comparing product importance, remaining work and blockers, with item-level parallel lanes as supporting detail. Use when asked what is next, which Feature to focus on, what to pick up, or what can run in parallel.
---

# What is next

**Project rules.** This skill is portable. Its companion names the tracker, the
script that reads it, and the write sets that put two items in one lane -
[`specht-conventions`](../specht-conventions/SKILL.md) § "Delivery", "Choosing
the next item". Read both.

## The answer, in one pass

Run the companion's script and use its derivations for the answer. Read the
complete queue for Feature comparisons, not only the highest-ranked items.
Do not rebuild scores or dependency blockers by hand: a hand pass misreads a
status that carries a comment, misses hidden folders, and forgets a dependency's
status.

## Default to Feature focus

Lead with Features the user can drive to completion. Name each Feature and its
stable id, then show its remaining child items, external prerequisite work,
decision holds, taken work and a dependency-respecting completion path. An item
outside the Feature can be the first step when it is required to finish it.

Compare three distinct questions:

- **Most important:** product-goal priority and authored value. A high score
  does not by itself mean highest product importance.
- **Closest to completion:** remaining work across the whole dependency
  closure, including prerequisites in other Features. Item counts and summed
  effort are queue measures, not elapsed-time estimates or coverage percentages.
- **Fewest blockers:** distinguish external prerequisites, internal sequencing,
  decision holds and work already taken. Zero external prerequisites does not
  mean there are no internal blockers.

Recommend a Feature focus with the reason and first available step, while
showing the alternatives. Once the user chooses a Feature, keep subsequent
next answers focused on finishing it until they change direction or it is done.
Recommending a Feature does not claim its items or authorise implementation.
Check its completion against the specification and tracker before calling it
finished; a completed child list alone does not prove every claim.

Item-level lanes support this choice. Do not revert to an item-first answer on
a later invocation. If the user asks specifically for parallel work, expand the
lane detail and preserve the shared-write-set restrictions below.

## What each part means

- **Next** is a set, not one item: the head of every lane. A head is the
  highest-ranked item in its lane that is ready to start, whose every hard
  prerequisite is done, that nobody has taken, and that is not a container for
  open children. Ties keep rank order, then id. The person asking chooses from
  the set; the answer does not choose for them.
- **Rank is computed by the companion's script on every run**, never stored
  and never by hand. It orders work by what gets the product, then a Feature,
  to done soonest. A rank that looks wrong is a grooming problem in the fields
  it is computed from, raised, not corrected in the answer.
- **Priority** is the tier rank sorts inside: work on the path to the product
  goal, then work some Feature needs, then work nothing waits on.
- **Toward** names the Feature an item's rank comes from and, in brackets, the
  work that Feature has left.
- **Gates** names the open items that wait on this one directly: what
  finishing it frees.
- **Blocked** lists items ranked at or above the last startable one that still
  wait on something, with what they wait on. A blocker carries the rank of what
  it gates, so the work that frees a blocked item ranks with it, not below it.
- **Blocked by status** lists items marked blocked that wait on no open item:
  what holds them is a decision, not work in the queue.
- **Taken** items are never offered, whatever their rank.
- **A status short of "ready to implement"** means the item still needs its
  design pass before code. Say so beside the answer; it is the first step, not a
  reason to skip the item.

## Parallel lanes

Two items can run at the same time only when neither waits on the other **and**
they share no write set: the same specification, the same generated output, or
the same hand-edited build file. Items that share one fall in one lane and run
in rank order inside it; separate lanes can run at once.

A shared write set is a merge conflict, not a dependency. Two items editing one
specification's traceability table, or two branches each regenerating one
committed workflow, will collide even though neither needs the other's
behaviour. A lane is a prediction from where an item lives; the design pass says
which files it actually touches, so restate a lane as confirmed only after it.

## Reporting

Lead with a Feature comparison: name and id, importance, work remaining,
blockers, completion path and next available step. Explain which Feature best
serves product delivery and which is closest to completion when those differ.
Keep a chosen Feature prominent rather than recommending unrelated items solely
because their rank is higher.

Follow with the supporting lanes, startable items, dependency blockers, status
holds and taken work. Lane detail carries the head, rank, priority, toward,
gates, status, title, queued items and shared write set; item detail carries the
description too. A concise response may link the full published snapshot for
these details. State the commit read: the queue moves with every merge, and an
answer from before a pull is stale.

## Never add

- A rank, priority or blocker worked out by hand instead of taken from the
  script's output.
- An edit to an item, a status change, or a claim on one - this skill only
  reads.
- A taken item offered as next.
- An item-first answer when the user is choosing a Feature to complete.
- A Feature marked finished from child counts without checking its agreement.
- A lane split that ignores a shared write set because no dependency joins the
  two items.
