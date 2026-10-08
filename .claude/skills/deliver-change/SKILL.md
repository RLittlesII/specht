---
name: deliver-change
description: Deliver a change through a work item, a branch, the specification, the build, and a pull request. Use when picking up work, filing an item, committing, or opening and watching a pull request.
---

# Deliver a change

**Project rules.** This skill is portable. Its companion names the tracker, the
branch and worktree conventions, the build commands and the merge strategy —
[`specht-conventions`](../specht-conventions/SKILL.md) § "Delivery". Read both.

## The shape of it

1. **A Feature starts at its specification.** Items are cut from its claims after
   agreement, not before. A bug, spike or chore starts at the item instead.
2. **Take exactly one item.** Signal that you have it in the tracker _before_ the
   branch, before the worktree, and before the first edit. An item already
   signalled as taken is never picked up — that signal is the only thing stopping
   two people building the same thing.
3. **One item in progress at a time, per Feature.** A task that cannot be finished
   without a sibling finishing first is a decomposition problem, not a small task.
4. **Do not start an item whose specification is unresolved.** Open questions that
   block the claims you are about to build are resolved first, or the exception is
   written down as a decision record naming what was assumed.
5. **The specification delta is part of the change.** Scenario and claim edits ship
   in the same pull request as the code, not after it. A correction that lives only
   in conversation does not survive to the next session.
6. **Cite the claim where the citation survives the merge.** Know which merge
   strategies the project uses before deciding where a citation goes: a squash or
   rebase merge can discard a pull-request body.
7. **Verify before you call it done.** Run the narrow check for what you touched,
   then the full build. A green build proves only that the targets it declares
   ran — read what they actually cover.
8. **Open the pull request, then watch it.** A failing check is yours to fix, not
   the reviewer's to discover.
9. **Review feedback is a loop, not a conversation.** For each round: make the
   change, commit, push, then resolve every thread the commit addressed with a
   reply naming only that commit. Nothing else goes in the thread - the diff is
   the answer, and a reviewer who wants more reopens it. A comment you disagree
   with is answered in the thread before any resolve, and stays open.

## Exemptions

A behaviour change that touches no scenario is suspect. An exemption is claimed by
**citing the constraints the change preserves** — a closed category (refactor,
rename, formatting, dependency bump), a real reason, and the existing constraint
ids that still hold. Reaching for an exemption because writing a scenario feels
slow is the one use of it that is forbidden outright. Nothing automates this
check; review enforces it.

## When a bug fix reveals a gap

The fix ships with a lesson in the same pull request: symptom, root cause, the
specification delta, and the claim that now proves it. A fix that reveals nothing
writes no lesson. A **process** lesson also updates the skill that would have
prevented it — the skill holds the rule, the lesson keeps the incident.

## Never add

- A second item in progress alongside the first.
- A branch or an edit before the item is signalled as taken.
- A claim, a status, or a rule duplicated into the item from the specification.
- A scenario edit deferred to a follow-up pull request.
- A skipped phase because the change "is small". Small changes skip roles, not
  the ordering.
- A green build read as proof of coverage.
- A review thread resolved without a pushed commit, or with prose beyond the
  commit that addressed it.
