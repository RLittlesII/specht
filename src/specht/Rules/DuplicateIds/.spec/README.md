---
title: "Specification: In-specification id uniqueness"
description: "Rules of the major schema version that adds epic 0101's rules, reporting, as errors by default, a constraint id declared twice in a specification's constraints table and an open-question id declared twice in its open-questions table"
type: feature
id: "F6"
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
depends_on: ["0001/F5", "0001/F7"]
blocks: []
spikes: []
created: "2026-10-09"
updated: "2026-10-09"
github_issue: null
synced_at: null
---

# Specification: In-specification id uniqueness

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-09 (after review round 1: the versions named by `0.1.0` and by what the later one adds) -->

A constraint or open-question id is a citation target: `0001-F5 C-12` must name one row. Two branches cut from the same trunk each take the next free number, and git merges both rows without a conflict ([lesson 0005](../../../../../.spec/lessons/0005-an-id-is-not-reserved-until-it-merges.md)); the check says nothing, because schema version `0.1.0` reads a claim id for uniqueness (`SPEC030`) and no constraint or open-question id at all. Every citation of the doubled id then names two rows, and only a reader finds it. This Feature removes that failure state: from the major schema version that adds these rules (OQ-4), the check reports, as an error, each constraint id declared twice in § 4 and each open-question id declared twice in § 11, with both rows located.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-09 (after review round 1: A-4 amended) -->

| #   | Persona                                      | Need                                                                        | Pain Point Today                                                        |
| --- | -------------------------------------------- | --------------------------------------------------------------------------- | ----------------------------------------------------------------------- |
| 1   | Whoever follows a citation such as `C-3`     | The citation names exactly one row                                          | A doubled id passes the check, and the citation names two rows          |
| 2   | The agent merging two branches               | Be told which id is declared twice and where both rows are, from the report | Two rows sharing an id merge cleanly; the clash is found by reading     |
| 3   | The reviewer of a specification pull request | Not carry id uniqueness in their head                                       | Review is the only thing that enforces it for constraints and questions |
| 4   | The maintainer of four repositories          | Lower or switch off the rule per repository                                 | A tree written before the rule may already carry a doubled id           |

### Assumptions

| ID  | Assumption                                                                                                                                                                                   |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | An error is printed, counted and fails the run; a manifest's rule settings lower a rule to a warning or disable it (`0001-F5` B-010, B-011).                                                 |
| A-2 | § 4 and § 11 are found by their role in the manifest once those roles are declared (OQ-2); this specification names them by their default titles.                                            |
| A-3 | An id is read through the manifest's grammar for its table, `constraint` or `openQuestion`, as `0001-F5` C-2 requires of every rule.                                                         |
| A-4 | The major schema version that adds these rules (OQ-4) has not been published, so its rule vocabulary can still take them; a version freezes at its first publish (`0001-F7` decision 0002).  |
| A-5 | A first cell that is empty, or is not exactly one id of its table's grammar, is reported by `0101-F3` (B-004, B-005), so this Feature can leave such a row unjudged without it going unseen. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-09 (after review round 1: B-003 and B-012 to B-014 amended, B-015 added, decision 0003) -->

| ID    | Claim                                                                                                                                                            | Source                         | Status |
| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------ | ------ |
| B-001 | Given a § 4 in which two rows' first cells are the same constraint id, this Feature reports that constraint id as declared twice.                                | decision 0002; C-11            | Active |
| B-002 | Given a § 11 in which two rows' first cells are the same open-question id, this Feature reports that open-question id as declared twice.                         | decision 0002; C-11            | Active |
| B-003 | Given an id declared on two or more rows of one table, this Feature reports one finding for each row after the first, on that row's line.                        | `SPEC030`; Must-5              | Active |
| B-004 | Given a finding of this Feature, its message names the id and the line of the first row that declares it.                                                        | `SPEC030`; Must-5              | Active |
| B-005 | Given a row marked withdrawn and a second row carrying the same id, this Feature reports the second row as it does any other duplicate.                          | AGENTS.md § "Stable IDs"; C-3  | Active |
| B-006 | Given an id declared once in a first cell and cited again in another cell or in prose, this Feature reports nothing.                                             | C-2                            | Active |
| B-007 | Given two § 4 rows whose first cells are the same text and not a constraint id, such as a bare `3`, this Feature reports nothing, leaving the rows to `0101-F3`. | A-5; C-2                       | Active |
| B-008 | Given a claim id declared twice in § 3, this Feature reports nothing, leaving the one finding to `SPEC030`.                                                      | decision 0002; C-8             | Active |
| B-009 | Given two specifications that each declare `C-1` once, this Feature reports nothing.                                                                             | C-1                            | Active |
| B-010 | Given a § 4 or a § 11 that holds no table, this Feature reports nothing for that section.                                                                        | C-2                            | Active |
| B-011 | Given a § 4 and a § 11 in which every id is declared once, this Feature reports nothing.                                                                         | decision 0002                  | Active |
| B-012 | Given a manifest that pins a schema version carrying this Feature's rules and sets no severity for them, their violations are errors.                            | decision 0001                  | Active |
| B-013 | Given a manifest that pins a schema version carrying this Feature's rules and disables them, this Feature reports nothing.                                       | decision 0001; `0001-F5` B-011 | Active |
| B-014 | Given a manifest pinning schema version `0.1.0`, this Feature reports nothing.                                                                                   | decision 0002; C-6             | Active |
| B-015 | Given two ids of one table whose text differs only by leading zeros, such as `C-1` and `C-01`, this Feature reports nothing.                                     | decision 0003; C-11            | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-09 (after review round 1: C-6 and C-8 amended, C-11 added, decision 0003) -->

| ID   | Constraint                                                                                                                             | Rules Out                                                                                                                                                       |
| ---- | -------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1  | Uniqueness is judged per table, inside one specification file.                                                                         | Comparing ids across specifications or Features, where `C-1` recurs by design; comparing § 4 against § 11; reading any file but the specification being judged. |
| C-2  | An id is declared only by a row's first cell, and only when that cell is exactly one id matching the manifest's grammar for the table. | A second id grammar in C# (`0001-F5` C-2); counting an id cited in another cell or in prose; a second finding beside `0101-F3` B-004 and B-005 for one row.     |
| C-3  | A row's status or retired marker never exempts it.                                                                                     | A withdrawn id taken again by a new row (AGENTS.md § "Stable IDs"); reading the retired marker to decide a duplicate (`0101-F2` OQ-1).                          |
| C-4  | A finding never proposes a replacement id.                                                                                             | "Renumber to `C-7`" in a message or an `expected`: the tool cannot know which row is already cited, or which numbers an unmerged branch holds (lesson 0005).    |
| C-5  | The rules report; they never rewrite a file.                                                                                           | A fix applied by the check; `format` renumbering or removing a row (`0101-F5` decision 0001).                                                                   |
| C-6  | The rules belong to the major schema version that adds epic `0101`'s rules (OQ-4) and to every later one.                              | Reporting under a manifest pinning `0.1.0`; adding the rules to `0.1.0`, or in a minor or a patch (`0001-F7` C-11; `0001-F1` C-5).                              |
| C-7  | The default severity is error, declared where `0101-F1` C-6 puts a rule's default; `0001-F5`'s rule settings override it.              | Warning by default (decision 0001); a switch for these rules outside the manifest's rule settings.                                                              |
| C-8  | Claim-id uniqueness stays `SPEC030`; this Feature adds no finding on § 3 or § 9.                                                       | Two findings for one doubled claim id; moving `SPEC030`'s duplicate half into a rule of a later schema version.                                                 |
| C-9  | The rules read the one tree the check was given.                                                                                       | A base, diff or pull-request input; a network call; any comparison against another revision (decision 0002).                                                    |
| C-10 | § 4 and § 11 are read by manifest role, and this Feature declares no role.                                                             | A section title literal in a rule; a second declaration of the roles `0101-F1`, `0101-F2` and `0101-F3` read the same sections by (OQ-2).                       |
| C-11 | Two ids are the same only when their text is the same, as `SPEC030` compares claim ids.                                                | Comparing by number, so that `C-1` and `C-01` are one id declared twice; dropping or padding zeros before comparing (decision 0003).                            |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-09 (after review round 1: rows 5 and 6 amended, row 13 added) -->

| #   | Item                                                                        | Exclusion Reason                                                                                                          |
| --- | --------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------- |
| 1   | A claim id declared twice in § 3, and § 9's one row per claim               | `SPEC030`, `SPEC031` (C-8; B-008).                                                                                        |
| 2   | Two files sharing a number in one `decisions/`, `adr/` or `lessons/` folder | A later Feature, proposed `0101-F7` and not yet specified: it reads file names, not a parsed specification.               |
| 3   | Two work items sharing an id, and `.issue/.sequence` disagreeing with them  | A later Feature, proposed `0101-F8` and not yet specified.                                                                |
| 4   | The same id in two specifications                                           | Not a duplicate: a constraint or open-question id is scoped to its Feature (C-1; B-009).                                  |
| 5   | An id a pull request adds that its base already holds                       | Seen only in the merge tree; `specht` takes one tree (C-9), and the second run is `0055-F2`'s to specify (decision 0002). |
| 6   | A missing id, and the order of rows                                         | `0101-F2`, `0101-F1`; whether a doubled id is also an ordering finding is OQ-5.                                           |
| 7   | A first cell that is empty, or not exactly one id                           | `0101-F3` B-004, B-005 (C-2; B-007).                                                                                      |
| 8   | Duplicate assumption ids, § 2 need numbers, § 5 row numbers and § 12 rows   | Not in the decided scope (decision 0002).                                                                                 |
| 9   | Choosing the replacement id, and renumbering                                | The author's, by the delivery rule for a clash; never the tool's (C-4, C-5).                                              |
| 10  | Per-rule severity and disable                                               | `0001-F5` B-010, B-011; this Feature sets a default (C-7).                                                                |
| 11  | Declaring the § 4 and § 11 section roles                                    | OQ-2 (C-10).                                                                                                              |
| 12  | The rule ids                                                                | OQ-1; `0101-F1` OQ-1.                                                                                                     |
| 13  | Treating `C-1` and `C-01` as one id                                         | Not a duplicate: ids are compared by text (decision 0003; C-11; B-015).                                                   |

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

<!-- last written by: spec-author, 2026-10-09 (after review round 1: B-003 and B-014 titles follow their scenarios, B-015 row added; test-writer owns the Test cells) -->

| Claim ID | Scenario                                                                          | Test    | Status  |
| -------- | --------------------------------------------------------------------------------- | ------- | ------- |
| B-001    | A constraint id declared twice is reported                                        | Missing | Missing |
| B-002    | An open-question id declared twice is reported                                    | Missing | Missing |
| B-003    | Every row after the first is reported; The second of two rows carries the finding | Missing | Missing |
| B-004    | The finding names the id and where it was first declared                          | Missing | Missing |
| B-005    | A withdrawn id taken again is a duplicate                                         | Missing | Missing |
| B-006    | An id cited outside a first cell is not a declaration                             | Missing | Missing |
| B-007    | Repeated first cells that are not ids are left to the form rule                   | Missing | Missing |
| B-008    | A claim id declared twice is left to the claim rule                               | Missing | Missing |
| B-009    | The same id in two specifications is not a duplicate                              | Missing | Missing |
| B-010    | A section with no table reports nothing                                           | Missing | Missing |
| B-011    | Ids each declared once report nothing                                             | Missing | Missing |
| B-012    | Duplicate-id findings are errors by default                                       | Missing | Missing |
| B-013    | Disabled duplicate-id rules report nothing                                        | Missing | Missing |
| B-014    | A repository pinned to 0.1.0 is not checked for duplicate ids                     | Missing | Missing |
| B-015    | Ids that differ only by leading zeros are two ids                                 | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-09 (after review round 1) -->

- 2026-10-09, after review round 1, before any of it merged: claims, constraints and scenarios name the existing schema version as `0.1.0` and the later one by what it adds, its number being OQ-4 (`0001-F7` decision 0005, OQ-18); B-003 covers two rows as well as three. The owner answered OQ-3: ids are compared by text (decision 0003; C-11; B-015). OQ-4 to OQ-6 raised. No id changed.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-09 (after review round 1: OQ-3 resolved, decision 0003; OQ-2 and OQ-3 Blocks extended; OQ-4 to OQ-6 raised) -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    | Blocks                              | Resolution                                                                                                                                              |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which rule ids does this Feature take, and is it one id for both tables or one each? `0101-F1` OQ-1 is open and proposes a `SPEC08x` block for `0101-F1` to `0101-F4`. The proposal here, not put to the owner, is a `SPEC09x` block for the uniqueness Features: `SPEC090` for a constraint id and `SPEC091` for an open-question id, so a repository can lower or disable each alone.                                                                                                                                                     | B-012, B-013                        | Open                                                                                                                                                    |
| OQ-2 | Which Feature declares the section roles for § 4 and § 11? The manifest's `roles` holds `claims`, `matrix` and `signOff` (`0001-F5` B-001) and no role for constraints or open questions, though both grammars exist; `0101-F1`, `0101-F2` and `0101-F3` read the same two sections. The proposed default is a `0001-F5` delta adding both roles once, for all four Features; `0001-F5` OQ-10 is the adjacent question.                                                                                                                     | B-001 to B-007, B-010, B-011, B-015 | Open                                                                                                                                                    |
| OQ-3 | Are two ids the same when their text is the same, or when their number is? The constraint and open-question grammars accept leading zeros, so `C-1` and `C-01` can both be written. By text, as `SPEC030` compares claim ids, they are two ids and `0101-F1` C-1 gives them one position; by number they are one id declared twice. The proposed default is by text, mirroring `SPEC030`.                                                                                                                                                   | B-001 to B-005                      | Resolved 2026-10-09 by the repository owner: by text. `C-1` and `C-01` are different ids, as `SPEC030` compares claim ids (decision 0003; C-11; B-015). |
| OQ-4 | Which schema version number do this Feature's rules take, and is it the one `0101-F1` to `0101-F5` take? A new `SPEC###` is a major (`0001-F7` C-11), and the number epic `0101`'s rules take is `0001-F7` OQ-18, open. This specification names that version by what it adds, so no claim's wording waits on the number; every scenario but B-014's pins it. The proposed default, not put to the owner, is the same major as the rest of the epic. Owner not asked.                                                                       | B-001 to B-013, B-015               | Open                                                                                                                                                    |
| OQ-5 | Is an id declared twice in § 4 or § 11 also an ordering finding of `0101-F1`? `0101-F1` B-002 and B-003 report rows "not in ascending order" and do not say whether two equal ids are in order; if they are not, one doubled id is an error here and a warning there. C-2 rules out a second finding beside `0101-F3` and is silent on `0101-F1`. The proposed default, not put to the owner, is that equal ids are in order, so the doubled id is this Feature's finding alone; that is a `0101-F1` delta, not made here. Owner not asked. | C-2; § 5 row 6                      | Open                                                                                                                                                    |
| OQ-6 | What marks a § 4 or § 11 row withdrawn? This Feature never reads the marker (C-3), so any answer gives the same finding, but B-005's Given needs a row so marked, and the marker is `0101-F2` OQ-1, open.                                                                                                                                                                                                                                                                                                                                   | B-005                               | Open                                                                                                                                                    |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-09 (round 1) -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| ------- | ------ | ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🔴     | spec-reviewer | Round 1 (2026-10-09) - blocked, reviewing `6c763f4`. Blocking: B-012 to B-014, C-6, A-4, the Background and decision 0002 say "schema version 1" and "schema version 2"; `0001-F7` decision 0005 makes a version `major.minor.patch` from `0.1.0`, B-039 rejects a manifest pinning `1` with exit 3, and the number epic `0101` takes is `0001-F7` OQ-18, owner not asked - item `0128` records "a new major schema version", not 2. Word them by the major that adds the rules and cite OQ-18 in § 11. Non-blocking: B-003 covers three or more rows, so no claim locates the finding for two; `0101-F1` decision 0001 is narrowed with no note on that record; whether a doubled id is also a `0101-F1` ordering finding is unstated (C-2, § 5 row 6); B-010's scenario covers § 11 only; B-008's Then should name `SPEC030`; OQ-2 and OQ-3 block more claims than listed; decision 0001's second reason and its cost are the author's and unlabelled. Holds: B-001 to B-014 each have one § 9 row and one tagged scenario; `0101-F6` and decisions 0001 and 0002 are free on `main` and in #78, #82, #83 and #85; `SPEC090` and `SPEC091` are proposed only; `depends_on` and both `blocks` agree; the decisions match item `0128`. |

## Tasks

None yet. Cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
