---
title: "Specification: Repository settings checklist"
description: "A checked-in checklist of the GitHub repository settings that make the checks binding - branch protection, required checks, merge methods, auto-merge, secrets and app installations - applied by hand once after the first push"
type: feature
id: "F8"
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
depends_on: ["F2", "F3", "F4"]
blocks: []
spikes: []
created: "2026-10-08"
updated: "2026-10-08"
github_issue: null
synced_at: null
---

# Specification: Repository settings checklist

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-08 -->

A workflow that runs is not a gate until the repository requires it: without branch protection a red pull request merges, without the merge methods AGENTS.md § SDLC relies on a squash drops the commit-message citations rule 3 depends on, and without the secret and app installations the coverage gate and dependency updates never report. None of that lives in a file, so after the first push it would be set from memory, or not at all. This Feature removes that failure state: one checked-in checklist names every repository setting the other Features need, it is applied by hand once after the first push, and a test keeps its required checks in step with the checks CI produces.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Persona                                    | Need                                                                   | Pain Point Today              |
| --- | ------------------------------------------ | ---------------------------------------------------------------------- | ----------------------------- |
| 1   | The maintainer, right after the first push | Every setting to apply, in one list, in one sitting                    | Settings exist only in memory |
| 2   | A reviewer of a settings change            | The settings as a reviewable file                                      | A settings page has no diff   |
| 3   | `0055-F2`, `0055-F3`, `0055-F4`            | Their checks required, their secret present, their app installed       | N/A (internal dependency)     |
| 4   | AGENTS.md rule 3                           | Squash built from commit messages, and rebase, as the only merge paths | Not configured                |

### Assumptions

| ID  | Assumption                                                                                                                                                                 |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The checklist sits beside this specification under `.github/settings/`; the `implementer` may move both together.                                                          |
| A-2 | `github_mode` stays `false`: GitHub issues are not the tracker, and the issue templates under `.github/ISSUE_TEMPLATE/` stay for outside reports only (owner, 2026-10-08). |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                                                                        | Source                                              | Status  |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------- | ------- |
| B-001 | Given the checklist, it requires a pull request before any change reaches `main`.                                                                                                                            | owner, 2026-10-08                                   | Active  |
| B-002 | Given the checklist and the CI workflow, the status checks the checklist requires from CI are exactly the per-operating-system checks the workflow produces.                                                 | owner, 2026-10-08; `0055-F2` B-003, B-007           | Active  |
| B-003 | Given the checklist, it requires the patch coverage status and does not require the project coverage status.                                                                                                 | owner, 2026-10-08; `0055-F3` B-003, B-005           | Active  |
| B-004 | Given the checklist, it enables squash merging with the commit messages as the squashed body.                                                                                                                | AGENTS.md § Specification-Driven Development rule 3 | Active  |
| B-005 | Given the checklist, it enables rebase merging and disables merge commits.                                                                                                                                   | AGENTS.md rule 3; `specht-conventions` § Delivery   | Active  |
| B-006 | Given the checklist, it enables merging a pull request automatically once its required checks pass.                                                                                                          | owner, 2026-10-08; `0055-F4` B-005                  | Active  |
| B-007 | Given the checklist and the committed workflows, every secret a workflow reads - the Codecov token and the Renovate workflow's token among them - is named in the checklist with the workflow that reads it. | owner, 2026-10-08; `0055-F3` C-2; `0055-F4` C-8     | Amended |
| B-008 | Given the checklist, it names the Codecov installation and no Renovate app installation, because Renovate runs as this repository's own workflow.                                                            | `0055-F3`; `0055-F4` OQ-6 (owner, 2026-10-08)       | Amended |
| B-009 | Given the checklist, it keeps GitHub issues enabled, for outside reports through the issue templates.                                                                                                        | owner, 2026-10-08; A-2                              | Active  |
| B-010 | Given the checklist, it sets the repository's visibility to public.                                                                                                                                          | OQ-1 (owner, 2026-10-08)                            | Active  |
| B-011 | Given the checklist, its protection of `main` requires no approving review; the required checks are the gate.                                                                                                | OQ-2 (owner, 2026-10-08)                            | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-08 -->

| ID  | Constraint                                                                             | Rules Out                                                                                                  |
| --- | -------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| C-1 | The settings are a checked-in document applied by hand (decision 0001).                | A script calling the GitHub API; a settings-as-code app; a setting recorded nowhere but the settings page. |
| C-2 | The checklist names secrets, never their values.                                       | A token, or any part of one, in the checklist or its history.                                              |
| C-3 | The checklist is the record of the settings: a setting changes in the checklist first. | A setting changed on GitHub that the checklist does not show.                                              |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Item                                                                     | Exclusion Reason                                                                              |
| --- | ------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------- |
| 1   | GitHub issues, labels and milestones as a tracker                        | `github_mode: false` (AGENTS.md § SDLC); work is tracked in `.issue/` items.                  |
| 2   | Turning an outside report into work                                      | A maintainer writes a `.issue/` item from it by hand; nothing here automates that.            |
| 3   | Producing the checks, the coverage statuses and the update pull requests | `0055-F2`, `0055-F3`, `0055-F4`.                                                              |
| 4   | Verifying that GitHub's live settings match the checklist                | C-1: no call to the GitHub API; the tool and its tests stay offline (AGENTS.md § Invariants). |
| 5   | Package feed permissions for consumers                                   | `0055-F7`.                                                                                    |

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

| Claim ID | Scenario                                          | Test    | Status  |
| -------- | ------------------------------------------------- | ------- | ------- |
| B-001    | Main accepts changes only through pull requests   | Missing | Missing |
| B-002    | The required checks are the checks CI produces    | Missing | Missing |
| B-003    | The patch status is required and the total is not | Missing | Missing |
| B-004    | A squash keeps the commit messages                | Missing | Missing |
| B-005    | Rebase is allowed and merge commits are not       | Missing | Missing |
| B-006    | A green pull request can merge itself             | Missing | Missing |
| B-007    | Every secret a workflow reads is listed           | Missing | Missing |
| B-008    | The service installations are listed              | Missing | Missing |
| B-009    | Outside reports can still be filed                | Missing | Missing |
| B-010    | The repository is public                          | Missing | Missing |
| B-011    | Required checks are the gate, not a review        | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-08 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                  | Blocks       | Resolution                                                                                                       |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------ | ---------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Is the repository public or private? It decides whether Codecov needs a token for pull requests from forks, whether outside reports are possible at all, and what reading the published package requires. | B-007, B-009 | Resolved 2026-10-08 by the repository owner: public. B-010 added.                                                |
| OQ-2 | Does branch protection require an approving review? With one maintainer, a required review blocks self-merging, and a dependency update merged without a person (`0055-F4` B-005) must be exempt.         | B-001, B-006 | Resolved 2026-10-08 by the repository owner: no approving review; the required checks are the gate. B-011 added. |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-08 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| ------- | ------ | ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Draft; not reviewed                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| 1-5     | 🟡     | spec-reviewer | Round 1 (2026-10-08) - reviewed in draft, reviewing commit `04fdc28`; no blocking findings of its own. B-001 to B-011 state the owner's settings and OQ-1 and OQ-2 faithfully; B-002 and B-003 agree with `0055-F2` B-003 and B-007 and `0055-F3` B-003 and B-005; B-011 is what `0055-F4` B-005 needs. B-007 and B-008 wait on `0055-F4` OQ-6: a self-hosted Renovate has no app installation and reads a token secret. Non-blocking (spec-author): B-006 and B-008 serve `0055-F4`, and neither Feature lists the edge; B-005 carries two assertions; with B-003 required, a pull request whose upload failed (`0055-F3` B-007) waits for a re-run, as `0055-F3` OQ-2 intends - one line in the checklist would say so. Left `draft`: the spec-author has not submitted it, and epic `0055` says its eight-Feature split awaits the owner's confirmation. |
| 1-5     | 🟢     | spec-reviewer | Round 2 (2026-10-08) - approved, reviewing `acf3cff`..`f32a31d`. B-007 and B-008 follow `0055-F4` OQ-6 and C-8 (the Renovate token secret, no app installation); `depends_on` names `0055-F4`, symmetric with its `blocks`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |

## Tasks

Cut 2026-10-08 into [`../.issue/`](../.issue/): `0089` (the Feature), with `0090` to `0091`.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
