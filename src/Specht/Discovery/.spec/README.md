---
title: "Specification: Discovery"
description: "Where specifications, items, epics and companions are found: the layouts, exclusions and file shapes come from the manifest, git is asked inside a work tree so ignored trees are never opened, and a pruned walk gives the same set outside one"
type: feature
id: "F6"
epic: "0001"
spec_status: draft
status: needs-decomposition
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
depends_on: ["F5"]
blocks: []
spikes: []
created: "2026-10-07"
updated: "2026-10-07"
github_issue: null
synced_at: null
---

# Specification: Discovery

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-07 -->

The engine walks every directory under the root and discards `bin`, `obj` and `node_modules` afterwards, and the two layouts, the exclusion list and the item, epic and companion file shapes are literals in its code. That is cheap in `hooked` and not in a repository with a front end, and a third layout means a fork. This Feature removes that failure state: discovery reads what to find and what to skip from the manifest, asks git inside a work tree so an ignored tree is never opened, and walks with pruning outside one, and both ways find the same files.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Persona                                             | Need                                                                                     | Pain Point Today                                                              |
| --- | --------------------------------------------------- | ---------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------- |
| 1   | The maintainer of a repository with a large tree    | A check that does not open what it will ignore                                           | A full walk, then a filter                                                     |
| 2   | The maintainer of a repository with its own layout  | Declare where specifications live in the manifest                                        | Two layouts, hardcoded                                                         |
| 3   | The pre-commit hook                                 | The same set git sees, including an untracked specification being added                  | N/A (internal dependency)                                                      |
| 4   | A CI step on an exported tree, or a tool with no git | The same set without git                                                                 | N/A (internal dependency)                                                      |
| 5   | This repository                                     | A place for a co-located epic file that the tool discovers                               | Schema version 1 reads epic files from `epics/**/epic.md` only (OQ-1)          |

### Assumptions

| ID  | Assumption                                                                                                                                                                                               |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | Git-backed discovery invokes the `git` executable on `PATH` as a child process; no git library enters the package set.                                                                                     |
| A-2 | The default exclusion list is `hooked`'s - `.git`, `.artifacts`, `.skillfile`, `.claude`, `graphify-out`, `bin`, `obj`, `node_modules` - plus the root `.spec/`, as `0001-F5` C-4 requires of every default. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-07 -->

| ID    | Claim                                                                                                                                                                                                                                       | Source              | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------- | ------ |
| B-001 | Given a manifest, discovery reads each specification layout from it - a name and a file glob - so the default manifest declares the legacy layout `epics/**/spec.md` and the co-located layout `**/.spec/README.md`, and a third layout added to the manifest is discovered with no code change. | README § 5 | Active |
| B-002 | Given a manifest, discovery reads its exclusion list from it - a bare directory name excluded at any depth, or a root-relative path excluded at that place - so the default manifest carries the list A-2 names and nothing is excluded by code. | README § 5          | Active |
| B-003 | Given a manifest, discovery reads from it the child-item file-name shape, the epic file glob and the companion-file glob, so the default manifest declares `<epic>-<nn>-*.md` beside a specification, `epics/**/epic.md`, and `*.feature` beside a specification. | README § 5 | Active |
| B-004 | Given a layout that declares which path segments carry the epic and the Feature id, `SPEC011` checks a specification found in that layout against them; given a layout that declares none, `SPEC011` reports nothing for it.                 | README § 4          | Active |
| B-005 | Given a root inside a git work tree with `git` on `PATH`, discovery asks git for the tracked and the untracked-but-not-ignored files matching the manifest's globs, and never opens a directory git ignores.                                 | README § 5; C-1     | Active |
| B-006 | Given a root outside a git work tree, or `git` absent from `PATH`, discovery walks the tree and declines to enter an excluded directory, so no child of an excluded directory is enumerated.                                                 | README § 5; C-1     | Active |
| B-007 | Given `hooked`'s tree, and this repository's, the git-backed and the walking discovery yield the same set of specifications, items, epics and companion files.                                                                             | README § 5; C-3     | Active |
| B-008 | Given discovered files, they are ordered by their root-relative path, ordinally, so the report's order is the same on every machine and in both discovery modes.                                                                             | `0001-F1` B-006     | Active |
| B-009 | Given the summary or the report names a layout, it names it by the manifest's name and counts the specifications found in it.                                                                                                              | `0001-F5` B-009     | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-07 -->

| ID   | Constraint                                                                                                       | Rules Out                                                                                   |
| ---- | ---------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------- |
| C-1  | Discovery never enumerates the children of an excluded or git-ignored directory.                                | A full enumeration followed by a filter; a walk that opens `node_modules` to reject it.      |
| C-2  | Git is a child process on `PATH`, invoked only for discovery, with the manifest's globs as pathspecs.            | A git library in the package set; a git call for anything but the file list.                 |
| C-3  | Both discovery modes are one contract: the same files, in the same order.                                       | A file one mode finds and the other does not; a mode-specific rule.                          |
| C-4  | The root `.spec/` is excluded by the default manifest, not by code.                                              | A hardcoded skip of the repository-wide `.spec/`.                                            |
| C-5  | A layout, an exclusion or a file shape is declared in the manifest once; discovery is the only reader.           | A rule that re-globs; a second list of excluded names anywhere.                              |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Item                                                                     | Exclusion Reason                                                                                   |
| --- | ------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------- |
| 1   | Following symbolic links                                                  | Neither mode follows them; a linked tree is not the repository's.                                   |
| 2   | Reading `.gitignore` itself outside a work tree                            | The walk uses the manifest's list only (C-5); inside a work tree, git is the reader.                |
| 3   | A watch mode                                                              | A command runs once (`0001-F2`).                                                                     |
| 4   | Submodules                                                                | A submodule is another repository with its own root and manifest.                                   |
| 5   | What a rule does with a discovered file                                   | `0001-F1` and `0001-F5`.                                                                            |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `implementer`, written after agreement. README § 5 "Discovery cost" is the drafted direction.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `test-writer`.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-07 -->

| Claim ID | Scenario                                                              | Test    | Status  |
| -------- | --------------------------------------------------------------------- | ------- | ------- |
| B-001    | A layout is discovered from the manifest                              | Missing | Missing |
| B-002    | An exclusion is read from the manifest                                | Missing | Missing |
| B-003    | Items, epics and companions are discovered from the manifest          | Missing | Missing |
| B-004    | Path identity applies only to a layout that declares it               | Missing | Missing |
| B-005    | Discovery in a git work tree never opens an ignored directory         | Missing | Missing |
| B-006    | Discovery outside git never enters an excluded directory              | Missing | Missing |
| B-007    | Both discovery modes agree                                            | Missing | Missing |
| B-008    | Discovered files are in one order everywhere                          | Missing | Missing |
| B-009    | A layout is counted by its manifest name                              | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-07 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-07 -->

| ID   | Question                                                                                                                                                                                                                                                                      | Blocks | Resolution |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ | ---------- |
| OQ-1 | Where a co-located epic file lives. This repository's epic is at `.spec/epics/0001-specht/epic.md`, which the version 1 glob `epics/**/epic.md` does not find, so the epic's frontmatter is unchecked. Does the default manifest gain a second epic glob, and which one? | B-003  | Open       |

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
