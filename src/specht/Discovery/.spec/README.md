---
title: "Specification: Discovery"
description: "Where specifications, items, epics and companions are found: the layouts, exclusions and file shapes come from the manifest, git is asked inside a work tree so ignored trees are never opened, and a pruned walk gives the same set outside one"
type: feature
id: "F6"
epic: "0001"
spec_status: approved
status: ready-for-architecture
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
updated: "2026-10-09"
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

| ID    | Claim                                                                                                                                                                                                                                                                                                        | Source                     | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------- | ------ |
| B-001 | Given a manifest, discovery reads each specification layout from it - a name and a file glob - so the default manifest declares the legacy layout `epics/**/spec.md` and the co-located layout `**/.spec/README.md`, and a third layout added to the manifest is discovered with no code change.             | brief § 5                  | Active |
| B-002 | Given a manifest, discovery reads its exclusion list from it - an entry with no `/` is a directory name excluded at any depth, and an entry beginning with `/` is a root-relative path excluded at that place - so the default manifest carries the list A-2 names and nothing is excluded by code.          | brief § 5; decision 0003   | Active |
| B-003 | Given a manifest, discovery reads from it the child-item file-name shape, the epic file glob and the companion-file glob, so the default manifest declares `<epic>-<nn>-*.md` beside a specification, `epics/**/epic.md` for an epic file at `epics/<epic>/epic.md`, and `*.feature` beside a specification. | brief § 5; decision 0001   | Active |
| B-004 | Given a layout that declares which path segments carry the epic and the Feature id, `SPEC011` checks a specification found in that layout against them; given a layout that declares none, `SPEC011` reports nothing for it.                                                                                 | brief § 4                  | Active |
| B-005 | Given a root inside a git work tree with `git` on `PATH`, discovery asks git for the tracked and the untracked-but-not-ignored files matching the manifest's globs, and never opens a directory git ignores.                                                                                                 | brief § 5; C-1             | Active |
| B-006 | Given a root outside a git work tree, or `git` absent from `PATH`, discovery walks the tree and declines to enter an excluded directory, so no child of an excluded directory is enumerated.                                                                                                                 | brief § 5; C-1             | Active |
| B-007 | Given a clean checkout of this repository at one commit, the git-backed and the walking discovery yield the same set of specifications, items, epics and companion files.                                                                                                                                    | brief § 5; C-3             | Active |
| B-008 | Given discovered files, they are ordered by their root-relative path, ordinally, so the report's order is the same on every machine and in both discovery modes.                                                                                                                                             | `0001-F1` B-006            | Active |
| B-009 | Given the summary or the report names a layout, it names it by the layout's name in the manifest.                                                                                                                                                                                                            | brief § 5; `0001-F3` B-005 | Active |
| B-010 | Given a root inside a git work tree, discovery passes the manifest's exclusions to git as exclude pathspecs, so git never lists an excluded directory and no file under one - tracked, or untracked and not git-ignored - is discovered.                                                                     | brief § 5; B-002; C-1; C-2 | Active |
| B-011 | Given a symbolic link to a directory under the root, neither discovery mode discovers a file through it.                                                                                                                                                                                                     | brief § 5; § 5 #1          | Active |

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

| #   | Item                                    | Exclusion Reason                                                                                                                          |
| --- | --------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Following symbolic links                | B-011 for a linked directory; a linked tree is not the repository's.                                                                      |
| 2   | Reading `.gitignore` itself             | The walk uses the manifest's list only (C-5), inside a work tree or not; in git mode, git is the reader.                                  |
| 3   | A watch mode                            | A command runs once (`0001-F2`).                                                                                                          |
| 4   | Submodules                              | A submodule is another repository with its own root and manifest; C-3 does not cover a tree holding one.                                  |
| 5   | What a rule does with a discovered file | `0001-F1` and `0001-F5`.                                                                                                                  |
| 6   | Proving B-007 on `hooked`'s tree        | Pending OQ-3: brief § 5 asks both modes to return the same set on `hooked`, and whether that is this Feature's claim is the owner's call. |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-09 (item 0005: design pass) -->

Item `0005` declares the inputs; the walk (`0007`), the git listing (`0008`) and path identity (`0006`) extend this table.

| #   | Concern                                                                         | Classification | Where                                                                                                                  |
| --- | ------------------------------------------------------------------------------- | -------------- | ---------------------------------------------------------------------------------------------------------------------- |
| 1   | Which layouts exist, by name and glob, and their order (B-001)                  | Business       | The manifest's `layouts`; `SpecManifest`, into `SpecStructure.Discovery`                                               |
| 2   | What is excluded, and the two forms an entry takes (B-002, decision 0003, C-4)  | Business       | The manifest's `exclusions`; applied in `SpecDiscovery`                                                                |
| 3   | The item, epic and companion file shapes (B-003, decision 0001)                 | Business       | The manifest's `taskFiles`, `epicFiles` and `companionFiles`; applied in `SpecDiscovery`                               |
| 4   | The name a summary or a report gives a layout (B-009)                           | Business       | The layout's `name`, carried on `SpecLocation.Layout` to `SpecCheckReport` and `SpecReportDocument`                    |
| 5   | An omitted discovery key reads as the default manifest's (`0001-F5` B-019, A-4) | Business       | `SpecManifest`, each of the five keys filled whole                                                                     |
| 6   | Discovery is the only reader of these keys (C-5)                                | Both           | `SpecDiscovery` takes `SpecStructure.Discovery`; `SpecModel.Load` and the rules stop globbing and stop naming a layout |
| 7   | How a glob is matched against a root-relative path                              | Technical      | One private glob-to-expression function in `SpecDiscovery`; no package                                                 |
| 8   | File-system access behind `System.IO.Abstractions` (`0001-F5` § 7; ADR-0001)    | Technical      | The `SpecDiscovery` members this item touches take the `IFileSystem` `SpecModel.Load` already holds                    |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-09 (item 0005: design pass) -->

Design for item `0005` (B-001, B-002, B-003, B-009), written before its tests and code. brief § 5 "Discovery cost" is the drafted direction for `0007` and `0008`, which are not designed here.

**The manifest keys - proposed, not ratified (OQ-4).** No claim or decision record fixes a key name, so the five names, their shapes and the glob dialect below are this section's proposal, awaiting the owner's ratification as a decision record, as `0001-F5` decision 0003 ratified `frontmatterSchemas`. The values are not a proposal: each is the literal the engine hardcodes today (A-2, `0001-F5` A-4, C-4).

```json
"layouts": [
  { "name": "legacy", "glob": "epics/**/spec.md" },
  { "name": "coLocated", "glob": "**/.spec/README.md" }
],
"exclusions": [".git", ".artifacts", ".skillfile", ".claude", "graphify-out", "bin", "obj", "node_modules", "/.spec"],
"taskFiles": "{task}-*.md",
"epicFiles": "epics/**/epic.md",
"companionFiles": "*.feature"
```

| Key              | Shape                                           | Read as                                                                                                                                                    | Claim |
| ---------------- | ----------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- | ----- |
| `layouts`        | An array of objects, each a `name` and a `glob` | The specification layouts, in the order the summary and the report list them. `glob` is matched against a file's root-relative path                        | B-001 |
| `exclusions`     | An array of strings                             | An entry with no `/` is a directory name excluded at any depth; an entry beginning with `/` is a root-relative path excluded at that place (decision 0003) | B-002 |
| `taskFiles`      | A string, a file-name glob                      | The file name of an item beside a specification. `{task}` stands for the `identifiers.task` grammar                                                        | B-003 |
| `epicFiles`      | A string, a glob                                | An epic file, matched against its root-relative path                                                                                                       | B-003 |
| `companionFiles` | A string, a file-name glob                      | A companion file beside a specification                                                                                                                    | B-003 |

Why these shapes:

- **Five top-level keys, not one `discovery` object.** A top-level key gets `0001-F5` B-012's unknown-key rejection and B-019's whole-value fill from the loader as it stands; a nested object would need its own copy of both, or a misspelt member would silently read as the default.
- **`layouts` is an array.** Its order is the order of the summary and the report, and a list is one value, so a manifest that declares `layouts` replaces the default list whole, as `sections` is replaced. An object keyed by name, filled per name as `identifiers` is, could never drop the legacy layout. Each entry is an object because `0006` adds the path-identity declaration beside `name` and `glob` (B-004).
- **The default names are `legacy` and `coLocated`.** They are what the summary and the report document print today (`0001-F2` B-002, `0001-F3` B-005), so the default manifest changes no output.
- **`taskFiles` says `task`**, the kind name `0001-F5` decision 0003 ratified for `frontmatterSchemas.task` and `identifiers.task` already carries; B-003's "item" is the same kind.
- **`{task}`, not a second grammar.** `0001-F5` C-2 gives each identifier one grammar in the manifest. B-003's shape `<epic>-<nn>-*.md` is the task id followed by `-*.md`, so the shape names the grammar and does not restate it: `0001-F5` B-006's three-digit sequence then needs one edit, not two. With the default grammar `^[0-9]{4}-[0-9]{2}$` the shape accepts exactly the names `SpecDiscovery.IsItemFileName` accepts today.

**The glob dialect - proposed (OQ-4).** A glob is `/`-separated and compared ordinally, case-sensitively, against a root-relative path with `/` separators ([`SpecDiscovery.Relative`](../../SpecDiscovery.cs)). A segment that is exactly `**` matches zero or more directories; `*` matches any run of characters inside one segment; every other character is itself. A file-name glob (`taskFiles`, `companionFiles`) has no `/` and is matched against the file name alone. Git's `:(glob)` pathspec reads these tokens the same way, so `0008` can pass a glob to git as written (C-2, C-3). `?`, `[...]` and a leading `!` are not part of the dialect; nothing rejects them yet (OQ-7).

**Where the keys are read.** [`SpecManifest`](../../SpecManifest.cs) stays the one reader of the manifest (`0001-F5` § 7). Item `0005` adds to it, and only this:

- five entries in `KnownKeys`: `layouts`, `exclusions`, `taskFiles`, `epicFiles`, `companionFiles`;
- one private function that reads the five keys, each `manifest[key] ?? Defaults[key]`, into one value;
- one `init` member on [`SpecStructure`](../../SpecStructure.cs), `Discovery`, set beside `FrontmatterSchemas`.

That value is a new record, `SpecDiscoveryInputs`, in its own file: `Layouts`, `Exclusions`, `TaskFiles`, `EpicFiles`, `CompanionFiles`. It is one record and not five members on `SpecStructure` because it is the one argument discovery takes, today and in both modes `0007` and `0008` add, and because no rule holds it (C-5). [`SpecLayout`](../../SpecLayout.cs) stops being an enum of two and becomes the record a `layouts` entry is read into, `Name` and `Glob`; a third layout is then a third value, with no code change (B-001). `SpecLocation.Layout` keeps its name and its type's name.

**The same block in both manifest copies.** The five keys go into `.spec/schema/spec-structure.schema.json` and `src/specht.tool/schema/v1/spec-structure.schema.json`, byte for byte, as one block placed between `$comment` and `sections`. They ship in the embedded `v1` manifest, so they are tool-owned and `init` writes them (`0001-F4` decision 0001, B-004); version 1 is still open to them (`0001-F7` decision 0002). Item `0015` adds its keys to the same three files in parallel: this item's JSON block sits at the top of the manifest, its `KnownKeys` entries directly after `schemaVersion`, and its `Discovery` initializer line above `FrontmatterSchemas`, so an item that appends after `frontmatterSchemas` touches no line this one does.

**How discovery consumes them.** [`SpecDiscovery`](../../SpecDiscovery.cs) stays a static class until ADR-0001's stage D (item `0107`), and its interface arrives with the second mode (stage E, `0007` and `0008`). Each member this item touches takes the `IFileSystem` and the `SpecDiscoveryInputs`: the first from [`SpecModel.Load`](../../SpecModel.cs), which already holds one, the second from `schemas.Structure.Discovery`.

| Member                 | Reads                     | Does                                                                                                                                                                                    |
| ---------------------- | ------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `FindSpecifications`   | `Layouts`, `Exclusions`   | For each layout in the manifest's order, every file whose root-relative path matches the layout's glob and is not excluded, ordinally by path, as a `SpecLocation` carrying that layout |
| `FindChildItems`       | `TaskFiles`               | Beside each specification, every file whose name matches the shape. Replaces `IsItemFileName`, which has no other caller                                                                |
| `FindEpics` (new)      | `EpicFiles`, `Exclusions` | Every file whose root-relative path matches the glob and is not excluded. Moves the `epics` and `epic.md` enumeration out of `SpecModel.Load`                                           |
| `FindCompanions` (new) | `CompanionFiles`          | Beside one specification, every file whose name matches the glob. Moves the `*.feature` enumeration out of `SpecModel.Load`                                                             |

A path is excluded when one of its directory segments equals a bare-name entry, or when it lies under a `/` entry's path. One private function turns a glob into a regular expression; the layouts, `epicFiles`, `taskFiles` and `companionFiles` all pass through it, `taskFiles` with `{task}` replaced by the task grammar less its `^` and `$`. A file two layouts match is discovered once, in the first of them in the manifest's order (OQ-7).

No package matches the globs. `Microsoft.Extensions.FileSystemGlobbing` would be a sixth engine dependency (`0001-F1` C-7; none joins without an ADR), walks through its own directory abstraction and not `IFileSystem`, and reads more tokens than the dialect has, which C-3 would then have to reconcile with git's.

**Naming a layout (B-009).** The name travels as data from the manifest to the output, and no enum or naming policy spells it:

```
layouts[].name -> SpecLayout.Name -> SpecLocation.Layout -> SpecCheckReport.Layouts -> SpecReportDocument.Layouts -> the summary line and "layout" in the JSON
```

- [`SpecCheckReport`](../../SpecCheckReport.cs) carries `Layouts`, one [`SpecReportLayout`](../../Report/SpecReportLayout.cs) per manifest layout in the manifest's order, a layout with no specification at `0`, in place of `LegacyCount` and `CoLocatedCount`; `SpecModel.LegacyCount` and `CoLocatedCount` go with them. `SpecReportLayout.Layout` becomes the name, a string; the type is reused, not doubled.
- [`SpecReportDocument.From`](../../Report/SpecReportDocument.cs) copies the report's list; `SummaryLines` prints the name as it is, and the `JsonNamingPolicy.CamelCase` over the enum goes (`0001-F2` § 7 names this item as where that changes).
- [`docs/schema/report.schema.json`](../../../../docs/schema/report.schema.json) closes `layout` to the enum `legacy`, `coLocated`, which a renamed or a third layout fails. It becomes a non-empty string. The member name and the document's shape do not change, and under the default manifest the document is the same bytes. The file is `0001-F3`'s published schema; `0001-F3` B-005 already says "each layout's name", and its § 7 is updated in the same change.
- `SpecCheckReport.MigrationSummary` names both layouts by literal and computes a percentage that presumes exactly those two. No claim asks for it and the tool never prints it (OQ-6).

**The literals that move.**

| Literal today                                                       | Where                                                                   | Manifest key                    |
| ------------------------------------------------------------------- | ----------------------------------------------------------------------- | ------------------------------- |
| `"epics"`, `"spec.md"`, the `SpecLayout.Legacy` tag                 | `SpecDiscovery.FindSpecifications`                                      | `layouts`                       |
| `".spec"`, `"README.md"`, the `SpecLayout.CoLocated` tag            | `SpecDiscovery.SpecDirectories`, `FindSpecifications`                   | `layouts`                       |
| The enum members' names, spelled `legacy` and `coLocated` in output | `SpecLayout`; `SpecReportDocument.Count`; `report.schema.json`'s `enum` | `layouts`, each entry's `name`  |
| The eight names in `ExcludedDirectories`                            | `SpecDiscovery`                                                         | `exclusions`                    |
| The root `.spec` skip (`rootSpec`)                                  | `SpecDiscovery.SpecDirectories`                                         | `exclusions`, as `/.spec` (C-4) |
| Four digits, `-`, two digits, `-`, `.md`, and the `"*.md"` filter   | `SpecDiscovery.IsItemFileName`, `FindChildItems`                        | `taskFiles`                     |
| `"epics"`, `"epic.md"`                                              | `SpecModel.Load`                                                        | `epicFiles`                     |
| `"*.feature"`                                                       | `SpecModel.Load`                                                        | `companionFiles`                |

**What the default manifest keeps, and the proof.** `0001-F5`'s baseline guard (`SpecManifestBaselineIntegrationTests`, both rows: the keys declared, and every key omitted) and `0001-F1` B-004's golden report stay green; they are this item's proof that the defaults are today's literals. Off the baseline tree, three behaviours differ from today's code, each because a claim says so: an exclusion applies to every layout and to epic files, where the code applied it to the co-located layout alone (B-002, C-1); `/.spec` excludes the whole root `.spec/` tree, where the code skipped only that directory's own `README.md` (C-4); and a layout's name in the output is whatever the manifest says (B-009).

**What stays for later items.**

- **The walk itself (`0007`).** `0005` matches the manifest's globs over the same unpruned enumeration the engine does today, and filters by `exclusions` afterwards. C-1 forbids that, and B-006 is where it ends: `0007` replaces the enumeration with a walk that prunes on `Exclusions`, and changes no input.
- **The git listing (`0008`).** The globs become `:(glob)` pathspecs and `Exclusions` become exclude pathspecs (B-005, B-010).
- **Path identity (`0006`).** `IdentityRule` reads `segments[1]` and `segments[2]` of a legacy path and selects the layout by enum today. Until `0006` declares the segments on the layout, the rule selects the layout named `legacy`: the one layout literal this item leaves in a rule, and a manifest that renames that layout turns `SPEC011` off for it (OQ-5).
- **One order across layouts (`0009`, B-008).** `0005` keeps today's order: the layouts in turn, each ordinal by path.
- **Rejecting a malformed exclusion entry (`0019`, `0001-F5` B-021).** `0005` reads an entry beginning with `/` as a path and any other as a name; an entry with an inner `/` matches no directory name until `0019` rejects it.
- **`ISpecDiscovery` and instance classes.** ADR-0001 stages D and E (`0107`, `0007`, `0008`).

**Files predicted to change.** In `src/specht`: `SpecManifest.cs`, `SpecStructure.cs`, `SpecLayout.cs`, `SpecLocation.cs` (documentation only), `SpecDiscovery.cs`, `SpecModel.cs`, `SpecCheckReport.cs`, `SpecCheckRunner.cs`, `Rules/IdentityRule.cs`, `Report/SpecReportLayout.cs`, `Report/SpecReportDocument.cs`, and the new `SpecDiscoveryInputs.cs`. Both manifest copies, `.spec/schema/spec-structure.schema.json` and `src/specht.tool/schema/v1/spec-structure.schema.json`. `docs/schema/report.schema.json`. § 7 of `0001-F2`, `0001-F3` and `0001-F5`, where they describe the enum, the summary's naming and the loader's known keys.

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

<!-- last written by: implementer, 2026-10-09 (item 0005: OQ-4 to OQ-7 raised) -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                                  | Blocks                                                                           | Resolution                                                                                                                                                                                                                                                                                                                              |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Where a co-located epic file lives. This repository's epic is at `.spec/epics/0001-specht/epic.md`, which the version 1 glob `epics/**/epic.md` does not find, so the epic's frontmatter is unchecked. Does the default manifest gain a second epic glob, and which one?                                                                                                                  | B-003                                                                            | Resolved 2026-10-07 by the repository owner: no second glob. Epic files live at `epics/<epic>/epic.md`, and this repository's moved to `epics/0001-specht/epic.md`; the old location was also inside the root `.spec/`, which C-4 excludes. Decision 0001.                                                                              |
| OQ-2 | Given a root inside a git work tree and `git` on `PATH`, but `git ls-files` fails (for example "dubious ownership"): does discovery fall back to walking, as B-006 does when git is absent, or fail the run, and with which exit code? README does not say.                                                                                                                               | A claim for this case                                                            | Open                                                                                                                                                                                                                                                                                                                                    |
| OQ-3 | brief § 5 says both discovery modes "must return the same set on `hooked`". Is that a claim of this Feature - and if so, how is it proven without copying `hooked`'s tree into fixtures - or is it out of scope, with B-007 proven on this repository alone? `0001-F1`'s baseline report runs one discovery mode, so it does not prove it.                                                | § 5 #6; a claim for `hooked`                                                     | Open                                                                                                                                                                                                                                                                                                                                    |
| OQ-4 | The manifest's discovery key names, their shapes and the glob dialect. A consumer edits them, so they are a public surface (`0001-F5` A-3, OQ-1), and no claim or decision record fixes them. § 7 proposes `layouts` (an array of `name` and `glob`), `exclusions`, `taskFiles` with the `{task}` placeholder, `epicFiles` and `companionFiles`, and a dialect of `**`, `*` and literals. | B-001, B-002, B-003, B-009: item `0005`'s tests and code                         | Open. Proposed by `implementer` 2026-10-09 in § 7; awaiting the owner's ratification as a decision record. Owner not asked.                                                                                                                                                                                                             |
| OQ-5 | Between item `0005` and item `0006`, `SPEC011` can select its layout only by the name `legacy`, so a manifest that renames that layout, as B-009's scenario does, silently turns `SPEC011` off. Is that interim accepted, or do `0005` and `0006` land in one pull request?                                                                                                               | Nothing in `0005`; the order `0005` and `0006` merge in                          | Open. Proposed default: accept the interim, because the default manifest keeps the name and `0006` follows. Owner not asked.                                                                                                                                                                                                            |
| OQ-6 | `SpecCheckReport.MigrationSummary` names both layouts by literal and computes a migration percentage that presumes exactly two layouts. No claim asks for it and the tool never prints it, but two tests assert its text, and B-009 has a summary name a layout by its manifest name. Is it removed, with those assertions, or does a claim keep it?                                      | The shape of `SpecCheckReport` under B-009                                       | Open. Proposed default: remove it, and `test-writer` drops the two assertions. Owner and `spec-author` not asked.                                                                                                                                                                                                                       |
| OQ-7 | No claim says what the discovery keys reject, or how two layouts share a file: (a) a file two layouts' globs both match; (b) two layouts with one name; (c) a layout entry with a member other than `name` and `glob`, or missing one; (d) a glob using a token outside the dialect (`?`, `[...]`). `0001-F5` B-012 to B-015 and B-021 list the rejections, and none is one of these.     | Nothing in `0005`; (d) must be answered before `0008` passes a glob to git (C-3) | Open. § 7's position for (a): discovered once, in the first matching layout. For (b) to (d) `0005` rejects nothing new, and a value of the wrong shape fails as an unreadable manifest, as `0001-F5` § 7 "What 0011 does not reject" says. Proposed default: each of (b) to (d) is a rejection in `0001-F5`, exit `3`. Owner not asked. |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-07 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| ------- | ------ | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟢     | spec-reviewer | Round 3 approved: B-010 now has git exclude the manifest's exclusions by pathspec, so no excluded tree is listed then discarded (C-1, C-2), and its scenario covers a tracked and an untracked-not-ignored file under `bin`. § 5 #6 now defers to OQ-3, Open, without taking a side. Round-1 and round-2 fixes hold. Non-blocking, for `test-writer`: § 9 names for B-002, B-005 and B-006 do not match or omit their scenarios' titles. |

## Tasks

Cut 2026-10-08 into [`../.issue/`](../.issue/): `0004` (the Feature), with `0005`, `0006`, `0007`, `0008` and `0009`.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
