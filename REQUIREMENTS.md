# Requirements Gathering Session Summary — extracting `spec-check` into its own repository
> Output of a requirements-gathering discovery session. The `source` input to
> the `business-requirements` skill. Not architecture or development input
> directly — it goes through `business-requirements` first. The draft
> specification `0008-F3` (`tools/SpecGovernance/.spec/README.md`) predates
> this session and is an input to it, not its conclusion.
---
## Session Metadata
| Field                | Value                                                              |
| -------------------- | ------------------------------------------------------------------ |
| **Domain**           | Specification governance; `.spec/` tooling                         |
| **Stakeholder role** | Repository owner — maintainer of `hooked` and three sibling repositories on the same `.spec/` model |
| **Date**             | 2026-10-07                                                         |
| **Session status**   | Complete                                                           |
---
## Problem Statement
> One sentence from the stakeholder's perspective. Confirmed via reflection.
Four repositories are being written in parallel on the same `.spec/` approach,
and the schema, templates and checker reach three of them by hand-copy from
`hooked`, so the stakeholder is iterating on three implementations in four
repositories of one thing that can be centralised.
**Why now** (urgency or trigger):
The four repositories are being written *at the same time*. Every day of
hand-copying is four forks drifting further apart; every improvement lands once
and must land three more times, or does not. Raised 2026-10-07 while grooming
epic `0008`, when `0008-F3` ("SpecCheck as a dotnet tool") was recognised as a
product of its own rather than a Feature of this repository.
---
## Personas
| Persona   | Role                                                                    | Goal                                                                                 | Pain today                                                                                           |
| --------- | ----------------------------------------------------------------------- | ------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------- |
| Primary   | The maintainer — owns `hooked` and three sibling repositories           | One schema and one checker, reused everywhere; a repository upgrades when it chooses | Maintains three implementations of one checker; each repository's schema is a fork of a past `hooked` |
| Secondary | The agent authoring a specification in any of the four repositories     | A repeatable generation contract, and a read-only oracle it can run mid-write        | Copies `hooked`'s schema and templates by hand; nothing checks the copy or the document it writes     |
---
## Pain Points
| Pain point                                                                        | Cost of not solving                                                                        | Existing workaround                                   | Why workaround is insufficient                                                                                     |
| --------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ | ----------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| The schema, templates and checker exist only inside `hooked`                      | Three implementations iterated in four repositories; a fix lands four times or drifts      | Hand-copy `.spec/schema/` and `.spec/templates/`      | A copy is a fork from one day; nothing versions it, nothing checks it, nothing brings it forward                   |
| The checker is a Nuke target over a project reference (`tools/SpecGovernance`)    | Every repository that wants the gate carries the engine's code and its build wiring        | Copy the engine project and the Nuke target           | Engine-per-repository is the maintenance the stakeholder is trying to end; the Nuke target should *call* a tool    |
| An agent mid-write has no oracle                                                  | A rogue edit (a grep that went wrong) is found by the reviewer, or by CI, not by the agent | Re-read the document; run the repository's Nuke build | Slow, repository-specific, and only exists in `hooked`                                                             |
| A repository that lags cannot catch up deliberately                               | Either every repository moves with `hooked` at once, or forks diverge silently             | None                                                  | Without a schema version there is nothing to pin and nothing to upgrade to                                         |
---
## Success Criteria
> Observable, confirmable conditions. Each must be specific enough to be testable.
| Criterion                                                                                                                                          | MoSCoW | Notes                                                                                                   |
| -------------------------------------------------------------------------------------------------------------------------------------------------- | ------ | ------------------------------------------------------------------------------------------------------- |
| The tool is installed in two repositories (`hooked` and one sibling) and checks both, with no engine code in either                                | Must   | The stakeholder's stated "I'll know it works" event                                                     |
| One command answers all three call sites with the same verdict: pre-commit hook, CI, and an agent running it mid-write                             | Must   | `hooked`'s Nuke `SpecCheck` target calls the tool; the engine's project reference goes                  |
| The schema is versioned; a repository pins a version and upgrades when it chooses                                                                  | Must   | "It won't matter that a repo lags behind" — the lag is deliberate, not drift                            |
| A specification is proven correctly written and formatted against the pinned schema version, with a file, a line and a rule for every violation   | Must   | Today's `SPEC###` vocabulary and MSBuild-shaped line is the baseline                                     |
| The schema is usable as the agent's generation contract, not only as a gate                                                                        | Should | Readable JSON Schema and manifest, as `.spec/schema/` is today; see open question on templates           |
| An agent running the tool mid-write gets everything the tool knows, in a form it can act on without a human reading it                             | Should | The output contract is an open question; the requirement is "any information it might need"             |
| `hooked`'s own run is unchanged by the extraction: the same violations, the same report, on the same tree                                          | Should | Carried from `0008-F3` B-029; the one excluded field is the report timestamp                            |
---
## Out of Scope
| Item                                                                              | Rationale for exclusion                                                                   |
| --------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| Anything Roslyn: the claim bridge (`0008-F1`) and the convention analyzers (`0008-F2`) | Stated by the stakeholder; both stay in `hooked`. The tool checks specifications only       |
| Rule plugins (loading a consumer's own `ISpecRule` assembly)                       | Carried from `0008-F3`'s decision 0001; rule disable and `--strict` instead                |
| Spec *revision* history                                                            | "Version your spec" was confirmed to mean the schema's version a repository pins, not a document's history |
---
## Constraints
| Constraint                                                                       | Source    | Hard / Soft | Notes                                                                                             |
| -------------------------------------------------------------------------------- | --------- | ----------- | ------------------------------------------------------------------------------------------------- |
| The folder is named `.spec/`                                                     | Policy    | Hard        | The one thing the stakeholder named as fixed                                                      |
| Everything else about the model — sections, grammars, markers — is reversible    | Policy    | Soft        | "Everything is reversible"; schema versioning is what makes a reversal safe for a lagging repository |
| Delivered as a `dotnet tool`                                                     | Technical | Soft        | Chosen as an easy delivery system; the runtime itself is open                                     |
| Ships after PR #217 merges                                                       | Time      | Hard        | `hooked`'s engine lands first, then is extracted; no parallel fork of it                          |
| Lives in its own repository, tool and schema together                            | Policy    | Hard        | None of the four repositories owns the global schema; `hooked` becomes consumer one               |
| The Nuke target calls the tool; no repository carries the engine                 | Technical | Hard        | Stakeholder's answer to "why not the Nuke target": it should just call the tool                   |
---
## Open Questions
| Question                                                                                                                                   | Owner / next step                                                                      |
| ------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------- |
| The agent-facing output contract: what the tool reports beyond file, line and rule, and its machine-readable form (`--json` document schema) | `business-requirements` to turn "any information it might need" into needs; `0008-F3` OQ-9 |
| How the four repositories install it: a NuGet feed, a GitHub package, a local tool manifest, or a global install                           | Stakeholder; `0008-F3` OQ-4                                                            |
| What a "schema version" is in the file: a frontmatter field a specification declares, a manifest field a repository pins, or the tool package version | Stakeholder with `business-requirements`; decides the upgrade story                    |
| Whether templates ship with the tool (`init` writes them) or stay per repository                                                           | Stakeholder; affects the "generation contract" criterion                                |
| Whether `0008-F3` stays in `hooked` as the *extraction* Feature or becomes the new repository's first epic                                  | Stakeholder; `hooked` keeps at minimum "replace the project reference with the tool"   |
| The tool's name, package id and command (`0008-F3` OQ-1 proposes `SpecGovernance` / `spec-check`)                                           | Stakeholder                                                                            |
| The schema files' `$id` URLs, which today point at `hooked`                                                                                | Moves with the repository; `0008-F3` OQ-6                                              |
---
## Stakeholder Confirmation
- [x] Summary reflected back and confirmed accurate by stakeholder
- [x] All contradictions resolved
- [x] Stakeholder agrees this is a sufficient basis for a requirements document
**Confirmed by**: repository owner  
**Confirmed on**: 2026-10-07
