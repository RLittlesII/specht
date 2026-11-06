---
title: "Specification: Documentation site"
description: "A static documentation site, built with DocFX from 0002-F1's generated API reference and the docs/*.md pages, and published from CI to GitHub Pages at a stable public URL"
type: feature
id: "F2"
epic: "0002"
spec_status: draft
status: ready-for-architecture
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
updated: "2026-11-06"
github_issue: null
synced_at: null
---

# Specification: Documentation site

## 1. Business Goal

<!-- last written by: spec-author, 2026-11-06 (the deferral lifted, decision 0001) -->

A consumer adopting `specht` reads its API reference and its usage pages only by cloning the repository and opening `docs/` or a build artifact; there is nothing to link to. This Feature removes that: it builds a site from `0002-F1`'s generated API reference and the `docs/*.md` pages, and publishes it from CI to a URL that stays the same across releases.

## 2. User Needs

<!-- last written by: spec-author, 2026-11-06 (A-1 to A-3 amended, decision 0001) -->

| #   | Persona                                      | Need                                                       | Pain Point Today                                                  |
| --- | -------------------------------------------- | ---------------------------------------------------------- | ----------------------------------------------------------------- |
| 1   | A maintainer evaluating or adopting `specht` | A browsable site covering the commands and the API surface | No site exists; `0002-F1`'s output is a local build artifact only |
| 2   | A contributor or a consumer's agent          | A canonical, linkable URL for `specht`'s documentation     | Nothing to link to until this Feature ships                       |

### Assumptions

| ID  | Assumption                                                                                                                     |
| --- | ------------------------------------------------------------------------------------------------------------------------------ |
| A-1 | DocFX is the site framework (OQ-2, `0002-F1` decision 0002). astro.build, named in an offhand grooming remark, was not chosen. |
| A-2 | This Feature consumes `0002-F1`'s generated reference and the `docs/*.md` pages; it generates no documentation content itself. |
| A-3 | GitHub Pages hosts the site, published from CI (OQ-1, decision 0001).                                                          |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-11-06 (B-001 amended, B-003 withdrawn, decision 0001) -->

| ID    | Claim                                                                                                                                                  | Source                                        | Status    |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------- | --------- |
| B-001 | Given `0002-F1`'s generated API reference and the `docs/*.md` pages, this Feature builds a static site with DocFX whose content comes from them alone. | owner requirements, 2026-09-29; decision 0001 | Active    |
| B-002 | Given the site builds in CI, this Feature publishes it through a CI pipeline to a public URL that stays the same across releases.                      | owner requirements, 2026-09-29                | Active    |
| B-003 | Given `0002-F1` has not shipped, or the owner has not lifted the deferral, no work on this Feature starts - no scaffold, no pipeline, no dependency.   | owner requirements, 2026-09-29; decision 0001 | Withdrawn |

## 4. Constraints

<!-- last written by: spec-author, 2026-11-06 (C-1 and C-2 amended, decision 0001) -->

| ID  | Constraint                                                                                                       | Rules Out                                                                                             |
| --- | ---------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| C-1 | The site's API reference is `0002-F1`'s generated output; building the site waits on the work that generates it. | A site with hand-written content standing in for the generated reference.                             |
| C-2 | The site framework is DocFX. Before design starts, an ADR records the alternatives considered and the trade-off. | A second site framework or a Node toolchain for the site; design starting before that ADR is written. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-11-06 (row 2 replaced, decision 0001) -->

| #   | Item                                               | Exclusion Reason                                                  |
| --- | -------------------------------------------------- | ----------------------------------------------------------------- |
| 1   | Generating the API reference or writing usage docs | `0002-F1`; this Feature only publishes what `0002-F1` produces.   |
| 2   | Any specification under `docs/` as site content    | A specification is the agreement, not a usage page (epic `0002`). |

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

| Claim ID | Scenario                                           | Test    | Status    |
| -------- | -------------------------------------------------- | ------- | --------- |
| B-001    | The site is built from the generated documentation | Missing | Missing   |
| B-002    | The site is published to a stable public URL       | Missing | Missing   |
| B-003    | Nothing starts before the deferral is lifted       | None    | Withdrawn |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-11-06 (decision 0001) -->

- 2026-11-06, the repository owner lifted the deferral and resolved OQ-1 and OQ-2 ([decision 0001](decisions/0001-the-deferral-is-lifted-and-the-site-is-published-to-github-pages.md); `0002-F1` decision 0002 for DocFX). B-003 withdrawn, and its `@B-003` scenario removed; its § 9 row reads `Withdrawn`. B-001 amended: it read "the commands' usage docs" and "in the framework OQ-2 decides", and now names the `docs/*.md` pages and DocFX; no item cited it, so its Status stays `Active`. C-1 amended: it ruled out starting while `0002-F1` is not done, and now holds only the content dependency. C-2 amended: it ruled out any framework by default, and now names DocFX and keeps the ADR. A-1 to A-3 amended. § 1 no longer calls the Feature deferred. § 5 row 2, any work before the deferral was lifted, is replaced by the exclusion of specifications as site content. OQ-3 and OQ-4 opened.

## 11. Open Questions

<!-- last written by: spec-author, 2026-11-06 (OQ-1 and OQ-2 resolved, OQ-3 and OQ-4 opened, decision 0001) -->

| ID   | Question                                                                                                                                                                            | Blocks | Resolution                                                                                                                                                                                |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Where is the site hosted and published - GitHub Pages or another target? Deferred until this Feature is unblocked.                                                                  | B-002  | Resolved 2026-11-06 by the repository owner (decision 0001): GitHub Pages, published from CI. The repository is private today, so a public URL depends on the owner's GitHub plan (OQ-3). |
| OQ-2 | Which static-site framework? astro.build is a candidate only (A-1); the answer is an ADR with the alternatives considered (C-2).                                                    | B-001  | Resolved 2026-11-06 by the repository owner (`0002-F1` decision 0002): DocFX. Considered and turned down: Astro Starlight. The ADR C-2 asks for is still written before design starts.    |
| OQ-3 | The repository is private. Does the owner's GitHub plan let a GitHub Pages site from it be read at a public URL, or does the repository become public before the site is published? | B-002  | Open. Raised 2026-11-06 with the owner's ruling on hosting, which noted it. Until the owner answers, B-002's public URL cannot be shown.                                                  |
| OQ-4 | Does CI publish the site on each push to `main`, or from each release? The first shows what `main` builds; the second shows what the shipped tool accepts.                          | B-002  | Open. Raised 2026-11-06 by `spec-author`; the owner's ruling says "from CI" and names no trigger. The repository owner has not been asked.                                                |

## 12. Sign-off

<!-- last written by: spec-author, 2026-10-07 -->

| Section | Status | Reviewer      | Note                                                 |
| ------- | ------ | ------------- | ---------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Awaiting first review; blocked and deferred by owner |

## Tasks

Cut 2026-11-06 into [`../.issue/`](../.issue/), on the repository owner's ruling of that date: `0407` carries B-001 and `0408` carries B-002.

## Scoring

| Field | Value | Basis                                                                                                                      |
| ----- | ----- | -------------------------------------------------------------------------------------------------------------------------- |
| value | 3     | Carried from the owner's intake scoring, 2026-09-29: a real but deferred need - a published site, not the tool's function. |
| risk  | 3     | Carried from the same scoring: undecided framework (C-2) and hosting target (OQ-1, OQ-2). rank = 3×20 − 3×3 + 0×5 = 51.    |
