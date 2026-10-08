---
title: "Specification: init"
description: "specht init writes the schema set and the templates of the newest version the tool ships into a repository's .spec/, from embedded copies that are the same bytes this repository checks itself with, and never overwrites a file"
type: feature
id: "F4"
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
depends_on: ["F2"]
blocks: ["F7"]
spikes: []
created: "2026-10-07"
updated: "2026-10-08"
github_issue: null
synced_at: null
---

# Specification: init

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-07 -->

A repository adopting the model copies `hooked`'s `.spec/schema/` and `.spec/templates/` by hand, from whatever day it looked, and nothing records which day. This Feature removes that failure state: `specht init` writes the schema set and the templates from copies embedded in the tool, so every consumer starts from the same bytes as this repository, and the copy is a fact of the tool version rather than of a day. It never overwrites, so a repository that already has a file keeps it.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Persona                             | Need                                                                                    | Pain Point Today                                        |
| --- | ----------------------------------- | --------------------------------------------------------------------------------------- | ------------------------------------------------------- |
| 1   | The maintainer adopting the tool    | One command that gives a bare repository the schema set and the templates               | `cp -R` from `hooked`, then edit the `$id` URLs by hand |
| 2   | The agent authoring a specification | The templates, beside the schema, as the generation contract (Should-5)                 | Copies the template from another repository             |
| 3   | A repository with local edits       | `init` leaves what exists alone and says so                                             | A copy overwrites                                       |
| 4   | `0001-F7`                           | The embedded version set `init` writes from, and a written manifest to pin a version in | N/A (internal dependency)                               |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                                         |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| A-1 | The schema set is four files - the manifest and the three frontmatter schemas - and the templates are four: `feature.md`, `decision.md`, `adr.md`, `lesson.md`. Together, "the eight files".                                                                       |
| A-2 | The shipping copy is embedded from `src/specht.tool/schema/v<n>/` and `templates/v<n>/`; the root `.spec/schema/` and `.spec/templates/` are the live copy this repository checks itself with. The two are the same bytes (B-004).                                 |
| A-3 | Into a bare root, `init` writes the newest version the tool ships, and takes no version argument; beside a manifest pinning an older shipped version it writes at that version (B-012), and beside an unshipped pin it refuses (B-013). OQ-1, resolved 2026-10-08. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                                                                                                                                                                                                                                      | Source                                                                                                                            | Status  |
| ----- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- | ------- |
| B-001 | Given `specht init` under a root with no `.spec/schema/` and no `.spec/templates/`, this Feature writes the eight files of the newest version it ships into those two folders and exits with code `0`.                                                                                                                                                                     | README § 5; README § 6                                                                                                            | Active  |
| B-002 | Given any of the eight files exists under the root, this Feature leaves it byte-for-byte as it was.                                                                                                                                                                                                                                                                        | README § 9; C-1                                                                                                                   | Active  |
| B-003 | Given a root that records no upstream schema source, a file `init` wrote is byte-identical to the tool's embedded copy.                                                                                                                                                                                                                                                    | README § 6; `0001-F7` decision 0004                                                                                               | Amended |
| B-004 | Given this repository at one commit, the embedded copies of the newest version's three frontmatter schemas and templates are byte-identical to the files under its `.spec/schema/` and `.spec/templates/`, and the embedded manifest equals its live manifest on every tool-owned key; rule settings and every other consumer-config key are excluded from the comparison. | README § 7; AGENTS.md; owner, 2026-10-08; `0001-F7` decision 0001                                                                 | Amended |
| B-005 | Given a root that records no upstream schema source, a frontmatter schema `init` wrote has the `$id` `https://github.com/rlittlesii/specht/schema/v<n>/<file>` for the version written.                                                                                                                                                                                    | README § 6; `0001-F7` C-6                                                                                                         | Amended |
| B-006 | Given `init` runs, stdout lists each of the eight files, relative to the root, as written or skipped.                                                                                                                                                                                                                                                                      | README § 9                                                                                                                        | Active  |
| B-007 | Given `--root` names a path that is not a directory, this Feature names that path on stderr as it was given, writes nothing, and exits with code `2`.                                                                                                                                                                                                                      | README § 5; C-4                                                                                                                   | Active  |
| B-008 | Given `init` runs, no file outside `<root>/.spec/schema/` and `<root>/.spec/templates/` is created or modified.                                                                                                                                                                                                                                                            | README § 9                                                                                                                        | Active  |
| B-009 | Given `specht init` without `--root`, this Feature takes the working directory as the root.                                                                                                                                                                                                                                                                                | AGENTS.md § CLI; README § 5                                                                                                       | Active  |
| B-010 | Given some of the eight files exist under the root and others do not, and the root has no manifest or a manifest pinning the newest version the tool ships, this Feature writes each absent one.                                                                                                                                                                           | README § 9                                                                                                                        | Active  |
| B-011 | Given some of the eight files exist under the root and others do not, and the root has no manifest or a manifest pinning the newest version the tool ships, this Feature exits with code `0`.                                                                                                                                                                              | README § 9                                                                                                                        | Active  |
| B-012 | Given some of the eight files exist under the root and others do not, and the manifest pins an older version the tool ships, this Feature writes each absent one at the pinned version and exits with code `0`.                                                                                                                                                            | OQ-1 (b); `0001-F7` B-005, B-013                                                                                                  | Active  |
| B-013 | Given a manifest pinning a version the tool does not ship, this Feature names the pinned version and the versions it ships on stderr, writes nothing, and exits with code `3`.                                                                                                                                                                                             | OQ-1 (c); `0001-F7` B-003; `0001-F2` decision 0003                                                                                | Active  |
| B-014 | Given a manifest that records an upstream schema source whose fetched content matches the recorded content hash, `init` writes that content into each frontmatter schema file absent under `<root>/.spec/schema/`, and leaves each present one as it was.                                                                                                                  | owner, 2026-10-08; `0001-F7` decision 0004, B-026, C-8, C-9; C-1; B-002                                                           | Amended |
| B-015 | Given a manifest that records an upstream schema source whose fetched content does not match the recorded content hash, `init` names the source on stderr, writes nothing, and exits with code `3`.                                                                                                                                                                        | owner, 2026-10-08; `0001-F7` decision 0004, B-027; `0001-F2` decision 0003                                                        | Active  |
| B-016 | Given a manifest that records an upstream schema source that cannot be reached when a frontmatter schema file is absent, `init` names the source on stderr, writes nothing, and exits with code `3`.                                                                                                                                                                       | owner, 2026-10-08; `0001-F7` decision 0004, B-035; B-015; `0001-F2` decision 0003                                                 | Active  |
| B-017 | Given `specht init` beside a manifest the check rejects - one that does not parse (`0001-F2` B-007), one `0001-F5` rejects (`0001-F5` B-022), or one recording an upstream schema source while explicitly selecting the embedded source (`0001-F7` B-036) - this Feature names the reason on stderr, writes nothing, and exits with code `3`, as the check does.           | owner, 2026-10-08 (every command answers a rejected manifest with `3`); `0001-F2` decision 0003; `0001-F7` B-020; `0001-F3` B-030 | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-08 -->

| ID  | Constraint                                                                                                                                                                                                | Rules Out                                                                                      |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------- |
| C-1 | `init` never overwrites a file.                                                                                                                                                                           | A `--force`; a prompt; a backup-and-replace.                                                   |
| C-2 | The shipping copy is embedded per version under `src/specht.tool`; the root `.spec/` is the live copy, and a test asserts the newest version equals it on the tool-owned files and manifest keys (B-004). | A third copy; a build step that generates one from the other and hides a drift.                |
| C-3 | `init` writes no file outside `<root>/.spec/schema/` and `<root>/.spec/templates/`; it may create those folders and `<root>/.spec/` to hold them.                                                         | Writing a tool manifest, a `nuget.config`, a hook or a workflow into the consumer.             |
| C-4 | Every path `init` derives and prints is relative to the root with `/` separators; the `--root` argument alone is echoed as the user gave it.                                                              | An absolute derived path on stdout or stderr; rewriting the user's `--root` before echoing it. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Item                                                                      | Exclusion Reason                                                     |
| --- | ------------------------------------------------------------------------- | -------------------------------------------------------------------- |
| 1   | Moving an existing `.spec/` to a newer version                            | `0001-F7` (`upgrade`).                                               |
| 2   | Writing the report schema                                                 | The tool's contract, not the consumer's (`0001-F3` A-1).             |
| 3   | Writing `.config/dotnet-tools.json`, `nuget.config`, a hook or a workflow | The consumer's install (README § 6); C-3.                            |
| 4   | Writing a specification, an epic or a record from a template              | The agent copies a template; the tool never writes a document.       |
| 5   | A `--force` or any overwrite                                              | C-1; README § 9.                                                     |
| 6   | A version argument to write an older set                                  | OQ-1 (a): a consumer starts at the newest and lags by not upgrading. |

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

| Claim ID | Scenario                                                                                                                                                                                       | Test    | Status  |
| -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------- | ------- |
| B-001    | Init writes the defaults into a bare repository                                                                                                                                                | Missing | Missing |
| B-002    | Init never overwrites an existing file                                                                                                                                                         | Missing | Missing |
| B-003    | A written file equals the embedded copy                                                                                                                                                        | Missing | Missing |
| B-004    | The embedded copies equal this repository's live copy on what the tool owns                                                                                                                    | Missing | Missing |
| B-005    | A written schema's id names this repository and the version                                                                                                                                    | Missing | Missing |
| B-006    | Init lists what it wrote and what it skipped                                                                                                                                                   | Missing | Missing |
| B-007    | A root that is not a directory is a missing-input failure                                                                                                                                      | Missing | Missing |
| B-008    | Init writes nowhere else                                                                                                                                                                       | Missing | Missing |
| B-009    | Init defaults the root to the working directory                                                                                                                                                | Missing | Missing |
| B-010    | Init writes what is missing beside an existing file                                                                                                                                            | Missing | Missing |
| B-011    | Init succeeds when some files already exist                                                                                                                                                    | Missing | Missing |
| B-012    | Init fills a partial tree at its pinned older version                                                                                                                                          | Missing | Missing |
| B-013    | Init refuses a version the tool does not ship                                                                                                                                                  | Missing | Missing |
| B-014    | Init writes a fetched upstream schema that matches its hash                                                                                                                                    | Missing | Missing |
| B-015    | Init refuses a fetched upstream schema that does not match its hash                                                                                                                            | Missing | Missing |
| B-016    | Init refuses an upstream source it cannot reach                                                                                                                                                | Missing | Missing |
| B-017    | Init beside a manifest that does not parse fails as the check does; Init beside a rejected manifest fails as the check does; Init beside a contradictory schema source fails as the check does | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-07 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-07 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | Blocks | Resolution                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Once the tool ships more than one version: (a) does `init` take a version argument to write an older set, or does a consumer always start at the newest and lag by not upgrading; and (b) when an existing manifest pins a version older than the newest, does `init` write the absent files at the newest (leaving files of two versions in one `.spec/` under a manifest that pins only one, which `0001-F7` B-005 and B-013 assume never happens), at the pinned version, or refuse; and (c) when an existing manifest pins a version the tool does not ship, does `init` write the absent files at the newest, or refuse? | B-002  | Resolved 2026-10-08 by the repository owner: (a) no version argument - a consumer starts at the newest and lags by not upgrading (§ 5 #6); (b) absent files are written at the pinned version, keeping one version in `.spec/` (B-012); (c) refuse, writing nothing, with exit `3`, as the check gives that pin (B-013). Rejected: a `--version` option; writing at the newest beside an older pin; writing at the newest beside an unshipped pin. |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-08 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| ------- | ------ | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1-5     | 🟢     | spec-reviewer | Approved (round 3): B-010 and B-011 are narrowed to "no manifest, or a manifest pinning the newest version the tool ships", their scenarios match, and OQ-1 Blocks now names only B-002, which cites OQ-1 without taking a side. OQ-1 (c) covers an unshipped pinned version; the (b) citation is corrected; AGENTS.md § CLI shows `specht init [--root <dir>]`; B-001 reads cleanly. Round-1 fixes (B-007/C-4, B-008/C-3) still hold, and claims, tags and § 9 agree. Non-blocking: B-006's Given ("`init` runs") also covers B-007's non-directory root and an OQ-1 refusal. Narrowing it to a root that is a directory would remove the overlap (spec-author).                                                                                                                                                                                                                                                                                                                                                                                            |
| 1-5     | 🟢     | spec-reviewer | Round 4 (2026-10-08) - approved, reviewing commit `e7cbd17`. B-012 and B-013 match OQ-1 (b) and (c); § 5 row 6 matches (a); B-013's `3` agrees with `0001-F7` B-003, B-021 and `0001-F2` decision 0003; B-002 no longer cites OQ-1; § 9 carries B-012 and B-013 once each, and their scenarios are tagged and match. Non-blocking (spec-author): the B-006 overlap noted in round 3 is now concrete, since B-006 ("`init` runs") requires the eight-file listing on stdout under B-013's refusal and B-007's failure; narrow B-006 to a run that writes. No claim covers `init` beside a manifest that does not parse or that `0001-F5` rejects, which the check and `upgrade` (`0001-F7` B-020) answer with `3`; raise it as an open question. B-012 joins write and exit where B-010/B-011 split them. B-012 and B-013 consume `0001-F7`'s embedded version set (item `0054` -> `0042`), an edge this Feature cannot declare without a cycle; § 2 should say so. A-3 restates B-012/B-013; the § 2, § 3, § 5 and § 11 stamps still read 2026-10-07.        |
| 1-5     | 🔴     | spec-reviewer | Round 5 (2026-10-08) - blocked, reviewing `acf3cff`..`f32a31d`. B-004 and C-2 narrow the live-copy comparison to tool-owned bytes and manifest keys, matching `0001-F7` decision 0001 and `0055-F1` C-6; B-015 states decision 0004's hash refusal with exit `3`, as `0001-F2` decision 0003 requires. Blocking (spec-author): B-014 writes the fetched upstream content into `<root>/.spec/schema/` with no condition on what exists, against C-1 and B-002 (never overwrite), and a file it writes contradicts B-003 (byte-identical to the embedded copy) and B-005 (a `rlittlesii/specht` `$id`; `0001-F7` C-6 now gives an upstream schema its own); scope B-014 to the absent files and B-003 and B-005 to a root that records no upstream source. Non-blocking: AGENTS.md § Configuration, README's agent-skills section, `specht-conventions` § Layout and § Never add, and its `references/specs.md` still say the live and embedded copies are the same bytes, which B-004 now limits for the manifest (spec-author, with the conventions' owner). |

## Tasks

Cut 2026-10-08 into [`../.issue/`](../.issue/): `0050` (the Feature), with `0051` to `0054`.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
