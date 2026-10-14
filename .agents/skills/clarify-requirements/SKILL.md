---
name: clarify-requirements
description: Resolve genuinely material requirement ambiguity in a specification-driven repository. Use when two plausible readings would change user-visible behaviour, the data shape, a public surface, or a provider's terms.
---

# Clarify requirements

**Project rules.** This skill is portable. Its companion names where answers are
recorded and which questions this project has already settled —
[`specht-conventions`](../specht-conventions/SKILL.md) § "Questions". Read both.

## First, look

The specification, the scenarios, the sibling Features' scope sections, the
decision records, the lessons, the code, and the tests. Most questions are already
answered there, and a question the project knows it has not decided is recorded as
an open question rather than re-asked.

## Decide or ask

- **Reversible, with no behavioural change?** Decide it, and document the
  assumption locally. Folder names, class structure, the order of two independent
  operations — those are yours.
- **Ask one concise question** when the answer cannot be found and the choice
  materially changes user-visible behaviour, the data shape, a public surface,
  credential handling, a provider's terms, or a guarantee the design makes.
  Include: the evidence you already have, two concrete readings, what each costs,
  and the smallest decision that unblocks you.
- **Ask what rule the answer follows**, not only which option wins. An undocumented
  ruling cannot be checked later, and a standing convention will eventually
  contradict it once code is built on both.
- **Never resolve material ambiguity by assumption** — not under a spike framing,
  not under a prototype framing, not because the session is nearly over.

## An exploratory comment is not a decision

"What if we…", "another thought…", a reference to how a previous project did it —
these are substantive enough to change direction and carry none of the structure
of a decision: no decision-required framing, no explicit choice, no rejected
alternative. **Escalate before allocating work against one.** "This sounds like it
could be a direction change — is it exploratory, or should it go through the
decision protocol?" is a cheap question that prevents a day of wasted design.

A choice that moves a load-bearing boundary goes through an explicit decision
gate, always.

## A pullback scopes only what was withdrawn

The opposite error costs as much and is harder to catch. When someone pulls back
from a direction, they withdrew **what they named**, not everything adjacent to
it — and "we deferred that" reads as caution rather than as a claim needing
evidence, so nobody checks it.

**Before recording that something is undesigned, open the specification and read
the section.** When a design genuinely is superseded, the honest move is an open
question or a decision-required block **on the specification that owns it** —
never a contradicting note in a third document that points at neither.

## Write the answer back

In the same change, so the next session starts from the answer:

- The answer becomes a claim, a scenario, a specification edit, or an
  out-of-scope row. **The specification is where it lands first.**
- A decision record captures what was chosen **and what was rejected, with its
  cost** — the second half is what stops the rejected option being rediscovered as
  a gap.
- A design change updates the section it affects and the item's acceptance
  criteria.

An answer that exists only in conversation was never recorded.

## Never add

- An assumption standing in for a material decision.
- A question asked that the specification already answers.
- A decision recorded without its rejected alternative.
- An exploratory comment treated as ratified.
- A note in a third document contradicting a specification it does not cite.
- A ruling that names the winning option but not the rule it follows.
