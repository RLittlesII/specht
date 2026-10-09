---
title: "Lesson 0004: Next offers a choice"
description: "/next answered with one record when the owner wanted the parallel work to choose from, because the next skill said to lead with one item and its script singled out a top item."
type: lesson
kind: process
---

# Lesson 0004: Next offers a choice

**Date:** 2026-10-09
**Kind:** process

## Symptom

The repository owner ran `/next` and corrected the answer: "`/next` isn't
supposed to return a SINGLE record. It's supposed to return a list of
paralelizable work that I can chose from!"

## Root cause

Item `0113` built the command to choose for the owner. The `next` skill's
Reporting said "Lead with the next item", and defined Next as the one
highest-ranked startable item. The script followed it: it printed
`**Next:** <id>` first and the parallel lanes last, so the parallel work the
owner chooses from came after an answer that had already chosen.

## Spec delta

None. `next` is a process skill; no Feature specifies it and no `.feature`
file is involved. Item `0115` records the fix.

## Claim

None. Verification is running the script: on `main` at `0b58a7c` it opens with
six lanes, headed by 0029, 0073, 0024, 0013, 0051 and 0035, and prints no
single next item.

## Skill

[`next`](../../.claude/skills/next/SKILL.md):

- The description answers "as a choice - one startable item per parallel
  lane".
- What each part means: "**Next** is a set, not one item: the head of every
  lane. ... The person asking chooses from the set; the answer does not choose
  for them."
- Reporting: "Lead with the lanes: one row per lane - its head, rank, status,
  title, the items queued behind it, and the write set it holds", replacing
  "Lead with the next item".
- Never add gains "A single item offered as the answer while more than one
  lane can start."

The companion's
[`delivery.md`](../../.claude/skills/specht-conventions/references/delivery.md)
states the new output order, and its `scripts/next.py` prints it.
