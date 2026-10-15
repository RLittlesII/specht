---
title: "Specification: The trace command"
description: "specht trace prints, for every claim of every specification, its § 9 status, the scenarios carrying its tag and which of its named tests resolve, as lines or as one JSON document, writing nothing and exiting 0 whatever the gaps"
type: feature
id: "F13"
epic: "0001"
spec_status: draft
status: blocked
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
depends_on: ["F2", "F5", "F12"]
blocks: []
spikes: []
created: "2026-10-14"
updated: "2026-10-15"
github_issue: null
synced_at: null
---

# Specification: The trace command

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-14 -->

The check reports only the rows that fail. A reader who wants to see where every claim stands, which scenarios carry it and which of its named tests exist, reads each § 9 by hand, greps the companions and the test tree, and gets a picture no one else can reproduce. This Feature removes that failure state: `specht trace` prints every claim of every specification with its § 9 status, the scenarios carrying its tag and the resolution of each test it names, through the same resolver the check uses (`0001-F12` C-3), as lines or as one JSON document, without judging the tree and without writing to it.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-14 -->

| #   | Persona                                           | Need                                                           | Pain Point Today                                        |
| --- | ------------------------------------------------- | -------------------------------------------------------------- | ------------------------------------------------------- |
| 1   | The maintainer reviewing a Feature's coverage     | Every claim's status, scenarios and tests in one listing       | Reads § 9, the companion and the test tree side by side |
| 2   | A script or dashboard over the specification tree | One document, stable in shape, holding each claim's resolution | Parses Markdown and Gherkin itself                      |
| 3   | The reviewer of a pull request                    | The same listing on every run of the same tree                 | A hand-made picture differs with whoever made it        |
| 4   | Epic `0201` candidate D                           | A text-level resolution surface to extend with code bindings   | N/A (internal dependency)                               |

### Assumptions

| ID  | Assumption                                                                                                                                                  |
| --- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | A test's resolution is one of `resolved`, `unresolved` and `unconfigured`, with the meanings `0001-F12` A-2 and B-007 give them (owner, 2026-10-14; B-004). |
| A-2 | The command has no rule id and is not schema-versioned: it runs the same under every shipped pin (owner, 2026-10-14).                                       |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-15 (B-001 and B-003 revised, B-009 added, review round 1) -->

| ID    | Claim                                                                                                                                                                                                                                                                                                                                                                           | Source                                                                                                                                    | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- | ------ |
| B-001 | Given `specht trace [--root <dir>]`, this Feature prints one line per claim per specification, holding the specification's path, the claim id, its § 9 status, the number of scenarios carrying its tag, and the number of its named tests that resolve out of the number named; for a claim with no § 9 row, the status reads `none` and the tests `0/0`.                      | owner, 2026-10-14; decision 0001; the line for a claim with no row is spec-author's reading after review round 1, not stated by the owner | Active |
| B-002 | Given `specht trace --json`, this Feature writes one document to stdout, and it validates against `docs/schema/trace.schema.json`.                                                                                                                                                                                                                                              | owner, 2026-10-14; decision 0001                                                                                                          | Active |
| B-003 | Given `specht trace --json`, each specification in the document carries its path, its id and its claims; each claim its id, its line, its § 3 status, its § 9 row as the row's line and its four cells, absent when the claim has no row, its scenarios as title, file and line, and its tests; and each test its name, its resolution, and its file and line when it resolves. | owner, 2026-10-14; `0001-F12` B-001; the absent row as B-001's                                                                            | Active |
| B-004 | Given a manifest with no `tests` key, every test's resolution is `unconfigured`, never `unresolved`.                                                                                                                                                                                                                                                                            | owner, 2026-10-14; `0001-F12` B-007                                                                                                       | Active |
| B-005 | Given a rule set `off` in the manifest's `rules`, the trace still reports the resolution that rule judges.                                                                                                                                                                                                                                                                      | owner, 2026-10-14; `0001-F5` B-061                                                                                                        | Active |
| B-006 | Given `specht trace`, this Feature exits `0` whatever gaps the trace shows, `2` for a missing root or manifest, and `3` for a manifest the check rejects, as the check does.                                                                                                                                                                                                    | owner, 2026-10-14; `0001-F2` B-006, B-007                                                                                                 | Active |
| B-007 | Given the same tree, `specht trace` gives byte-identical output, with specifications in discovery order and claims in § 3 order.                                                                                                                                                                                                                                                | owner, 2026-10-14; brief § 9; `0001-F6`                                                                                                   | Active |
| B-008 | Given `specht trace`, no file under the root or elsewhere is created, modified or deleted.                                                                                                                                                                                                                                                                                      | owner, 2026-10-14; brief § 9                                                                                                              | Active |
| B-009 | Given the check run with `--json`, its report carries no trace of the claims: the trace is this Feature's output alone, and the check's report stays the verdict.                                                                                                                                                                                                               | owner, 2026-10-14; decision 0001 call 4                                                                                                   | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-14 -->

| ID  | Constraint                                                                                                                    | Rules Out                                                                                                                |
| --- | ----------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| C-1 | Resolution is `0001-F12`'s resolver (`0001-F12` C-3; owner, 2026-10-14).                                                      | A second parser of § 9 cells, tags or test files in the command; a trace that disagrees with the check on the same tree. |
| C-2 | Offline and read-only, and every path is relative to the root with `/` separators (brief § 9; owner, 2026-10-14).             | A network call; a write into the tree; an absolute path in a line or in the document.                                    |
| C-3 | Within a tool major version the document changes only by addition (owner, 2026-10-14; as `0001-F3` C-4).                      | Renaming or removing a field, or changing a field's type, inside a major version.                                        |
| C-4 | A command parses, calls the engine and folds the result into an exit code; nothing else lives in the command (`0001-F2` C-4). | Reading Markdown, Gherkin or test files inside `Features/Trace/`.                                                        |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-15 (row 3 cites B-009, review round 1) -->

| #   | Item                                                      | Exclusion Reason                                                                                                                               |
| --- | --------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | A rendered graph or an HTML page                          | Not asked for; the document is the surface a renderer reads (owner, 2026-10-14).                                                               |
| 2   | Code bindings between a claim and the code that serves it | Epic `0201` candidate D extends this surface; this Feature answers epic `0201` OQ-11 as far as text-level resolution goes (owner, 2026-10-14). |
| 3   | A trace block inside the check's `--json` report          | Rejected: the check's report stays the verdict, and the trace is not one (owner, 2026-10-14; decision 0001; B-009).                            |
| 4   | Writing the trace to a file                               | Not decided (OQ-1).                                                                                                                            |
| 5   | Judging a row, or reporting a violation                   | `0001-F12`'s rules; the trace exits `0` whatever it shows (B-006).                                                                             |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-14 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-14 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-14 -->

Pending: owned by `test-writer`, written after agreement.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-15 (B-009 row added, review round 1) -->

| Claim ID | Scenario                                                     | Test    | Status  |
| -------- | ------------------------------------------------------------ | ------- | ------- |
| B-001    | The trace prints one line per claim                          | Missing | Missing |
| B-002    | The JSON trace validates against its schema                  | Missing | Missing |
| B-003    | The JSON trace carries each claim's row, scenarios and tests | Missing | Missing |
| B-004    | With no test files declared every test is unconfigured       | Missing | Missing |
| B-005    | A rule switched off does not hide a resolution               | Missing | Missing |
| B-006    | The trace succeeds whatever gaps it shows                    | Missing | Missing |
| B-007    | The same tree gives the same trace                           | Missing | Missing |
| B-008    | The trace writes nothing                                     | Missing | Missing |
| B-009    | The check's report carries no trace                          | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-15 (review round 1) -->

- 2026-10-15, after `spec-reviewer` round 1, before this specification first merged. Added: B-009, the claim the `@boundary` scenario "The check's report carries no trace" now carries, from decision 0001 call 4. B-001 and B-003 say what a claim with no § 9 row shows, as spec-author's reading. OQ-2 widened to the status markers and `tests` under a `0.1.0` pin.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-15 (OQ-2 widened, review round 1) -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             | Blocks              | Resolution |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------- | ---------- |
| OQ-1 | Does `specht trace` take `--report <path>`, writing the document to a file as the check's `--report` does? Recommendation: not yet.                                                                                                                                                                                                                                                                                                                                                                                                                                                  | § 5 row 4           | Open       |
| OQ-2 | Under a manifest that pins schema `0.1.0`, how does the trace read a tree? This command is not schema-versioned (A-2), but `0001-F12` B-001 reads the § 9 columns by position only under `0.2.0` or later, the covered, partial, waived, withdrawn and superseded markers exist only from `0.2.0` (`0001-F5` B-058), and `tests` is rejected under `0.1.0` (`0001-F5` B-055). Proposed: columns by position in `tables.matrix`, the five markers at their `0.2.0` defaults, and every test's resolution `unconfigured` (B-004). Raised by spec-author, 2026-10-14, not by the owner. | B-001, B-003, B-004 | Open       |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-15 (matrix-resolution delta, round 2) -->

| Section     | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| ----------- | ------ | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5, 11     | 🔴     | spec-reviewer | Round 1 (2026-10-15) - blocked, reviewing the uncommitted working tree on `b9dfb84`. Ids are new, § 9 carries each claim once, and `depends_on` F2, F5 and F12 is symmetric. Blocking (spec-author): (1) the `@B-002 @boundary` scenario "The check's report carries no trace" documents decision 0001 call 4 and § 5 row 3, which no claim states; B-002 is about `specht trace --json`. Add a claim for it, here or in `0001-F3`, and tag the scenario with it, or remove the scenario. Non-blocking: (2) OQ-2 is rightly an open question; it should also name the status markers (`0001-F5` B-058) and `tests`, which `0001-F5` B-055 rejects under `0.1.0`, both of which A-2's "the same under every shipped pin" meets. (3) B-001 does not say what a line holds for a claim with no § 9 row, a case B-006 lets the trace reach. (4) Decision 0001's Affects omits OQ-2. |
| 1-5, 10, 11 | 🟢     | spec-reviewer | Round 2 (2026-10-15) - approved, reviewing the uncommitted working tree on `b9dfb84` after spec-author's round-1 revision; this row clears the round-1 row above. All four round-1 findings are closed. B-009, a new id, states decision 0001 call 4, and the boundary scenario now carries `@B-009`; § 5 row 3 cites it and § 9 lists it once. OQ-2 names the markers and `tests`; B-001 and B-003 say what a claim with no § 9 row shows, labelled as spec-author's reading, with a scenario; decision 0001's Affects reads B-001 to B-009 and OQ-1, OQ-2. `spec_status` stays `draft` while OQ-1 and OQ-2 are open.                                                                                                                                                                                                                                                          |

## Tasks

None yet. Cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
