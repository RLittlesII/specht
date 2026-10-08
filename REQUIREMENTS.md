# Requirements Gathering Session Summary — extracting `specht` into its own repository

> Output of a requirements-gathering discovery session. The `source` input to
> the `business-requirements` skill. Not architecture or development input
> directly — it goes through `business-requirements` first. The draft
> specification `0008-F3` (`tools/SpecGovernance/.spec/README.md`) predates
> this session and is an input to it, not its conclusion.
>
> Two sessions on the same day. Session 1 produced the first summary; session 2
> re-ran discovery against this file and the seed brief (`README.md`), closed
> every open question, and corrected three contradictions. The tables below are
> the current state; § Revision log records what session 2 changed and why.
> Open questions are resolved in place, never deleted.

---

## Session Metadata

| Field                | Value                                                                                                               |
| -------------------- | ------------------------------------------------------------------------------------------------------------------- |
| **Domain**           | Specification governance; `.spec/` tooling                                                                          |
| **Stakeholder role** | Repository owner — maintainer of `hooked`, Transporter and two more sibling repositories on the same `.spec/` model |
| **Date**             | 2026-10-07 (session 1 and session 2)                                                                                |
| **Session status**   | Complete                                                                                                            |

---

## Problem Statement

> One sentence from the stakeholder's perspective. Confirmed via reflection in
> both sessions.
> Four repositories are being written in parallel on the same `.spec/` approach,
> and the schema, templates and checker reach three of them by hand-copy from
> `hooked`, so the stakeholder is iterating on three implementations in four
> repositories of one thing that can be centralised.

Origin, corrected in session 2: Transporter originated the `.spec/` model;
`hooked` copied it from Transporter, then grew the schema and the checker.
Transporter's tree therefore predates the schema.
**Why now** (urgency or trigger):
The four repositories are being written _at the same time_. Every day of
hand-copying is four forks drifting further apart; every improvement lands once
and must land three more times, or does not. Raised 2026-10-07 while grooming
epic `0008`, when `0008-F3` ("SpecCheck as a dotnet tool") was recognised as a
product of its own rather than a Feature of this repository.
---

## Personas

| Persona   | Role                                                                     | Goal                                                                                 | Pain today                                                                                            |
| --------- | ------------------------------------------------------------------------ | ------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------- |
| Primary   | The maintainer — owns `hooked`, Transporter and two sibling repositories | One schema and one checker, reused everywhere; a repository upgrades when it chooses | Maintains three implementations of one checker; each repository's schema is a fork of a past `hooked` |
| Secondary | The agent authoring a specification in any of the four repositories      | A repeatable generation contract, and a read-only oracle it can run mid-write        | Copies `hooked`'s schema and templates by hand; nothing checks the copy or the document it writes     |

---

## Pain Points

| Pain point                                                                     | Cost of not solving                                                                                               | Existing workaround                                   | Why workaround is insufficient                                                                                  |
| ------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| The schema, templates and checker exist only inside `hooked`                   | Three implementations iterated in four repositories; a fix lands four times or drifts                             | Hand-copy `.spec/schema/` and `.spec/templates/`      | A copy is a fork from one day; nothing versions it, nothing checks it, nothing brings it forward                |
| The checker is a Nuke target over a project reference (`tools/SpecGovernance`) | Every repository that wants the gate carries the engine's code and its build wiring                               | Copy the engine project and the Nuke target           | Engine-per-repository is the maintenance the stakeholder is trying to end; the Nuke target should _call_ a tool |
| An agent mid-write has no oracle                                               | A rogue edit (a grep that went wrong) is found by the reviewer, or by CI, not by the agent                        | Re-read the document; run the repository's Nuke build | Slow, repository-specific, and only exists in `hooked`                                                          |
| A repository that lags cannot catch up deliberately                            | Either every repository moves with `hooked` at once, or forks diverge silently                                    | None                                                  | Without a schema version there is nothing to pin and nothing to upgrade to                                      |
| Transporter's `.spec/` tree was written before any schema existed              | The second install is not a clean start; whatever the tool reports there must be repairable from the report alone | None                                                  | Nothing today says what a pre-schema document is missing, only that it fails                                    |

---

## Success Criteria

> Observable, confirmable conditions. Each must be specific enough to be testable.
>
> | Criterion                                                                                                                                                                                                      | MoSCoW | Notes                                                                                                                                                     |
> | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ | --------------------------------------------------------------------------------------------------------------------------------------------------------- |
> | The tool is installed in `hooked` and Transporter and checks both, with no engine code in either                                                                                                               | Must   | The stakeholder's stated "I'll know it works" event. Transporter's tree predates the schema, so this also proves the tool handles a tree it did not write |
> | One command answers all three call sites with the same verdict: pre-commit hook, CI, and an agent running it mid-write                                                                                         | Must   | `hooked`'s Nuke `SpecCheck` target calls the tool; the engine's project reference goes                                                                    |
> | The schema is versioned; the manifest pins `schemaVersion`; the tool embeds every schema version it has ever shipped and validates with the pinned one                                                         | Must   | "It won't matter that a repo lags behind" — the lag is deliberate, not drift. A schema version is never deleted from the tool                             |
> | A specification is proven correctly written and formatted against the pinned schema version, with a file, a line, a rule and a message for every violation                                                     | Must   | Today's `SPEC###` vocabulary and MSBuild-shaped line is the baseline                                                                                      |
> | Every violation also carries what the rule expected — the headers, the grammar, the section order — so an agent repairs the document from the report alone, without reading the rule catalogue or the template | Must   | Session 2: promoted from "everything the tool knows" (Should). This is also how documents migrate between schema versions                                 |
> | The schema source is configurable: the embedded schema for the pinned version by default; the consumer's on-disk `.spec/schema/` files when the manifest or a CLI argument says so                             | Should | Session 2. The manifest itself is always read from disk; only the frontmatter schemas switch                                                              |
> | The schema is usable as the agent's generation contract, not only as a gate                                                                                                                                    | Should | Readable JSON Schema and manifest, as `.spec/schema/` is today; templates ship with the tool and `init` writes them                                       |
> | `hooked`'s own run is unchanged by the extraction: the same violations, the same report, on the same tree                                                                                                      | Should | Carried from `0008-F3` B-029; the one excluded field is the report timestamp, which is dropped                                                            |

---

## Out of Scope

| Item                                                                                   | Rationale for exclusion                                                                                                                                                                         |
| -------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Anything Roslyn: the claim bridge (`0008-F1`) and the convention analyzers (`0008-F2`) | Stated by the stakeholder; both stay in `hooked`. The tool checks specifications only                                                                                                           |
| Rule plugins (loading a consumer's own `ISpecRule` assembly)                           | Carried from `0008-F3`'s decision 0001; rule disable and `--strict` instead                                                                                                                     |
| Spec _revision_ history                                                                | "Version your spec" was confirmed to mean the schema's version a repository pins, not a document's history                                                                                      |
| Autofix (`--fix`, or `upgrade` rewriting documents)                                    | Session 2. The tool never rewrites a specification. `upgrade` touches only `.spec/schema/` and `.spec/templates/`; documents migrate by the agent reading the violations and their expectations |

---

## Constraints

| Constraint                                                                                                                            | Source    | Hard / Soft | Notes                                                                                                                                                                                                                                                                                                       |
| ------------------------------------------------------------------------------------------------------------------------------------- | --------- | ----------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| The folder is named `.spec/`                                                                                                          | Policy    | Hard        | The one thing the stakeholder named as fixed                                                                                                                                                                                                                                                                |
| Everything else about the model — sections, grammars, markers — is reversible                                                         | Policy    | Soft        | "Everything is reversible"; schema versioning is what makes a reversal safe for a lagging repository                                                                                                                                                                                                        |
| Delivered as a `dotnet tool`                                                                                                          | Technical | Hard        | Session 2: was Soft. Everything downstream — `spectre-cli`, `dotnet-tool`, the local tool manifest, the Nuke call — already assumes it. Consumers need the .NET SDK                                                                                                                                         |
| Lives in its own repository, tool and schema together                                                                                 | Policy    | Hard        | The frontmatter schemas, the manifest and the templates live in `specht` beside the engine as the single source. No consumer owns them; each holds a copy `init` wrote and `upgrade` refreshes. The alternative is the problem statement: whichever consumer owned the schema becomes the fork origin again |
| The Nuke target calls the tool; no repository carries the engine                                                                      | Technical | Hard        | Stakeholder's answer to "why not the Nuke target": it should just call the tool                                                                                                                                                                                                                             |
| The engine is copied from `hooked` branch `refactor/ai-offering` (PR #217, open) at `6afe8ab`, with the SHA cited in the first commit | Technical | Hard        | Session 2: replaces "ships after PR #217 merges". The engine, tests, schemas and templates at `6afe8ab` are byte-identical to the `a6d056f` seed; only the three draft-spec files were removed                                                                                                              |
| `hooked` consumes `specht` _instead of_ merging the engine from PR #217                                                               | Technical | Hard        | Session 2. The engine never lands on `hooked` `main`; PR #217 shrinks to its agent-contracts half or is superseded. `hooked`'s path forward is "replace the engine with the tool"                                                                                                                           |
| A schema version, once shipped in the tool, is never deleted                                                                          | Policy    | Hard        | Session 2. A repository pinned to v1 restores any later tool and still checks against v1. Same rule as IDs: never renumber, never reuse                                                                                                                                                                     |
| Published to GitHub Packages on `rlittlesii/specht` first                                                                             | Technical | Soft        | Session 2. Each consumer carries a `nuget.config` source entry and CI needs a token with package read. Moving to NuGet.org is the reversal                                                                                                                                                                  |
| Deadline                                                                                                                              | Time      | —           | None. "As soon as we can get it done." The order of work (README § 8) is the only sequencing                                                                                                                                                                                                                |

---

## Open Questions

> Resolved in place with the date and the decision; never deleted.
>
> | Question                                                                                                                                              | Resolution (2026-10-07, session 2)                                                                                                                                                |
> | ----------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
> | The agent-facing output contract: what the tool reports beyond file, line and rule, and its machine-readable form (`--json` document schema)          | Each violation carries what the rule expected, as well as file, line, rule and message. The report's JSON Schema is published in this repository. Shape per rule: open, see below |
> | How the four repositories install it: a NuGet feed, a GitHub package, a local tool manifest, or a global install                                      | Local tool manifest per repository; package on GitHub Packages first                                                                                                              |
> | What a "schema version" is in the file: a frontmatter field a specification declares, a manifest field a repository pins, or the tool package version | A manifest field, `schemaVersion`. The tool embeds every version forever; embedded is the default source, on-disk by manifest field or CLI argument                               |
> | Whether templates ship with the tool (`init` writes them) or stay per repository                                                                      | In the tool; `init` writes them; never overwritten                                                                                                                                |
> | Whether `0008-F3` stays in `hooked` as the _extraction_ Feature or becomes the new repository's first epic                                            | This repository's epic `0001` is the tool. `hooked` keeps "consume the tool"                                                                                                      |
> | The tool's name, package id and command (`0008-F3` OQ-1 proposes `SpecGovernance` / `spec-check`)                                                     | Product Specht; command `specht`; package `specht.tool`; namespace `specht`                                                                                                       |
> | The schema files' `$id` URLs, which today point at `hooked`                                                                                           | `https://github.com/rlittlesii/specht/schema/v1/<file>`                                                                                                                           |
> | Which sibling is the second install                                                                                                                   | Transporter                                                                                                                                                                       |
> | Whether the extraction waits for PR #217 to merge                                                                                                     | No. Copy from the open branch now; `hooked` consumes the tool instead of merging the engine                                                                                       |
> | Who migrates documents when a schema version changes what a document must look like                                                                   | The agent, from the violations. The tool never rewrites a specification                                                                                                           |
> | Whether the tool must keep checking a repository pinned to an old schema version, and for how long                                                    | Forever; every version embedded, none deleted                                                                                                                                     |

Still open after session 2:

| Question                                                                                                         | Owner / next step                                     |
| ---------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------- |
| The shape of the expectation each rule reports in `--json`, and the published JSON Schema of the report document | `spec-author`, epic `0001`; one claim per rule family |
| The manifest field name and the CLI argument name that select the schema source                                  | `spec-author`, epic `0001`                            |
| The names of the third and fourth sibling repositories                                                           | Stakeholder; not needed for "done"                    |

---

## Stakeholder Confirmation

- [x] Summary reflected back and confirmed accurate by stakeholder
- [x] All contradictions resolved
- [x] Stakeholder agrees this is a sufficient basis for a requirements document
      **Confirmed by**: repository owner  
      **Confirmed on**: 2026-10-07

---

## Revision log

Session 2, 2026-10-07. Discovery re-run against session 1's summary and the
seed brief (`README.md`) after the owner decided naming and layout and seeded
the repository. Eight gaps were found against the discovery checklist; all
eight closed.

| Changed                                 | From                                             | To                                                                              | Why                                                                            |
| --------------------------------------- | ------------------------------------------------ | ------------------------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| Should-6, agent-facing output           | "everything the tool knows", Should              | Every violation carries what the rule expected, Must                            | Untestable as written; the agent must repair from the report alone             |
| Must-1, "installed in two repositories" | `hooked` and "one sibling"                       | `hooked` and Transporter                                                        | Named the second install; Transporter's tree predates the schema               |
| Problem statement, origin               | three siblings copied from `hooked`              | Transporter originated the model; `hooked` copied from Transporter              | Corrected the direction of the copy                                            |
| Constraint, PR #217                     | "ships after PR #217 merges", Time, Hard         | Copy from the open branch at `6afe8ab`; `hooked` consumes instead of merging    | Verified: #217 is open; the engine is not on `hooked` `main`                   |
| Constraint, `dotnet tool`               | Soft                                             | Hard                                                                            | Everything downstream already assumes it                                       |
| Constraint, schema versions             | — (absent)                                       | Tool embeds every version forever; none deleted; embedded is the default source | Decided; keeps the tool deterministic and offline                              |
| Out of scope, autofix                   | implied by "never writes into a consumer's tree" | Stated: no `--fix`; `upgrade` never rewrites documents                          | Reader of Out of Scope should not have to infer it                             |
| Constraint, publishing                  | NuGet.org (README default)                       | GitHub Packages first, Soft                                                     | Only the owner's four repositories consume it today                            |
| Open questions                          | seven open                                       | all seven resolved in place; three new ones carried forward                     | Naming, `$id`, layout, epic home, templates, timestamp decided since session 1 |
