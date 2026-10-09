---
title: "Specification: End-to-end check scaling"
description: "A benchmark of the engine's whole check - manifest load to report, from a root on disk - over generated trees of 1, 10, 100 and 1000 specifications, so how its time and allocation grow with the tree is a recorded finding"
type: feature
id: "F3"
epic: "0109"
spec_status: approved
status: blocked
priority: med
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Performance measurement"
author: "spec-author"
milestone: null
children: []
depends_on: ["F1"]
blocks: []
spikes: []
created: "2026-10-09"
updated: "2026-10-09"
github_issue: null
synced_at: null
---

# Specification: End-to-end check scaling

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-09 -->

The check runs at three call sites, one a pre-commit hook a person waits on (brief § 2 Must-1), and nobody knows how its cost grows as a repository's specifications multiply: a check that is linear at ten specifications and quadratic at a thousand is discovered by the consumer whose tree reaches a thousand. This Feature removes that failure state: the engine's whole check is measured over trees of four sizes a decade apart, so whether its time and allocation grow linearly or worse is a recorded finding before any consumer's tree is that large.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-09 -->

| #   | Persona                             | Need                                                                      | Pain Point Today                                              |
| --- | ----------------------------------- | ------------------------------------------------------------------------- | ------------------------------------------------------------- |
| 1   | The maintainer of four repositories | Know what a check costs as a repository grows, before one grows           | A slow hook is the first signal                               |
| 2   | `benchmarker`                       | One benchmark whose result reads as a growth shape across tree sizes      | No benchmark exists to cite against a scaling concern         |
| 3   | `implementer`                       | See whether a change moved the whole check, not only the stage it touched | Stage numbers do not add up to the whole when stages interact |

### Assumptions

| ID  | Assumption                                                                                                                                              |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The tree sizes and the generated, clean tree are `0109-F1`'s (`0109-F1` decision 0004, B-015).                                                          |
| A-2 | The whole check is the engine's run from a root - `SpecCheckRunner`'s entry that takes one, as ADR-0005 § Decision (a) shapes it - and not the command. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-09 -->

| ID    | Claim                                                                                                                                      | Source                   | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------ | ------ |
| B-001 | Given generated trees of 1, 10, 100 and 1000 specifications, this Feature measures one whole check of each.                                | owner, 2026-10-09; C-1   | Active |
| B-002 | Given a measured check at any size, this Feature's measured call returns a report that counts every specification generated for that size. | C-2                      | Active |
| B-003 | Given a measured check, this Feature measures it over a tree that lays every specification out co-located, beside the code it specifies.   | OQ-1 (owner, 2026-10-09) | Active |
| B-004 | Given a result, this Feature names in it the number of specifications it was measured over.                                                | C-1                      | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-09 -->

| ID  | Constraint                                                                                                                                                                            | Rules Out                                                                                                                 |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------- |
| C-1 | A whole check's time and allocation is a reported finding against the number of specifications in the tree, stated as a growth shape - constant, linear or worse - never a threshold. | A gate on it, or on its growth; a single-size measurement; a finding of "fast" or "slow".                                 |
| C-2 | The measured call is the engine's whole run from a root on disk - manifest load, discovery, model build, evaluation and the report - and nothing of the host.                         | Measuring the command, the process, argument parsing or output formatting (`0109-F4`); a run cut short before the report. |
| C-3 | The tree is on disk, and the finding says the number includes the file system and its cache.                                                                                          | Reading the tree into memory before the measurement; a measured check that skips the disk is not a check.                 |
| C-4 | Each size's tree is generated once, before its measurements, and reused across them.                                                                                                  | Generating or deleting a tree inside the measurement.                                                                     |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-09 -->

| #   | Item                                                        | Exclusion Reason                                                                  |
| --- | ----------------------------------------------------------- | --------------------------------------------------------------------------------- |
| 1   | Where inside the check the time goes                        | `0109-F2`, stage by stage.                                                        |
| 2   | The tool's process start, and the command around the engine | `0109-F4`.                                                                        |
| 3   | A tree with violations                                      | `0109-F1` decision 0004: the generated tree is clean.                             |
| 4   | The legacy `epics/<epic>/<feature>/spec.md` layout          | Owner, 2026-10-09: co-located only (OQ-1).                                        |
| 5   | Writing the report document to a file                       | `--report` is the host's; the measured run returns the report and writes nothing. |
| 6   | A consumer's real tree, measured in place                   | Never: fixtures are generated (`0109-F1` C-7).                                    |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-09 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-09 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-09 -->

Pending: owned by `test-writer`, written after agreement.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-09 -->

| Claim ID | Scenario                                     | Test    | Status  |
| -------- | -------------------------------------------- | ------- | ------- |
| B-001    | A whole check is measured at every tree size | Missing | Missing |
| B-002    | The measured check covers the whole tree     | Missing | Missing |
| B-003    | The measured tree is laid out co-located     | Missing | Missing |
| B-004    | Each result names its tree size              | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-09 -->

- 2026-10-09, spec-reviewer finding on commit `4b23e76`: B-002 to B-004 restated with this Feature as subject. C-3's exclusion no longer calls `0109-F2` B-007's evaluation in-memory; it reads `.feature` files from disk too.
- 2026-10-09, the owner resolved OQ-1: co-located only. B-003 stands; § 5 row 4 cites the decision.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-09 -->

| ID   | Question                                                                                                                                                                                                                                                                                          | Blocks | Resolution                                                  |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ | ----------------------------------------------------------- |
| OQ-1 | Is the legacy `epics/<epic>/<feature>/spec.md` layout measured as well as the co-located one? Discovery reads both, and Transporter's tree is legacy (brief § 2 "Done means"); measuring both doubles the scaling run. Default: co-located only, the layout this repository and the template use. | B-003  | Resolved 2026-10-09: co-located only; B-003 stands (owner). |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-09 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                          |
| ------- | ------ | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Awaiting first review.                                                                                                                                                                                                                                                                                        |
| 1-5     | 🟢     | spec-reviewer | Round 1 (2026-10-09) - approved, reviewing the uncommitted OQ answers on `cdb0876`. OQ-1 is resolved in place to the owner's co-located-only answer; B-003 stands and cites it; § 5 row 4 cites the decision; § 9 and every `@B` tag resolve, and the `@B-003 @boundary` scenario states the excluded layout. |

## Tasks

None yet. Cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
