---
title: "Specification: The check command"
description: "specht, the default command: one verdict on a repository's .spec/ tree, as MSBuild-shaped lines and an exit code, the same at a pre-commit hook, in CI and from an agent's shell"
type: feature
id: "F2"
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
depends_on: ["F1"]
blocks: ["F3", "F4", "F5"]
spikes: []
created: "2026-10-07"
updated: "2026-10-08"
github_issue: null
synced_at: null
---

# Specification: The check command

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-07 -->

The gate runs only as `hooked`'s Nuke target, so a pre-commit hook, a CI step and an agent mid-write each reach it differently or not at all, and a second repository has no step to add. This Feature removes that failure state: `specht` is one command, installed from a package through a local tool manifest, that reads a root, prints one MSBuild-shaped line per violation and the summary, and exits with a code the three call sites all understand (Must-1). It writes nothing under the root but the `--report` file `0001-F3` owns.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Persona                              | Need                                                                                          | Pain Point Today                                                  |
| --- | ------------------------------------ | --------------------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| 1   | The maintainer's pre-commit hook     | A command that exits non-zero on a staged specification with an error, fast, with no build    | Only `hooked` has a hook, and it bootstraps Nuke to read markdown |
| 2   | The CI build, here and in a consumer | MSBuild-shaped lines GitHub annotates on the diff and an exit code that fails the job         | Exists through `hooked`'s Nuke target only                        |
| 3   | The agent authoring a specification  | A read-only oracle it can run mid-write before it reverts or fixes a damaged draft            | Re-read the document, or run the repository's whole build         |
| 4   | A consumer's Nuke `SpecCheck` target | A tool to call through the local tool manifest, in place of a project reference to the engine | Carries the engine's code and build wiring                        |
| 5   | `0001-F3`, `0001-F4`, `0001-F7`      | A host to join as an option or a command                                                      | N/A (internal dependency)                                         |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                                                                                           |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | `check` is the default command: `specht` with no command name runs it. `init` and `upgrade` are sibling commands (`dotnet-tool` § Vertical Slice).                                                                                                                                                                   |
| A-2 | Until the first package is published, this repository's `SpecCheck` target runs `dotnet run --project src/specht.tool -- --root .`; the local tool manifest replaces that the moment a package exists (`specht-conventions` § Build and test). B-011 holds either way; B-014 holds from the first published package. |
| A-3 | The manifest path is `<root>/.spec/schema/spec-structure.schema.json`, the name `hooked` uses; renaming it is not this Feature's.                                                                                                                                                                                    |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                                                                                                                                | Source                                                                       | Status  |
| ----- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------- | ------- |
| B-001 | Given a root (the working directory, or `--root <dir>`), this Feature evaluates every rule `0001-F1` evaluates and prints each violation on stdout as one MSBuild-shaped line, `<path>(<line>): <severity> SPEC###: <message>`, in the engine's order.               | README § 5; decision 0001                                                    | Active  |
| B-002 | Given a run completes, this Feature prints after the violations the summary: the specification count per layout, the item count, the count of rule ids evaluated, and the error and warning counts, each as `0001-F3` B-005 defines it.                              | `hooked` `Build.SpecCheck.cs`; `0001-F3` B-005                               | Active  |
| B-003 | Given at least one error-severity violation, this Feature exits with code `1`; given none, with code `0`.                                                                                                                                                            | README § 5                                                                   | Active  |
| B-004 | Given `--strict`, this Feature exits with code `1` when any violation of any severity was reported.                                                                                                                                                                  | README § 5                                                                   | Active  |
| B-005 | Given `--root` names a path that is not a directory, this Feature writes a message naming that path as it was given on stderr, writes nothing on stdout, and exits with code `2`.                                                                                    | README § 5; README § 9                                                       | Active  |
| B-006 | Given a root with no manifest at the manifest path, this Feature writes a message naming the manifest path on stderr, writes nothing on stdout, and exits with code `2`.                                                                                             | README § 5                                                                   | Active  |
| B-007 | Given a file at the manifest path that is not well-formed JSON or does not parse into the manifest shape the engine reads, this Feature writes a message naming the manifest path relative to the root on stderr, writes nothing on stdout, and exits with code `3`. | README § 5; § 5 row 4                                                        | Active  |
| B-008 | Given `--help`, this Feature prints usage naming the `--root` and `--strict` options, and exits with code `0`.                                                                                                                                                       | `spectre-cli` § Help; § 5 rows 1-3                                           | Active  |
| B-009 | Given a run without `--json`, stdout carries only the violation lines and the summary, and every message about the tool itself goes to stderr.                                                                                                                       | `dotnet-tool` § Conventions; C-3; `0001-F3` B-001 owns stdout under `--json` | Active  |
| B-010 | Given `0001-F5`'s rule settings exist and one root and its manifest, a shell invocation, this repository's `SpecCheck` target and the pre-commit hook, each pointed at that root, print the same lines and exit with the same code.                                  | Must-1; owner, 2026-10-08; C-8; decision 0004                                | Amended |
| B-011 | Given `0001-F5`'s rule settings exist and this repository's own tree, `./build.sh SpecCheck` runs the `specht` command against the repository root and exits with the command's code.                                                                                | README § 7; A-2; owner, 2026-10-08; C-8; decision 0004                       | Amended |
| B-012 | Given any run, no file under the root is created, modified or deleted except the file `--report` names.                                                                                                                                                              | README § 9; `0001-F3` B-022, B-024, C-7 and its decision 0002; § 5 row 10    | Active  |
| B-013 | Given any output on stdout or stderr, every path the tool derives is relative to the root with `/` separators, and the only path that may be absolute is one typed on the command line (`--root`, `--report`), echoed as given.                                      | README § 9                                                                   | Active  |
| B-014 | Given a published `specht.tool` package, `./build.sh SpecCheck` invokes the tool through this repository's committed local tool manifest, not through `dotnet run --project`.                                                                                        | README § 7; C-6; A-2                                                         | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-08 -->

| ID  | Constraint                                                                                                                                                                                                                                            | Rules Out                                                                                                                                                                  |
| --- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | Withdrawn 2026-10-07: an imported framework pattern, not a constraint; recorded as decision 0002.                                                                                                                                                     | Nothing; decision 0002 records what the framework choice rules out.                                                                                                        |
| C-2 | Exit codes are named constants in one static class and are returned, never thrown: `0`, `1`, `2`, `3`, `4` as README § 5 defines them (`4`: decision 0003); an exception is mapped through the exception handler.                                     | A magic number at a return site; an exception crossing the command boundary to Spectre's `-1`.                                                                             |
| C-3 | stdout carries the product only, as plain text an MSBuild log parser reads; everything about the tool itself goes to stderr.                                                                                                                          | A banner, a progress line, colour markup or a tool error on stdout; a violation line on stderr.                                                                            |
| C-4 | A command parses, calls the runner and folds the report into an exit code; nothing else lives in a command.                                                                                                                                           | A rule, a reader or a path helper inside `Features/`.                                                                                                                      |
| C-5 | Only `src/specht.tool` packs: `PackAsTool`, `ToolCommandName` `specht`, `PackageId` `specht.tool`; `IsPackable=false` everywhere else; versions central.                                                                                              | Flipping `IsPackable` repo-wide; a `Version=` on a `PackageReference`; a second packable project.                                                                          |
| C-6 | A consumer installs the tool through a committed local tool manifest, and its build calls the tool, never the engine.                                                                                                                                 | A global install as the documented path; a project reference from a consumer's `.build/` to `src/specht`.                                                                  |
| C-7 | The command is tested through Spectre's command tester against synthetic trees under a temporary root; the one test that reads this repository's real tree is the self-check (B-011, B-014).                                                          | A unit test over the repository's real specifications; a unit-tier test that shells out to an installed tool.                                                              |
| C-8 | `SpecCheck` and the pre-commit hook exit with the command's code only from `0001-F5`'s arrival; before it they report and never fail, as `0055-F1` C-7 and B-025 decide - a dependency on `0001-F5` and `0055-F1` (owner, 2026-10-08; decision 0004). | A test of B-010 or B-011 that expects the command's code from the target or the hook before `0001-F5` lands; a `SpecCheck` or hook failure on this repository before then. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Item                                                                                                                   | Exclusion Reason                                                                                                             |
| --- | ---------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| 1   | The `--json` and `--report` documents, and `--explain`, and their `--help` entries                                     | `0001-F3`.                                                                                                                   |
| 2   | `init`, and its `--help` entry                                                                                         | `0001-F4`.                                                                                                                   |
| 3   | `upgrade`, and the schema-source option, and their `--help` entries                                                    | `0001-F7`.                                                                                                                   |
| 4   | Rejecting a well-formed manifest - an unknown key, rule id, grammar, role or exclusion form - and the exit code for it | `0001-F5` (README § 8 step 5); this Feature's exit code `3` covers only a manifest that does not parse (B-007).              |
| 5   | Autofix of any kind                                                                                                    | README § 2; B-012 is the boundary.                                                                                           |
| 6   | `hooked`'s Nuke target calling the tool                                                                                | A Feature of `hooked`.                                                                                                       |
| 7   | The release workflow that publishes `specht.tool` to GitHub Packages                                                   | `0055-F6`; not a behaviour of the command.                                                                                   |
| 8   | A rule plugin model                                                                                                    | Rejected in decision 0001.                                                                                                   |
| 9   | A watch mode, an editor or language-server surface                                                                     | A command for a shell and a CI step; an editor surface is a different product.                                               |
| 10  | Writing the `--report` file, the one write under the root a run may make                                               | `0001-F3` B-022 writes it, B-024 overwrites it and C-7 makes it the only write; exempt from B-012 (`0001-F3` decision 0002). |
| 11  | The exit code of `SpecCheck` and the hook before `0001-F5`'s rule settings exist                                       | `0055-F1` B-025 and C-7; C-8.                                                                                                |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `implementer`, written after agreement. The slice layout is `dotnet-tool` § Vertical Slice.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `test-writer`.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-07 -->

| Claim ID | Scenario                                                           | Test    | Status  |
| -------- | ------------------------------------------------------------------ | ------- | ------- |
| B-001    | A run prints every violation as a diagnostic line                  | Missing | Missing |
| B-002    | A run ends with the summary                                        | Missing | Missing |
| B-003    | An error-severity violation fails the run                          | Missing | Missing |
| B-004    | Strict mode fails the run on a warning                             | Missing | Missing |
| B-005    | A root that is not a directory is a missing-input failure          | Missing | Missing |
| B-006    | A root without a manifest is a missing-input failure               | Missing | Missing |
| B-007    | A manifest that does not parse is an invalid-configuration failure | Missing | Missing |
| B-008    | Help names the check's options                                     | Missing | Missing |
| B-009    | The product goes to stdout and the tool's own messages to stderr   | Missing | Missing |
| B-010    | Three call sites get one verdict                                   | Missing | Missing |
| B-011    | This repository checks itself with the tool                        | Missing | Missing |
| B-012    | A check writes nothing but the named report                        | Missing | Missing |
| B-013    | No derived path in the output is absolute                          | Missing | Missing |
| B-014    | The build reaches the tool through the local tool manifest         | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-07 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-07 -->

None.

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-08 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| ------- | ------ | ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟢     | spec-reviewer | Round 3 - approved: B-007 now covers only a manifest that does not parse, with no edge to `0001-F5`, and § 5 row 4 hands well-formed-manifest rejections to `0001-F5` B-022/B-023; B-012 and § 5 row 10 cite `0001-F3` B-022, B-024, C-7 and decision 0002. Non-blocking: decision 0002 records a code-structure choice (an ADR by the template), routed to `implementer` to re-home under `adr/` at § 7.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| 1-5     | 🟢     | spec-reviewer | Round 4 (2026-10-08) - approved, reviewing commit `e7cbd17`. C-2 lists `4` and cites decision 0003, which follows the template (date, decider, call, why, rejected with costs, affects, reversal). No claim of this Feature is needed for `4`: the check never returns it, `0001-F3` B-020 claims the behaviour, and C-2 holds the constant. Non-blocking: decision 0003 Affects omits `0001-F7` B-021/OQ-5 and `0001-F4` B-013/OQ-1 (c), which its call names and which cite it (spec-author). Two listings still give four codes: `.claude/skills/dotnet-tool/references/conventions.md` lines 47-52 ("decided (README § 5)"), whose generic example above it also binds `4` to `ExternalServiceError` and `NotFound` to `6`, and `.claude/skills/specht-conventions/references/specs.md` line 111 (`exit 0/1/2/3`); fix before `0026` or `0040` is implemented. The § 4 stamp still reads 2026-10-07.                                                                                                                                                             |
| 1-5     | 🟢     | spec-reviewer | Round 5 (2026-10-08) - approved, reviewing `acf3cff`..`f32a31d`. § 5 row 7 now hands the release workflow to `0055-F6`, which claims it; no claim, constraint or scenario changed.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| 1-5     | 🔴     | spec-reviewer | Round 5 (2026-10-08) - blocked, found reviewing the epic 0055 item cut `6113181`..`0657b68`. B-011 has `./build.sh SpecCheck` exit with the command's code on this repository's tree, and B-010 has the target and the hook exit with the shell's code; `0055-F1` C-7 and B-025, `0055-F2` C-4 and `0055-F6` C-6 (owner, 2026-10-08) have `SpecCheck` exit 0 whatever the check reports until `0001-F5`'s rule settings exist, and until then the check exits 1 on this tree (`SPEC060` is an error on every approved specification with a `Missing` § 9 row). No order of landing satisfies both: 0031 carries B-010 and B-011, `0055-F1`'s 0061 carries B-025. Blocking (spec-author): amend B-010 and B-011 to hold from `0001-F5`'s arrival and to defer to `0055-F1` B-025 before it - this Feature is the one out of line with the owner's decision. Non-blocking (cutter): 0032's summary still has it add the package to the tool manifest and calls the first publish "not an item here"; 0087 (`0055-F7` B-003) now does the first and 0084 is the second. |
| 1-5     | 🟢     | spec-reviewer | Round 6 (2026-10-08) - approved, reviewing `74a3def`..`f9d0ce1`. Round-5 blocker cleared: B-010 and B-011 are Amended in place with `0001-F5`'s rule settings in their Given and a Source citing the owner and decision 0004, as `0001-F3` B-030 and `0001-F7` were; ids, § 9 rows and the `@B-010`/`@B-011` tags are unchanged and both scenarios gain the Given. With 0061 report-only, 0062 gating and 0031 proving after 0062, no delivered claim of this Feature contradicts `0055-F1` B-023, B-025 or C-7 in any landing order. C-8, § 5 row 11 and decision 0004 cite ids that exist; the frontmatter is unchanged, so no `F2`-`F5` cycle. Non-blocking (spec-author): C-8 says the hook reports before `0001-F5`, but no claim has the hook run the check then (`0055-F1` B-013's Given; 0060 routes the call to 0062); say the hook does not run the check until then.                                                                                                                                                                                      |
| Tasks   | 🟢     | spec-reviewer | Re-cut (2026-10-08) - approved, reviewing `f9d0ce1`. 0031 waits on 0026, 0060 and 0062 and builds no wiring: the three call sites can agree on a code only once 0062 gates. 0032 on 0061 is real: 0061 now wires the `dotnet run` call B-014 replaces, and 0087 adds the manifest entry. `priority`, `rank`, `blocks` and parent/children recompute across all 93 items, and the graph is acyclic with each Feature item waiting on its children.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |

## Tasks

Cut 2026-10-08 into [`../.issue/`](../.issue/): `0025` (the Feature), with `0026` to `0032`.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
