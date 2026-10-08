---
title: "Specification: Documentation site"
description: "A static documentation site that publishes 0002-F1's generated API reference and usage docs to a stable public URL; deferred by the owner until specht itself is built, and with its framework still undecided"
type: feature
id: "F2"
epic: "0002"
spec_status: draft
status: blocked
priority: med
value: 3
risk: 3
rank: 51
scored_by: "vane"
scored_on: "2026-09-29"
domain: "Documentation"
author: "spec-author"
milestone: null
children: []
depends_on: ["F1"]
blocks: []
spikes: []
created: "2026-10-07"
updated: "2026-10-07"
github_issue: null
synced_at: null
---

# Specification: Documentation site

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-07 -->

Once `0002-F1` generates an API reference and the commands carry usage docs, a consumer adopting `specht` should be able to read them on a published site, not by cloning the repository and opening build artifacts. This Feature owns that site: it consumes `0002-F1`'s output as its only content source and publishes it to a stable URL. **It is deferred by the owner**: no work starts until `specht` itself is built (epic `0001`). The specification exists now so the intent and the dependency shape are recorded, and so `0002-F1`'s output is structured to be consumed by a site, not so this Feature is worked.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Persona                                      | Need                                                       | Pain Point Today                                                  |
| --- | -------------------------------------------- | ---------------------------------------------------------- | ----------------------------------------------------------------- |
| 1   | A maintainer evaluating or adopting `specht` | A browsable site covering the commands and the API surface | No site exists; `0002-F1`'s output is a local build artifact only |
| 2   | A contributor or a consumer's agent          | A canonical, linkable URL for `specht`'s documentation     | Nothing to link to until this Feature ships                       |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                                                                       |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| A-1 | astro.build was named as the site framework in an offhand grooming remark in `hooked` (PR #212), not at a decision gate: no alternatives were weighed and no decision was recorded. `hooked` captured that as its lesson L-0001. It is carried here as a candidate only; OQ-2 routes the choice. |
| A-2 | This Feature consumes `0002-F1`'s generated reference and the commands' usage docs; it generates no documentation content itself.                                                                                                                                                                |
| A-3 | The hosting target (for example GitHub Pages) is not decided; it is OQ-1, answered when the deferral is lifted.                                                                                                                                                                                  |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-07 -->

| ID    | Claim                                                                                                                                                                         | Source         | Status |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------- | ------ |
| B-001 | Given `0002-F1`'s generated API reference and the commands' usage docs, this Feature builds a static site whose content comes from them alone, in the framework OQ-2 decides. | hooked PR #212 | Active |
| B-002 | Given the site builds in CI, this Feature publishes it through a CI pipeline to a public URL that stays the same across releases.                                             | hooked PR #212 | Active |
| B-003 | Given `0002-F1` has not shipped, or the owner has not lifted the deferral, no work on this Feature starts - no scaffold, no pipeline, no dependency.                          | hooked PR #212 | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-07 -->

| ID  | Constraint                                                                                                             | Rules Out                                                                                                       |
| --- | ---------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| C-1 | This Feature depends on `0002-F1` shipping first; it has no content source otherwise.                                  | Starting while `0002-F1` is not done; a site with hand-written content standing in for the generated reference. |
| C-2 | The site framework is not decided. Before design starts, an ADR records the alternatives considered and the trade-off. | Adopting astro.build, or any framework, by default.                                                             |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Item                                                             | Exclusion Reason                                                          |
| --- | ---------------------------------------------------------------- | ------------------------------------------------------------------------- |
| 1   | Generating the API reference or writing usage docs               | `0002-F1`; this Feature only publishes what `0002-F1` produces.           |
| 2   | Any work before `0002-F1` ships and the owner lifts the deferral | The owner's decision, carried from `hooked`'s 2026-09-29 grooming; B-003. |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `implementer`; not pursued while this Feature is blocked.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `implementer`; not pursued while this Feature is blocked.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `test-writer`; not pursued while this Feature is blocked.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-07 -->

| Claim ID | Scenario                                           | Test    | Status  |
| -------- | -------------------------------------------------- | ------- | ------- |
| B-001    | The site is built from the generated documentation | Missing | Missing |
| B-002    | The site is published to a stable public URL       | Missing | Missing |
| B-003    | Nothing starts before the deferral is lifted       | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-07 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-07 -->

| ID   | Question                                                                                                                         | Blocks | Resolution |
| ---- | -------------------------------------------------------------------------------------------------------------------------------- | ------ | ---------- |
| OQ-1 | Where is the site hosted and published - GitHub Pages or another target? Deferred until this Feature is unblocked.               | B-002  | Open       |
| OQ-2 | Which static-site framework? astro.build is a candidate only (A-1); the answer is an ADR with the alternatives considered (C-2). | B-001  | Open       |

## 12. Sign-off

<!-- last written by: spec-author, 2026-10-07 -->

| Section | Status | Reviewer      | Note                                                 |
| ------- | ------ | ------------- | ---------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Awaiting first review; blocked and deferred by owner |

## Tasks

None yet. Cut from § 3 after agreement and after the deferral is lifted; the candidate cut is in the epic.

## Scoring

| Field | Value | Basis                                                                                                                           |
| ----- | ----- | ------------------------------------------------------------------------------------------------------------------------------- |
| value | 3     | Carried from hooked 0008-F2's intake scoring, 2026-09-29: a real but deferred need - a published site, not the tool's function. |
| risk  | 3     | Carried from the same scoring: undecided framework (C-2) and hosting target (OQ-1, OQ-2). rank = 3×20 − 3×3 + 0×5 = 51.         |
