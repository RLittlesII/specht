---
title: "Specification: The manifest carries the roles"
description: "Every literal the engine hardcodes about a repository's model - section roles, table headers, markers, frontmatter keys, identity and edge forms, id grammars, schema file names, per-rule severity and disable - is read from the manifest, so another repository brings its own schema and hooked's run is unchanged"
type: feature
id: "F5"
epic: "0001"
spec_status: draft
status: needs-decomposition
priority: high
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Specification governance"
author: "spec-author"
milestone: null
children: []
depends_on: ["F1"]
blocks: ["F6", "F7"]
spikes: []
created: "2026-10-07"
updated: "2026-10-07"
github_issue: null
synced_at: null
---

# Specification: The manifest carries the roles

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-07 -->

Half of the model's contract is not in the schema files: which section holds the claims and which the matrix, what `Missing`, 🟡 and 🔴 mean, which frontmatter key is the parent and which the edge, how an item's number is read. Those are string literals in the engine, invisible to whoever is held to them and unchangeable without forking it. This Feature removes that failure state: every such literal is read from the manifest the engine already loads, a rule reads a role, and the default manifest reproduces `hooked`'s report to the byte, so a repository brings its own schema and `hooked` notices nothing.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Persona                                      | Need                                                                                                  | Pain Point Today                                                                                 |
| --- | -------------------------------------------- | ----------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| 1   | The maintainer of a sibling repository       | Rename a section, a marker or a key in the manifest and have the same rules follow                     | Each is a `const string` in a rule; renaming means a fork                                         |
| 2   | Transporter, a tree the schema did not write | Lower or disable the rules its pre-schema tree cannot yet pass, and promote them as the tree is repaired | All-or-nothing: every rule is an error                                                           |
| 3   | The agent reading a violation                | A message that spells the key, the section and the layout as the manifest it is editing does           | The message names `hooked`'s literal                                                              |
| 4   | `0001-F6`                                    | The manifest as the one place a layout, an exclusion or a file shape is declared                        | N/A (internal dependency)                                                                        |
| 5   | `0001-F7`                                    | A manifest with a shape a version can be pinned on                                                      | N/A (internal dependency)                                                                        |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                           |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The manifest keeps its file name and place, `.spec/schema/spec-structure.schema.json`; it gains keys, in the same change that teaches the engine to read them, and this repository's live copy is amended in that change. |
| A-2 | Each literal moves one rule at a time, each move with a test that the default manifest reproduces the baseline report (README § 8 step 5). That is a delivery rule and lives in the skills; here it is B-016.           |
| A-3 | The exact key names in the manifest for roles, markers, keys and rule settings are the `implementer`'s to propose in § 7 and the spec-author's to ratify as a decision record. See OQ-1.                             |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-07 -->

| ID    | Claim                                                                                                                                                                                                                                         | Source                           | Status |
| ----- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------- | ------ |
| B-001 | Given a manifest, the rules that read a section do so by role - claims, matrix, sign-off - each mapped in the manifest to a title from `sections`, so renaming the claims section in the manifest moves `SPEC030`, `SPEC031` and `SPEC021` with it. | README § 5                  | Active |
| B-002 | Given a manifest, the table headers `SPEC013` requires are read per role from it, so a renamed header column in the manifest is what the rule expects.                                                                                         | README § 5                       | Active |
| B-003 | Given a manifest, `SPEC060` and `SPEC061` read the missing-test cell value, the draft and blocked sign-off markers and the approved status value from it, so the default manifest declares `Missing`, 🟡, 🔴 and `approved`.                      | README § 5                       | Active |
| B-004 | Given a manifest, every frontmatter key a rule reads - `id`, `epic`, `type`, `spec_status`, `parent`, `children`, `spikes`, `depends_on`, `blocks` - is named in it, and a rule's message names the key as the manifest spells it.               | README § 5                       | Active |
| B-005 | Given a manifest, the qualified identity form and the two edge forms - local `F2` and qualified `0002/F1` - are declared in it, so `SPEC050`-`SPEC052` and `SPEC043` resolve an edge by the declared forms.                                      | README § 5                       | Active |
| B-006 | Given a manifest, `SPEC043` and `SPEC044` read an item's epic and sequence through named groups of the manifest's task grammar, so a three-digit sequence declared there is read and checked for contiguity.                                    | README § 5                       | Active |
| B-007 | Given a manifest, `SPEC021` finds a scenario's claim tag with the manifest's claim grammar and no other expression, so a four-digit claim number declared there resolves between § 3 and the tags.                                               | README § 5                       | Active |
| B-008 | Given a manifest, `SPEC001`-`SPEC004` load each frontmatter schema by the file name the manifest gives for the Feature, item and epic kinds.                                                                                                   | README § 5                       | Active |
| B-009 | Given a rule message or a summary line names a layout, a section, a key or the manifest's own path, it names the manifest's value.                                                                                                             | README § 5                       | Active |
| B-010 | Given a manifest setting a rule id to warning severity, this Feature reports that rule's violations at warning severity, so they are printed, counted, and fail the run only under `--strict`.                                                  | README § 4; decision `0001-F2` 0001 | Active |
| B-011 | Given a manifest disabling a rule id, this Feature reports nothing for that rule and the count of rules evaluated excludes it.                                                                                                                 | README § 4; decision `0001-F2` 0001 | Active |
| B-012 | Given a manifest with a key the engine does not know, this Feature rejects it as invalid, naming the key.                                                                                                                                      | README § 5                       | Active |
| B-013 | Given a manifest naming a rule id outside the pinned version's vocabulary, this Feature rejects it as invalid, naming the id.                                                                                                                  | README § 5; C-3                  | Active |
| B-014 | Given a manifest whose grammar does not compile as a regular expression, this Feature rejects it as invalid, naming the grammar.                                                                                                              | README § 5                       | Active |
| B-015 | Given a manifest whose role names a title absent from `sections`, this Feature rejects it as invalid, naming the role.                                                                                                                        | README § 5                       | Active |
| B-016 | Given the default manifest - the one `init` writes - and `hooked`'s tree at the baseline commit, this Feature's report equals the baseline report `0001-F1` B-004 names.                                                                      | Should-7; README § 8 step 5      | Active |
| B-017 | Given a manifest, nothing in it adds a rule: the vocabulary is the pinned version's, whatever the manifest says.                                                                                                                               | decision `0001-F2` 0001; C-3     | Active |
| B-018 | Given an invalid manifest, this Feature rejects it before any rule runs, so no violation is reported beside the rejection.                                                                                                                     | C-5                              | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-07 -->

| ID   | Constraint                                                                                                                                       | Rules Out                                                                                                             |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------- |
| C-1  | A rule reads a section, a marker, a key name or a path through a manifest role; no such literal lives in a rule.                                | A `private const string` section title or marker; `"epics"`, `".spec"` or `"epic.md"` in a rule; a message that spells a key the manifest could rename. |
| C-2  | Each identifier has one grammar, in the manifest, and every reader uses it.                                                                      | A second regular expression for a claim, item, Feature or epic id anywhere in C#; a prefix read by character count.    |
| C-3  | The rule vocabulary is the pinned version's `SPEC###` ids; a manifest may disable a rule or lower its severity, and may not add one.               | Assembly loading; a rule type named in the manifest; a rule declared as data.                                          |
| C-4  | The default manifest equals `hooked`'s behaviour: the baseline report is reproduced after every literal moves.                                   | A default that differs from `hooked`'s literal "because it is better".                                                  |
| C-5  | The manifest is validated whole before any rule runs.                                                                                            | A half-run that reports violations and then exits `3`; a lazy read that fails inside a rule.                           |
| C-6  | The manifest is always read from disk at `<root>/.spec/schema/spec-structure.schema.json`.                                                        | An embedded manifest as a fallback; a manifest found anywhere else.                                                    |
| C-7  | A manifest key names where a rule looks and what a marker says; what the rule asserts is the engine's.                                           | A key that changes a rule's logic - a different contiguity rule, a different symmetry rule.                             |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Item                                                                           | Exclusion Reason                                                                                      |
| --- | ------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------- |
| 1   | A rule plugin model                                                            | Rejected in decision `0001-F2` 0001; C-3.                                                              |
| 2   | Layouts, exclusions, item, epic and companion file shapes                       | Also manifest keys, but read by discovery: `0001-F6`.                                                  |
| 3   | `schemaVersion` and the schema source                                           | `0001-F7`.                                                                                             |
| 4   | Renaming the manifest file                                                      | A-1; every skill and hook names the current name, and nothing here needs the rename.                   |
| 5   | A manifest key that changes a rule's logic                                      | C-7; a data-driven rule body is the plugin model by another route.                                      |
| 6   | Moving the literals in one change                                               | A-2; one rule at a time, each with its baseline test.                                                   |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `implementer`, written after agreement. The literal each claim moves is cited by file and line in `hooked`'s draft `0008-F3` § 3 (`docs/reference/0008-F3-draft-spec.md`, README § 3).

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `test-writer`.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-07 -->

| Claim ID | Scenario                                                           | Test    | Status  |
| -------- | ------------------------------------------------------------------ | ------- | ------- |
| B-001    | A section is read by role                                          | Missing | Missing |
| B-002    | Table headers are read by role                                     | Missing | Missing |
| B-003    | Markers are read from the manifest                                 | Missing | Missing |
| B-004    | Frontmatter keys are read from the manifest                        | Missing | Missing |
| B-005    | Identity and edge forms are read from the manifest                 | Missing | Missing |
| B-006    | Item ids follow the task grammar                                   | Missing | Missing |
| B-007    | Claim tags follow the claim grammar                                | Missing | Missing |
| B-008    | Frontmatter schemas are loaded by the manifest's file names        | Missing | Missing |
| B-009    | Messages name the manifest's values                                | Missing | Missing |
| B-010    | A rule's severity is lowered from the manifest                     | Missing | Missing |
| B-011    | A rule is disabled from the manifest                               | Missing | Missing |
| B-012    | An unknown key is invalid                                          | Missing | Missing |
| B-013    | An unknown rule id is invalid                                      | Missing | Missing |
| B-014    | A grammar that does not compile is invalid                         | Missing | Missing |
| B-015    | A role naming an absent section is invalid                         | Missing | Missing |
| B-016    | The default manifest reproduces the baseline                       | Missing | Missing |
| B-017    | A rule cannot be added from the manifest                           | Missing | Missing |
| B-018    | An invalid manifest stops the run before any rule                  | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-07 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-07 -->

| ID   | Question                                                                                                                                                                                           | Blocks        | Resolution |
| ---- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------- | ---------- |
| OQ-1 | The manifest's key names for roles, markers, frontmatter keys, forms and rule settings. A consumer edits them, so they are a public surface; the `implementer` proposes them in § 7 and a decision record ratifies them. | B-001 to B-011 | Open  |

## 12. Sign-off

<!-- last written by: spec-author, 2026-10-07 -->

| Section | Status | Reviewer      | Note                  |
| ------- | ------ | ------------- | --------------------- |
| 1-5     | 🟡     | spec-reviewer | Awaiting first review |

## Tasks

None yet. Cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
