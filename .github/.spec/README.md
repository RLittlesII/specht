---
title: "Specification: Dependency updates"
description: "Renovate proposes updates as pull requests grouped by ecosystem - NuGet packages, GitHub Actions, the .NET SDK, dotnet local tools - merges a minor or patch update itself once every required check passes, leaves a major for review, and leaves the engine's three pinned dependencies alone until the baseline tag exists"
type: feature
id: "F4"
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
depends_on: ["F2"]
blocks: []
spikes: []
created: "2026-10-08"
updated: "2026-10-08"
github_issue: null
synced_at: null
---

# Specification: Dependency updates

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-08 -->

Every version this repository depends on - packages, the SDK, local tools, workflow actions - is pinned, so without a mechanism they age silently until an update is large and urgent at once; and the one mechanism that would update them indiscriminately would also move the engine's three dependencies off `hooked`'s versions before the baseline exists, breaking the extraction the whole of epic `0001` is measured against. This Feature removes that failure state: Renovate proposes updates as pull requests grouped by ecosystem, a minor or patch update merges itself once every required check passes, a major waits for a person, and the engine's dependencies are left alone until README § 8 step 2's baseline tag exists.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Persona        | Need                                                                           | Pain Point Today                            |
| --- | -------------- | ------------------------------------------------------------------------------ | ------------------------------------------- |
| 1   | The maintainer | Routine updates applied without attention, and only when the build agrees      | Every update is a manual edit               |
| 2   | The maintainer | A breaking update shown to a person before it lands                            | Nothing distinguishes a major from a patch  |
| 3   | The reviewer   | One pull request per ecosystem, not one per package                            | The configuration present groups NuGet only |
| 4   | `0001-F1`      | The engine's three dependencies held at `hooked`'s versions until the baseline | Nothing excludes them                       |

### Assumptions

| ID  | Assumption                                                                                                                                      |
| --- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The schedule and timezone in `.github/renovate.json` today (before 6am on Monday, America/Chicago) are kept; the owner did not change them.     |
| A-2 | Renovate runs as the GitHub app installed on the repository, and a merge without a person relies on the repository allowing it (`0055-F8`).     |
| A-3 | "The engine's three dependencies" are Markdig, YamlDotNet and JsonSchema.Net, the `Engine` group of `Directory.Packages.props` (`0001-F1` C-7). |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                                                                   | Source                                | Status  |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------- | ------- |
| B-001 | Given minor or patch updates available for two or more NuGet packages, this Feature proposes all of them in one pull request.                                                                           | owner, 2026-10-08                     | Active  |
| B-002 | Given minor or patch updates available for two or more GitHub Actions whose versions the build project declares, this Feature proposes all of them in one pull request that changes those declarations. | owner, 2026-10-08; OQ-1               | Amended |
| B-003 | Given a newer minor or patch release of the .NET SDK than `global.json` pins, this Feature proposes it in one pull request.                                                                             | owner, 2026-10-08                     | Active  |
| B-004 | Given minor or patch updates available for two or more tools in the local tool manifest, this Feature proposes all of them in one pull request.                                                         | owner, 2026-10-08                     | Active  |
| B-005 | Given a minor or patch update pull request whose required checks all pass, this Feature merges it without a person.                                                                                     | owner, 2026-10-08                     | Active  |
| B-006 | Given a minor or patch update pull request with a failing required check, the pull request stays open and unmerged.                                                                                     | owner, 2026-10-08                     | Active  |
| B-007 | Given a major update, this Feature proposes it in a pull request that it never merges itself.                                                                                                           | owner, 2026-10-08; OQ-2               | Active  |
| B-008 | Given a newer version of Markdig, YamlDotNet or JsonSchema.Net, while C-1's exclusion holds, this Feature proposes no update to it.                                                                     | owner, 2026-10-08; `0001-F1` C-7, C-9 | Active  |
| B-009 | Given any GitHub Actions update, the pull request changes no generated workflow file.                                                                                                                   | OQ-1 (owner, 2026-10-08); C-3         | Active  |
| B-010 | Given any update, this Feature opens no GitHub issue.                                                                                                                                                   | OQ-2 (owner, 2026-10-08); C-5         | Active  |
| B-011 | Given any update pull request this Feature opens, it carries the label `dependencies`.                                                                                                                  | OQ-3 (owner, 2026-10-08)              | Active  |
| B-012 | Given major updates available for two packages, this Feature proposes each in a pull request of its own.                                                                                                | OQ-4 (owner, 2026-10-08)              | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-08 -->

| ID  | Constraint                                                                                                                                                                     | Rules Out                                                                                                                                                |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | The engine's three dependencies are excluded from updates until README § 8 step 2's baseline tag exists; the exclusion is lifted by a change made after the tag that cites it. | Lifting the exclusion before the tag; an update that moves the engine off `hooked`'s versions before the baseline report is committed (`0001-F1` B-004). |
| C-2 | Renovate's behaviour is the checked-in configuration file.                                                                                                                     | A setting made only in the app's web interface, which no pull request reviews.                                                                           |
| C-3 | An update never edits a generated workflow file.                                                                                                                               | A Renovate pull request to `.github/workflows/ci.yml` or any workflow NUKE generates (`0055-F2` C-1).                                                    |
| C-4 | A version is changed only where it is declared once: `Directory.Packages.props`, `global.json` or the local tool manifest.                                                     | A version written into a `.csproj` to satisfy an update.                                                                                                 |
| C-5 | No dependency dashboard (owner, 2026-10-08).                                                                                                                                   | A Renovate dashboard issue; a major update waiting on approval in an issue rather than as an open pull request.                                          |
| C-6 | A GitHub Action's version is declared in the build project's source, where Renovate updates it; the workflow YAML is regenerated from it (owner, 2026-10-08).                  | An action version written only in a workflow file; a Renovate rule matching a file under `.github/workflows/`.                                           |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Item                                                           | Exclusion Reason                                                      |
| --- | -------------------------------------------------------------- | --------------------------------------------------------------------- |
| 1   | Allowing a merge without a person in the repository's settings | `0055-F8`.                                                            |
| 2   | The checks an update waits on                                  | `0055-F2`, `0055-F3`; required by `0055-F8`.                          |
| 3   | Updating the engine's dependencies after the baseline tag      | A later change under C-1, with `0001-F1`'s baseline test as its gate. |
| 4   | Vulnerability alerts and security advisories                   | Not asked for.                                                        |
| 5   | `hooked`'s and Transporter's dependency updates                | Tracked in those repositories (owner, 2026-10-08).                    |

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

| Claim ID | Scenario                                             | Test    | Status  |
| -------- | ---------------------------------------------------- | ------- | ------- |
| B-001    | Package updates arrive together                      | Missing | Missing |
| B-002    | Workflow action updates arrive together              | Missing | Missing |
| B-003    | An SDK update arrives on its own                     | Missing | Missing |
| B-004    | Local tool updates arrive together                   | Missing | Missing |
| B-005    | A green routine update merges itself                 | Missing | Missing |
| B-006    | A red routine update waits                           | Missing | Missing |
| B-007    | A major update waits for a person                    | Missing | Missing |
| B-008    | The engine's dependencies are left alone             | Missing | Missing |
| B-009    | An action update leaves the generated workflow alone | Missing | Missing |
| B-010    | Updates open no issue                                | Missing | Missing |
| B-011    | Update pull requests are labelled                    | Missing | Missing |
| B-012    | Each major update is reviewed alone                  | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-08 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                                                                                                            | Blocks       | Resolution                                                                                                                                                                                                                                                          |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | The owner asked for GitHub Actions updates (B-002) and for a NUKE-generated workflow (`0055-F2` C-1). An action's version in a generated workflow is written by the generator, so an edit to the YAML is undone by the next build (C-3). Do generated workflows' actions move only with the NUKE package, leaving B-002 to hand-written workflows (of which there may be none), or does the build declare action versions Renovate can update in the build project? | B-002        | Resolved 2026-10-08 by the repository owner: Renovate updates action versions in the build project's source through a custom manager, the YAML is regenerated from it, and Renovate never edits a generated workflow. B-002 amended; B-009 and C-6 added; C-3 kept. |
| OQ-2 | Renovate's dependency dashboard is a GitHub issue, and the configuration present makes a major wait for approval on it; this repository uses no GitHub issues (`github_mode: false`). Proposed default: no dashboard, and a major waits as an open pull request (B-007).                                                                                                                                                                                            | B-007        | Resolved 2026-10-08: proposed default accepted by the repository owner. B-010 and C-5 added.                                                                                                                                                                        |
| OQ-3 | The configuration present labels every update pull request `dependencies`; AGENTS.md § SDLC uses no labels for tracking. Is a label on a pull request kept?                                                                                                                                                                                                                                                                                                         | —            | Resolved 2026-10-08 by the repository owner: kept. B-011 added.                                                                                                                                                                                                     |
| OQ-4 | Are major updates grouped by ecosystem like minor and patch, or one pull request per package so each breaking change is reviewed alone?                                                                                                                                                                                                                                                                                                                             | B-007        | Resolved 2026-10-08 by the repository owner: one pull request per package. B-012 added.                                                                                                                                                                             |
| OQ-5 | An action update changes the build project only (B-009, C-6), but `0055-F2` B-009 fails a run whose committed workflow differs from what the build generates, so the update pull request would fail its own checks and never merge itself (B-005). Who commits the regenerated workflow to the update pull request: a CI step pushing to the update branch, a Renovate post-upgrade step, or a person?                                                              | B-002, B-005 | Open                                                                                                                                                                                                                                                                |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-08 -->

| Section | Status | Reviewer      | Note                |
| ------- | ------ | ------------- | ------------------- |
| 1-5     | 🟡     | spec-reviewer | Draft; not reviewed |

## Tasks

None cut. Items are cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
