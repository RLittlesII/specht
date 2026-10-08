---
title: "Specification: Consuming the package"
description: "What a repository needs from this one to install specht.tool through its local tool manifest - the GitHub Packages feed, the package source entry, the token scope and the commands - written down once, and used by this repository itself after the first publish"
type: feature
id: "F7"
epic: "0055"
spec_status: draft
status: needs-decomposition
priority: high
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Build and release"
author: "spec-author"
milestone: null
children: []
depends_on: ["F6"]
blocks: []
spikes: []
created: "2026-10-08"
updated: "2026-10-08"
github_issue: null
synced_at: null
---

# Specification: Consuming the package

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-08 -->

A consumer cannot install a package from a feed it does not know, with a credential nobody told it to create, and README § 6 only says that consumers "carry a `nuget.config` source and a token with package read". So `hooked`, Transporter and this repository's own self-check (`0001-F2` B-014) would each work out the feed, the source entry and the token on their own, differently. This Feature removes that failure state: one document says what a repository commits and what it supplies at run time to install `specht.tool` through its local tool manifest, and this repository installs its own published package the same way.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Persona                                 | Need                                                                 | Pain Point Today               |
| --- | --------------------------------------- | -------------------------------------------------------------------- | ------------------------------ |
| 1   | The maintainer of a consumer repository | The feed, the source entry, the token and the commands, in one place | Only README § 6's one sentence |
| 2   | A consumer's CI                         | To restore the tool with a token it is given, not one committed      | Nothing says how               |
| 3   | `0001-F2`                               | This repository's manifest naming the published package, for B-014   | N/A (internal dependency)      |

### Assumptions

| ID  | Assumption                                                                                                                                                                                   |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | Reading a NuGet package from GitHub Packages requires an authenticated token even when the package is public, so every consumer, this repository included, supplies one (README § 6).        |
| A-2 | A consumer's adoption work - `hooked` (README § 8 step 4) and Transporter (step 7) - is tracked in that repository; this Feature supplies only what they need from here (owner, 2026-10-08). |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                                                      | Source                        | Status  |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------- | ------- |
| B-001 | Given an empty repository that adds the documented package source and supplies a token with package read, the documented install command adds `specht.tool` to its local tool manifest.    | README § 6; owner, 2026-10-08 | Active  |
| B-002 | Given a clone whose committed local tool manifest names `specht.tool` and whose committed package source is the documented one, `dotnet tool restore` with a read token restores the tool. | README § 6; `0001-F2` C-6     | Active  |
| B-003 | Given the first package has been published, this repository's committed local tool manifest names `specht.tool` at a published version.                                                    | `0001-F2` B-014; README § 7   | Active  |
| B-004 | Given the install document, it names the feed's address and the package source entry a consumer commits.                                                                                   | owner, 2026-10-08             | Active  |
| B-005 | Given the install document, it names the token scope a consumer needs and how a CI run supplies the token.                                                                                 | owner, 2026-10-08; README § 6 | Active  |
| B-006 | Given the install document, it names the commands that install the tool locally and restore it in a clone.                                                                                 | owner, 2026-10-08             | Active  |
| B-007 | Given a clone and no GitHub token, the solution restores and builds.                                                                                                                       | OQ-2 (owner, 2026-10-08)      | Amended |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-08 -->

| ID  | Constraint                                                                                                                      | Rules Out                                                                                                         |
| --- | ------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| C-1 | A committed package source takes its credential from the environment.                                                           | A token, password or username-and-token pair in any committed `nuget.config`, here or in the document's examples. |
| C-2 | The documented install is local, through a committed tool manifest (`0001-F2` C-6).                                             | A global install as the documented path.                                                                          |
| C-3 | The document describes this repository's feed and nothing a consumer's own build does with the tool.                            | Consumer build wiring, such as a NUKE target, written here as if it were this repository's to maintain.           |
| C-4 | The install document is a section of this repository's `README.md` (owner, 2026-10-08).                                         | A second install document elsewhere in this repository while that section exists.                                 |
| C-5 | This repository's own runs read the feed with the workflow's own token granted package read (`0055-F2` B-011, `0055-F6` B-010). | A stored read token as a repository secret for this repository's own runs.                                        |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Item                                                           | Exclusion Reason                                                                |
| --- | -------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| 1   | `hooked` adopting the tool                                     | Tracked in `hooked` (owner, 2026-10-08).                                        |
| 2   | Transporter installing the tool                                | Tracked in Transporter (owner, 2026-10-08).                                     |
| 3   | Publishing the package                                         | `0055-F6`.                                                                      |
| 4   | Writing the schema and templates into a consumer after install | `specht init`, `0001-F4`.                                                       |
| 5   | What `SpecCheck` does once the manifest names the tool         | `0001-F2` B-014.                                                                |
| 6   | Installing from NuGet.org                                      | Not published there (`0055-F6` C-3).                                            |
| 7   | Moving the install document into `docs/`                       | Later, under epic `0002`, which owns usage documentation; C-4 holds until then. |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-08 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-08 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-08 -->

Pending: owned by `test-writer`.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-08 -->

| Claim ID | Scenario                                            | Test    | Status  |
| -------- | --------------------------------------------------- | ------- | ------- |
| B-001    | A new repository installs the tool from the feed    | Missing | Missing |
| B-002    | A clone restores the pinned tool                    | Missing | Missing |
| B-003    | This repository installs its own published tool     | Missing | Missing |
| B-004    | The document names the feed and the source entry    | Missing | Missing |
| B-005    | The document names the token and how CI supplies it | Missing | Missing |
| B-006    | The document names the install and restore commands | Missing | Missing |
| B-007    | A contributor without a token can build             | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-08 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                                            | Blocks              | Resolution                                                                                                                                     |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Where does the install document live: this repository's `README.md`, or a page under `docs/`, where `0002-F1` places usage documentation?                                                                                           | B-004, B-005, B-006 | Resolved 2026-10-08 by the repository owner: a section of `README.md`, moving into epic `0002`'s documentation later. C-4 and § 5 row 7 added. |
| OQ-2 | Once this repository's own sources include the authenticated feed (B-003), can a contributor without a GitHub token still restore and build the solution, with only the tool restore needing it? Proposed default: yes, as a claim. | B-003               | Resolved 2026-10-08: proposed default accepted by the repository owner. B-007 added.                                                           |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-08 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| ------- | ------ | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Draft; not reviewed                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| 1-5     | 🟡     | spec-reviewer | Round 1 (2026-10-08) - reviewed in draft, reviewing commit `04fdc28`; no blocking findings. OQ-1 and OQ-2 are claimed, C-1 keeps every credential out of tracked files, and B-003 agrees with `0001-F2` B-014 and A-2. Non-blocking (spec-author): the Business Goal has this repository install its own package the same way, but no claim here or in `0055-F2` or `0055-F6` says how this repository's own runs get the read token A-1 requires; B-007's second clause restates A-1 rather than asserting anything new; `0055-F1` B-010 is contradicted after B-003 (recorded there). Left `draft`: the spec-author has not submitted it, and epic `0055` says its eight-Feature split awaits the owner's confirmation. |

## Tasks

None cut. Items are cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
