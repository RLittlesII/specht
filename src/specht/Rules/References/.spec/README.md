---
title: "Specification: Dangling id references"
description: "Under a schema version carrying its rule, the check warns on each claim, constraint or open-question id a specification cites in its own text that no row of that specification declares, leaving ids qualified with another Feature's key unresolved"
type: feature
id: "F2"
epic: "0228"
spec_status: draft
status: ready
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
blocks: ["F1"]
spikes: []
created: "2026-10-28"
updated: "2026-10-28"
github_issue: null
synced_at: null
---

# Specification: Dangling id references

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-28 -->

A consumer ran ten review rounds on one specification and the check reported 0 errors every round; many rounds came from cross-references gone stale, a claim, constraint or question cited by an id no row declares. Nothing resolves a citation today: this tree's specifications cite unqualified ids 8078 times, and 29 of those name no row of the citing specification (2026-10-28), each found only when a reviewer follows it. This Feature removes that failure state for citations within one specification: under a schema version carrying its rule, the check warns at the citing line on each unqualified claim, constraint or open-question id that no row of the same specification declares.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-28 -->

| #   | Persona                                  | Need                                                               | Pain Point Today                                 |
| --- | ---------------------------------------- | ------------------------------------------------------------------ | ------------------------------------------------ |
| 1   | The reviewer following a citation        | Every unqualified id the specification cites names one of its rows | Opens § 3, § 4 or § 11 for each id, or trusts it |
| 2   | The spec-author renaming or merging rows | The citations the change left behind, reported at their lines      | Finds them in the next review round              |
| 3   | `0228-F1`                                | One reading of what a cell cites, shared with the Blocks rule      | N/A (internal dependency)                        |

### Assumptions

| ID  | Assumption                                                                                                                                                            |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | An id is declared only by a row's first cell holding exactly one id that matches the manifest's grammar for that table (`0101-F6` C-2).                               |
| A-2 | A warning is printed and counted and fails the run only under `--strict`, and a rule set off reports nothing (`0001-F5` B-010, B-011).                                |
| A-3 | A Feature key is the epic and Feature id joined by `-`, such as `0001-F7`, written bare or as inline code, as the feature template tells an author to cite a sibling. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-28 -->

| ID    | Claim                                                                                                                                                                                                                    | Source                                                                                                    | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------- | ------ |
| B-001 | Given an unqualified claim id cited in a specification's text that no § 3 row of that specification declares, this Feature reports the dangling-citation rule at the citing line, its identifier the claim id.           | owner, 2026-10-28; decision 0001 call 1; epic `0228` R5                                                   | Active |
| B-002 | Given an unqualified constraint id cited in a specification's text that no § 4 row of that specification declares, this Feature reports the dangling-citation rule at the citing line, its identifier the constraint id. | owner, 2026-10-28; decision 0001 call 1; epic `0228` R5                                                   | Active |
| B-003 | Given an unqualified open-question id cited in a specification's text that no § 11 row of that specification declares, this Feature reports the dangling-citation rule at the citing line, its identifier the id.        | owner, 2026-10-28; decision 0001 call 1; epic `0228` R5                                                   | Active |
| B-004 | Given an undeclared id cited in prose, a list item, a table cell or an HTML comment of the specification's body, this Feature reports it.                                                                                | owner, 2026-10-28; spec-author's reading for HTML comments: they hold 234 citations here, none dangling   | Active |
| B-005 | Given an id inside an inline-code span or a fenced code block, this Feature does not read it as a citation.                                                                                                              | owner, 2026-10-28 (asked); spec-author's reading: an example id is written in code, as in `0101-F2` B-001 | Active |
| B-006 | Given an id in the specification's frontmatter, this Feature does not read it as a citation.                                                                                                                             | spec-author's reading: frontmatter edges are `SPEC050` to `SPEC052`'s                                     | Active |
| B-007 | Given the first cell of a § 3, § 4 or § 11 row, this Feature reads it as a declaration and not as a citation.                                                                                                            | A-1                                                                                                       | Active |
| B-008 | Given the first cell of a § 9 row, this Feature reports nothing for it, leaving a § 9 row that cites a non-claim to `SPEC031`.                                                                                           | brief § 4 `SPEC031`; one finding per fault, as `0101-F6` C-8                                              | Active |
| B-009 | Given a claim id in a § 11 Blocks cell, this Feature reports nothing for it, leaving it to `0228-F1`'s Blocks rule.                                                                                                      | `0228-F1` B-001; one finding per fault                                                                    | Active |
| B-010 | Given a constraint or open-question id in a § 11 Blocks cell that no row declares, this Feature reports it as it reports any other citation.                                                                             | `0228-F1` B-004                                                                                           | Active |
| B-011 | Given an id that follows a Feature key in the same clause, such as `0001-F7` C-11, this Feature does not resolve it against the citing specification and reports nothing for it.                                         | owner, 2026-10-28 (asked); OQ-3 proposed default; A-3                                                     | Active |
| B-012 | Given a Feature key, its reach ends at the next `;`, sentence-ending `.`, parenthesis, Feature key or table-cell boundary, so in `0001-F5` B-055, B-058; B-002 only B-002 is resolved here.                              | OQ-4 proposed default; spec-author's reading of this tree's citations                                     | Active |
| B-013 | Given a range such as B-014 to B-019, this Feature resolves the two ids written and reads no id between them as cited.                                                                                                   | OQ-4 proposed default                                                                                     | Active |
| B-014 | Given a cited id whose row's Status is the withdrawn or the superseded marker, this Feature reports nothing for it.                                                                                                      | owner, 2026-10-28 (asked); spec-author's reading: the row is declared (AGENTS.md § "Stable IDs")          | Active |
| B-015 | Given the specification's companion `.feature` file, this Feature reads none of it: its claim tags stay `SPEC021`'s, and its steps name example ids by design.                                                           | owner, 2026-10-28 (asked); brief § 4 `SPEC021`; spec-author's reading                                     | Active |
| B-016 | Given a decision record, an ADR or a lesson beside the specification, this Feature reads none of it.                                                                                                                     | owner, 2026-10-28 (asked); epic `0228` § Constraints: no rule reads record content                        | Active |
| B-017 | Given a § 4 whose first cells hold no constraint id, such as a bare ordinal, this Feature resolves no constraint citation in that specification and reports nothing for one.                                             | OQ-6 proposed default; the `specht-conventions` specs reference § "The § 4 constraint-id gap"             | Active |
| B-018 | Given an undeclared id cited on several lines, this Feature reports it once at each line, and once on a line that cites it twice.                                                                                        | spec-author's reading: a finding sits where the fix is made                                               | Active |
| B-019 | Given a specification discovered under the legacy `epics/<epic>/<feature>/spec.md` layout, this Feature judges it as it judges a co-located one.                                                                         | owner, 2026-10-28 (asked); AGENTS.md § "Where a specification lives"; spec-author's reading               | Active |
| B-020 | Given a manifest pinning a schema version that carries this Feature's rule and setting no level for it, the dangling-citation rule reports warnings.                                                                     | epic `0228` R5 (owner, 2026-10-15); decision 0001 call 3                                                  | Active |
| B-021 | Given a manifest pinning schema `0.2.1` or earlier, this Feature's rule does not run, and the report is byte-identical to the one the tool gives that tree without this Feature.                                         | `0001-F7` C-1, C-11; as `0001-F12` B-021                                                                  | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-28 -->

| ID  | Constraint                                                                                                                                               | Rules Out                                                                                                                   |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| C-1 | An unqualified id resolves only to a row the citing specification declares.                                                                              | Resolving it against a sibling specification, an epic or an item; a repository-wide index of ids (epic `0228` candidate C). |
| C-2 | Every id grammar, section and marker the rule compares comes from the manifest (brief § 5; `0001-F5` C-2).                                               | A claim, constraint or question regex, a section title or `"Withdrawn"` written in a rule.                                  |
| C-3 | Resolution is text only, of the one tree the check was given (brief § 9).                                                                                | Git history, a network call, a second revision of the tree, or the time of the run.                                         |
| C-4 | One citation reader serves this Feature and `0228-F1`.                                                                                                   | A second reading of qualifiers, ranges or clauses in the Blocks rule; two rules disagreeing on what one cell cites.         |
| C-5 | The rule reports; it never rewrites a file and never proposes a replacement id (brief § 9).                                                              | Qualifying, deleting or renumbering a citation from the check; "did you mean B-012" in a message or an `expected`.          |
| C-6 | The rule joins the vocabulary at a `0.minor` schema step after `0.2.1`, `0.3.0` at the earliest, and stays in every later version (`0001-F7` C-1, C-11). | Adding it to `0.1.0`, `0.2.0` or `0.2.1`; reporting under those pins; a patch step that adds it.                            |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-28 -->

| #   | Item                                                                                  | Exclusion Reason                                                                                        |
| --- | ------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| 1   | A citation qualified with a Feature key, resolved against that Feature                | Epic `0228` candidate C, item `0307` (B-011; OQ-3).                                                     |
| 2   | Resolving a citation from the command line                                            | Epic `0228` R4, candidate C.                                                                            |
| 3   | A claim made in prose, such as "OQ-3 is Open", held against the row's Resolution      | Declined by the owner, 2026-10-28: it parses prose (decision 0001 call 2).                              |
| 4   | Anything that needs git history                                                       | Declined by the owner, 2026-10-28 (C-3; decision 0001 call 2).                                          |
| 5   | A § 12 review note checked against repository history                                 | Declined by the owner, 2026-10-28: it breaks "same tree, same report" (`0228-F1` decision 0001 call 3). |
| 6   | Whether a Then step is implied by another claim's refusal                             | Declined by the owner, 2026-10-28: semantic, the reviewer's work (`0228-F1` decision 0001 call 3).      |
| 7   | Constraint tracing, Then-step wording lint, term variants, and a warning baseline     | Spikes `0316`, `0315`, `0317` and `0318` (`0228-F1` decision 0001 call 4).                              |
| 8   | Claim ids in a § 11 Blocks cell, and whether an open question survives approval       | `0228-F1` (B-009).                                                                                      |
| 9   | A § 9 row citing a non-claim, and a scenario tag citing one                           | `SPEC031`, `SPEC021` (B-008, B-015).                                                                    |
| 10  | A withdrawn claim a scenario still carries                                            | `SPEC035` (`0001-F12` B-019); a prose citation of a retired id is history and resolves (B-014).         |
| 11  | The row a deleted id left behind                                                      | `0101-F2`, which reports the gap; this Feature reports each citation of the id, at its line.            |
| 12  | Assumption ids, need numbers, § 5 row numbers, ADR, decision, lesson and item numbers | Not in the decided need (owner, 2026-10-28); the record and item forms are epic `0228` R4's.            |
| 13  | Citations in epics, items and records                                                 | Only specifications are read (B-016; epic `0228` § Constraints).                                        |
| 14  | The manifest keys for the constraints and open-questions sections                     | `0001-F5` (OQ-5).                                                                                       |
| 15  | The rule id and the schema version that carries it                                    | OQ-1, OQ-2; `0001-F7`.                                                                                  |
| 16  | What the rule's `expected` carries                                                    | `0001-F3`.                                                                                              |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-28 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-28 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-28 -->

Pending: owned by `test-writer`, written after agreement.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-28 -->

| Claim ID | Scenario                                                              | Test    | Status  |
| -------- | --------------------------------------------------------------------- | ------- | ------- |
| B-001    | A claim cited and never declared is reported                          | Missing | Missing |
| B-002    | A constraint cited and never declared is reported                     | Missing | Missing |
| B-003    | An open question cited and never declared is reported                 | Missing | Missing |
| B-004    | A citation is read wherever the body holds it                         | Missing | Missing |
| B-005    | An id in code is not a citation                                       | Missing | Missing |
| B-006    | An id in frontmatter is not a citation                                | Missing | Missing |
| B-007    | A row's own id is a declaration                                       | Missing | Missing |
| B-008    | A traceability row's claim id is left to its own rule                 | Missing | Missing |
| B-009    | A claim in a Blocks cell is left to the Blocks rule                   | Missing | Missing |
| B-010    | A constraint in a Blocks cell is resolved                             | Missing | Missing |
| B-011    | An id qualified with another Feature is not resolved here             | Missing | Missing |
| B-012    | A Feature key qualifies ids up to the end of its clause               | Missing | Missing |
| B-013    | A range cites its two ends                                            | Missing | Missing |
| B-014    | A citation of a withdrawn claim resolves                              | Missing | Missing |
| B-015    | The companion scenarios are not read                                  | Missing | Missing |
| B-016    | A record beside the specification is not read                         | Missing | Missing |
| B-017    | Constraint citations are not resolved against ordinal constraint rows | Missing | Missing |
| B-018    | An undeclared id is reported once at each line that cites it          | Missing | Missing |
| B-019    | A legacy-layout specification is judged the same                      | Missing | Missing |
| B-020    | The dangling-citation rule is a warning by default                    | Missing | Missing |
| B-021    | Under schema 0.2.1 the dangling-citation rule does not run            | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-28 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-28 -->

| ID   | Question                                                                                                                                          | Blocks              | Resolution                                                                                                                                                                                                                                                                                                                                                                        |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which rule id carries the verdict, and in which group?                                                                                            | B-001 to B-003      | Open. Proposed default: one id for all three kinds, in a citation group `SPEC07x`, where `0001-F1` § 5 row 7 already names `SPEC070` for constraint-citation resolution. The id is the vocabulary's at the version that ships it (item `0169`); none is reserved here. Rejected by the default: one id per kind, which splits one fault three ways.                               |
| OQ-2 | Which schema version carries the rule?                                                                                                            | B-020, B-021        | Open. Proposed default: `0.3.0`, with `0228-F1`'s rules (`0228-F1` OQ-2). Rejected by the default: adding to `0.2.1`, which has shipped (`0001-F7` C-1).                                                                                                                                                                                                                          |
| OQ-3 | Is a citation qualified with another Feature's key, such as `0001-F7` C-11, resolved against that Feature by this rule, or left out of its reach? | B-011               | Open. Proposed default: left out of reach. Epic `0228` candidate C holds the one resolver for qualified keys, shared by a command and a rule, and item `0307` specifies it; resolving them here builds a second resolver before that agreement. Rejected by the default: resolving them here now.                                                                                 |
| OQ-4 | How far does a Feature key reach, and does a range cite the ids between its ends?                                                                 | B-012, B-013        | Open. Proposed default: B-012 and B-013. Measured on this tree, 2026-10-28: with that reach and ranges read by their ends, 8078 unqualified citations, 29 dangling, among them `Manifest B-061` naming a Feature by title and a § 10 note of a constraint dropped before merge. Rejected by the default: a reach to the end of the line, which hides unqualified ids after a `;`. |
| OQ-5 | Which manifest keys name the constraints and open-questions sections?                                                                             | B-002, B-003, B-010 | Open. Proposed default: the `0001-F5` delta `0101-F6` OQ-2 proposes, `roles.constraints` and `roles.openQuestions`, shared with `0228-F1` OQ-4.                                                                                                                                                                                                                                   |
| OQ-6 | In a specification whose § 4 first cells are bare ordinals, are constraint citations resolved by counting rows, reported, or not resolved?        | B-017               | Open. Proposed default: not resolved (B-017). Rejected by the default: counting rows, which a row inserted above shifts silently; reporting every constraint citation, which floods a consumer before it normalizes § 4.                                                                                                                                                          |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-28 -->

| Section | Status | Reviewer      | Note                                                                         |
| ------- | ------ | ------------- | ---------------------------------------------------------------------------- |
| 1-5, 11 | 🟡     | spec-reviewer | Draft awaiting the owner's answers to OQ-1 to OQ-6 and a first review round. |

## Tasks

None cut; the agreement is awaiting review and the owner's answers to § 11.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
