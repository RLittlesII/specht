---
title: "Specification: The check command"
description: "specht, the default command: one verdict on a repository's .spec/ tree, as MSBuild-shaped lines and an exit code, the same at a pre-commit hook, in CI and from an agent's shell"
type: feature
id: "F2"
epic: "0001"
spec_status: draft
status: needs-decomposition
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
blocks: ["F3", "F4"]
spikes: []
created: "2026-10-07"
updated: "2026-10-07"
github_issue: null
synced_at: null
---

# Specification: The check command

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-07 -->

The gate runs only as `hooked`'s Nuke target, so a pre-commit hook, a CI step and an agent mid-write each reach it differently or not at all, and a second repository has no step to add. This Feature removes that failure state: `specht` is one command, installed from a package through a local tool manifest, that reads a root, prints one MSBuild-shaped line per violation and the summary, and exits with a code the three call sites all understand (Must-1). It never writes.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Persona                                        | Need                                                                                           | Pain Point Today                                                                       |
| --- | ---------------------------------------------- | ---------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| 1   | The maintainer's pre-commit hook               | A command that exits non-zero on a staged specification with an error, fast, with no build      | Only `hooked` has a hook, and it bootstraps Nuke to read markdown                      |
| 2   | The CI build, here and in a consumer           | MSBuild-shaped lines GitHub annotates on the diff and an exit code that fails the job           | Exists through `hooked`'s Nuke target only                                             |
| 3   | The agent authoring a specification            | A read-only oracle it can run mid-write before it reverts or fixes a damaged draft              | Re-read the document, or run the repository's whole build                              |
| 4   | A consumer's Nuke `SpecCheck` target           | A tool to call through the local tool manifest, in place of a project reference to the engine   | Carries the engine's code and build wiring                                             |
| 5   | `0001-F3`, `0001-F4`, `0001-F7`                | A host to join as an option or a command                                                        | N/A (internal dependency)                                                              |

### Assumptions

| ID  | Assumption                                                                                                                                                                           |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| A-1 | `check` is the default command: `specht` with no command name runs it. `init` and `upgrade` are sibling commands (`dotnet-tool` § Vertical Slice).                                   |
| A-2 | Until the first package is published, this repository's `SpecCheck` target runs `dotnet run --project src/Specht.Tool -- --root .`; the local tool manifest replaces that the moment a package exists (`specht-conventions` § Build and test). B-011 holds either way. |
| A-3 | The manifest path is `<root>/.spec/schema/spec-structure.schema.json`, the name `hooked` uses; renaming it is not this Feature's.                                                      |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-07 -->

| ID    | Claim                                                                                                                                                                                                                       | Source                                      | Status |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------- | ------ |
| B-001 | Given a root (the working directory, or `--root <dir>`), this Feature evaluates every rule `0001-F1` evaluates and prints each violation on stdout as one MSBuild-shaped line, `<path>(<line>): <severity> SPEC###: <message>`, in the engine's order. | README § 5; decision 0001           | Active |
| B-002 | Given a run completes, this Feature prints after the violations the summary: the specification count per layout, the item count, the count of rules evaluated, and the error and warning counts.                             | `hooked` `Build.SpecCheck.cs`               | Active |
| B-003 | Given at least one error-severity violation, this Feature exits with code `1`; given none, with code `0`.                                                                                                                  | README § 5                                  | Active |
| B-004 | Given `--strict`, this Feature exits with code `1` when any violation of any severity was reported.                                                                                                                        | README § 5                                  | Active |
| B-005 | Given `--root` names a path that is not a directory, this Feature writes a message naming that path on stderr, writes nothing on stdout, and exits with code `2`.                                                           | README § 5                                  | Active |
| B-006 | Given a root with no manifest at the manifest path, this Feature writes a message naming the manifest path on stderr, writes nothing on stdout, and exits with code `2`.                                                   | README § 5                                  | Active |
| B-007 | Given a manifest `0001-F5` rejects as invalid, this Feature writes the message naming the offending key on stderr, writes nothing on stdout, and exits with code `3`.                                                       | README § 5; `0001-F5`                       | Active |
| B-008 | Given `--help`, this Feature prints usage naming the root, report, strict, json and explain options and the `init` and `upgrade` commands, and exits with code `0`.                                                        | `spectre-cli` § Help                        | Active |
| B-009 | Given a run, stdout carries only the violation lines and the summary (or, under `--json`, the report document), and every message about the tool itself goes to stderr.                                                    | `dotnet-tool` § Conventions; C-3            | Active |
| B-010 | Given the same root and the same manifest, a shell invocation, this repository's `SpecCheck` target and the pre-commit hook print the same lines and exit with the same code.                                              | Must-1                                      | Active |
| B-011 | Given this repository's own tree, `./build.sh SpecCheck` runs the tool as a consumer would and exits with the tool's code.                                                                                                  | README § 7                                  | Active |
| B-012 | Given any run, no file under the root is created, modified or deleted.                                                                                                                                                      | README § 9                                  | Active |
| B-013 | Given any output on stdout or stderr, every path in it is relative to the root with `/` separators, and no absolute path appears.                                                                                          | README § 9                                  | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-07 -->

| ID   | Constraint                                                                                                                                                                                    | Rules Out                                                                                                                      |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| C-1  | The command line is parsed by `Spectre.Console.Cli`: an `AsyncCommand` per command, its `Settings` nested inside it, `Validate()` for cross-option rules, an injected `IAnsiConsole`.          | `System.CommandLine`; hand-parsed `args`; a settings class in its own file; a static `AnsiConsole.*` call.                      |
| C-2  | Exit codes are named constants in one static class and are returned, never thrown: `0`, `1`, `2`, `3` as README § 5 defines them; an exception is mapped through the exception handler.       | A magic number at a return site; an exception crossing the command boundary to Spectre's `-1`.                                 |
| C-3  | stdout carries the product only, as plain text an MSBuild log parser reads; everything about the tool itself goes to stderr.                                                                  | A banner, a progress line, colour markup or a tool error on stdout; a violation line on stderr.                                 |
| C-4  | A command parses, calls the runner and folds the report into an exit code; nothing else lives in a command.                                                                                   | A rule, a reader or a path helper inside `Features/`.                                                                           |
| C-5  | Only `src/Specht.Tool` packs: `PackAsTool`, `ToolCommandName` `specht`, `PackageId` `Specht.Tool`; `IsPackable=false` everywhere else; versions central.                                       | Flipping `IsPackable` repo-wide; a `Version=` on a `PackageReference`; a second packable project.                               |
| C-6  | A consumer installs the tool through a committed local tool manifest, and its build calls the tool, never the engine.                                                                         | A global install as the documented path; a project reference from a consumer's `.build/` to `src/Specht`.                       |
| C-7  | The command is tested through Spectre's command tester against synthetic trees under a temporary root; the one test that reads this repository's real tree is the self-check (B-011).          | A unit test over the repository's real specifications; a unit-tier test that shells out to an installed tool.                   |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Item                                                                       | Exclusion Reason                                                                                              |
| --- | -------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| 1   | The `--json` and `--report` documents, and `--explain`                     | `0001-F3`; this Feature only reserves the options in `--help` (B-008).                                         |
| 2   | `init`                                                                     | `0001-F4`.                                                                                                    |
| 3   | `upgrade`, and the schema-source option                                    | `0001-F7`.                                                                                                    |
| 4   | What makes a manifest invalid                                              | `0001-F5`; this Feature only maps the verdict to exit code `3` (B-007).                                        |
| 5   | Autofix of any kind                                                        | README § 2; B-012 is the boundary.                                                                             |
| 6   | `hooked`'s Nuke target calling the tool                                    | A Feature of `hooked`.                                                                                         |
| 7   | The release workflow that publishes `Specht.Tool` to GitHub Packages       | Repository tooling (README § 8 step 1 and 3), not a behaviour of the command.                                   |
| 8   | A rule plugin model                                                        | Rejected in decision 0001.                                                                                     |
| 9   | A watch mode, an editor or language-server surface                         | A command for a shell and a CI step; an editor surface is a different product.                                  |

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

| Claim ID | Scenario                                                            | Test    | Status  |
| -------- | ------------------------------------------------------------------- | ------- | ------- |
| B-001    | A run prints every violation as a diagnostic line                   | Missing | Missing |
| B-002    | A run ends with the summary                                         | Missing | Missing |
| B-003    | An error-severity violation fails the run                           | Missing | Missing |
| B-004    | Strict mode fails the run on a warning                              | Missing | Missing |
| B-005    | A root that is not a directory is a missing-input failure           | Missing | Missing |
| B-006    | A root without a manifest is a missing-input failure                | Missing | Missing |
| B-007    | An invalid manifest is an invalid-configuration failure             | Missing | Missing |
| B-008    | Help names every option and command                                 | Missing | Missing |
| B-009    | The product goes to stdout and the tool's own messages to stderr    | Missing | Missing |
| B-010    | Three call sites get one verdict                                    | Missing | Missing |
| B-011    | This repository checks itself with the tool                         | Missing | Missing |
| B-012    | A check never writes                                                | Missing | Missing |
| B-013    | No output carries an absolute path                                  | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-07 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-07 -->

None.

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
