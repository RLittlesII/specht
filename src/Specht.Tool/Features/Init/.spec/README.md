---
title: "Specification: init"
description: "specht init writes the schema set and the templates of the newest version the tool ships into a repository's .spec/, from embedded copies that are the same bytes this repository checks itself with, and never overwrites a file"
type: feature
id: "F4"
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
depends_on: ["F2"]
blocks: ["F7"]
spikes: []
created: "2026-10-07"
updated: "2026-10-07"
github_issue: null
synced_at: null
---

# Specification: init

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-07 -->

A repository adopting the model copies `hooked`'s `.spec/schema/` and `.spec/templates/` by hand, from whatever day it looked, and nothing records which day. This Feature removes that failure state: `specht init` writes the schema set and the templates from copies embedded in the tool, so every consumer starts from the same bytes as this repository, and the copy is a fact of the tool version rather than of a day. It never overwrites, so a repository that already has a file keeps it.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Persona                                   | Need                                                                                        | Pain Point Today                                                     |
| --- | ----------------------------------------- | ------------------------------------------------------------------------------------------- | -------------------------------------------------------------------- |
| 1   | The maintainer adopting the tool          | One command that gives a bare repository the schema set and the templates                    | `cp -R` from `hooked`, then edit the `$id` URLs by hand               |
| 2   | The agent authoring a specification       | The templates, beside the schema, as the generation contract (Should-5)                      | Copies the template from another repository                           |
| 3   | A repository with local edits             | `init` leaves what exists alone and says so                                                  | A copy overwrites                                                     |
| 4   | `0001-F7`                                 | The embedded version set `init` writes from, and a written manifest to pin a version in       | N/A (internal dependency)                                             |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                        |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The schema set is four files - the manifest and the three frontmatter schemas - and the templates are four: `feature.md`, `decision.md`, `adr.md`, `lesson.md`. Together, "the eight files".                                      |
| A-2 | The shipping copy is embedded from `src/Specht.Tool/schema/v<n>/` and `templates/v<n>/`; the root `.spec/schema/` and `.spec/templates/` are the live copy this repository checks itself with. The two are the same bytes (B-004). |
| A-3 | `init` writes the newest version the tool ships. Writing an older version on request is OQ-1.                                                                                                                                     |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-07 -->

| ID    | Claim                                                                                                                                                                                                 | Source                      | Status |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------- | ------ |
| B-001 | Given `specht init` under a root with no `.spec/schema/` and no `.spec/templates/`, this Feature writes the eight files of the newest version it ships into those two folders and exits with code `0`. | README § 5; README § 6      | Active |
| B-002 | Given any of the eight files exists under the root, this Feature leaves it byte-for-byte as it was, reports it on stdout as skipped, writes the absent ones, and exits with code `0`.                  | README § 9                  | Active |
| B-003 | Given a file `init` wrote, it is byte-identical to the tool's embedded copy.                                                                                                                           | README § 6                  | Active |
| B-004 | Given this repository at one commit, the embedded copies of the newest version equal the files under its `.spec/schema/` and `.spec/templates/`, and a test proves it.                                 | README § 7; AGENTS.md       | Active |
| B-005 | Given a frontmatter schema `init` wrote, its `$id` is `https://github.com/rlittlesii/specht/schema/v<n>/<file>` for the version written.                                                               | README § 6                  | Active |
| B-006 | Given `init` runs, stdout lists each of the eight files, relative to the root, as written or skipped.                                                                                                   | README § 9                  | Active |
| B-007 | Given `--root` names a path that is not a directory, this Feature names it on stderr, writes nothing, and exits with code `2`.                                                                          | README § 5                  | Active |
| B-008 | Given `init` runs, nothing outside `<root>/.spec/schema/` and `<root>/.spec/templates/` is created or modified.                                                                                        | README § 9                  | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-07 -->

| ID   | Constraint                                                                                                                                                 | Rules Out                                                                                  |
| ---- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| C-1  | `init` never overwrites a file.                                                                                                                            | A `--force`; a prompt; a backup-and-replace.                                               |
| C-2  | The shipping copy is embedded per version under `src/Specht.Tool`; the root `.spec/` is the live copy, and a test asserts the newest version equals it.     | A third copy; a build step that generates one from the other and hides a drift.            |
| C-3  | `init` writes only under `<root>/.spec/schema/` and `<root>/.spec/templates/`.                                                                             | Writing a tool manifest, a `nuget.config`, a hook or a workflow into the consumer.          |
| C-4  | Every path `init` prints is relative to the root with `/` separators.                                                                                      | An absolute path on stdout.                                                                 |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Item                                                                      | Exclusion Reason                                                                                   |
| --- | ------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| 1   | Moving an existing `.spec/` to a newer version                             | `0001-F7` (`upgrade`).                                                                             |
| 2   | Writing the report schema                                                  | The tool's contract, not the consumer's (`0001-F3` A-1).                                           |
| 3   | Writing `.config/dotnet-tools.json`, `nuget.config`, a hook or a workflow  | The consumer's install (README § 6); C-3.                                                          |
| 4   | Writing a specification, an epic or a record from a template               | The agent copies a template; the tool never writes a document.                                     |
| 5   | A `--force` or any overwrite                                               | C-1; README § 9.                                                                                   |

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

| Claim ID | Scenario                                                       | Test    | Status  |
| -------- | -------------------------------------------------------------- | ------- | ------- |
| B-001    | Init writes the defaults into a bare repository                | Missing | Missing |
| B-002    | Init never overwrites an existing file                         | Missing | Missing |
| B-003    | A written file equals the embedded copy                        | Missing | Missing |
| B-004    | The embedded copies equal this repository's live copy          | Missing | Missing |
| B-005    | A written schema's id names this repository and the version    | Missing | Missing |
| B-006    | Init lists what it wrote and what it skipped                   | Missing | Missing |
| B-007    | A root that is not a directory is a missing-input failure      | Missing | Missing |
| B-008    | Init writes nowhere else                                       | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-07 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-07 -->

| ID   | Question                                                                                                                                                     | Blocks | Resolution |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------ | ---------- |
| OQ-1 | Once the tool ships more than one version, does `init` take a version argument to write an older set, or does a consumer always start at the newest and lag by not upgrading? | —      | Open       |

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
