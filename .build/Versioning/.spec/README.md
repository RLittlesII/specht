---
title: "Specification: Package versioning"
description: "Nerdbank.GitVersioning computes the package version from version.json and the git height, public on main and v* tags and suffixed elsewhere, the release tag is created from it with nbgv tag, and the package version never moves the schema version"
type: feature
id: "F5"
epic: "0055"
spec_status: approved
status: ready-for-architecture
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
depends_on: ["F1"]
blocks: ["F6"]
spikes: []
created: "2026-10-08"
updated: "2026-10-08"
github_issue: null
synced_at: null
---

# Specification: Package versioning

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-08 -->

Nothing sets the package's version, so every `Pack` would produce the same default number, two builds of different commits would be indistinguishable to a consumer's local tool manifest, and the first release would carry whatever someone typed on the day. This Feature removes that failure state: the version is a function of the commit, computed by Nerdbank.GitVersioning from `version.json` and the git height, public on `main` and on `v*` tags and visibly a prerelease everywhere else, and the release tag is made from the computed version rather than typed. The package version moves independently of the schema version a repository pins.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Persona                               | Need                                                   | Pain Point Today                            |
| --- | ------------------------------------- | ------------------------------------------------------ | ------------------------------------------- |
| 1   | A consumer pinning the tool           | One version means one commit                           | Every pack carries the same default version |
| 2   | The maintainer cutting a release      | A tag that cannot disagree with what is built          | A tag would be typed by hand                |
| 3   | The maintainer testing a branch build | A branch package that cannot be mistaken for a release | Nothing marks a build as a prerelease       |
| 4   | `0055-F1`, `0055-F6`                  | The version to pack and to publish                     | N/A (internal dependency)                   |

### Assumptions

| ID  | Assumption                                                                                                                                                                 |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | A release is cut by running `nbgv tag` on a commit of `main`, which creates the `v*` tag from the computed version; nobody types a version into a tag (owner, 2026-10-08). |
| A-2 | `version.json`'s `publicReleaseRefSpec` names `main` and `v*` tags, and only those (owner, 2026-10-08).                                                                    |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                                                | Source                                         | Status  |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------- | ------- |
| B-001 | Given one commit built from the same ref, with its full history, on any two machines, the build computes the same package version on both.                                           | owner, 2026-10-08; decision 0001; B-003; B-004 | Amended |
| B-002 | Given two commits on `main` where the second follows the first and `version.json` is unchanged, the second's computed version is higher than the first's.                            | owner, 2026-10-08; decision 0001               | Active  |
| B-003 | Given a commit built from `main` or from a `v*` tag, the computed version carries no prerelease suffix.                                                                              | owner, 2026-10-08; A-2                         | Active  |
| B-004 | Given a commit built from any other ref, the computed version carries a prerelease suffix naming the commit.                                                                         | owner, 2026-10-08; A-2                         | Active  |
| B-005 | Given the `Pack` target, the version inside the package equals the version computed for the commit.                                                                                  | owner, 2026-10-08; `0055-F1` B-009             | Active  |
| B-006 | Given a commit that changes the version in `version.json` and nothing else, the schema versions the tool embeds and the `schemaVersion` `specht init` writes are unchanged.          | owner, 2026-10-08; `0001-F7` B-012; C-1        | Active  |
| B-007 | Given a fresh clone after the local tools are restored, `dotnet nbgv tag` on a commit of `main` creates the tag `v<version>` at that commit, `<version>` being its computed version. | owner, 2026-10-08; A-1                         | Active  |
| B-008 | Given `version.json` as first committed, the version computed for a commit of `main` begins `0.1.`.                                                                                  | OQ-1 (owner, 2026-10-08)                       | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-08 -->

| ID  | Constraint                                                                                                                               | Rules Out                                                                                                                                   |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | The package version and the schema version are independent numbers; neither is computed from the other (`0001-F7` owns `schemaVersion`). | A package major tied to the schema version; a schema version bumped to mark a tool release; a tool release forced by a schema change alone. |
| C-2 | The version is computed from the commit's full history.                                                                                  | A shallow checkout in any build that packs (`0055-F2` C-2, `0055-F6`).                                                                      |
| C-3 | Nerdbank.GitVersioning is the only source of the version.                                                                                | A `<Version>` in `Directory.Build.props` or a project file; a version passed in by a workflow variable or typed into a tag.                 |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Item                                                  | Exclusion Reason                                                                       |
| --- | ----------------------------------------------------- | -------------------------------------------------------------------------------------- |
| 1   | Schema versions, `schemaVersion` and `specht upgrade` | `0001-F7`.                                                                             |
| 2   | Publishing on a tag                                   | `0055-F6`.                                                                             |
| 3   | A `--version` option on the command                   | The command's surface is epic `0001`'s; not asked for.                                 |
| 4   | When the first tag is cut                             | `0055-F6` C-2.                                                                         |
| 5   | Versioning any assembly other than the packed tool's  | Only `specht.tool` packs (`0001-F2` C-5); the rest takes the same version, unasserted. |

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

| Claim ID | Scenario                                           | Test    | Status  |
| -------- | -------------------------------------------------- | ------- | ------- |
| B-001    | One commit has one version everywhere              | Missing | Missing |
| B-002    | A later commit has a higher version                | Missing | Missing |
| B-003    | A main-branch build is a public version            | Missing | Missing |
| B-004    | A branch build is a prerelease                     | Missing | Missing |
| B-005    | The package carries the computed version           | Missing | Missing |
| B-006    | A package version change leaves the schema version | Missing | Missing |
| B-007    | The release tag is made from the computed version  | Missing | Missing |
| B-008    | The first version line is 0.1                      | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-08 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                           | Blocks       | Resolution                                                       |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------ | ---------------------------------------------------------------- |
| OQ-1 | What version does `version.json` start at? The first publish freezes schema version 1 (`0001-F7` decision 0002) whatever the package version is (C-1); whether that publish is `0.x` or `1.0` is the owner's call. | B-002, B-007 | Resolved 2026-10-08 by the repository owner: `0.1`. B-008 added. |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-08 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| ------- | ------ | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Draft; not reviewed                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| 1-5     | 🔴     | spec-reviewer | Round 1 (2026-10-08) - blocked, reviewing commit `04fdc28`. Decision 0001, OQ-1 and C-1 are stated faithfully, and C-1 agrees with `0001-F7`. Blocking (spec-author): B-001 says one commit computes the same version on any two machines, with no condition on the ref, while B-003 and B-004 give the same commit a public version from `main` and a prerelease from any other ref; state B-001 for the same commit built from the same ref. Non-blocking (spec-author): B-005 tests `0055-F1`'s `Pack` but `depends_on` is empty; decision 0001's Affects stops at B-007. |
| 1-5     | 🟢     | spec-reviewer | Round 2 (2026-10-08) - approved, reviewing `acf3cff`..`f32a31d`. B-001 is now the same commit from the same ref, agreeing with B-003 and B-004; `depends_on` names `0055-F1` (B-005); decision 0001's Affects is complete.                                                                                                                                                                                                                                                                                                                                                   |
| Tasks   | 🟢     | spec-reviewer | Item cut (2026-10-08) - approved, reviewing `6113181`..`0657b68`. 0079-0081 carry B-001-B-008 once. 0081 on 0049 is real: B-006 asserts the `schemaVersion` `specht init` writes, which `0001-F7` B-012 (0049) defines and B-006 cites; holding only the test keeps the release path (0080, 0083) off epic 0001.                                                                                                                                                                                                                                                             |

## Tasks

Cut 2026-10-08 into [`../.issue/`](../.issue/): `0078` (the Feature), with `0079` to `0081`.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
