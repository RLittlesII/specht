---
name: deliver-change
description: Deliver a change through a work item, a branch, the specification, the build, a pull request, and a completion report. Use when picking up work, filing an item, committing, or opening and watching a pull request.
---

# Deliver a change

**Project rules.** This skill is portable. Its companion names the tracker, the
branch and worktree conventions, the build commands, the merge strategy and
report publication —
[`specht-conventions`](../specht-conventions/SKILL.md) § "Delivery". Read both.

## The shape of it

1. **A Feature starts at its specification.** Items are cut from its claims after
   agreement, not before. A bug, spike, chore or refactor starts at the item
   instead; a refactor cites the constraints it preserves, never a claim.
2. **Take exactly one item.** Signal that you have it in the tracker _before_ the
   branch, before the worktree, and before the first edit. An item already
   signalled as taken is never picked up — that signal is the only thing stopping
   two people building the same thing.
3. **One item in progress at a time, per Feature.** A task that cannot be finished
   without a sibling finishing first is a decomposition problem, not a small task.
   Two changes that amend one specification's agreement are sequenced the same
   way, never written side by side: each would take the same next id.
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
10. **End with a delivery report.** Gather every contributor's handoff across
    the chain, create the report described below, publish it using the companion's
    conventions, and hand over its link. Update the same report after subsequent
    review rounds.

## Gather handoffs as the work proceeds

The coordinating agent collects a brief handoff from each participating agent
before it finishes or its context is lost. Include contributors to rework and
review rounds as well as the first pass. When one person or agent performs several
steps, attribute each step to that contributor; a report does not require extra
agents.

Each handoff records the step and contributor, work performed and findings,
decisions with their reasons, artifacts changed or produced, validation commands
and observed results, unresolved concerns, and a completion note stating what was
finished and what remains. Effort means the work performed; add time or token
counts only when measured, naming the measurement source. If a handoff is missing,
recover what the artifacts and available outputs establish and label the gap.

These handoffs supply the report. They do not replace the upstream artifacts each
role must read or the decision records it must write.

## The delivery report

Identify the item, scope, specification claims or exemption, contributors, branch,
commits and pull request where available. State the overall outcome and any next
action or blocker.

Cover every process step in order: taking and scoping the item, specification and
scenarios (or the exemption), tests, implementation, benchmarking when applicable,
verification, review and rework, and pull-request delivery and check monitoring.
For **each step**, include:

- **Contributor:** the agent or person responsible, including separate
  contributions when more than one participated.
- **Effort and findings:** what each contributor investigated, authored, changed
  or checked, and what they found.
- **Decisions:** the choices and reasons, linking the authoritative records;
  state when no new decision was needed.
- **Evidence:** links to the relevant artifacts, claims, tests, commits or review
  findings, with observed validation results.
- **Status and completion note:** complete, pending, blocked or not applicable,
  followed by what finished, what remains and why. Explain inapplicable steps
  instead of silently omitting them.

Reconcile the handoffs against the delivered artifacts and checks. Preserve
unresolved findings and disagreements; distinguish an agent's reported result
from a result independently verified. Never infer sign-off, passing CI or a merge
from a local green build or an open pull request. If work stops before delivery
finishes, publish the report with the unfinished steps and blockers visible.

The report is a delivery summary; specifications and the tracker remain the
authorities for content and delivery state. Keep their facts linked rather than
creating another agreement in the report. The final handover links the published
report and states the observed delivery outcome.

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
- A final handover without a delivery report, or a report that silently drops a
  contributor, an unfinished step or an unresolved finding.
- Invented effort measurements, validation results or completion claims.
