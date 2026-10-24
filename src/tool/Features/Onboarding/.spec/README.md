---
title: "Specification: The onboarding report"
description: "An opt-in report that explains every finding from a repository's initial Specht check, in the terminal and as a standalone file"
type: feature
id: "F15"
epic: "0001"
spec_status: draft
status: ready-for-architecture
priority: med
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Specification governance"
author: "spec-author"
milestone: null
children: []
depends_on: ["F2"]
blocks: []
spikes: []
created: "2026-10-23"
updated: "2026-10-23"
github_issue: null
synced_at: null
---

# Specification: The onboarding report

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-23 -->

A repository owner who runs Specht for the first time receives a list of diagnostics but must look up each rule and interpret its evidence to understand the state of the repository. This Feature provides an explicitly requested onboarding report that explains every finding from that check, both in the terminal and in a standalone report file, so the owner can understand what Specht found before deciding what to address.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-23 -->

| #   | Persona                            | Need                                                                                                              | Pain Point Today                                                                                         |
| --- | ---------------------------------- | ----------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- |
| 1   | A repository owner adopting Specht | A complete explanation of the findings from the initial check, available in the terminal and as a standalone file | The diagnostic stream names rules and evidence but requires the owner to look up and interpret each rule |

### Assumptions

| ID  | Assumption                                                                                                                                         |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | A requested onboarding report runs the check once and describes that execution's findings; it does not compare runs or require stored run history. |
| A-2 | The report uses the selected schema version's rule definitions, matching the check that produced the findings.                                     |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-23 -->

| ID    | Claim                                                                                                                                                                                                                                 | Source                                                     | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------- | ------ |
| B-001 | Given a user explicitly requests an onboarding report, this Feature runs the check once and includes every finding from that execution, including its rule id, severity, repository-relative file, line, and identifier when present. | User need, 2026-10-23; `0001-F2` B-001                     | Active |
| B-002 | Given a finding in an onboarding report, this Feature explains the rule in human-readable terms and identifies the violation message and evidence that triggered the finding.                                                         | User need, 2026-10-23; `0001-F1` B-010                     | Active |
| B-003 | Given a requested onboarding report, this Feature presents the same complete set of findings in the terminal and in a standalone report file.                                                                                         | Owner decision, 2026-10-23; decision 0001                  | Active |
| B-004 | Given an onboarding report request, this Feature summarizes the findings by rule and gives the total number of findings.                                                                                                              | Screenshot reference supplied by the owner, 2026-10-23     | Active |
| B-005 | Given a check with no findings, this Feature says that no findings were reported.                                                                                                                                                     | Completeness of B-001; owner, 2026-10-23                   | Active |
| B-006 | Given an onboarding report is not explicitly requested, this Feature does not change the check command's output or exit code.                                                                                                         | Owner decision, 2026-10-23; decision 0001; `0001-F2` B-009 | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-23 -->

| ID  | Constraint                                                                                  | Rules Out                                                                           |
| --- | ------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| C-1 | The report is derived from a single check result and is deterministic for that result.      | Comparing the current tree with earlier runs or adding run history                  |
| C-2 | The report does not change the check's findings, severity, ordering contract, or exit code. | Making the report an alternate checker or an output mode that changes gate behavior |
| C-3 | The report is read-only apart from writing the explicitly selected standalone report file.  | Applying fixes or changing files in the checked repository                          |
| C-4 | Every path the report derives is relative to the checked root and uses forward slashes.     | Leaking machine-specific absolute paths                                             |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-23 -->

| #   | Item                                                                      | Exclusion Reason                                                                                               |
| --- | ------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| 1   | Automatically showing the onboarding explanation during an ordinary check | The owner chose an explicitly requested report so the check's established output remains stable; decision 0001 |
| 2   | Fixing, suppressing, or regrading findings                                | The report explains the check result; it does not change repository content or checker policy                  |
| 3   | Comparing findings across multiple check runs                             | This report covers one check execution; it does not keep run history                                           |

## 6. Concern Separation

Pending implementer authorship after agreement.

## 7. Technical Design

Pending implementer authorship after agreement.

## 8. Testing Strategy

Pending test-writer authorship after agreement.

## 9. Traceability Matrix

| Claim ID | Scenario                                                  | Test    | Status  |
| -------- | --------------------------------------------------------- | ------- | ------- |
| B-001    | One check supplies the complete report                    | Missing | Missing |
| B-002    | The report explains each finding                          | Missing | Missing |
| B-003    | The terminal and file reports contain the same findings   | Missing | Missing |
| B-004    | The report summarizes findings by rule                    | Missing | Missing |
| B-005    | A clean check is explained                                | Missing | Missing |
| B-006    | An ordinary check keeps its existing output and exit code | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-23 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-23 -->

| ID   | Question                                                                                      | Blocks | Resolution |
| ---- | --------------------------------------------------------------------------------------------- | ------ | ---------- |
| OQ-1 | What command syntax requests the report, and what file format does the standalone report use? | B-003  | Open       |

## 12. Sign-off

| Section | Status | Reviewer      | Note                     |
| ------- | ------ | ------------- | ------------------------ |
| 1-5     | 🟡     | spec-reviewer | Agreement review pending |

## Tasks

None. Work items are cut from the claims after agreement.

## Scoring

| Field | Value | Basis                       |
| ----- | ----- | --------------------------- |
| value | 0     | Not scored before agreement |
| risk  | 0     | Not scored before agreement |
