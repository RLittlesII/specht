---
title: "Specification: The format command"
description: "specht format moves whole table rows and frontmatter keys into the order the ordering rules declare, edits nothing else, never renumbers an id, and with --check writes nothing and exits 1 when it would change a file"
type: feature
id: "F5"
epic: "0101"
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
depends_on: ["F1", "F4", "0001/F7"]
blocks: []
spikes: []
created: "2026-10-08"
updated: "2026-10-08"
github_issue: null
synced_at: null
---

# Specification: The format command

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-08 -->

Once `0101-F1` and `0101-F4` report rows and keys out of order, someone has to move them, and moving a table row by hand is where cells get trimmed, a row gets dropped and an id gets "tidied". The check cannot do it: it is a read-only oracle at three call sites (Must-1). This Feature removes that failure state with a separate command, `specht format`, which moves whole table rows and whole frontmatter keys into the declared order and edits nothing else, so the fix is mechanical, reviewable as a pure reordering, and never touches what a row or a key says.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Persona                                   | Need                                                                    | Pain Point Today                                                        |
| --- | ----------------------------------------- | ----------------------------------------------------------------------- | ----------------------------------------------------------------------- |
| 1   | The maintainer of four repositories       | Put every specification in order with one command                       | Rows moved by hand, file by file                                        |
| 2   | The agent authoring a specification       | Clear ordering findings without rewriting a row it could damage         | An agent moving rows re-types them, and a re-typed cell drifts          |
| 3   | A CI step or a hook in a consumer's build | A verify mode that fails when a file is out of order and writes nothing | Only the check's warning, which says what but cannot say "would change" |
| 4   | The reviewer of the pull request          | A diff that is visibly a reordering and nothing more                    | A hand reorder can hide an edit                                         |

### Assumptions

| ID  | Assumption                                                                                                                                                                |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The command line is `specht format [--root <dir>] [--check]`, beside `specht`, `init` and `upgrade`; `--root` defaults to the current directory as it does for the check. |
| A-2 | `--check` behaves as `dotnet format --verify-no-changes` does: it reports what would change, writes nothing, and fails.                                                   |
| A-3 | The order applied is `0101-F1`'s for table rows and `0101-F4`'s for frontmatter keys, read from the same manifest and pinned version as the check.                        |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                                                           | Source                                | Status |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------- | ------ |
| B-001 | Given a specification whose § 3 rows are out of ascending id order, this Feature rewrites the file with § 3 in that order.                                                                      | decision 0001; `0101-F1` B-001        | Active |
| B-002 | Given a specification whose § 4 rows are out of ascending id order, this Feature rewrites the file with § 4 in that order.                                                                      | decision 0001; `0101-F1` B-002        | Active |
| B-003 | Given a specification whose § 11 rows are out of ascending id order, this Feature rewrites the file with § 11 in that order.                                                                    | decision 0001; `0101-F1` B-003        | Active |
| B-004 | Given a specification whose § 9 rows do not follow § 3, this Feature rewrites the file with § 9 in the claims' ascending order.                                                                 | decision 0001; `0101-F1` B-004        | Active |
| B-005 | Given a Feature specification or an epic file whose frontmatter keys are out of the declared order, this Feature rewrites the file with the keys in that order.                                 | decision 0001; `0101-F4` B-001, B-002 | Active |
| B-006 | Given a table this Feature reordered, the table's lines afterwards are its lines before, each exactly once, in a new order.                                                                     | decision 0001; C-1                    | Active |
| B-007 | Given frontmatter this Feature reordered, its lines afterwards are its lines before, each exactly once, in a new order.                                                                         | decision 0001; C-1                    | Active |
| B-008 | Given a file this Feature rewrote, every line outside the reordered tables and frontmatter is byte-identical to before.                                                                         | decision 0001; C-1                    | Active |
| B-009 | Given a tree with no `0101-F1` or `0101-F4` finding, this Feature changes no file.                                                                                                              | decision 0001                         | Active |
| B-010 | Given any run, no file under the root is created or deleted, and the only files modified are those this Feature reordered.                                                                      | decision 0001; brief § 9; C-2         | Active |
| B-011 | Given a claim sequence with a gap, this Feature leaves the same ids in every table, so no id is renumbered and no row is inserted.                                                              | decision 0001; C-1                    | Active |
| B-012 | Given a `0101-F3` finding, this Feature leaves that heading and those cells as they were, and the check reports the finding afterwards.                                                         | decision 0001; `0101-F3` C-1          | Active |
| B-013 | Given ids out of order in a bulleted list or a sentence, this Feature leaves that text as it was.                                                                                               | `0101-F1` decision 0002               | Active |
| B-014 | Given a run that rewrote one or more files, this Feature prints each rewritten file and exits `0`.                                                                                              | A-2                                   | Active |
| B-015 | Given `--check` and a tree this Feature would change, this Feature writes nothing, prints each file it would change and exits `1`.                                                              | A-2; decision 0001                    | Active |
| B-016 | Given `--check` and a tree this Feature would not change, this Feature writes nothing and exits `0`.                                                                                            | A-2                                   | Active |
| B-017 | Given any output on stdout or stderr, every path this Feature derives is relative to the root with `/` separators, and the only path that may be absolute is a typed `--root`, echoed as given. | brief § 9; C-5                        | Active |
| B-018 | Given this Feature has run, the check reports no `0101-F1` or `0101-F4` finding on the tree.                                                                                                    | C-4                                   | Active |
| B-019 | Given this Feature has run once, a second run changes no file.                                                                                                                                  | C-4                                   | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-08 -->

| ID  | Constraint                                                                                                                                     | Rules Out                                                                                                                                        |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| C-1 | The command moves whole table rows and whole frontmatter keys; it never edits a cell, a heading, a value or prose, and never renumbers an id.  | Renumbering to close a gap; trimming or re-padding a cell; re-quoting a value; retitling a heading; inserting or deleting a row (decision 0001). |
| C-2 | The command writes only the files it reorders, in place, under the root.                                                                       | A backup or temporary file left in the tree; a new file; a write to `.spec/schema/` or `.spec/templates/`; a write outside the root.             |
| C-3 | The check stays read-only; reordering is this command's alone.                                                                                 | A `--fix` on the check; the check calling this command; any write from a check run (`0001-F2` B-012).                                            |
| C-4 | The order applied is the one the check's rules declare, read through the same engine.                                                          | A second ordering implementation in the command that can disagree with `0101-F1` or `0101-F4`.                                                   |
| C-5 | Deterministic and offline: the result is a function of the tree, the manifest and the pinned version, and every derived path is root-relative. | A network call; a clock, locale or machine name in the output; an absolute derived path (brief § 9).                                             |
| C-6 | A command parses, calls the engine and folds the result into an exit code; nothing else lives in the command (`0001-F2` C-4).                  | Row or key reordering inside `Features/Format/`.                                                                                                 |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Item                                                              | Exclusion Reason                                                                      |
| --- | ----------------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| 1   | Fixing a heading or a table's form                                | `0101-F3` is report-only (decision 0001; B-012).                                      |
| 2   | Restoring a missing id or renumbering one                         | Ids are never renumbered; a missing row is restored by its author (`0101-F2`; B-011). |
| 3   | Reordering ids in prose                                           | `0101-F1` decision 0002; B-013.                                                       |
| 4   | General Markdown formatting - padding, alignment, wrapping        | The repository's Markdown formatter.                                                  |
| 5   | Items, decision records, lessons and ADRs                         | No rule of epic `0101` orders them (`0101-F4` OQ-3).                                  |
| 6   | `upgrade` rewriting a document, or migrating one between versions | Still out (brief § 2, § 6; `0001-F7` C-2).                                            |
| 7   | A hook or a CI step that runs this command                        | Each consumer's build; this repository's build is epic `0055`.                        |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-08 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-08 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-08 -->

Pending: owned by `test-writer`, written after agreement.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-08 -->

| Claim ID | Scenario                                         | Test    | Status  |
| -------- | ------------------------------------------------ | ------- | ------- |
| B-001    | Claims are put in id order                       | Missing | Missing |
| B-002    | Constraints are put in id order                  | Missing | Missing |
| B-003    | Open questions are put in id order               | Missing | Missing |
| B-004    | The matrix is put in the claims' order           | Missing | Missing |
| B-005    | Frontmatter keys are put in the declared order   | Missing | Missing |
| B-006    | A reordered table holds the same lines           | Missing | Missing |
| B-007    | Reordered frontmatter holds the same lines       | Missing | Missing |
| B-008    | Nothing outside the moved rows and keys changes  | Missing | Missing |
| B-009    | A tree in order is left untouched                | Missing | Missing |
| B-010    | Format writes only the files it reorders         | Missing | Missing |
| B-011    | Format never renumbers an id or fills a gap      | Missing | Missing |
| B-012    | Form findings are left for their author          | Missing | Missing |
| B-013    | Ids in prose are left as written                 | Missing | Missing |
| B-014    | A run that rewrote files lists them and succeeds | Missing | Missing |
| B-015    | Verify mode fails on a tree it would change      | Missing | Missing |
| B-016    | Verify mode passes a tree in order               | Missing | Missing |
| B-017    | No derived path in the output is absolute        | Missing | Missing |
| B-018    | The check finds nothing to reorder after format  | Missing | Missing |
| B-019    | A second run changes nothing                     | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-08 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                           | Blocks         | Resolution |
| ---- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------- | ---------- |
| OQ-1 | How are frontmatter keys moved? A text-level move of each key's lines keeps quotes, comments and flow style byte for byte (B-007). Rewriting through YamlDotNet's representation model orders by the parsed tree but re-emits every value, which can change quoting, drop comments and reflow arrays. Which is it, and is a comment attached to the key below it or the key above? | B-005, B-007   | Open       |
| OQ-2 | What does the command do with a tree the check would refuse: a missing root or manifest (the check exits `2`), a rejected manifest (`3`), a document whose frontmatter does not parse? Exit as the check does, or skip the file and go on?                                                                                                                                         | —              | Open       |
| OQ-3 | Does the command honour the manifest's rule settings? If `0101-F1`'s or `0101-F4`'s rule is disabled, or the manifest pins version 1, does it skip that reordering, or reorder regardless?                                                                                                                                                                                         | B-001 to B-005 | Open       |
| OQ-4 | What does the command do with a table it cannot fully order: a row whose first cell is empty or not exactly one id (`0101-F3`), a duplicate id (`SPEC030`), or a § 9 row naming no claim (`SPEC031`)? Leave the whole table as it is, order the rest around it, or move it to the end?                                                                                             | B-001 to B-004 | Open       |

## 12. Sign-off

<!-- last written by: spec-author, 2026-10-08 -->

| Section | Status | Reviewer      | Note                   |
| ------- | ------ | ------------- | ---------------------- |
| 1-5     | 🟡     | spec-reviewer | Awaiting first review. |

## Tasks

None yet. Cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
