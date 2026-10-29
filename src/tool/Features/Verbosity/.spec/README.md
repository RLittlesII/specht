---
title: "Specification: Output verbosity"
description: "A requested verbosity level changes only what a specht command prints on stdout, never its exit code, its --json document, its --report file, its stderr or what it writes under the root, and never turns a rule off"
type: feature
id: "F17"
epic: "0001"
spec_status: draft
status: blocked
priority: low
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Specification governance"
author: "spec-author"
milestone: null
children: []
depends_on: ["F2", "F3"]
blocks: []
spikes: []
created: "2026-10-29"
updated: "2026-10-29"
github_issue: null
synced_at: null
---

# Specification: Output verbosity

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-29 -->

Every `specht` command prints one fixed amount on stdout: the check its violation lines and summary, `upgrade` and `init` a line per file. A reader who wants less has one lever today, a rule's level, and it changes the verdict as well as the output. This Feature, if its need is confirmed (OQ-5), lets an invocation ask for less on stdout while the verdict, the exit code, the JSON document and the report file stay exactly what the same run gives without the request.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-29 -->

| #   | Persona                                                       | Need                                                                  | Pain Point Today                                                                                                   |
| --- | ------------------------------------------------------------- | --------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| 1   | A consumer who reads the check's output only when it fails    | Less on stdout from a passing or failing run, with the same verdict   | Unconfirmed (OQ-5); the owner asked on 2026-10-29 whether some people want it quiet, without missing rows reported |
| 2   | A hook, a build target or CI that reads the exit code or JSON | The verdict and the report unchanged whatever level a person asks for | N/A (a contract this Feature must not break)                                                                       |

### Assumptions

| ID  | Assumption                                                                                                                                                                           |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| A-1 | The owner's 2026-10-29 words were a question, not a decision; this specification records the agreement it would need and leaves the need open (OQ-5).                                |
| A-2 | A "quiet request" in these claims is whatever invocation OQ-1 and OQ-2 settle on; no claim depends on its spelling.                                                                  |
| A-3 | Missing coverage rows are `SPEC060`'s violations, and `specht rules set SPEC060 off` or `warning` already silences them or stops them failing for one repository (`0001-F11` B-001). |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-29 -->

| ID    | Claim                                                                                                                                                                   | Source                                                    | Status |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------- | ------ |
| B-001 | Given a run that requests no verbosity level, this Feature leaves every command's stdout as that command's own Feature defines it.                                      | Owner question, 2026-10-29; `0001-F2` B-001, B-002, B-009 | Active |
| B-002 | Given a quiet request, this Feature exits with the code the same run exits with without it.                                                                             | `AGENTS.md` § Invariants; `0001-F2` B-003, B-004          | Active |
| B-003 | Given a quiet request beside `--json`, this Feature writes on stdout the same report document the same run writes without it.                                           | `AGENTS.md` § Invariants; `0001-F3`                       | Active |
| B-004 | Given a quiet request beside `--report <path>`, this Feature writes to that path the same bytes the same run writes without it.                                         | `AGENTS.md` § Invariants; `0001-F3`                       | Active |
| B-005 | Given a quiet request, this Feature writes on stderr the same text the same run writes without it.                                                                      | `0001-F2` B-005 to B-007, C-3                             | Active |
| B-006 | Given a quiet request on a command that writes under the root, this Feature creates, changes and deletes the same files with the same bytes as the same run without it. | `AGENTS.md` § Invariants; `0001-F4` C-1; `0001-F7` B-017  | Active |
| B-007 | Given a quiet request on a check, this Feature prints none of the summary lines `0001-F2` B-002 defines.                                                                | Owner question, 2026-10-29                                | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-29 -->

| ID  | Constraint                                                                                          | Rules Out                                                                                                                                           |
| --- | --------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | A verbosity level changes only what a command writes on stdout outside `--json`.                    | A level that changes an exit code, the `--json` document, the `--report` file, a stderr message or a file under the root                            |
| C-2 | Turning a rule off or down stays a rule level in the manifest (`0001-F11`).                         | A level that drops, adds or regrades the violations of a named rule, such as hiding `SPEC060`'s missing rows, so a rule has a second off switch     |
| C-3 | The level is selected only by what the invocation names, or by the manifest if OQ-2 adds a setting. | Selecting a level by whether stdout is a terminal, by an environment variable or by detecting CI, so the same tree and command give the same output |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-29 -->

| #   | Item                                                                       | Exclusion Reason                                                                                                                 |
| --- | -------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Silencing or regrading one rule, such as missing coverage rows (`SPEC060`) | `0001-F11`: `specht rules set SPEC060 off` or `warning`; C-2                                                                     |
| 2   | Any change to the report document or the `--report` file                   | `0001-F3` owns the report; C-1                                                                                                   |
| 3   | Any change to an exit code or to `--strict`                                | `0001-F2` B-003, B-004; C-1                                                                                                      |
| 4   | Colour, tables, trees or other presentation of the output                  | Epic `0194`                                                                                                                      |
| 5   | Amending a `0001-F7` claim about `upgrade` or `schema` output              | `0001-F7` is the 0.3.0 milestone's work; its claims hold for every run that requests no level (B-001), and OQ-3 decides the rest |
| 6   | Explaining findings at length                                              | `0001-F15`, the onboarding report                                                                                                |

## 6. Concern Separation

Pending implementer authorship after agreement.

## 7. Technical Design

Pending implementer authorship after agreement.

## 8. Testing Strategy

Pending test-writer authorship after agreement.

## 9. Traceability Matrix

| Claim ID | Scenario                                        | Test    | Status  |
| -------- | ----------------------------------------------- | ------- | ------- |
| B-001    | A run without a level prints what it does today | Missing | Missing |
| B-002    | A quiet request keeps the exit code             | Missing | Missing |
| B-003    | A quiet request keeps the JSON document         | Missing | Missing |
| B-004    | A quiet request keeps the report file           | Missing | Missing |
| B-005    | A quiet request keeps stderr                    | Missing | Missing |
| B-006    | A quiet request writes the same files           | Missing | Missing |
| B-007    | A quiet check prints no summary                 | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-29 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-29 -->

| ID   | Question                                                                                                                                                                                                                           | Blocks         | Resolution |
| ---- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------- | ---------- |
| OQ-1 | What option name or names request a verbosity level?                                                                                                                                                                               | B-002 to B-007 | Open       |
| OQ-2 | Is there a manifest setting for the level as well as the command-line option, and which wins when both are present?                                                                                                                | C-3            | Open       |
| OQ-3 | Which commands does a quiet request change: the check's summary lines (`0001-F2` B-002), `upgrade`'s rewritten and skipped listing (`0001-F7` B-017, B-030, B-031, B-061), `init`'s written and skipped listing (`0001-F4` B-006)? | B-007          | Open       |
| OQ-4 | Does a quiet check still print every violation line, only error-severity lines, or none?                                                                                                                                           | B-007          | Open       |
| OQ-5 | What consumer need does a quiet level meet that a rule level does not? The 2026-10-29 question names missing rows, which `specht rules set SPEC060 off` already silences per repository.                                           | B-001 to B-007 | Open       |
| OQ-6 | Is there a level louder than today's output, and what would it add?                                                                                                                                                                | B-001          | Open       |

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
