---
title: Questions in specht
description: Where answers already exist in this repository, what this project has already settled, and where a new answer is written back
type: reference
---

# Questions

Extends [`clarify-requirements`](../../clarify-requirements/SKILL.md).

## Look here first, in this order

1. **`.spec/brief.md` § 6** — the open questions the owner has already given a
   default for. Proceed on the default and record it as a decision the owner
   can reverse; do not re-ask it, and do not pick a second default silently
   (brief § 9).
2. **The Feature's specification** — § 3 claims, § 4 constraints, § 5 out of
   scope, § 11 open questions. A question the project knows it has not decided is
   already an `OQ-n`.
3. **Sibling Features' § 5.** Most apparent gaps are another Feature's stated
   scope.
4. **`.spec/brief.md`** § 2, § 5 and § 9 — the requirements, the drafted design and
   the non-negotiables.
5. **`AGENTS.md`** — the chain, the four rules, the ID schemes.
6. **Decision records** beside the Feature, and ADRs in its `adr/` and in
   `.spec/adr/`. An accepted ADR is a rule.
7. **Lessons** — the Feature's `lessons/` and the repo-wide `.spec/lessons/`.
8. **The code and the tests.** The extracted tests say what the engine
   promises; read them before the engine (brief § 9). Once code exists, the
   compiler is the authority on a signature.

## Already settled — do not re-ask

From brief § 2, § 5 and § 6, as of 2026-10-07:

- **The folder is named `.spec/`.** The one hard constraint on the model.
  Everything else about it — sections, grammars, markers — is reversible under
  schema versioning.
- **One repository, tool and schema together.** No consumer owns the schema;
  `hooked` is consumer one.
- **Delivered as a `dotnet tool`**, package `specht.tool`, command `specht`,
  installed per repository through a local tool manifest and published to
  GitHub Packages on `rlittlesii/specht` first; NuGet.org is the reversal
  (brief § 2, § 6). The consumer's build calls the tool; no consumer carries the
  engine.
- **A schema version is the manifest's `schemaVersion`.** A repository pins by
  editing one number; the tool embeds every version it knows and validates with
  the pinned one; `specht upgrade` rewrites the manifest and schema files to the
  next version and prints what changed.
- **Templates ship in the tool**, written by `specht init` beside the schema,
  never overwritten. They are part of the generation contract.
- **The agent-facing contract is `--json`**: violations (rule, severity, file,
  line, identifier, message), counts, layouts, and the schema version checked
  against, with a published JSON Schema in this repository. `--explain SPEC031`
  prints the rule's full text.
- **Every path in every output is repository-relative.** An absolute path
  anywhere is a defect.
- **The tool never writes into a consumer's tree** except through `init`,
  `upgrade`, the caller-named `--report` file and, from epic `0101`, `format`,
  which only moves whole table rows and frontmatter keys (brief § 9; `0101-F5`
  decision 0001). `init` never overwrites an existing file.
- **No rule plugins.** Per-rule disable and `--strict` instead. No Roslyn: the
  claim bridge and the convention analyzers stay in `hooked`.
- **The engine is copied from `hooked`, never regenerated**, and the rule
  vocabulary is not widened before schema versioning exists (brief § 8 step 6).
- **`generatedAtUtc` is dropped from the report**, so `hooked`'s run is
  byte-identical before and after the extraction.
- **Pipe tables only.** A grid table is "no table" (`SPEC013`).
- **`priority` is authored in the spec**, not mirrored from a label.

## Decide it yourself

Reversible, with no behavioural change: folder names, class structure, the order of
two independent operations, a private helper's shape. Document the assumption
where you made it.

## Ask, with the question shaped

When the answer is not in the list above and the choice changes user-visible
behaviour, the report's shape, the manifest's shape, a public surface, or a
guarantee the design makes — ask **one** question carrying: the evidence you
already have, two concrete readings, what each costs, and the smallest decision
that unblocks you. Ask what rule the answer follows, not only which option wins.

A brief § 6 question that turns out to need the owner after all is the one case
where stopping is the rule: do not pick a second default (brief § 9).

## Write the answer back

In the same change. **The specification is where it lands first**: a claim, a
scenario, a § 4 constraint, a § 5 row, or an `OQ-n` resolved in place with the
date and the decision. A decision record also names **what was rejected and what
rejecting it costs**.

An answer that exists only in conversation was never recorded.

## Never add

- A question the specification or brief § 6 already answers.
- An assumption standing in for a material decision.
- A second default for a brief § 6 question, chosen silently.
- A decision recorded without its rejected alternative.
- An exploratory comment treated as ratified.
- A claim about what is undesigned, written without opening the section.
