---
title: "Specification: The engine, extracted unchanged"
description: "hooked's SpecGovernance rule engine copied into src/specht and renamed, with the same verdict on the same tree, as the baseline every later Feature is measured against"
type: feature
id: "F1"
epic: "0001"
spec_status: approved
status: ready-for-architecture
priority: critical
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Specification governance"
author: "spec-author"
milestone: null
children: []
depends_on: []
blocks: ["F2", "F5"]
spikes: []
created: "2026-10-07"
updated: "2026-10-08"
github_issue: null
synced_at: null
---

# Specification: The engine, extracted unchanged

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-07 -->

The rule engine exists only inside `hooked`, as a project reference from its build, under a namespace that names that repository. Every other repository on the model copies it or goes without a gate. This Feature removes that failure state by moving the engine into this repository as the library `specht`, with no behaviour change: the same twenty-one rule ids, the same violations in the same order on the same tree. It is the baseline every later Feature proves itself against (brief § 8 step 2).

## 2. User Needs

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Persona                             | Need                                                                              | Pain Point Today                                                                     |
| --- | ----------------------------------- | --------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| 1   | The maintainer of four repositories | The engine moves without its verdict moving, so the extraction is provably a move | Three hand-copied checkers; no way to tell whether a copy still agrees with `hooked` |
| 2   | `hooked`'s Nuke `SpecCheck` target  | The same report from the library it will stop referencing                         | N/A (internal dependency)                                                            |
| 3   | `0001-F2`                           | A library with no entry point to call                                             | N/A (internal dependency)                                                            |
| 4   | The agent authoring a specification | A deterministic oracle: the same tree gives the same answer on any machine        | Only `hooked`'s build runs it, and only there                                        |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                                                                                                             |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The 56 tests extracted with the engine are its behaviour contract; a claim here is one the tests already prove or one the baseline report proves (B-004).                                                                                                                                                                              |
| A-2 | The source is `hooked` `refactor/ai-offering` at `6afe8ab` (brief § 3). The three draft-spec files removed on that branch go to `docs/reference/`, never into `src/specht`.                                                                                                                                                            |
| A-3 | Schema version 1 is the four files under `.spec/schema/` and the four templates under `.spec/templates/` as `hooked` holds them at `6afe8ab`, with only the `$id` URLs re-homed, the optional epic `title` and `description` keys added by `0001-F7` decision 0003, and the manifest keys `0001-F5` and `0001-F6` add (`0001-F7` A-2). |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-07 -->

| ID    | Claim                                                                                                                                                                                                                                               | Source                                                                                                                                                  | Status    |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- | --------- |
| B-001 | Given a tree that breaks each of the twenty-one rules in brief § 4 once, this Feature reports violations under exactly those twenty-one ids, `SPEC001` to `SPEC061`, and under no other id.                                                         | brief § 4                                                                                                                                               | Active    |
| B-002 | Given a specification in the legacy layout and one in the co-located layout, this Feature applies every rule except `SPEC011` (legacy layout only) to both and reports neither for the layout it is in.                                             | brief § 4, § 5; decision 0001                                                                                                                           | Active    |
| B-003 | Given a specification in the co-located layout, this Feature takes its identity from its frontmatter `epic` and `id`, so the same specification moved to another path keeps its identity.                                                           | `SPEC012`; brief § 4                                                                                                                                    | Active    |
| B-004 | Given `hooked`'s tree at `6afe8ab`, this Feature's verdicts - each violation's rule id, severity, file, line, identifier and message, in order (C-9) - equal those in the baseline report committed at `docs/reference/hooked-6afe8ab-report.json`. | Should-7; brief § 8 step 2; C-9; decision 0003                                                                                                          | Active    |
| B-005 | Given the built library, its assembly name and root namespace are `specht` and neither carries `Hooked`.                                                                                                                                            | brief § 6                                                                                                                                               | Active    |
| B-006 | Given the same tree and the same schema files, this Feature's violations and their order are unchanged when the clock, an environment variable, the locale, the machine name or the root's location changes.                                        | brief § 9; C-3; C-4                                                                                                                                     | Active    |
| B-007 | Given any violation, its file is relative to the root with `/` separators.                                                                                                                                                                          | brief § 9; C-4                                                                                                                                          | Active    |
| B-008 | Given a contracted section whose table is a grid table, this Feature reads the section as carrying no table and parses no row from the grid.                                                                                                        | brief § 5; C-6                                                                                                                                          | Active    |
| B-009 | Given the engine as copied, a diff of `src/specht` against `hooked` at `6afe8ab` shows the namespace rename and nothing else.                                                                                                                       | Withdrawn 2026-10-07: a one-time diff against another repository no test can fail; it is now the check C-2 records at the baseline tag (decision 0002). | Withdrawn |
| B-010 | Given a violation, it carries a rule id, a severity, a file, a line, an identifier where the rule has one, and a message.                                                                                                                           | `hooked` report shape                                                                                                                                   | Active    |
| B-011 | Given two specifications declaring the same frontmatter `epic` and `id`, this Feature reports the duplicate identity once.                                                                                                                          | `SPEC012`; brief § 4                                                                                                                                    | Active    |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-07 -->

| ID  | Constraint                                                                                                                                                                                                                                                                                                                                                                                                                                                            | Rules Out                                                                                                                                       |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | The engine is a library with no entry point.                                                                                                                                                                                                                                                                                                                                                                                                                          | `OutputType=Exe` on `src/specht`; a console write anywhere in the engine.                                                                       |
| C-2 | The engine is copied, not regenerated: at the baseline tag (brief § 8 step 2) a diff against `hooked` at `6afe8ab` shows only the namespace rename in each copied `*.cs` file, `AssemblyName` and `RootNamespace` set to `specht` and the `Directory.Build.props` comments dropped in the copied `.csproj`, and the copied `.spec/` removed (brief § 3); the diff is recorded in that tag's commit. `src/specht/.spec/`, this specification, is not part of the copy. | A fresh draft of any rule; a refactor "while we are here"; a renamed type; any other `.csproj` edit, such as a package version or `OutputType`. |
| C-3 | Deterministic and offline: the verdict is a function of the tree and the schema files.                                                                                                                                                                                                                                                                                                                                                                                | A network call; a clock, environment variable, locale or machine name reaching the output.                                                      |
| C-4 | Every path the engine emits is repository-relative with `/` separators.                                                                                                                                                                                                                                                                                                                                                                                               | An absolute path in a violation, a report or a test fixture; an OS separator in output.                                                         |
| C-5 | The rule vocabulary is fixed per schema version.                                                                                                                                                                                                                                                                                                                                                                                                                      | A new `SPEC###` id before `0001-F7`; a rule renamed or renumbered.                                                                              |
| C-6 | Tables are read from Markdig's pipe-table model only.                                                                                                                                                                                                                                                                                                                                                                                                                 | Splitting a line on `\|`; a grid-table extension; a second Markdown parser.                                                                     |
| C-7 | Dependencies are Markdig, YamlDotNet (representation model) and JsonSchema.Net at `hooked`'s pinned versions, in central package management.                                                                                                                                                                                                                                                                                                                          | A version in a `.csproj`; a YAML deserializer that coerces dates; a fourth engine dependency.                                                   |
| C-8 | Schema version 1 reads its manifest and schemas from `<root>/.spec/schema/`.                                                                                                                                                                                                                                                                                                                                                                                          | A second location for the schema set before `0001-F7`.                                                                                          |
| C-9 | From the baseline tag until brief § 8 step 5, no edit changes a verdict: on any tree, each violation's rule id, severity, file, line, identifier and message, and their order, equal the baseline engine's. `0001-F3` may reshape the report around them at step 3.                                                                                                                                                                                                   | A rule fixed, a violation added, dropped, reordered or reworded before `0001-F5` moves the literals into the manifest.                          |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Item                                                                                                                                                     | Exclusion Reason                                                                        |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------- |
| 1   | A command line, exit codes, `--help`                                                                                                                     | `0001-F2`.                                                                              |
| 2   | A new report field (the count of rule ids evaluated, layout counts, the schema source, `expected`), dropping `generatedAtUtc`, and the `--json` document | `0001-F3` (its B-005, B-006, B-007), at brief § 8 step 3; C-9 holds the verdicts still. |
| 3   | Writing anything into a consumer's tree                                                                                                                  | `0001-F4` (`init`) and `0001-F7` (`upgrade`); this Feature never writes.                |
| 4   | Reading a section, marker, key or grammar through a manifest role                                                                                        | `0001-F5`; until then the literals stay where `hooked` has them (C-9).                  |
| 5   | Discovery through git, or a pruned walk                                                                                                                  | `0001-F6`.                                                                              |
| 6   | A second schema version, or a `schemaVersion` key                                                                                                        | `0001-F7`.                                                                              |
| 7   | A new rule, including `SPEC070`                                                                                                                          | C-5; a new rule is a new schema version.                                                |
| 8   | `hooked` replacing its project reference with the tool                                                                                                   | A Feature of `hooked`; this repository only has to give it the same verdicts (B-004).   |
| 9   | Anything Roslyn                                                                                                                                          | `hooked` `0008-F1` and `0008-F2` (brief § 2).                                           |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `implementer`, written after agreement. The design input is brief § 3: the files to copy, by purpose.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `test-writer`. The 56 extracted tests are the starting point (A-1).

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-07 -->

| Claim ID | Scenario                                              | Test    | Status  |
| -------- | ----------------------------------------------------- | ------- | ------- |
| B-001    | Every rule in the vocabulary is applied, and no other | Missing | Missing |
| B-002    | Both layouts are held to the same rules               | Missing | Missing |
| B-003    | Identity comes from the frontmatter, not the path     | Missing | Missing |
| B-004    | The report on hooked's tree is unchanged              | Missing | Missing |
| B-005    | Nothing carries the old repository's name             | Missing | Missing |
| B-006    | Nothing about the machine reaches the verdict         | Missing | Missing |
| B-007    | Every path is relative to the root                    | Missing | Missing |
| B-008    | A grid table is read as no table                      | Missing | Missing |
| B-009    | The copy differs from hooked only by the namespace    | Missing | Missing |
| B-010    | A violation carries the six baseline fields           | Missing | Missing |
| B-011    | One identity at two paths is reported                 | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-07 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                                                                          | Blocks | Resolution                                                                                                                                                                                                                                                                        |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | The version 1 epic frontmatter schema sets `additionalProperties: false` and lists no `title` or `description`, while AGENTS.md § "Documentation structure" requires both on every tracked markdown file. Which gives way, and is that a schema version 2 change? | —      | Moved 2026-10-07 to `0001-F7` OQ-4: it blocks no claim here, and its answer is a schema version 2 change, which `0001-F7` owns. Not answered in this Feature. `0001-F7` OQ-4 was resolved 2026-10-08 (`0001-F7` decision 0003): version 1 accepts both keys on an epic, optional. |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-07 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                              |
| ------- | ------ | ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟢     | spec-reviewer | Approved (round 2). Round-1 blocking findings fixed. Non-blocking, routed: C-2 should name its scope (engine only, or also `test/specht.tests` with its `.csproj` edits); decision 0002 Rejected bullet conflicts with B-004 reading `hooked`'s checkout; B-006 machine name cannot be varied in a test; B-003 cites `SPEC012`; § 5 row 2 calls existing counts new; § 9 B-009 row `Missing` for a withdrawn claim (test-writer). |
| 1-5     | 🟢     | spec-reviewer | Round 3 (2026-10-08) - approved, reviewing `acf3cff`..`f32a31d`. A-3 now includes decision 0003's optional epic `title` and `description` keys, matching `0001-F7` A-2 and decision 0003's Affects; nothing else changed. Non-blocking (spec-author): A-3 still omits the manifest keys `0001-F5` and `0001-F6` add to version 1, which `0001-F7` A-2 composes on top of it.                                                      |
| 1-5     | 🟢     | spec-reviewer | Round 4 (2026-10-08) - approved, reviewing `01f1f73`..`9a379ce`. A-3 now adds the `0001-F5` and `0001-F6` manifest keys, agreeing with `0001-F7` A-2; the round-3 non-blocking finding is fixed.                                                                                                                                                                                                                                  |

## Tasks

Cut 2026-10-08 into [`../.issue/`](../.issue/): `0020` (the Feature), with `0021` to `0024`.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
