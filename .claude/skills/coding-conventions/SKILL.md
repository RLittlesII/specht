---
name: coding-conventions
description: Conventions for any code, test, documentation or diagram change in a specification-driven repository — orient first, stop on a gap, keep the design direct, put a rule where it runs, and keep a skill to the rule. Use for any change.
---

# Coding conventions

**Project rules.** This skill is portable. Its companion names the layout, the
analyzer settings, the package policy and the commands —
[`specht-conventions`](../specht-conventions/SKILL.md) § "Coding". Read both.

## Before implementing

- **Orient first.** Read the claim, the scenario, the sections that own the
  design, and the code that already exists. Most apparent gaps are a sibling's
  stated scope.
- **Stop when artifacts conflict.** Two documents disagreeing is a finding, not an
  ambiguity to average out. Escalate to the role that owns the section.
- **Test the assumption before building on it.** A spike that proves a mechanism is
  cheaper than a design that assumed it.
- **Say what is out of scope** for the change, in the change.
- **Extend what is already in place.** A library, tool or approach the
  repository already holds is the owner's decision, not a draft. When it misses a
  claim, make it fit through its own extension points. Replacing it is a new
  decision: ask before the change, and record it in the specification - never
  only in a pull-request body.

## Design

- **Keep it direct.** The smallest construction that satisfies the claim. Write
  enough to turn the test green and stop. Behaviour no claim states does not get
  built, however obvious or cheap it looks.
- **Pick the approach that fits, not the one that scales.** "Best" here means
  the one a reader understands fastest and the one the claim actually needs —
  not the most general, the most configurable, or the most clever. A pattern is
  worth its name only when the problem already has that shape.
- **No speculative abstraction.** An interface, a base class, a generic
  parameter, a factory, an options object, a helper: each needs a **second real
  caller or a substitution that exists today**. "We will need it later" is not a
  need; the later change is cheaper than the wrong seam. One implementation
  behind an interface is a rename waiting to happen.
- **An interface at a seam, not at every class.** A seam is where something is
  substituted: a test double, a second provider, a second host.
- **Delete rather than generalize.** When two things nearly rhyme, leave them
  duplicated until the third appears and the shape is obvious. Premature
  deduplication couples two callers that were free to diverge.
- **Asynchronous signatures for I/O.** The return type says whether a method is
  asynchronous; never block on it.
- **Compose writes so they are atomic.** A partially applied write is the failure
  mode that costs the most to diagnose.
- **Reuse found is cited.** A second implementation of something already present
  is a finding, not a deliverable.

## Naming and layout

Build configuration is authoritative. The formatter, the analyzers and the editor
configuration decide naming, modifier order, braces and line length — prose in a
skill does not override them, and a convention that matters is encoded there so it
fails the build rather than a review.

## Dependencies

Versions are declared centrally for the whole repository, never in a single
project file. A new dependency lands as a declaration **and** a reference in the
same change, with a reason beside it if the version is pinned for a reason.

## Generated files and guards

- **Never hand-edit a generated file.** Regenerate it, deliberately, and commit the
  regeneration.
- **Never commit a generated report.** Reports carry absolute paths from the
  machine that produced them.
- **Put the rule where it runs.** A rule enforced only by review is a rule that
  will be missed; prefer the analyzer, the schema, or the build target. A test that
  asserts a convention beats a paragraph asking for it.
- **A suppression carries its reason** on the line beside it.

## Say it once, and briefly

Every artifact here is read far more often than written, by people and agents
with a budget.

- **One statement per fact.** No preamble, no restatement in the next paragraph,
  no summary of what was just said.
- **A table or a list where prose would ramble.** A claim is one sentence. A
  constraint is one sentence plus what it rules out.
- **Name the thing, not the journey to it.** The reasoning belongs in an ADR or
  a decision record, where it can be reviewed; the artifact carries the outcome.
- **Cut the hedge.** "Probably", "it may be worth", "we might want to" in a
  specification means the question is open - record it as one.

Terse is not curt: keep the fact that costs real damage when missed, and the
trap that is not obvious. Compression stops where a reader would guess wrong.

## Documentation and diagrams

- Every tracked markdown file opens with frontmatter. Skill and role files declare
  `name` and `description`; everything else declares `title`, `description` and
  `type`. Runtime prompts are exempt.
- One diagram notation, inline. A diagram that needs a toolchain to read is a
  diagram nobody reads.
- `<summary>` stays one line. A remark is one sentence; reasoning belongs in the
  specification or an ADR, where it can be reviewed.
- Test data is synthetic and invented. Never a captured real payload.

## Skills

A skill is one of three kinds, and never a mixture:

| Kind       | About                                            | May name a path or command |
| ---------- | ------------------------------------------------ | -------------------------- |
| method     | how work is done, portable to another repository | no                         |
| companion  | this repository                                  | yes — it is the only one   |
| technology | a library or tool, silent about the product      | the library's own surface  |

A skill holds **the rule, the trap, and the `Never add` list**. Facts live where
they are authoritative and the skill links them. Restating a fact in a skill
creates a second place for it to drift, and the skill is the copy that goes stale.

**When two skills touch one subject, one owns it and the other links.** Decide
which before writing the second copy, not after they disagree - the drift is
silent, and a reader cannot tell which is current. A `Never add` item may restate
a rule as a single prohibition; a second explanation of it may not. Where the
ownership boundaries sit is written once, in the project's own instructions —
its companion skill names the file.

## Before finishing

- Run the narrow check for what you touched, then the full build.
- Read the diff for changes you did not intend — a formatter pass, a reordered
  file, a stray generated artifact.
- Update the specification section the design change affected, in the same change.

## Never add

- A hand edit to a generated file.
- A dependency version pinned in a single project file.
- A suppression with no reason beside it.
- A blocking wait on an asynchronous call.
- A fact in a skill that is authoritative somewhere else.
- A convention enforced only by asking.
- A captured real payload as a fixture.
- An interface, base class or helper with one caller and no substitution.
- A generalization made before the third case appeared.
- A configuration point nothing configures.
- A paragraph restating the sentence above it.
- A replacement for a library or approach already in place, made without asking.
