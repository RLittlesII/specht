---
title: "Specification: The frontmatter command"
description: "specht frontmatter checks one document's frontmatter, read from a path or from standard input, against the schema for a named kind that the check selects for a root, and reports it as the check does, so frontmatter bound for a GitHub issue is verified before it leaves the author"
type: feature
id: "F8"
epic: "0001"
spec_status: draft
status: blocked
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
depends_on: ["F2", "F3", "F7"]
blocks: []
spikes: []
created: "2026-10-09"
updated: "2026-10-09"
github_issue: null
synced_at: null
---

# Specification: The frontmatter command

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-09 -->

Frontmatter written onto a GitHub issue is never in a `.spec/` tree on disk, so the check cannot see it: a wrong key, a missing field or a bad date is found only after the issue exists, if at all. This Feature removes that failure state: `specht frontmatter` checks one document's frontmatter, read from a path or from standard input, against the schema for a named kind, the schema the check would select for the given root, and reports it with the check's lines and exit codes, so the author verifies the frontmatter before the issue is written.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-09 -->

| #   | Persona                                  | Need                                                                                  | Pain Point Today                                               |
| --- | ---------------------------------------- | ------------------------------------------------------------------------------------- | -------------------------------------------------------------- |
| 1   | The maintainer writing an issue          | Verify the frontmatter of an issue body before posting it                             | The check reads only a tree; an issue body is checked by eye   |
| 2   | The agent drafting an issue body         | A read-only oracle for one document's frontmatter, fed on a pipe, mid-write           | Write the body into the tree to check it, then remove it again |
| 3   | A script or CI step that relays an issue | The check's diagnostic lines and exit codes for one document, so it branches the same | No command checks a document that is not in the tree           |
| 4   | `0001-F2`, `0001-F3`, `0001-F7`          | A sibling command that reuses the check's line format, summary and schema selection   | N/A (internal dependency)                                      |

### Assumptions

| ID  | Assumption                                                                                                                                                                                   |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The command line is `specht frontmatter --kind <kind> <path\|-> [--root <dir>]`, beside `specht`, `init` and `upgrade`; `--root` defaults to the current directory as it does for the check. |
| A-2 | The kinds are the three frontmatter schemas a schema version holds, named as their files are: `task`, `feature-spec` and `epic` (OQ-1).                                                      |
| A-3 | A GitHub issue body carries its frontmatter as a markdown document does, a `---`-delimited mapping on its first line, so one reader serves both.                                             |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-09 -->

| ID    | Claim                                                                                                                                                                                                                    | Source                                       | Status  |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------- | ------- |
| B-001 | Given `--kind` and a path naming a file, this Feature checks the frontmatter of that file's text.                                                                                                                        | owner, 2026-10-09                            | Active  |
| B-002 | Given `-` in place of a path, this Feature checks the frontmatter of the text read from standard input.                                                                                                                  | owner, 2026-10-09                            | Active  |
| B-003 | Given frontmatter that fails its kind's schema, this Feature reports each failure under that kind's rule id: `SPEC002` for `feature-spec`, `SPEC003` for `task`, `SPEC004` for `epic`.                                   | brief § 4; owner, 2026-10-09; C-1            | Active  |
| B-004 | Given frontmatter that fails its kind's schema on a key, this Feature reports that failure at the line of that key in the document.                                                                                      | `0001-F1`; C-2                               | Active  |
| B-005 | Given a document with no frontmatter, this Feature reports `SPEC001` at line 1.                                                                                                                                          | brief § 4; C-1                               | Active  |
| B-006 | Given a root, this Feature checks the document against the kind's frontmatter schema of the version and source the check selects for that root.                                                                          | `0001-F7` B-001, B-009, B-011; decision 0001 | Active  |
| B-007 | Given a document that satisfies its kind's schema and that a rule other than `SPEC001` to `SPEC004` would fail in a tree, this Feature reports no violation.                                                             | owner, 2026-10-09; C-1; § 5 row 2            | Active  |
| B-008 | Given a root whose own specifications carry violations, this Feature reports only the violations of the document it was given.                                                                                           | owner, 2026-10-09; C-1                       | Active  |
| B-009 | Given a document in the root's tree, this Feature reports for it the same `SPEC001` to `SPEC004` violations the check reports for that file.                                                                             | C-2; decision 0001                           | Active  |
| B-010 | Given a violation, this Feature prints it on stdout as the one MSBuild-shaped line `0001-F2` B-001 and B-016 print for it.                                                                                               | owner, 2026-10-09; `0001-F2` B-001, B-016    | Amended |
| B-011 | Given a document named by a path, each violation line names the document by that path as it was typed.                                                                                                                   | `0001-F2` B-013; brief § 9                   | Active  |
| B-012 | Given a document read from standard input, each violation line names the document `<stdin>`.                                                                                                                             | owner, 2026-10-09                            | Active  |
| B-013 | Given a run completes, this Feature prints after the violations the error count and the warning count, each as `0001-F3` B-005 counts it.                                                                                | owner, 2026-10-09; `0001-F3` B-005; OQ-3     | Active  |
| B-014 | Given at least one error-severity violation, this Feature exits with code `1`.                                                                                                                                           | `0001-F2` B-003; C-6                         | Active  |
| B-015 | Given no violation, this Feature exits with code `0`.                                                                                                                                                                    | `0001-F2` B-003; C-6                         | Active  |
| B-016 | Given `--root` names a path that is not a directory, this Feature writes a message naming that path as it was given on stderr, writes nothing on stdout, and exits with code `2`.                                        | `0001-F2` B-005; C-6                         | Active  |
| B-017 | Given a root with no manifest at the manifest path, this Feature writes a message naming the manifest path on stderr, writes nothing on stdout, and exits with code `2`.                                                 | `0001-F2` B-006; C-6; decision 0001          | Active  |
| B-018 | Given a file at the manifest path that does not parse into the manifest shape, this Feature writes a message naming the manifest path relative to the root on stderr, writes nothing on stdout, and exits with code `3`. | `0001-F2` B-007; C-6                         | Active  |
| B-019 | Given a manifest pinning a version the tool does not ship, this Feature names the pinned version and the versions it ships on stderr, writes nothing on stdout, and exits with code `3`.                                 | `0001-F7` B-003; C-6                         | Active  |
| B-020 | Given `--kind` names a kind other than `task`, `feature-spec` and `epic`, this Feature names that kind as typed and the three kinds on stderr, writes nothing on stdout, and exits with code `4`.                        | `0001-F2` decision 0003; C-6; OQ-1           | Active  |
| B-021 | Given a path that names no file, this Feature writes a message naming that path as it was typed on stderr, writes nothing on stdout, and exits with code `4`.                                                            | `0001-F2` decision 0003; C-6                 | Active  |
| B-022 | Given any run, stdout carries only the violation lines and the summary, and every message about the tool itself goes to stderr.                                                                                          | `0001-F2` B-009, C-3                         | Active  |
| B-023 | Given any run, no file is created, modified or deleted, under the root or anywhere else.                                                                                                                                 | brief § 9; C-3                               | Active  |
| B-024 | Given any output on stdout or stderr, every path the tool derives is relative to the root with `/` separators, and the only path that may be absolute is one typed on the command line, echoed as given.                 | brief § 9; `0001-F2` B-013                   | Active  |
| B-025 | Given the same document text, kind and root, two runs print the same stdout and exit with the same code.                                                                                                                 | brief § 9; C-4                               | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-09 -->

| ID  | Constraint                                                                                                                                 | Rules Out                                                                                                                                             |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | The command evaluates the frontmatter rule alone, so `SPEC001` to `SPEC004` are the only rule ids it can report.                           | A structure, `.feature`, claim, child, dependency or approval rule run over the document; a new `SPEC###` id for this command (brief § 4).            |
| C-2 | The command and the check reach one verdict on one frontmatter: the same reader, the same schema selection and the same rule produce both. | A second frontmatter reader or schema loader that can disagree with the check; an embedded schema used when the root has no manifest (decision 0001). |
| C-3 | The command is read-only: it writes nothing, anywhere.                                                                                     | A `--report` or `--fix`; writing the standard-input text to a file under the root or to a temporary file in order to check it.                        |
| C-4 | Deterministic and offline: the result is a function of the document text, the kind, the root's manifest and the tool version.              | A network call, a GitHub call or a fetch of an issue by number or URL; a clock, locale or machine name in the output.                                 |
| C-5 | A command parses, calls the engine and folds the result into an exit code; nothing else lives in the command (`0001-F2` C-4).              | Frontmatter reading or schema evaluation inside `Features/Frontmatter/`.                                                                              |
| C-6 | Exit codes are `0001-F2` C-2's constants, with the meanings the check gives them.                                                          | A sixth exit code; a code that means one thing to this command and another to the check.                                                              |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-09 -->

| #   | Item                                                                                                                                       | Exclusion Reason                                                                                                                        |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Fetching a document from GitHub or any network, an issue by number or URL included                                                         | C-4; the check is offline (brief § 9). A caller pipes the issue body to standard input.                                                 |
| 2   | The cross-file and structural rules: `children` and `depends_on` resolution, dependency symmetry, `@B` tags, § 9 rows, the twelve sections | They need the tree; the check owns them (`0001-F2`, `0001-F1`). B-007 is the boundary.                                                  |
| 3   | Any write: a `--report` file, a fix, or posting the frontmatter onto an issue                                                              | C-3; the only writers are `init`, `upgrade`, the pin command, the check's `--report` and `format` (brief § 9; `0001-F7` decision 0005). |
| 4   | A new rule id, or an issue kind with a schema of its own                                                                                   | The rule vocabulary is fixed per schema version (brief § 4); a new kind is a new schema version (OQ-1).                                 |
| 5   | `--json` and the report document for this command                                                                                          | Not decided (OQ-2); `0001-F3` owns the report document.                                                                                 |
| 6   | Frontmatter key order                                                                                                                      | `0101-F4`, in the schema version after `0.1.0`; its number is `0001-F7` OQ-18.                                                          |
| 7   | Mirroring an issue's labels into the document, or the document's status into labels                                                        | Label-mirror drift is not this tool's (brief § 9).                                                                                      |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-09 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-09 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-09 -->

Pending: owned by `test-writer`, written after agreement.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-09 -->

| Claim ID | Scenario                                                  | Test    | Status  |
| -------- | --------------------------------------------------------- | ------- | ------- |
| B-001    | A document named by a path is checked                     | Missing | Missing |
| B-002    | A document on standard input is checked                   | Missing | Missing |
| B-003    | A failure is reported under its kind's rule id            | Missing | Missing |
| B-004    | A failure on a key is reported at that key's line         | Missing | Missing |
| B-005    | A document with no frontmatter is reported                | Missing | Missing |
| B-006    | The schema is the one the check selects for the root      | Missing | Missing |
| B-007    | A rule that needs the tree does not run                   | Missing | Missing |
| B-008    | The root's own specifications are not checked             | Missing | Missing |
| B-009    | A document in the tree gets the check's verdict           | Missing | Missing |
| B-010    | A violation is printed as a diagnostic line               | Missing | Missing |
| B-011    | A document named by a path is reported by that path       | Missing | Missing |
| B-012    | A document on standard input is reported as stdin         | Missing | Missing |
| B-013    | A run ends with the error and warning counts              | Missing | Missing |
| B-014    | An error fails the run                                    | Missing | Missing |
| B-015    | A clean document succeeds                                 | Missing | Missing |
| B-016    | A missing root is a missing-input failure                 | Missing | Missing |
| B-017    | A root without a manifest is a missing-input failure      | Missing | Missing |
| B-018    | An unreadable manifest is an invalid-manifest failure     | Missing | Missing |
| B-019    | A version the tool does not ship is invalid configuration | Missing | Missing |
| B-020    | An unknown kind is not found                              | Missing | Missing |
| B-021    | A path that names no file is not found                    | Missing | Missing |
| B-022    | Messages about the tool go to standard error              | Missing | Missing |
| B-023    | A run writes nothing                                      | Missing | Missing |
| B-024    | No derived path in the output is absolute                 | Missing | Missing |
| B-025    | The same input gives the same verdict                     | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-09 -->

- 2026-10-09, B-010 amended in place: it restated the line form `0001-F2` B-001 held, which now carries the violation's identifier (`0001-F2` decision 0006). B-010 cites `0001-F2` B-001 and B-016 for the form and no longer copies it.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-09 -->

| ID   | Question                                                                                                                                                                                                               | Blocks       | Resolution                                                 |
| ---- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------ | ---------------------------------------------------------- |
| OQ-1 | Does a GitHub issue need an `issue` kind with a schema of its own, or does it reuse `task`? A new kind is a new frontmatter schema, so a new schema version (brief § 4).                                               | B-003, B-020 | Open. Proceeds on the default: an issue reuses `task`.     |
| OQ-2 | Does the command take `--json`, and if so is its document `0001-F3`'s report for one document, and what do that report's specification and item counts hold for a document read from standard input?                   | —            | Open. No claim is written; § 5 row 5 until it is answered. |
| OQ-3 | Beside the error and warning counts (B-013), does the summary carry the check's other lines - the specification count per layout, the item count, the count of rule ids evaluated - for a document that has no layout? | B-013        | Open. B-013 claims only the error and warning counts.      |
| OQ-4 | What does the command do when `--kind` or the document argument is missing: a usage message and which exit code, given `0001-F2` C-2 forbids Spectre's `-1`?                                                           | —            | Open. No claim is written until it is answered.            |

## 12. Sign-off

<!-- last written by: spec-author, 2026-10-09 -->

| Section | Status | Reviewer      | Note                   |
| ------- | ------ | ------------- | ---------------------- |
| 1-5     | 🟡     | spec-reviewer | Awaiting first review. |

## Tasks

None yet. Cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
