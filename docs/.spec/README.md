---
title: "Specification: Docs as first-class citizen"
description: "Documentation generated inside the build: an XML doc comment on every public type and member, an API reference generated from them by a NUKE target, and each command's usage doc beside its code, changed in the same pull request as the behaviour"
type: feature
id: "F1"
epic: "0002"
spec_status: draft
status: needs-decomposition
priority: high
value: 4
risk: 2
rank: 79
scored_by: "vane"
scored_on: "2026-09-29"
domain: "Documentation"
author: "spec-author"
milestone: null
children: []
depends_on: []
blocks: ["F2"]
spikes: []
created: "2026-10-07"
updated: "2026-10-09"
github_issue: null
synced_at: null
---

# Specification: Docs as first-class citizen

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-07 -->

No Feature of epic `0001` names documentation as something it owns. Nothing requires a public type or member to carry a doc comment, nothing generates a reference from them, and nothing says where a command's usage documentation lives or that it changes when the command does. So `specht`'s API surface and its usage docs will either not exist, or exist as hand-written prose with no structural guarantee that they still describe the code. This Feature removes that failure state at the mechanism level: documentation is generated inside the build, from the code that defines it, and checked by the same build that compiles and tests it. It stands up no hosted site; that is `0002-F2`, deferred.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Persona                                                           | Need                                                                                                                  | Pain Point Today                                                                                           |
| --- | ----------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| 1   | A contributor reading or extending `specht`'s public surface      | Every public type and member carries a doc comment saying what it does, so intent is discoverable without the body    | No convention exists; a public type can ship undocumented and nothing flags it                             |
| 2   | A contributor wanting a browsable reference of the public surface | A reference generated from the doc comments themselves, never a hand-maintained copy                                  | Nothing generates anything; there is no mechanism queued up to document the surface once it exists         |
| 3   | A contributor writing or reading a command's usage documentation  | One known home for a command's usage doc, and the expectation that it changes in the same pull request as the command | No co-location convention exists; a usage doc, if written, has no defined home and drifts from the command |
| 4   | The owner                                                         | Documentation wired into the build now, before a public surface accumulates with no convention behind it              | Every concern deferred until later has cost more to retrofit than to start with                            |
| 5   | `0002-F2`                                                         | A generated reference and a set of usage docs to publish                                                              | N/A (internal dependency)                                                                                  |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                     |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| A-1 | NUKE drives the build (`.build/Build.cs`); documentation generation is a NUKE target, not a script outside it.                                                                                                                 |
| A-2 | .NET's XML documentation comment (`///`) is the doc-comment convention; no third-party syntax is introduced.                                                                                                                   |
| A-3 | The generated reference is a build artifact under `.artifacts/`, consumed later by `0002-F2`; this Feature publishes it nowhere.                                                                                               |
| A-4 | A "usage doc" is narrative documentation of a command - what it does and how a consumer runs it - distinct from the command's `.spec/README.md`, which is the agreement and stays the agreement.                               |
| A-5 | The public surface is the public types and members of `src/specht` and `src/specht.tool`. `src/specht` is not packable, but its public surface is the engine's contract with the tool and with every contributor who reads it. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-07 -->

| ID    | Claim                                                                                                                                                                                                                                | Source         | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------- | ------ |
| B-001 | Given a public type or public member in `src/specht` or `src/specht.tool` with no XML doc comment, this Feature fails the build with `CS1591` reported as an error.                                                                  | hooked PR #212 | Active |
| B-002 | Given the build's documentation target runs, this Feature writes an API reference generated from the XML doc comments of the current public surface under `.artifacts/`, and no hand-maintained copy of it exists in the repository. | hooked PR #212 | Active |
| B-003 | Given a command under `src/specht.tool/Features/<Command>/`, this Feature places that command's usage doc in that folder, at the location the convention names (OQ-2), distinct from the command's `.spec/README.md`.                | hooked PR #212 | Active |
| B-004 | Given this Feature is delivered, no hosted site, publish step or site framework exists in the repository or its CI.                                                                                                                  | hooked PR #212 | Active |
| B-005 | Given `./build.sh` runs its default target, the documentation enforcement (B-001) and generation (B-002) run as part of it, with no separate manual step.                                                                            | hooked PR #212 | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-07 -->

| ID  | Constraint                                                                                                                                          | Rules Out                                                                                                               |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| C-1 | NUKE owns documentation generation; it is a target in `.build/Build.cs`'s graph.                                                                    | A shell script, a `dotnet` command run by hand, or a CI step that is not a NUKE target.                                 |
| C-2 | The output is a local or CI build artifact only.                                                                                                    | A hosted site, a publish step, a site-framework dependency - all `0002-F2`'s.                                           |
| C-3 | Doc-comment enforcement is scoped to the public surface.                                                                                            | `CS1591`-style enforcement on internal or private members; a blanket `TreatWarningsAsErrors` change made only for this. |
| C-4 | The generated reference is deterministic and carries no absolute path (AGENTS.md § Invariants), and it is never committed.                          | A timestamp or a machine path in the artifact; a generated reference checked into git.                                  |
| C-5 | A pull request that changes a command's behaviour changes that command's usage doc in the same pull request; `spec-reviewer` enforces it at review. | A documentation-only follow-up pull request; a usage doc left describing the old behaviour.                             |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-09 (row 4 names the `docs/` folders `0001-F3` owns) -->

| #   | Item                                               | Exclusion Reason                                                                                                                          |
| --- | -------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Standing up a hosted documentation site            | `0002-F2`, deferred until the tool is built.                                                                                              |
| 2   | Publishing the generated reference anywhere        | `0002-F2`, once the deferral is lifted.                                                                                                   |
| 3   | `--help` text                                      | Generated by Spectre.Console.Cli from the command declarations (`0001-F2` B-008, and each command's own Feature) - a different mechanism. |
| 4   | `--explain` rule text and the report's JSON Schema | `0001-F3`, which owns `docs/rules/` and `docs/schema/`. A usage doc links to them and never copies them.                                  |
| 5   | Rewriting any `.spec/**` document as a usage doc   | A specification is the agreement, owned by its roles; a usage doc is a separate, code-adjacent artifact (A-4).                            |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `test-writer`.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-07 -->

| Claim ID | Scenario                                                        | Test    | Status  |
| -------- | --------------------------------------------------------------- | ------- | ------- |
| B-001    | An undocumented public member fails the build                   | Missing | Missing |
| B-002    | The API reference is generated from the doc comments            | Missing | Missing |
| B-003    | A command's usage doc sits beside the command                   | Missing | Missing |
| B-004    | Documentation generation publishes nothing                      | Missing | Missing |
| B-005    | The default build runs documentation enforcement and generation | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-07 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-07 -->

| ID   | Question                                                                                                                                                                        | Blocks | Resolution |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ | ---------- |
| OQ-1 | Which generator produces the API reference - DocFX, a Roslyn-based generator, or another - and in what format (Markdown, JSON, HTML)? `implementer`'s call, recorded as an ADR. | B-002  | Open       |
| OQ-2 | What is a usage doc's file name and location inside a command folder (for example `Features/<Command>/USAGE.md`), given the folder already holds `.spec/`?                      | B-003  | Open       |

## 12. Sign-off

<!-- last written by: spec-author, 2026-10-07 -->

| Section | Status | Reviewer      | Note                  |
| ------- | ------ | ------------- | --------------------- |
| 1-5     | 🟡     | spec-reviewer | Awaiting first review |

## Tasks

None yet. Cut from § 3 after agreement; the candidate cut is in the epic.

## Scoring

| Field | Value | Basis                                                                                                                                             |
| ----- | ----- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| value | 4     | Carried from hooked 0008-F1's intake scoring, 2026-09-29: closes a cross-cutting gap no prior epic owns, raised by the owner directly.            |
| risk  | 2     | Carried from the same scoring: build-time tooling inside an existing NUKE pipeline, no runtime or external surface. rank = 4×20 − 2×3 + 1×5 = 79. |
