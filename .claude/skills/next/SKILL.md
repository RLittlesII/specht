---
name: next
description: Answer "what is next" from the work-item queue as a choice - one startable item per parallel lane, so several can be picked up at once without colliding - then the startable items, the high-ranked items still waiting and on what, and the items already taken. Use when asked what to pick up, what is next, or what can be worked on at the same time.
---

# What is next

**Project rules.** This skill is portable. Its companion names the tracker, the
script that reads it, and the write sets that put two items in one lane -
[`specht-conventions`](../specht-conventions/SKILL.md) § "Delivery", "Choosing
the next item". Read both.

## The answer, in one pass

Run the companion's script and report what it prints. Do not rebuild the answer
by hand: a hand pass over a whole queue misreads a status that carries a
comment, misses items in hidden folders, and forgets a dependency's status.

## What each part means

- **Next** is a set, not one item: the head of every lane. A head is the
  highest-ranked item in its lane that is ready to start, whose every hard
  prerequisite is done, that nobody has taken, and that is not a container for
  open children. Ties keep rank order, then id. The person asking chooses from
  the set; the answer does not choose for them.
- **Rank is read, never computed here.** It is derived where the tracker says;
  a rank that looks wrong is a grooming problem, raised, not corrected in the
  answer.
- **Blocked** lists items ranked at or above the last startable one that still
  wait on something, with what they wait on. A high-ranked blocked item is
  often the better thing to unblock than the next startable one.
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

Lead with the lanes: one row per lane - its head, rank, priority, status,
title, the items queued behind it, and the write set it holds. Then the
startable table - id, rank, priority, status, title, description - the blocked
list and the taken list.
State the commit the answer was read at: the queue moves with every merge, and
an answer from before a pull is stale.

## Never add

- A rank, priority or blocker computed by hand instead of read.
- An edit to an item, a status change, or a claim on one - this skill only
  reads.
- A taken item offered as next.
- A single item offered as the answer while more than one lane can start.
- A lane split that ignores a shared write set because no dependency joins the
  two items.
