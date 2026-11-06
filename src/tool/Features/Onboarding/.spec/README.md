---
title: "Specification: The onboarding report"
description: "An opt-in report that explains every finding from a repository's initial Specht check, in the terminal and as a standalone file"
type: feature
id: "F15"
epic: "0001"
spec_status: in-review
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
updated: "2026-11-06"
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

<!-- last written by: spec-author, 2026-11-06 (B-001 and B-003 amended, decision 0002) -->

| ID    | Claim                                                                                                                                                                                                                                            | Source                                                                                                                                         | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------- | ------ |
| B-001 | Given a user requests an onboarding report with `specht onboard`, this Feature runs the check once and includes every finding from that execution, including its rule id, severity, repository-relative file, line, and identifier when present. | User need, 2026-10-23; `0001-F2` B-001; amended 2026-11-06 (owner; decision 0002): was "Given a user explicitly requests an onboarding report" | Active |
| B-002 | Given a finding in an onboarding report, this Feature explains the rule in human-readable terms and identifies the violation message and evidence that triggered the finding.                                                                    | User need, 2026-10-23; `0001-F1` B-010                                                                                                         | Active |
| B-003 | Given a requested onboarding report, this Feature presents the same complete set of findings in the terminal and in a standalone Markdown file at the path the caller names.                                                                     | Owner decision, 2026-10-23; decision 0001; amended 2026-11-06 (owner; decision 0002): was "in a standalone report file"                        | Active |
| B-004 | Given an onboarding report request, this Feature summarizes the findings by rule and gives the total number of findings.                                                                                                                         | Screenshot reference supplied by the owner, 2026-10-23                                                                                         | Active |
| B-005 | Given a check with no findings, this Feature says that no findings were reported.                                                                                                                                                                | Completeness of B-001; owner, 2026-10-23                                                                                                       | Active |
| B-006 | Given an onboarding report is not explicitly requested, this Feature does not change the check command's output or exit code.                                                                                                                    | Owner decision, 2026-10-23; decision 0001; `0001-F2` B-009                                                                                     | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-23 -->

| ID  | Constraint                                                                                  | Rules Out                                                                           |
| --- | ------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| C-1 | The report is derived from a single check result and is deterministic for that result.      | Comparing the current tree with earlier runs or adding run history                  |
| C-2 | The report does not change the check's findings, severity, ordering contract, or exit code. | Making the report an alternate checker or an output mode that changes gate behavior |
| C-3 | The report is read-only apart from writing the explicitly selected standalone report file.  | Applying fixes or changing files in the checked repository                          |
| C-4 | Every path the report derives is relative to the checked root and uses forward slashes.     | Leaking machine-specific absolute paths                                             |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-11-06 (rows 4 and 5, decision 0002) -->

| #   | Item                                                                            | Exclusion Reason                                                                                               |
| --- | ------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| 1   | Automatically showing the onboarding explanation during an ordinary check       | The owner chose an explicitly requested report so the check's established output remains stable; decision 0001 |
| 2   | Fixing, suppressing, or regrading findings                                      | The report explains the check result; it does not change repository content or checker policy                  |
| 3   | Comparing findings across multiple check runs                                   | This report covers one check execution; it does not keep run history                                           |
| 4   | An onboarding option on the check command, such as `specht --onboarding <path>` | The owner chose a command of its own and left the check untouched; decision 0002                               |
| 5   | An HTML report file, or any file format but Markdown                            | The owner chose Markdown for the standalone file; decision 0002                                                |

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

<!-- last written by: spec-author, 2026-11-06 -->

- 2026-11-06, the owner resolved OQ-1 ([decision 0002](decisions/0002-the-report-is-specht-onboard-and-its-file-is-markdown-at-output.md)): the report is the command `specht onboard [--root <dir>] --output <path>`, and its standalone file is Markdown. Amended in place, ids kept: B-001 names the command, B-003 the Markdown file at the caller-named path, and B-003's scenario follows. § 5 rows 4 and 5 added. OQ-2 raised. `spec_status` is `in-review`; items `0384` to `0386` cut.

## 11. Open Questions

<!-- last written by: spec-author, 2026-11-06 (OQ-1 resolved by the owner, decision 0002; OQ-2 raised) -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | Blocks                                                          | Resolution                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | What command syntax requests the report, and what file format does the standalone report use?                                                                                                                                                                                                                                                                                                                                                                                                                                                                            | B-003                                                           | Resolved 2026-11-06 by the repository owner (decision 0002), answering a multiple-choice question: a new command, `specht onboard [--root <dir>] --output <path>`, prints the report in the terminal and writes the standalone file at the path `--output` names; the file is Markdown; the check command is untouched. Rejected: an HTML file; a flag on the check, `specht --onboarding <path>`. B-001 and B-003 amended in place to say so; § 5 rows 4 and 5 added. |
| OQ-2 | What does `specht onboard` exit with, and print, when the check reports findings, when the root or manifest is missing or the manifest is invalid, and when `--output` is absent? Readings: (a) the check's codes for an input failure, `2` and `3`, and `0` once the report is written whatever it found, since the report explains and does not gate (C-2); (b) the check's own exit code, `1` when it reports findings. Recommendation: (a), with `--output` required, as the owner's syntax shows it unbracketed. Raised 2026-11-06 by spec-author. Owner not asked. | a claim for the command's exit codes; item `0385` reaching done | Open                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |

## 12. Sign-off

<!-- last written by: spec-author, 2026-11-06 (row note only; the review is spec-reviewer's) -->

| Section                                                    | Status | Reviewer      | Note                                                                                                                                                                                                               |
| ---------------------------------------------------------- | ------ | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1-5, 10, 11, decision 0002, Tasks (items `0384` to `0386`) | 🟡     | spec-reviewer | Agreement review pending. The owner resolved OQ-1 and approved B-001 to B-006 for cutting on 2026-11-06 (decision 0002); B-001 and B-003 amended, § 5 rows 4 and 5 added, OQ-2 raised, items `0384` to `0386` cut. |

## Tasks

Cut 2026-11-06 into [`../.issue/`](../.issue/), on the owner's approval of B-001 to B-006 and ruling on OQ-1 ([decision 0002](decisions/0002-the-report-is-specht-onboard-and-its-file-is-markdown-at-output.md)): `0384` (the Feature), with `0385`, the `specht onboard` command and the terminal report from one check run (B-001, B-002, B-004 to B-006), and `0386`, the Markdown file at `--output` with the same findings (B-003), after `0385`.

## Scoring

| Field | Value | Basis                       |
| ----- | ----- | --------------------------- |
| value | 0     | Not scored before agreement |
| risk  | 0     | Not scored before agreement |
