---
name: role-agents
description: The four documented role contracts that own the specification chain in this repository, and the rule that each role trusts only the artifact of the role before it
---

# Role agents

`specht` is specification-driven: the specification is the artifact of record and
code is downstream of it. Four roles own that chain, and each is a **documented
contract** — for whoever takes the role, a person or an agent.

| Role                                  | Turns                              | Into                                      |
| ------------------------------------- | ---------------------------------- | ----------------------------------------- |
| [`spec-author`](spec-author.md)       | a decided need                     | the agreement, and the `.feature` file    |
| [`test-writer`](test-writer.md)       | a claim                            | a failing scenario and failing tests      |
| [`implementer`](implementer.md)       | a failing test                     | production code, and the design that explains it |
| [`spec-reviewer`](spec-reviewer.md)   | a diff                             | a sign-off, or findings                   |

## Sequential trust

**Each role trusts only the artifact the preceding role produced — never a chat
summary of it.** The test writer works from the claim, not from the conversation
that produced the claim. The implementer works from the failing test and the
claim it cites, not from a description of what the test is getting at. A
conversational handoff leaves no artifact and does not survive to the next
session or the next agent.

This is also why no role fills another's sections. Writing into someone else's
section removes the only signal that the preceding stage is incomplete.
A role that finds its input missing or wrong **escalates to the owning role**; it
does not quietly supply the gap.

## Where ownership is written

Which role owns which section is written in exactly one place:
[`specht-conventions`](../.skills/specht-conventions/SKILL.md)
§ "Section ownership". It is not restated here, and not restated in the role
files — a second copy is a second thing to drift.

## Standing notes

- **No role is mandatory for a small change; the ordering is.** A one-line fix
  does not need four handoffs. It still may not reach code before the claim it
  satisfies exists.
- **Scenarios execute.** `test/Specht.Acceptance` runs the `.feature` files
  through Reqnroll. Unit tests still cover the concerns — a passing acceptance scenario
  is not a substitute for them, and § 9 cites both.
- **Every role writes less than it wants to.** Enough code to turn the test
  green; no abstraction without a second caller that exists today; one statement
  per fact. The rule is in
  [`coding-conventions`](../.skills/coding-conventions/SKILL.md) §§ "Design" and
  "Say it once, and briefly" - each role file names only what that means for the
  artifact it owns.
- **These are contracts, not loadable agents.** The directory is `.agents/`, not
  `.claude/agents/`. `.claude/` is gitignored and reinstallable; a rule placed
  there cannot be reviewed in a pull request, so no rule lives there.
