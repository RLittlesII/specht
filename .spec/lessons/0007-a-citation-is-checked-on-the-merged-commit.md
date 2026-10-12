---
title: "Lesson 0007: A citation is checked on the merged commit"
description: "Three squash merges reached main with an empty body though their branch commits cited what they delivered, and the rule's wording, that the body opens with the citation, is one a squash of several commits cannot meet; history is not rewritten, so this file records what those merges specify."
type: lesson
kind: process
---

# Lesson 0007: A citation is checked on the merged commit

**Date:** 2026-10-12
**Kind:** process

## Symptom

On 2026-10-12 the sixteen merges on `main` from `4fd1cc6` to `261797b` were
read against the rule that a merged commit's body opens with `Delivers <id>` or
`Specifies <feature path>`. The squash setting is `COMMIT_OR_PR_TITLE` with
`COMMIT_MESSAGES`.

**Two merges have an empty body.** The citation is in the subject's Feature id
and nowhere else on `main`.

- `4fd1cc6`, pull request #122,
  `spec(0001-F9): specify reading and setting the configuration from the command line`.
  The branch held one commit, `8588e70`, whose body opened
  `Specifies src/tool/Features/Manifest/.spec/README.md`.
- `03f9cc7`, pull request #125,
  `spec(0001-F7): hold the first release for a schema sourced at a URL`. The
  branch held one commit, `e745f8e`, whose body opened
  `Specifies src/tool/.spec/README.md`.

**Six merges have a body that opens with a list, not the citation.** Each is a
squash of more than one commit, so its body opens with `* <subject>` and the
`Delivers` or `Specifies` line follows inside the list: `02ff4d2` (#123),
`60de077` (#126), `881fe6f` (#129), `dcf6fc2` (#132), `83d6f80` (#133) and
`44ec021` (#138). The citation survived in all six; the wording of the rule
did not hold.

**Outside those sixteen**, read the same day:

- `b4c3392`, pull request #121, the merge before `4fd1cc6`, has an empty body
  too. It is a squash of nine commits, five of which opened `Delivers` and two
  `Specifies`.
- `6fe8b27`, pull request #141, merged after the audit, is a seventh squash
  whose body opens with the list.
- Commits on `main` before 2026-10-11 with no body are the repository owner's
  `chore:` commits, which name no item. They are not part of this lesson.

### What the empty-bodied merges carry

History stays as it is, so this is the record.

| Commit    | Pull request | Specifies                                                                                                                                                                                                                                                                                                                                                                              | Items                                |
| --------- | ------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------ |
| `b4c3392` | #121         | [`src/tool/Features/Check/.spec/README.md`](../../src/tool/Features/Check/.spec/README.md) (`0001-F2`), B-017 to B-036                                                                                                                                                                                                                                                                 | delivers 0171, 0172, 0173 and 0174   |
| `4fd1cc6` | #122         | [`src/tool/Features/Manifest/.spec/README.md`](../../src/tool/Features/Manifest/.spec/README.md) (`0001-F9`), with [`0001-F10`](../../src/tool/Features/Rules/.spec/README.md), [`0001-F11`](../../src/tool/Features/Rules/Set/.spec/README.md) and amendments to [`0001-F4`](../../src/tool/Features/Init/.spec/README.md) and [`0001-F5`](../../src/specht/Manifest/.spec/README.md) | cuts 0175                            |
| `03f9cc7` | #125         | [`src/tool/.spec/README.md`](../../src/tool/.spec/README.md) (`0001-F7`), with [`0001-F4`](../../src/tool/Features/Init/.spec/README.md), [`0001-F3`](../../src/specht/Report/.spec/README.md) and [`0055-F6`](../../.build/Releasing/.spec/README.md)                                                                                                                                 | cuts 0176, 0177, 0178, 0179 and 0180 |

## Root cause

Two gaps, one in the wording and one in where the rule was checked.

**The wording could not be met.** Under `COMMIT_MESSAGES` a squash of several
commits builds its body as a `* <subject>` list. "The body opens with" holds
for a squash of one commit and for no other.

**The rule was checked on the branch and never on `main`.** All three
empty-bodied merges came from branch commits that carried the citation. None
of the three bodies is the one the squash setting builds: a one-commit squash
takes the commit's body, as `979b3a8` (#136) and `261797b` (#139) did, and a
nine-commit squash takes the list. Something replaced the body at the merge.

- For #122 the pull request's auto-merge request records an empty commit body.
  #139's records the full body, and that body reached `main`. Which command or
  click stored the empty one is not established.
- For #121 and #125 the pull request records no auto-merge request, and how
  the body was lost is not established.

Nothing read the merged commit afterwards, so each loss went unseen until the
audit.

## Spec delta

None. No Feature specification changes and `specht`'s behaviour is untouched.
Item [`0193`](../../.issue/0193-commit-citations-survive-the-merge.yml) records
this lesson, the rule, and the repository owner's decision of 2026-10-12,
"Forward fix only": `main`'s history is not rewritten and no git note is
added.

## Claim

None - a process lesson proves nothing about the tool. Nothing checks a commit
message; whoever merges enforces the rule under § Skill.

## Skill

[`specht-conventions` § Delivery](../../.claude/skills/specht-conventions/references/delivery.md),
"Cite the item and the claim in the commit message", now says: every commit on
a branch carries the citation as the first line of its body; the citation
inside a squash's `* <subject>` list satisfies the rule; a merge does not
override the squash body; an auto-merge's stored body is read; and the merged
commit is read on `main`, a miss being recorded and not repaired. Its
`Never add` gains the commit with no body and the merge that overrides the
squash body.

`AGENTS.md` § SDLC carries the same rule in one bullet and links the skill.
[`deliver-change`](../../.claude/skills/deliver-change/SKILL.md) step 6
already says to cite where the citation survives the merge, and is unchanged.
