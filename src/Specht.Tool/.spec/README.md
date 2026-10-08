---
title: "Specification: Schema versioning"
description: "The manifest pins schemaVersion; the tool embeds every schema version it has ever shipped and validates with the pinned one; specht upgrade moves a repository's schema set and templates to the next version and never touches a document; the schema source is embedded by default and on-disk by configuration"
type: feature
id: "F7"
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
depends_on: ["F4", "F5"]
blocks: []
spikes: []
created: "2026-10-07"
updated: "2026-10-07"
github_issue: null
synced_at: null
---

# Specification: Schema versioning

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-07 -->

A repository's `.spec/schema/` is a copy of `hooked`'s from one day, and nothing says which day, so either every repository moves with `hooked` at once or the copies diverge silently, and a repository that lags cannot catch up on purpose. This Feature removes that failure state: the manifest pins one number, `schemaVersion`; the tool embeds every version it has ever shipped and checks with the pinned one; `specht upgrade` moves the schema set and the templates to the next version and prints what changed. Lagging becomes deliberate, catching up becomes one command, and a document is only ever changed by the agent, from the report.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Persona                                           | Need                                                                                             | Pain Point Today                                                                 |
| --- | ------------------------------------------------- | ------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------- |
| 1   | The maintainer of a lagging repository            | Pin a version and stay there until ready; the newest tool still checks it                         | A fork from one day, with no version to name                                      |
| 2   | The maintainer upgrading a repository             | One command that moves the schema set and templates forward and says what changed                 | Hand-copy, then diff by eye                                                       |
| 3   | The agent migrating documents                     | After an upgrade, the check reports what each document is missing, with what the rule expected    | Nothing says what changed between versions                                        |
| 4   | A maintainer trying a schema edit locally         | Point the check at the on-disk schema files instead of the embedded ones, deliberately            | The on-disk copy is the only copy, so a hand edit is silently authoritative        |
| 5   | `0001-F4`                                         | The embedded version set and the newest version number to write                                   | N/A (internal dependency)                                                         |
| 6   | `0001-F3`                                         | The version and the source to name in the report                                                  | N/A (internal dependency)                                                         |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                 |
| --- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | A manifest with no `schemaVersion` key is version 1, the version that predates the key. `hooked`'s manifest at the baseline commit has no key and must keep checking unchanged (Should-7).                                                   |
| A-2 | Version 1 is the schema set as `0001-F1` A-3 defines it, plus the keys `0001-F5` and `0001-F6` add to the manifest; those keys are version 1 because the default manifest reproduces the baseline and every key has that default.          |
| A-3 | "Prints what changed" is, at least, the version moved from and to and the name of every file rewritten. Whether it is also a diff is OQ-3.                                                                                                 |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-07 -->

| ID    | Claim                                                                                                                                                                                                                    | Source                      | Status |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------- | ------ |
| B-001 | Given a manifest carrying `schemaVersion: n` for a version the tool ships, the check validates frontmatter, sections and identifiers with the embedded schema set for `n`.                                               | Must-2; README § 6          | Active |
| B-002 | Given a manifest with no `schemaVersion` key, the check treats it as version 1 and the report names version 1.                                                                                                           | A-1; Should-7               | Active |
| B-003 | Given a manifest pinning a version the tool does not ship, the check names the pinned version and the versions it ships on stderr, writes nothing on stdout, and exits with code `3`.                                     | README § 5                  | Active |
| B-004 | Given the tool ships version `n`, it ships every version below `n`, and a test enumerates the embedded set to prove it.                                                                                                  | README § 2 constraint       | Active |
| B-005 | Given `specht upgrade` under a root pinned to `n` where the tool ships `n+1`, this Feature rewrites the manifest, the schema files and the templates to `n+1`, prints the version moved from and to and each file rewritten, and exits with code `0`. | README § 5; README § 6; A-3 | Active |
| B-006 | Given `specht upgrade` under a root pinned to the newest version the tool ships, this Feature prints that it is current, rewrites nothing, and exits with code `0`.                                                        | README § 5                  | Active |
| B-007 | Given `specht upgrade`, no file outside `<root>/.spec/schema/` and `<root>/.spec/templates/` is created, modified or deleted.                                                                                            | README § 2; README § 6      | Active |
| B-008 | Given `specht upgrade` under a root pinned to `n` where the tool ships `n+2`, this Feature moves to `n+1` only, so a second run moves to `n+2`.                                                                             | AGENTS.md § Configuration   | Active |
| B-009 | Given the manifest or a command-line argument selects the on-disk source, the check reads the three frontmatter schemas from `<root>/.spec/schema/` instead of the embedded set, and the manifest itself is read from disk either way. | Should-8; README § 6; OQ-1 | Active |
| B-010 | Given the embedded source, which is the default, a hand edit to an on-disk frontmatter schema changes nothing in the verdict.                                                                                            | Should-8; README § 6        | Active |
| B-011 | Given a run, the report names the schema version checked against and whether the schemas came from the embedded set or from disk.                                                                                       | `0001-F3` B-005             | Active |
| B-012 | Given `specht init` writes the newest version, the manifest it writes carries `schemaVersion` equal to that version.                                                                                                    | `0001-F4` B-001             | Active |
| B-013 | Given a schema version has shipped in a released tool, its rule vocabulary, section list, grammars and schema files never change in a later tool; a change is the next version.                                           | README § 9; C-1             | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-07 -->

| ID   | Constraint                                                                                                                                 | Rules Out                                                                                                  |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------- |
| C-1  | A schema version, once shipped, is embedded in every later tool and is never changed or deleted.                                           | Dropping version 1 "nobody uses it"; patching a shipped version in place.                                   |
| C-2  | `upgrade` writes only under `<root>/.spec/schema/` and `<root>/.spec/templates/`.                                                           | Rewriting a specification, an epic, a record or a `.feature`; `--fix`.                                      |
| C-3  | `upgrade` moves one version at a time.                                                                                                     | A `--to <n>` that skips versions; an upgrade that leaves the tree at a version the agent did not repair for. |
| C-4  | The manifest is always read from `<root>/.spec/schema/spec-structure.schema.json`; only the three frontmatter schemas have a selectable source. | An embedded manifest; a schema-source switch that also switches the manifest.                           |
| C-5  | The tool, not the consumer, knows the difference between two versions.                                                                     | A migration script shipped to the consumer; a consumer-side changelog the tool reads.                      |
| C-6  | A frontmatter schema's `$id` carries its version: `https://github.com/rlittlesii/specht/schema/v<n>/<file>`.                                | A version-less `$id`; an `$id` under another repository.                                                   |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Item                                                                       | Exclusion Reason                                                                                              |
| --- | -------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| 1   | Migrating documents between versions                                       | README § 2; the agent, from the violations and their `expected` (`0001-F3`).                                   |
| 2   | A `downgrade` command                                                      | A repository pins by editing one number; moving the files back is `init` into an empty folder at that version (OQ `0001-F4` OQ-1). |
| 3   | The content of schema version 2                                             | Its own epic, after this Feature; nothing here says what changes.                                              |
| 4   | Versioning the report document                                              | `0001-F3` C-4; the report is versioned with the tool.                                                          |
| 5   | A version pinned per specification                                          | Decided: the version is the repository's (README § 6); a document declares none.                               |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `test-writer`.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-07 -->

| Claim ID | Scenario                                                          | Test    | Status  |
| -------- | ----------------------------------------------------------------- | ------- | ------- |
| B-001    | The pinned version is the one checked with                        | Missing | Missing |
| B-002    | A manifest without a version is version 1                         | Missing | Missing |
| B-003    | A version the tool does not ship is invalid configuration         | Missing | Missing |
| B-004    | Every version ever shipped is still shipped                       | Missing | Missing |
| B-005    | Upgrade moves the schema set and templates to the next version    | Missing | Missing |
| B-006    | Upgrade at the newest version changes nothing                     | Missing | Missing |
| B-007    | Upgrade never touches a document                                  | Missing | Missing |
| B-008    | Upgrade moves one version at a time                               | Missing | Missing |
| B-009    | The on-disk source is selected by configuration                   | Missing | Missing |
| B-010    | The embedded source ignores an on-disk edit                       | Missing | Missing |
| B-011    | The report names the version and the source                       | Missing | Missing |
| B-012    | Init pins the newest version                                      | Missing | Missing |
| B-013    | A shipped version never changes                                   | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-07 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-07 -->

| ID   | Question                                                                                                                                                                                                                                                                                   | Blocks       | Resolution |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------ | ---------- |
| OQ-1 | The manifest field name and the command-line argument name that select the schema source (carried from `REQUIREMENTS.md` § "Still open"). The `implementer` proposes both in § 7; a decision record ratifies them.                                                                             | B-009        | Open       |
| OQ-2 | What `upgrade` does with a file under `.spec/schema/` or `.spec/templates/` the consumer has hand-edited since `init`: README § 6 says `upgrade` rewrites the files; AGENTS.md § Configuration says neither `init` nor `upgrade` overwrites a file that exists. Rewrite, skip and report, or refuse the whole upgrade? | B-005 | Open |
| OQ-3 | Whether "prints what changed" is the file list and the version pair (A-3) or also a diff of each rewritten file.                                                                                                                                                                             | B-005        | Open       |

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
