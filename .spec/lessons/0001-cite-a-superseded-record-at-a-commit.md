---
title: "Lesson 0001: Cite a superseded record at a commit"
description: "A record the specifications have absorbed is deleted and cited at its last commit; a citation is not a reason to keep a file live."
type: lesson
kind: process
---

# Lesson 0001: Cite a superseded record at a commit

**Date:** 2026-10-08
**Kind:** process

## Symptom

Asked whether `REQUIREMENTS.md` was still needed, the agent answered "keep it":
five files cite it, and deleting it "breaks citations the same way renumbering
an id does." The repository owner overruled: the specifications are the source
of truth, so the citations should break — or rather, name the file at a commit.

## Root cause

An identifier and a file path were treated as the same kind of citation. The
rule that ids are never reused and never renumbered exists because an id has no
history to fall back on; a file does — git keeps every version, and
`git show <commit>:<path>` resolves a pinned citation forever. Nothing said what
happens to an input record — a requirements session, a seed draft — once the
specifications absorb it, so the agent defaulted to keeping it live, where it
invites edits nobody reviews against the specifications it fed.

## Spec delta

No Feature specification changes; `specht`'s behaviour is untouched. Item
[`0003`](../../.issue/0003-retire-requirements-md.yml) deleted `REQUIREMENTS.md`
and pinned its six citations to commit `254aabc`, after confirming each open
question it carried is held in a specification or needs no answer.

## Claim

- None — a process lesson proves nothing about the tool. Item `0003`'s
  acceptance criteria are the check that this instance was done; the rule under
  § Skill is what stops the next one.

## Skill

`AGENTS.md` § Stable IDs, since this repository has no `.skills/` yet and the
installed `specht-conventions` is gitignored:

> A record the specifications have absorbed is retired, not kept: delete it and
> cite it at its last commit (`` `REQUIREMENTS.md` at commit `254aabc` ``). A
> citation pins a file to a commit; it is never a reason to keep the file live.
