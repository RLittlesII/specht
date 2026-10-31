---
title: "Lesson 0002: A removed item leaves its issue on the board"
description: "The mirror keeps and labels stale the issue of a removed item, so a pull request that removes items leaves open work on the public board until someone is told to close it"
type: lesson
kind: process
---

# Lesson 0002: A removed item leaves its issue on the board

**Date:** 2026-10-30
**Kind:** process

## Symptom

Pull request #604 (`d9f993a6`, owner decision 2026-10-28) removed items
`0075`, `0076`, `0077`, `0089`, `0090` and `0091`. Their mirror issues
#326, #327, #328, #333, #334 and #335 stayed open in milestone 0.3.0,
labelled `stale`, for two days, so the public board showed 0.3.0 at 79%
complete with seven open issues when one (`0041`) was open work. A status
read from the local queue listed only `0041`; the owner had to ask why the
two disagreed, and closed the six issues as not planned on 2026-10-30.

## Root cause

The mirror did what the Feature specifies: an issue whose item file is gone is
neither closed nor deleted, is labelled `stale` and is named in the run's step
summary (B-033, B-034, B-035; decision 0001 item 11; `mirror_items.py` adds the
label and has no close for an orphan). The gap is in the delivery flow: no
rule told a pull request that removes an item what it leaves on GitHub, nobody
was told to read the step summary's `### Stale` lines after the merge, and the
only way an item closes its issue is `done`, which closes it as `completed`.
`references/delivery.md` forbids a hand edit of a mirrored issue and forbids a
deleted item, so a removal by owner decision had no sanctioned follow-up.

## Spec delta

No claim changes. B-033 is the owner's decision and stands. Whether the mirror
should close an orphan as not planned itself is a design call for the owner,
recorded as OQ-12 in § 11 with its default.

## Claim

- None changes. B-033, B-034 and B-035 hold as written.

## Skill

`specht-conventions` `references/delivery.md` gains § "Removing an item leaves
its issue on the board": a pull request that removes an item or an epic names
the issues it leaves behind, the author hands the owner the `### Stale`
numbers from the mirror run on `main` after the merge, and a status read
compares the queue's open count for a milestone with the board's.
