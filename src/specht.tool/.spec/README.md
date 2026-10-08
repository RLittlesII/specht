---
title: "Specification: Schema versioning"
description: "The manifest pins schemaVersion; the tool embeds every schema version it has ever shipped and validates with the pinned one; specht upgrade moves a repository's schema set and templates to the next version and never touches a document; the schema source is embedded by default and on-disk by configuration"
type: feature
id: "F7"
epic: "0001"
spec_status: in-review
status: ready-for-architecture
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
depends_on: ["F3", "F4", "F5"]
blocks: []
spikes: []
created: "2026-10-07"
updated: "2026-10-08"
github_issue: null
synced_at: null
---

# Specification: Schema versioning

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-07 -->

A repository's `.spec/schema/` is a copy of `hooked`'s from one day, and nothing says which day, so either every repository moves with `hooked` at once or the copies diverge silently, and a repository that lags cannot catch up on purpose. This Feature removes that failure state: the manifest pins one number, `schemaVersion`; the tool embeds every version it has ever shipped and checks with the pinned one; `specht upgrade` moves the schema set and the templates to the next version and prints what changed. Lagging becomes deliberate, catching up becomes one command, and a document is only ever changed by the agent, from the report.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Persona                                   | Need                                                                                           | Pain Point Today                                                            |
| --- | ----------------------------------------- | ---------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------- |
| 1   | The maintainer of a lagging repository    | Pin a version and stay there until ready; the newest tool still checks it                      | A fork from one day, with no version to name                                |
| 2   | The maintainer upgrading a repository     | One command that moves the schema set and templates forward and says what changed              | Hand-copy, then diff by eye                                                 |
| 3   | The agent migrating documents             | After an upgrade, the check reports what each document is missing, with what the rule expected | Nothing says what changed between versions                                  |
| 4   | A maintainer trying a schema edit locally | Point the check at the on-disk schema files instead of the embedded ones, deliberately         | The on-disk copy is the only copy, so a hand edit is silently authoritative |
| 5   | `0001-F4`                                 | The embedded version set and the newest version number to write                                | N/A (internal dependency)                                                   |
| 6   | `0001-F3`                                 | The version and the source to name in the report                                               | N/A (internal dependency)                                                   |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                   |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| A-1 | A manifest with no `schemaVersion` key is version 1, the version that predates the key. `hooked`'s manifest at the baseline commit has no key and must keep checking unchanged (Should-7).                   |
| A-2 | Version 1 is the schema set the first published package embeds (decision 0002): `0001-F1` A-3's set plus the manifest keys `0001-F5` and `0001-F6` add, since nothing is published before README § 8 step 5. |
| A-3 | "Prints what changed" is the version moved from and to and the name of every file rewritten, and no diff (OQ-3, resolved 2026-10-08).                                                                        |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                                                                                                                                                   | Source                                         | Status |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------- | ------ |
| B-001 | Given a manifest carrying `schemaVersion: n` for a version the tool ships and the embedded source, the check validates frontmatter with the embedded frontmatter schemas for `n`.                                                                                                       | Must-2; README § 6; C-4                        | Active |
| B-002 | Given a manifest with no `schemaVersion` key, the check uses version 1, and the version the report names (`0001-F3` B-005) is 1.                                                                                                                                                        | A-1; Should-7; `0001-F3` B-005                 | Active |
| B-003 | Given a manifest pinning a version the tool does not ship, the check names the pinned version and the versions it ships on stderr, writes nothing on stdout, and exits with code `3`.                                                                                                   | README § 5                                     | Active |
| B-004 | Given the tool ships version `n`, it ships every version below `n`, and a test enumerates the embedded set to prove it.                                                                                                                                                                 | README § 2 constraint                          | Active |
| B-005 | Given `specht upgrade` under a root pinned to `n` where the tool ships `n+1`, this Feature rewrites the three frontmatter schemas and the templates with version `n+1`'s and exits with code `0`.                                                                                       | README § 5; README § 6; decision 0001          | Active |
| B-006 | Given `specht upgrade` under a root pinned to the newest version the tool ships, this Feature prints that it is current, rewrites nothing, and exits with code `0`.                                                                                                                     | README § 5                                     | Active |
| B-007 | Given `specht upgrade`, no file outside `<root>/.spec/schema/` and `<root>/.spec/templates/` is created, modified or deleted.                                                                                                                                                           | README § 2; README § 6                         | Active |
| B-008 | Given `specht upgrade` under a root pinned to `n` where the tool ships `n+2`, this Feature moves to `n+1` only, so a second run moves to `n+2`.                                                                                                                                         | AGENTS.md § Configuration                      | Active |
| B-009 | Given the manifest or a command-line argument selects the on-disk source, the check reads the three frontmatter schemas from `<root>/.spec/schema/` instead of the embedded set.                                                                                                        | Should-8; README § 6; OQ-1                     | Active |
| B-010 | Given the embedded source, which is the default, a hand edit to an on-disk frontmatter schema changes nothing in the verdict.                                                                                                                                                           | Should-8; README § 6                           | Active |
| B-011 | Given a run, the schema source the report names (`0001-F3` B-005) is the one this Feature selected: disk when B-009 selects it, the embedded set otherwise.                                                                                                                             | Should-8; `0001-F3` B-005                      | Active |
| B-012 | Given `specht init` writes the newest version, the manifest it writes carries `schemaVersion` equal to that version.                                                                                                                                                                    | `0001-F4` B-001                                | Active |
| B-013 | Given a schema version has shipped in a published package, its embedded schema, manifest and template files are byte-identical in every later tool; a change is the next version.                                                                                                       | README § 9; C-1; decision 0002                 | Active |
| B-014 | Given a manifest pinning `n` for a version the tool ships, the check evaluates exactly the rule ids of version `n`'s vocabulary.                                                                                                                                                        | Must-2; README § 4                             | Active |
| B-015 | Given a schema version has shipped in a published package, the rule ids its vocabulary holds are the same in every later tool.                                                                                                                                                          | README § 4; README § 9; C-1; decision 0002     | Active |
| B-016 | Given `specht upgrade` under a root pinned to `n` where the tool ships `n+1`, this Feature changes only `schemaVersion`, to `n+1`, and adds each key version `n+1` introduces that the manifest lacks, so every key already present - including one `n+1` introduces - keeps its value. | AGENTS.md § Configuration; decision 0001; OQ-6 | Active |
| B-017 | Given `specht upgrade` under a root pinned to `n` where the tool ships `n+1`, this Feature prints the version moved from and to and each file it rewrote, and no diff of any file.                                                                                                      | README § 5; A-3; decision 0001; OQ-3           | Active |
| B-018 | Given `specht upgrade` with `--root` naming a path that is not a directory, this Feature writes nothing and exits with code `2`.                                                                                                                                                        | README § 5                                     | Active |
| B-019 | Given `specht upgrade` under a root with no manifest, this Feature writes nothing and exits with code `2`.                                                                                                                                                                              | README § 5                                     | Active |
| B-020 | Given `specht upgrade` under a root whose manifest is invalid, this Feature writes nothing and exits with code `3`.                                                                                                                                                                     | README § 5; `0001-F5`                          | Active |
| B-021 | Given `specht upgrade` under a root pinned to a version the tool does not ship, this Feature names the pinned version and the versions it ships on stderr, writes nothing, and exits with code `3`, as the check does (B-003).                                                          | README § 5; B-003; OQ-5                        | Active |
| B-022 | Given an epic file whose frontmatter carries `title` and `description` as non-empty strings, under a root pinned to version 1, the check reports no frontmatter violation for either key.                                                                                               | OQ-4; decision 0003                            | Active |
| B-023 | Given an epic file under a root pinned to version 1 whose frontmatter carries `title` or `description` as an empty string, the check reports a frontmatter violation (`SPEC004`) for that key.                                                                                          | decision 0003; owner 2026-10-08                | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-07 -->

| ID  | Constraint                                                                                                                                                                                                                                                                     | Rules Out                                                                                                                                                              |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | A schema version, once published in a package, is embedded in every later tool and is never changed or deleted (decision 0002).                                                                                                                                                | Dropping version 1 "nobody uses it"; patching a published version in place; counting an unpublished step-3 pack as shipped.                                            |
| C-2 | `upgrade` writes only under `<root>/.spec/schema/` and `<root>/.spec/templates/`.                                                                                                                                                                                              | Rewriting a specification, an epic, a record or a `.feature`; `--fix`.                                                                                                 |
| C-3 | `upgrade` moves one version at a time.                                                                                                                                                                                                                                         | A `--to <n>` that skips versions; an upgrade that leaves the tree at a version the agent did not repair for.                                                           |
| C-4 | The manifest - sections, grammars, roles and rule settings - is always read from `<root>/.spec/schema/spec-structure.schema.json`; the pinned version selects only the frontmatter schemas and the rule vocabulary, and only the frontmatter schemas have a selectable source. | An embedded manifest; sections or grammars taken from the pinned version's defaults over the on-disk manifest; a schema-source switch that also switches the manifest. |
| C-5 | The tool, not the consumer, knows the difference between two versions.                                                                                                                                                                                                         | A migration script shipped to the consumer; a consumer-side changelog the tool reads.                                                                                  |
| C-6 | A frontmatter schema's `$id` carries its version: `https://github.com/rlittlesii/specht/schema/v<n>/<file>`.                                                                                                                                                                   | A version-less `$id`; an `$id` under another repository.                                                                                                               |
| C-7 | Every path `upgrade` derives and prints is relative to the root with `/` separators; a path the user typed (`--root`) is echoed as given.                                                                                                                                      | An absolute path for a rewritten file; a `--root` argument rewritten before it is echoed.                                                                              |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Item                                 | Exclusion Reason                                                                                                                                                                                                              |
| --- | ------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Migrating documents between versions | README § 2; the agent, from the violations and their `expected` (`0001-F3`).                                                                                                                                                  |
| 2   | A `downgrade` command                | A repository pins by editing one number. `init` takes no version argument (`0001-F4` OQ-1, resolved 2026-10-08), so moving the files back is not a supported path; a consumer starts at the newest and lags by not upgrading. |
| 3   | The content of schema version 2      | Its own epic, after this Feature; nothing here says what changes.                                                                                                                                                             |
| 4   | Versioning the report document       | `0001-F3` C-4; the report is versioned with the tool.                                                                                                                                                                         |
| 5   | A version pinned per specification   | Decided: the version is the repository's (README § 6); a document declares none.                                                                                                                                              |

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

<!-- last written by: spec-author, 2026-10-08 -->

| Claim ID | Scenario                                                       | Test    | Status  |
| -------- | -------------------------------------------------------------- | ------- | ------- |
| B-001    | The pinned version is the one checked with                     | Missing | Missing |
| B-002    | A manifest without a version is version 1                      | Missing | Missing |
| B-003    | A version the tool does not ship is invalid configuration      | Missing | Missing |
| B-004    | Every version ever shipped is still shipped                    | Missing | Missing |
| B-005    | Upgrade moves the schema set and templates to the next version | Missing | Missing |
| B-006    | Upgrade at the newest version changes nothing                  | Missing | Missing |
| B-007    | Upgrade never touches a document                               | Missing | Missing |
| B-008    | Upgrade moves one version at a time                            | Missing | Missing |
| B-009    | The on-disk source is selected by configuration                | Missing | Missing |
| B-010    | The embedded source ignores an on-disk edit                    | Missing | Missing |
| B-011    | The report names the selected source                           | Missing | Missing |
| B-012    | Init pins the newest version                                   | Missing | Missing |
| B-013    | A shipped version never changes                                | Missing | Missing |
| B-014    | The pinned version's rules are the ones evaluated              | Missing | Missing |
| B-015    | A shipped version's rules never change                         | Missing | Missing |
| B-016    | Upgrade keeps the consumer's manifest settings                 | Missing | Missing |
| B-017    | Upgrade prints what it changed                                 | Missing | Missing |
| B-018    | Upgrade under a missing root is a missing-input failure        | Missing | Missing |
| B-019    | Upgrade without a manifest is a missing-input failure          | Missing | Missing |
| B-020    | Upgrade with an invalid manifest is invalid configuration      | Missing | Missing |
| B-021    | Upgrade from a version the tool does not ship rewrites nothing | Missing | Missing |
| B-022    | Version 1 accepts a title and a description on an epic         | Missing | Missing |
| B-023    | Version 1 rejects an empty <key> on an epic                    | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-07 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                      | Blocks | Resolution                                                                                                                                                                                                                                                                  |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | The manifest field name and the command-line argument name that select the schema source (carried from `REQUIREMENTS.md` § "Still open", at commit `254aabc`). The `implementer` proposes both in § 7; a decision record ratifies them.                                                                                                                                       | B-009  | Open                                                                                                                                                                                                                                                                        |
| OQ-2 | What `upgrade` does with a file under `.spec/schema/` or `.spec/templates/` the consumer has hand-edited since `init`: README § 6 says `upgrade` rewrites the files; AGENTS.md § Configuration says neither `init` nor `upgrade` overwrites a file that exists. Rewrite, skip and report, or refuse the whole upgrade?                                                        | B-005  | Resolved 2026-10-07 (decision 0001): rewrite the frontmatter schemas and templates; in the manifest change only `schemaVersion` and the keys the next version adds. Claimed by B-005, B-016, B-017.                                                                         |
| OQ-3 | Whether "prints what changed" is the file list and the version pair (A-3) or also a diff of each rewritten file.                                                                                                                                                                                                                                                              | B-017  | Resolved 2026-10-08 by the repository owner: the version pair and the file list, no diff - `git diff` already shows content. Rejected: a diff per file. B-017 and A-3 say so.                                                                                               |
| OQ-4 | Carried from `0001-F1` OQ-1: the version 1 epic frontmatter schema sets `additionalProperties: false` and lists no `title` or `description`, while AGENTS.md § "Documentation structure" requires both on every tracked markdown file. Which gives way, and is the change version 1 (still open to change before first publish, decision 0002) or version 2? Owner not asked. | —      | Resolved 2026-10-08 by the repository owner (decision 0003): version 1 accepts `title` and `description` on an epic as optional non-empty strings; neither is required. Rejected: a version 2 change, an epic exemption from AGENTS.md, requiring both keys. B-022 says so. |
| OQ-5 | The exit code of `specht upgrade` under a root pinned to a version the tool does not ship. README § 5 names `3` for an invalid manifest and B-003 treats such a pin as invalid for the check, but README does not decide it for `upgrade`.                                                                                                                                    | B-021  | Resolved 2026-10-08 by the repository owner: `3`, as the check gives the same pin, so one manifest gets one answer from every command (`0001-F2` decision 0003). Rejected: `4`, not found, which is for what the command line names. B-021 now names `3`.                   |
| OQ-6 | When the manifest already carries a key version `n+1` adds, does `upgrade` keep the consumer's value or write `n+1`'s default? Decision 0001 does not say.                                                                                                                                                                                                                    | B-016  | Resolved 2026-10-08 by the repository owner: keep the consumer's value; `upgrade` adds only the keys the manifest lacks. Rejected: writing `n+1`'s default, which discards a deliberate setting. B-016 now says so.                                                         |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-08 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               |
| ------- | ------ | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟢     | spec-reviewer | Approved (round 2). Round-1 blockers fixed: B-001/B-014 split matches C-4 and `0001-F5` C-6; A-2, B-013, C-1 agree under decision 0002; B-005/B-016/B-017 follow decision 0001 and OQ-2 is resolved. Non-blocking: C-4 Rules Out "An embedded manifest" should read "an embedded manifest read in place of the on-disk one" (B-013 freezes an embedded manifest); README § 6 still says `upgrade` rewrites the manifest; `depends_on` omits F6 though A-2 makes its keys version 1.                                                                                                                                                                                                                                                |
| 1-5     | 🟢     | spec-reviewer | Round 3 (2026-10-08) - approved, reviewing commit `e7cbd17`. B-016 matches OQ-6 (keep a present key's value, add only absent keys) and its scenario asserts both halves; B-017 and A-3 match OQ-3 and the scenario asserts no diff; B-021 names `3` with stderr as B-003 does, agreeing with `0001-F2` decision 0003 and `0001-F4` B-013; § 5 row 2 agrees with `0001-F4` § 5 row 6; § 9 is unchanged and complete. Non-blocking (spec-author): B-021 says "writes nothing" but not whether stdout is empty, where B-003, the parity it claims, says it is; B-021's Source cites OQ-5 but not decision 0003; A-3 now restates B-017; the § 2, § 3, § 5 and § 11 stamps still read 2026-10-07. Round-2 non-blocking findings stand. |

## Tasks

Cut 2026-10-08 into [`../.issue/`](../.issue/): `0041` (the Feature), with `0042` to `0049`.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
