---
title: "Specification: The coverage gate"
description: "The test run writes coverage, CI uploads it to Codecov, new code in a pull request must be at least 80% covered or the patch status fails, and the project total is reported and never blocks"
type: feature
id: "F3"
epic: "0055"
spec_status: approved
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
depends_on: ["F1", "F2"]
blocks: ["F8"]
spikes: []
created: "2026-10-08"
updated: "2026-10-08"
github_issue: null
synced_at: null
---

# Specification: The coverage gate

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-08 -->

A `.codecov.yml` exists, but nothing produces coverage, nothing uploads it, and the file it holds would block a pull request on the repository's total while setting no target for the code the pull request adds. So a change can arrive with its new code untested and pass, while an unrelated drop in the total blocks a change that is fully tested. This Feature removes that failure state: every test run writes coverage, CI uploads it, the code a pull request changes must be at least 80% covered, and the total is reported for the reviewer without ever blocking.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Persona                        | Need                                                         | Pain Point Today                                                |
| --- | ------------------------------ | ------------------------------------------------------------ | --------------------------------------------------------------- |
| 1   | The reviewer of a pull request | To know the new code is tested without reading every test    | Nothing measures coverage                                       |
| 2   | The author of a pull request   | A gate about their change, not about code they did not touch | The configuration present would block on the repository's total |
| 3   | The maintainer                 | The total visible over time                                  | Nothing is uploaded                                             |
| 4   | `0055-F8`                      | A status name to require                                     | N/A (internal dependency)                                       |

### Assumptions

| ID  | Assumption                                                                                                                                                                                           |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | `.codecov.yml` today sets the project status with no target and therefore blocking, and the patch status with no target; this Feature replaces both settings. Its other keys are not this Feature's. |
| A-2 | Every operating system's coverage is uploaded and Codecov merges the uploads for one commit, so a line covered only on Windows counts as covered.                                                    |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                             | Source                                          | Status  |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------- | ------- |
| B-001 | Given the `Test` target runs, each test project runs once and this Feature writes one Cobertura coverage report per test project under `.artifacts/coverage/`.    | `specht-conventions` § Testing; `0055-F1` B-005 | Amended |
| B-002 | Given a CI run on any operating system, this Feature uploads that run's coverage reports to Codecov for the run's commit.                                         | owner, 2026-10-08; A-2                          | Active  |
| B-003 | Given a pull request whose changed lines of measured code are less than 80% covered, the patch coverage status on its head commit fails.                          | owner, 2026-10-08                               | Active  |
| B-004 | Given a pull request whose changed lines of measured code are 80% covered or more, the patch coverage status on its head commit passes.                           | owner, 2026-10-08                               | Active  |
| B-005 | Given a pull request that lowers the total coverage of measured code, the project coverage status reports the total and the change, and passes.                   | owner, 2026-10-08                               | Active  |
| B-006 | Given a pull request whose only changed lines are outside `src/`, the patch coverage status counts none of them.                                                  | OQ-1 (owner, 2026-10-08); C-4                   | Active  |
| B-007 | Given a CI run whose upload to Codecov fails, that operating system's check does not fail on that account and the run reports a warning naming the failed upload. | OQ-2 (owner, 2026-10-08)                        | Active  |
| B-008 | Given a pull request in which no line of measured code changed, the patch coverage status passes.                                                                 | C-4; `0055-F4` B-005; `0055-F8` B-003           | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-08 -->

| ID  | Constraint                                                                                                      | Rules Out                                                                                        |
| --- | --------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| C-1 | The patch target, 80%, is written once, in the Codecov configuration (owner, 2026-10-08).                       | A second copy of the number in the build, a workflow or a skill; a target per operating system.  |
| C-2 | The Codecov upload token is a repository secret.                                                                | A token in `.codecov.yml`, a workflow, the build project or any tracked file.                    |
| C-3 | Coverage is measured from the test tiers `0055-F1` runs, and from nothing else.                                 | A separate coverage-only test run; coverage from a test that runs outside the build.             |
| C-4 | Measured code is `src/**` only (owner, 2026-10-08).                                                             | Test projects, `.build/` or any other path raising or lowering the patch or the project number.  |
| C-5 | The patch gate is Codecov's patch status as Codecov reports it; nothing in CI recomputes or substitutes for it. | A CI step that fails a run on a coverage number; a run failed because the upload failed (B-007). |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Item                                                  | Exclusion Reason                                              |
| --- | ----------------------------------------------------- | ------------------------------------------------------------- |
| 1   | A target for the total that blocks                    | Owner, 2026-10-08: the total is reported, not a gate (B-005). |
| 2   | Requiring the patch status before a merge             | `0055-F8`; this Feature produces the status.                  |
| 3   | Running the tests                                     | `0055-F1`.                                                    |
| 4   | Codecov's pull-request comment and its other settings | Not a gate; left as configured (A-1).                         |
| 5   | Coverage of mutation, branch or path kinds as a gate  | Not asked for; the gate is line coverage of changed lines.    |

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

| Claim ID | Scenario                                                | Test    | Status  |
| -------- | ------------------------------------------------------- | ------- | ------- |
| B-001    | A test run writes coverage                              | Missing | Missing |
| B-002    | Integration uploads coverage                            | Missing | Missing |
| B-003    | Under-tested new code fails the patch status            | Missing | Missing |
| B-004    | Tested new code passes the patch status                 | Missing | Missing |
| B-005    | A lower total is reported and does not block            | Missing | Missing |
| B-006    | Changes outside the product are not measured            | Missing | Missing |
| B-007    | A failed upload warns and does not fail the run         | Missing | Missing |
| B-008    | A change with no measured lines passes the patch status | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-08 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                        | Blocks              | Resolution                                                                                                                                                                           |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| OQ-1 | What is "measured code"? Proposed default: `src/**` only, so test projects and `.build/` neither raise nor lower either number.                                                                                 | B-003, B-004, B-005 | Resolved 2026-10-08: proposed default accepted by the repository owner. C-4 and B-006 added.                                                                                         |
| OQ-2 | When the upload to Codecov fails (an outage, a missing token on a fork's pull request), does the operating system's CI check fail, or does the missing coverage status alone block the merge through `0055-F8`? | B-002               | Resolved 2026-10-08 by the repository owner: a failed upload does not fail CI and is reported as a warning; the patch gate is Codecov's status when it reports. B-007 and C-5 added. |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-08 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| ------- | ------ | ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Draft; not reviewed                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
| 1-5     | 🟡     | spec-reviewer | Round 1 (2026-10-08) - reviewed in draft, reviewing commit `04fdc28`; no blocking findings. The claims state the owner's decisions - an 80% patch target written once, an informational total, `src/**` measured, a failed upload warning without failing - one each and testably, and § 9 and the tags are complete. Non-blocking (spec-author): B-006 says lines outside `src/` count for nothing but not the patch status's verdict when no measured line changed, which a dependency update (`0055-F4` B-005) needs to pass because `0055-F8` B-003 requires the status; B-001's one report per test project assumes `Test` runs each project once, which `0055-F1` B-005 does not say. Left `draft`: the spec-author has not submitted it, and epic `0055` says its eight-Feature split awaits the owner's confirmation. |
| 1-5     | 🟢     | spec-reviewer | Round 2 (2026-10-08) - approved, reviewing `acf3cff`..`f32a31d`. B-008 closes the round-1 gap: a pull request with no measured line changed passes the patch status, which `0055-F4` B-005 and `0055-F8` B-003 need. Non-blocking (spec-author): B-001's one report per test project still assumes `Test` runs each project once.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| 1-5     | 🟢     | spec-reviewer | Round 3 (2026-10-08) - approved, reviewing `01f1f73`..`9a379ce`. B-001 now says each test project runs once, consistent with `0055-F1` B-005; the round-2 non-blocking finding is fixed.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |

## Tasks

None cut. Items are cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
