---
title: "Specification: Discovery"
description: "Where specifications, items, epics and companions are found: the layouts, exclusions and file shapes come from the manifest, git is asked inside a work tree so ignored trees are never opened, and a pruned walk gives the same set outside one"
type: feature
id: "F6"
epic: "0001"
spec_status: approved
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

| #   | Persona                                              | Need                                                                    | Pain Point Today                                                                                                                                              |
| --- | ---------------------------------------------------- | ----------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | The maintainer of a repository with a large tree     | A check that does not open what it will ignore                          | A full walk, then a filter                                                                                                                                    |
| 2   | The maintainer of a repository with its own layout   | Declare where specifications live in the manifest                       | Two layouts, hardcoded                                                                                                                                        |
| 3   | The pre-commit hook                                  | The same set git sees, including an untracked specification being added | N/A (internal dependency)                                                                                                                                     |
| 4   | A CI step on an exported tree, or a tool with no git | The same set without git                                                | N/A (internal dependency)                                                                                                                                     |
| 5   | This repository                                      | Its epic file discovered and its frontmatter checked                    | The epic sat at `.spec/epics/`, inside the excluded root `.spec/` and outside `epics/**/epic.md`; it now lives at `epics/0001-specht/epic.md` (decision 0001) |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                   |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| A-1 | Git-backed discovery invokes the `git` executable on `PATH` as a child process; no git library enters the package set. Resolved 2026-10-07 by decision 0002.                                                 |
| A-2 | The default exclusion list is `hooked`'s - `.git`, `.artifacts`, `.skillfile`, `.claude`, `graphify-out`, `bin`, `obj`, `node_modules` - plus the root `.spec/`, as `0001-F5` C-4 requires of every default. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-07 -->

| ID    | Claim                                                                                                                                                                                                                                                                                                        | Source                      | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------- | ------ |
| B-001 | Given a manifest, discovery reads each specification layout from it - a name and a file glob - so the default manifest declares the legacy layout `epics/**/spec.md` and the co-located layout `**/.spec/README.md`, and a third layout added to the manifest is discovered with no code change.             | README § 5                  | Active |
| B-002 | Given a manifest, discovery reads its exclusion list from it - an entry with no `/` is a directory name excluded at any depth, and an entry beginning with `/` is a root-relative path excluded at that place - so the default manifest carries the list A-2 names and nothing is excluded by code.          | README § 5; decision 0003   | Active |
| B-003 | Given a manifest, discovery reads from it the child-item file-name shape, the epic file glob and the companion-file glob, so the default manifest declares `<epic>-<nn>-*.md` beside a specification, `epics/**/epic.md` for an epic file at `epics/<epic>/epic.md`, and `*.feature` beside a specification. | README § 5; decision 0001   | Active |
| B-004 | Given a layout that declares which path segments carry the epic and the Feature id, `SPEC011` checks a specification found in that layout against them; given a layout that declares none, `SPEC011` reports nothing for it.                                                                                 | README § 4                  | Active |
| B-005 | Given a root inside a git work tree with `git` on `PATH`, discovery asks git for the tracked and the untracked-but-not-ignored files matching the manifest's globs, and never opens a directory git ignores.                                                                                                 | README § 5; C-1             | Active |
| B-006 | Given a root outside a git work tree, or `git` absent from `PATH`, discovery walks the tree and declines to enter an excluded directory, so no child of an excluded directory is enumerated.                                                                                                                 | README § 5; C-1             | Active |
| B-007 | Given a clean checkout of this repository at one commit, the git-backed and the walking discovery yield the same set of specifications, items, epics and companion files.                                                                                                                                    | README § 5; C-3             | Active |
| B-008 | Given discovered files, they are ordered by their root-relative path, ordinally, so the report's order is the same on every machine and in both discovery modes.                                                                                                                                             | `0001-F1` B-006             | Active |
| B-009 | Given the summary or the report names a layout, it names it by the layout's name in the manifest.                                                                                                                                                                                                            | README § 5; `0001-F3` B-005 | Active |
| B-010 | Given a root inside a git work tree, discovery passes the manifest's exclusions to git as exclude pathspecs, so git never lists an excluded directory and no file under one - tracked, or untracked and not git-ignored - is discovered.                                                                     | README § 5; B-002; C-1; C-2 | Active |
| B-011 | Given a symbolic link to a directory under the root, neither discovery mode discovers a file through it.                                                                                                                                                                                                     | README § 5; § 5 #1          | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-07 -->

| ID  | Constraint                                                                                                                                                              | Rules Out                                                                                                                                       |
| --- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | Discovery opens no directory the manifest excludes, in either mode, and in git mode none that git ignores.                                                              | A full enumeration followed by a filter; a walk that opens `node_modules` to reject it; a git listing of an excluded tree discarded afterwards. |
| C-2 | Git is invoked only for discovery, with pathspecs drawn from the manifest (decision 0002 for how it is invoked).                                                        | A git call for anything but the file list; a pathspec the manifest does not declare.                                                            |
| C-3 | On a tree holding no submodule, whose every git-ignored directory the manifest also excludes, both discovery modes are one contract: the same files, in the same order. | A file one mode finds and the other does not on such a tree; a mode-specific rule.                                                              |
| C-4 | The root `.spec/` is excluded by the default manifest, not by code.                                                                                                     | A hardcoded skip of the repository-wide `.spec/`.                                                                                               |
| C-5 | A layout, an exclusion or a file shape is declared in the manifest once; discovery is the only reader.                                                                  | A rule that re-globs; a second list of excluded names anywhere.                                                                                 |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Item                                    | Exclusion Reason                                                                                                                           |
| --- | --------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| 1   | Following symbolic links                | B-011 for a linked directory; a linked tree is not the repository's.                                                                       |
| 2   | Reading `.gitignore` itself             | The walk uses the manifest's list only (C-5), inside a work tree or not; in git mode, git is the reader.                                   |
| 3   | A watch mode                            | A command runs once (`0001-F2`).                                                                                                           |
| 4   | Submodules                              | A submodule is another repository with its own root and manifest; C-3 does not cover a tree holding one.                                   |
| 5   | What a rule does with a discovered file | `0001-F1` and `0001-F5`.                                                                                                                   |
| 6   | Proving B-007 on `hooked`'s tree        | Pending OQ-3: README § 5 asks both modes to return the same set on `hooked`, and whether that is this Feature's claim is the owner's call. |

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

| Claim ID | Scenario                                                      | Test    | Status  |
| -------- | ------------------------------------------------------------- | ------- | ------- |
| B-001    | A layout is discovered from the manifest                      | Missing | Missing |
| B-002    | An exclusion is read from the manifest                        | Missing | Missing |
| B-003    | Items, epics and companions are discovered from the manifest  | Missing | Missing |
| B-004    | Path identity applies only to a layout that declares it       | Missing | Missing |
| B-005    | Discovery in a git work tree never opens an ignored directory | Missing | Missing |
| B-006    | Discovery outside git never enters an excluded directory      | Missing | Missing |
| B-007    | Both discovery modes agree                                    | Missing | Missing |
| B-008    | Discovered files are in one order everywhere                  | Missing | Missing |
| B-009    | A layout is named by its manifest name                        | Missing | Missing |
| B-010    | A manifest exclusion applies in a git work tree               | Missing | Missing |
| B-011    | A linked directory is not followed                            | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-07 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-07 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                    | Blocks                       | Resolution                                                                                                                                                                                                                                                 |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Where a co-located epic file lives. This repository's epic is at `.spec/epics/0001-specht/epic.md`, which the version 1 glob `epics/**/epic.md` does not find, so the epic's frontmatter is unchecked. Does the default manifest gain a second epic glob, and which one?                                                                    | B-003                        | Resolved 2026-10-07 by the repository owner: no second glob. Epic files live at `epics/<epic>/epic.md`, and this repository's moved to `epics/0001-specht/epic.md`; the old location was also inside the root `.spec/`, which C-4 excludes. Decision 0001. |
| OQ-2 | Given a root inside a git work tree and `git` on `PATH`, but `git ls-files` fails (for example "dubious ownership"): does discovery fall back to walking, as B-006 does when git is absent, or fail the run, and with which exit code? README does not say.                                                                                 | A claim for this case        | Open                                                                                                                                                                                                                                                       |
| OQ-3 | README § 5 says both discovery modes "must return the same set on `hooked`". Is that a claim of this Feature - and if so, how is it proven without copying `hooked`'s tree into fixtures - or is it out of scope, with B-007 proven on this repository alone? `0001-F1`'s baseline report runs one discovery mode, so it does not prove it. | § 5 #6; a claim for `hooked` | Open                                                                                                                                                                                                                                                       |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-07 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| ------- | ------ | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟢     | spec-reviewer | Round 3 approved: B-010 now has git exclude the manifest's exclusions by pathspec, so no excluded tree is listed then discarded (C-1, C-2), and its scenario covers a tracked and an untracked-not-ignored file under `bin`. § 5 #6 now defers to OQ-3, Open, without taking a side. Round-1 and round-2 fixes hold. Non-blocking, for `test-writer`: § 9 names for B-002, B-005 and B-006 do not match or omit their scenarios' titles. |

## Tasks

None yet. Cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
