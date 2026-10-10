---
title: "Lesson 0005: An id is not reserved until it merges"
description: "Pull requests cut in parallel from one main each took the next free id, so decisions, claims, constraints, questions and work items were claimed two and three times; git reported none of it, and one duplicate item id reached main."
type: lesson
kind: process
---

# Lesson 0005: An id is not reserved until it merges

**Date:** 2026-10-09
**Kind:** process

## Symptom

On 2026-10-09 several pull requests were written in parallel, each branched from
the same `main`, and each took "the next free id" from its own copy of the tree.
Git reported text conflicts or nothing; the clashes were found by reading.

- **`0001-F5` decision 0003, claimed three times.** Pull request #67 landed
  first (`2486870`) with
  [`0003-the-frontmatter-schema-file-names-are-one-key-by-kind.md`](../../src/specht/Manifest/.spec/decisions/0003-the-frontmatter-schema-file-names-are-one-key-by-kind.md).
  #62 had written its own 0003 and landed it as 0004 (`22fa2cb`); #69 had
  written a third and landed it as 0005 (`9f01e20`). The three files have
  different names, so git would have merged all three as 0003 with no conflict.
- **`0001-F5` B-024 to B-026, C-10, OQ-2 and OQ-3, claimed twice**, by #62 and
  #69. #69's became B-036 to B-038, C-12, OQ-8 and OQ-9. `main` had also raised
  OQ-2 through #67, so #62's questions moved to OQ-3 to OQ-7.
  [`0001-F5` § 10](../../src/specht/Manifest/.spec/README.md) records both
  renumbers.
- **Work item 0118, claimed by three open pull requests at once**: #69, #71 and
  #72, all cut while `.issue/.sequence` read `0117`. #71 cut 0118 to 0124 and
  has since moved them to 0120 to 0126.
- **`0001-F7` B-037 and B-038, claimed twice**, by #62, which is on `main`, and
  by #71, which wrote B-037 to B-057.
- **A stacked pull request conflicted twice.** #69 was stacked on #62; when #62
  was squash-merged, #69 conflicted again though its tree was already correct.
- **One duplicate landed.** #69 merged with item 0118, and #72 merged after it
  (`5fa5964`) with its own. `main` holds
  [`src/specht/Manifest/.issue/0118-configurable-claim-tags.yml`](../../src/specht/Manifest/.issue/0118-configurable-claim-tags.yml)
  and
  [`.build/ContinuousIntegration/.issue/0118-windows-gates-on-main-only.yml`](../../.build/ContinuousIntegration/.issue/0118-windows-gates-on-main-only.yml),
  and a bare `0118` in a `depends_on`, a `blocks` or a commit subject no longer
  names one item.

Each later pull request paid for a merge of `main`, a renumber with every
citation followed through the specifications, the `@B-` tags, the items and
`README.md`, a fresh `spec-reviewer` round, and corrected commit messages and
pull-request text - the squash is built from commit messages, and the original
commits carry the old numbers.

## Root cause

An id is claimed by writing it on a branch, and nothing reserves it until the
branch merges. `.issue/.sequence` holds the last id claimed, but only on the
copy of the tree a session read. `AGENTS.md` § Stable IDs protects an id on
`main`; nothing covered the time between a branch taking a number and that
branch merging. The existing rule, "claim after rebasing", checks `main` alone,
and an open pull request is not on `main`.

It is a process gap, not a tool defect, and merging faster or slower does not
close it: the pull request that merges second is always the one holding a
number someone else took.

## Spec delta

None. No Feature specification changes and `specht`'s behaviour is untouched.
Item [`0127`](../../.issue/0127-reserve-an-id-before-a-branch-claims-it.yml)
records this lesson and the rule. Item
[`0128`](../../.issue/0128-detect-an-id-claimed-twice.yml) is filed for a build
check, and carries the question of what happens to the two items 0118 already
on `main`, which this lesson does not decide.

Items 0127 and 0128 were numbered by the rule: `.issue/.sequence` on `main`
read `0119`, and pull request #71 claims 0120 to 0126, so those seven ids are
skipped.

## Claim

None - a process lesson proves nothing about the tool. Until item `0128`
delivers a check, review is what enforces the rule under § Skill.

## Skill

[`specht-conventions` § Delivery](../../.claude/skills/specht-conventions/references/delivery.md)
gains **"Reserve an id before a branch claims it"**: check `main` and every open
pull request before taking a number, reserve item ids by landing the
`.issue/.sequence` bump first, keep one writer per specification, and renumber
an unmerged clash on the branch before merging `main`. Its `Never add` gains
the id claimed from `main` alone and the renumber after a merge.

The portable half is one sentence each in
[`spec-and-traceability`](../../.claude/skills/spec-and-traceability/SKILL.md)
§ "Identifiers are claimed after rebasing" - a change in review holds ids too -
and [`deliver-change`](../../.claude/skills/deliver-change/SKILL.md) step 3 -
two changes to one specification's agreement are sequenced. `AGENTS.md` carries
the trap in its short list and links the skill.
