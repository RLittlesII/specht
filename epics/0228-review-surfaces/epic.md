---
title: "Epic 0228: review surfaces"
description: "The read-only answers the reviewer of a change needs and the check does not give: the tree's facts as one document, the delta between two runs, a citation resolved to its row, an approval gate on open questions, a report scoped to the files a change touched, and, if the owner widens the tool's scope, an item's claims checked against its specification and a commit message's citations checked against the tree"
id: "0228"
type: epic
status: needs-decomposition
priority: med
milestone: null
children: []
created: "2026-10-15"
updated: "2026-10-15"
github_issue: null
---

# Epic 0228: review surfaces

## Summary

The `spec-reviewer` role judges a diff against the claims its commit cites,
the § 9 matrix, § 5 and the accepted records (`.agents/roles/spec-reviewer.md`).
`specht` judges a tree: it reports every violation in every specification and
returns nothing else. A review surface is a read-only answer the reviewer of a
change needs and the check does not give: the tree's facts as one document, the
delta between two runs, a citation resolved to its row, an approval gate on open
questions, a report scoped to the files a change touched, and, where the owner
widens the tool's scope, an item's claims checked against its specification and
a commit message's citations checked against the tree.

The need was decided by the repository owner on 2026-10-15, from two readings:
the tool run on this repository's own tree (37 specifications, 0 errors, 196
warnings, every one `SPEC060` lowered to warning by the manifest), and the
reviewer contract read against what that run can answer. This file is the
record of that decision. Nothing here is designed: no command's options are
fixed, no document's shape is chosen and no rule's id is assigned. The split
into Features is a candidate the owner has not confirmed, and no Feature
specification exists yet.

## Business Value

**The problem.** The reviewer's input is a diff and its citations; the tool's
input is a tree. The 37 specifications carry 1273 cross-Feature citations of
the form `NNNN-Fn B-nnn`, `C-n` or `OQ-n`, and no rule resolves one: the
reviewer's first step, that the claims a commit cites exist and are Active, is
a search by hand. The run reports 196 warnings and says nothing about which of
them a given change introduced. The tree's facts - a claim's text and status,
an open question's resolution, the records beside a specification - are read
from Markdown for every review.

**Why now.** `0001-F12` and `0001-F13` are approved, and the reviewer contract
names, step by step, what it reads; each step the tool cannot answer is one an
agent answers from memory or by grep. Lesson 0007 records eight of sixteen
merges in one day whose citation the rule's check did not reach, and that
nothing checks a commit message.

**The solved state.** The reviewer of a change asks the tool, in one call
each, what a citation resolves to, which violations the change introduced,
whether a specification is fit to be approved, and what the tree says about a
claim; a script asks the same and gets the same answer.

Who it is for. Where two of them want different things, the primary persona's
need wins.

| Persona                                         | Standing  | Goal                                                                            |
| ----------------------------------------------- | --------- | ------------------------------------------------------------------------------- |
| The reviewer of a change                        | Primary   | Judge the diff and its citations against the tree, with no search by hand       |
| An agent authoring or repairing a specification | Secondary | Resolve a citation before writing it, and see what a change to the tree changed |
| A script or CI step                             | Secondary | The same answers as documents, with no output to scrape                         |

What it costs them today:

| Pain                               | Cost                                                                                |
| ---------------------------------- | ----------------------------------------------------------------------------------- |
| A citation is checked by grep      | 1273 citations, each read by hand or trusted                                        |
| A warning has no owner             | 196 warnings on the tree, none attributed to a change                               |
| A renumber is invisible            | A contiguous renumber leaves no gap, and `0101-F2` sees gaps only                   |
| An open question survives approval | That no question is open is a hand check, recorded in `0001-F13`'s round-4 sign-off |

## Success criteria

The tiers are derived: the owner approved the plan's order of work, which
authors candidates A to E and raises F and G as questions before any
specification. The owner tiered no criterion, and OQ-1 asks. A Must is the
epic; a Could is delivered only if the owner's answer admits it.

| #   | Criterion                                                                                                                                                                                                                                                                                                                                           | Tier  | Notes                                                                                                                   |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----- | ----------------------------------------------------------------------------------------------------------------------- |
| R1  | The tree's facts are answerable as one read-only document per specification: its path and identity, its statuses, each claim with its text, status and line, each constraint, each § 5 row, each open question with whether it is resolved, each § 12 row, the § 10 deltas, its dependencies, and the records beside it by number, title and status | Must  | Epic `0132` candidate A. The document gets a published schema as the report has (`0001-F3` C-1)                         |
| R2  | Two documents from two runs give the violations introduced and the violations fixed, keyed by an identity that survives a line move, never by line                                                                                                                                                                                                  | Must  | The identity is an additive field on the report, named in its schema (`0001-F3` C-1, C-4)                               |
| R3  | Two facts documents give the claim ids added, the ids removed, the claim text changed under an unchanged id, the status transitions and the open questions resolved                                                                                                                                                                                 | Must  | The contiguous renumber `0101-F2` cannot see                                                                            |
| R4  | A citation resolves in one call to its file, line, text and status, or the call fails with exit `4`                                                                                                                                                                                                                                                 | Must  | The forms are `NNNN-Fn B-nnn`, `C-n`, `OQ-n`, `ADR-nnnn`, a Feature's decision and a lesson by number                   |
| R5  | A citation in a specification that resolves to no row and no file is reported, as a warning by default                                                                                                                                                                                                                                              | Must  | `SPEC070` is the name reserved today (`0001-F1` § 5 row 7); the id is the vocabulary's, see "Boundaries"                |
| R6  | An approved specification with an open question whose Resolution is empty or undated is reported, as an error by default                                                                                                                                                                                                                            | Must  | Beside `SPEC060` and `SPEC061`; a consumer lowers it as it lowers them (`docs/what-it-checks.md`)                       |
| R7  | The check's output can be scoped to named files, with the whole tree still evaluated and one line saying how many violations fell outside                                                                                                                                                                                                           | Must  | The run on the list `git diff --name-only` gives is the reviewer's run; whether the exit code follows the scope is OQ-6 |
| R8  | An item's claims resolve to its parent's § 3, and an item `done` while a claimed id's § 9 row is `Missing` is reported                                                                                                                                                                                                                              | Could | On the task layout the tool reads; whether this repository's items join is OQ-4                                         |
| R9  | A commit message's citations resolve as R4 resolves them, and a dangling or malformed one fails the call                                                                                                                                                                                                                                            | Could | Whether a commit message is within the tool's scope is OQ-5                                                             |

## Features

**A candidate cut, awaiting the owner's confirmation.** Nothing below is
agreed. No Feature here has an id, a specification or a place in `children`:
a candidate takes its Feature id when its specification is written, and joins
`children` then, so that every child resolves to a file.

The candidates are cut by capability dimension, where the invariant changes:

| Candidate | Name                                | Invariant it would carry                                                                                      | Serves | Tier  |
| --------- | ----------------------------------- | ------------------------------------------------------------------------------------------------------------- | ------ | ----- |
| A         | Tree facts on the command line      | The tree's facts are a read-only function of the tree and the pinned schema version, given as structured data | R1     | Must  |
| B         | The delta between two runs          | Two documents in, one delta out; no tree, no repository and no history is read                                | R2, R3 | Must  |
| C         | Citation resolution                 | A citation resolves one way wherever it is read: by the command, by the rule and by the commit-message check  | R4, R5 | Must  |
| D         | The approval gate on open questions | A specification approved with an open question is a violation of its own, judged from its own § 11            | R6     | Must  |
| E         | A scoped report                     | The verdict is the tree's; only what is printed and counted is the caller's list                              | R7     | Must  |
| F         | Item-to-claim rules                 | An item's claims are a subset of its specification's § 3, and `done` is never beside `Missing`                | R8     | Could |
| G         | Commit-message citations            | A commit message's citations resolve as a specification's do, through candidate C                             | R9     | Could |

Where the candidate cut was tested, and what the owner is asked to confirm:

- **Candidate A is epic `0132`'s candidate A**, cut there from MCP so that it
  survives MCP never being built. One epic owns it. This epic depends on it:
  R3 reads two of its documents. R1 is listed so that the dependency is named,
  not so that two epics confirm one Feature; which epic confirms it is OQ-2.
- **Candidate B is cut from candidate A.** R2 reads two report documents and
  needs no facts document; R3 reads two facts documents. Both carry one
  invariant, that nothing but the two documents is read, and neither can fail
  with candidate A correct.
- **Candidate C joins a command and a rule** because they carry one invariant:
  the resolver is one, and a citation the command resolves is one the rule
  accepts. They would be cut apart if the rule must wait for a schema version
  and the command need not (OQ-3).
- **Candidate D is cut from candidate C.** It reads one specification's
  frontmatter and § 11 and resolves nothing across files; it is the one
  candidate the existing parse already serves.
- **Candidate E may be no Feature.** `--only` is an option on the check
  command, and its claims may belong on `0001-F2` and `0001-F3` with a § 5 row
  saying the tree is still fully evaluated. Whether it is a Feature or a delta
  is part of OQ-1.
- **Candidates F and G each widen the tool's scope.** F changes the task
  schema's shape; G reads a file that is not in the tree. Each waits on the
  owner's answer (OQ-4, OQ-5) before a specification is written, as the
  questions reference requires for the manifest's shape and a public surface.
- **The AGENTS.md "&" heuristic** found no conjunction in any candidate name.

## What this epic waits on

This epic reads what other Features build and builds none of it. Candidates A
to C resolve claims, statuses and tests as `0001-F12` and `0001-F13` resolve
them and read the report `0001-F3` defines; none of that is delivered yet.
Nothing below stops a Feature specification being written.

| Capability                                                                          | Specified in                     | Work items                                            | Blocks                                                                            |
| ----------------------------------------------------------------------------------- | -------------------------------- | ----------------------------------------------------- | --------------------------------------------------------------------------------- |
| The § 9 matrix resolved against the companion scenarios and the declared test files | `0001-F12` (approved, 21 claims) | `0222` delivered; `0227` is open in pull request #204 | A, B, C: the resolver they read through                                           |
| `trace`: every claim's status, scenarios and tests, as lines or one document        | `0001-F13` (approved, 20 claims) | None cut                                              | A: the shape of a per-claim answer, and its `docs/schema/trace.schema.json`       |
| A rule's full text, and `expected` per finding                                      | `0001-F3` B-019, B-020, C-2, C-6 | `0037` to `0040`                                      | B: the identity hashes `expected`, and every `expected` in today's report is `{}` |
| `rules`: every rule id with its family, default level and manifest level            | `0001-F10` (draft)               | None cut                                              | D: the family the gate joins, and how a consumer sees its level                   |
| `frontmatter`: one document's frontmatter checked from a path or standard input     | `0001-F8` A-1, B-012 (draft)     | None cut                                              | G: the precedent for a document read from outside the tree                        |

## Constraints

Every constraint is hard. None is this epic's invention: each is an existing
rule of the repository or the owner's word, and each is listed so that a
Feature specification cut from this epic carries it forward.

| Constraint                                                                                                       | Source                                                                   | What it rules out                                                                                                                             |
| ---------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------- |
| The check never touches the network, and no surface here does                                                    | AGENTS.md § Invariants; brief § 9                                        | A fetch from any candidate; only `init` and `upgrade` may fetch                                                                               |
| The same inputs give the same answer                                                                             | AGENTS.md § Invariants; `0001-F3` C-3                                    | A timestamp or a run id in a facts, delta or resolution document; two answers for the same two documents                                      |
| Nothing is written outside the existing list of writes                                                           | AGENTS.md § Invariants; brief § 9                                        | A write from any candidate; a cache beside the tree; a baseline file                                                                          |
| No output carries an absolute path                                                                               | AGENTS.md § Invariants; brief § 9                                        | A path in a facts document, a delta or a resolution that is not relative to the root the tool was given                                       |
| The rule vocabulary is fixed per schema version; a new `SPEC###` is a new schema version, after brief § 8 step 6 | `0001-F1` C-5; `0001-F7` C-11; brief § 9                                 | R5 or R6 shipping in a published version; a rule added to a version after it is locked, except as `0001-F12` A-4 allows for `0.2.0`           |
| The tool is given no base, diff or pull-request input                                                            | `0055-F2` C-7                                                            | A `--base` option, a git call for anything but the file list, a rule that compares two trees; candidate B takes two documents the caller made |
| Specification revision history is out of scope                                                                   | brief § 2                                                                | Candidate B keeping any run; a store of past reports; a history of a claim                                                                    |
| Per-rule levels and `--strict` are the only way to accept or refuse a finding                                    | brief § 5; `docs/what-it-checks.md`                                      | A baseline file of accepted violations; a suppression comment; a third mechanism beside the manifest's `rules`                                |
| No rule reads record content                                                                                     | `docs/what-it-checks.md`; `0101-F7` C-2                                  | Candidate C opening a decision, an ADR or a lesson to resolve a citation; it reads file names only                                            |
| The command line stays the agent-facing contract of record; `--json` and `--explain` stay the contract           | Owner, 2026-10-09; epic `0132`; `specht-conventions` questions reference | An answer available only through MCP; a surface here that MCP serves and the command line does not                                            |

## Boundaries with recorded specifications

Each record below already draws a line this epic touches. This file amends
none of them: the amendment is the work of the Feature that owns the change.

| Record                                                                                | Existing boundary                                                                                                                             | Effect of this epic                                                                                                                                                                                                                                                                          |
| ------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Epic `0132` candidate A                                                               | Tree facts on the command line, cut from MCP, unconfirmed                                                                                     | Shared with this epic's candidate A. One epic confirms it (OQ-2) and the other depends on it; no Feature is owned twice                                                                                                                                                                      |
| Epic `0132` § Open questions, "a check of one specification has no command-line form" | Asked of the owner, unanswered                                                                                                                | Candidate E answers it without a second command: the whole tree is checked and the output is scoped                                                                                                                                                                                          |
| `0001-F1` § 5 row 7 and C-5; `specht-conventions` specs reference                     | `SPEC070` is named for constraint-citation resolution and has no specification; a new rule is a new schema version                            | Candidate C specifies it. The id is the vocabulary's at the schema version that ships it: item `0169` renumbers every rule id to four digits grouped by the thousands digit, so the candidate takes whatever id that vocabulary assigns, and `SPEC070` is the name it is cited by until then |
| `0101-F1` OQ-1                                                                        | Which rule ids epic `0101` takes, around the reserved `SPEC070`                                                                               | Resolved in the same schema-version step that assigns candidates C and D their ids (OQ-3)                                                                                                                                                                                                    |
| `0101-F2` C-1                                                                         | A gap is judged per table, from 1 to the highest number present                                                                               | Unchanged. A contiguous renumber leaves no gap; R3 sees it from two facts documents                                                                                                                                                                                                          |
| `0101-F9` B-024 to B-026                                                              | `SPEC015` reports a stamp whose role the manifest's `owners` key does not give the section (decision 0004, 2026-10-14)                        | Unchanged and not a candidate: section ownership is specified. This repository's manifest sets no `owners`, so the rule reports nothing here until it does                                                                                                                                   |
| `docs/what-it-checks.md`                                                              | "No rule reads `adr/`, `lessons/` or `decisions/`"; "`specht` never reads `.issue/`"                                                          | The first gains one line only if candidate C is confirmed: a rule reads record file names, never content. The second changes only if OQ-4 lets `taskFiles` name this repository's items; candidate F on the task layout the tool reads changes neither                                       |
| `0055-F2` C-7                                                                         | The tool is given no base, diff or pull-request input; its Rules Out names a rule that compares two trees                                     | Unchanged. Candidate B is not a rule and reads no tree; whether C-7 stands as written beside it is OQ-8                                                                                                                                                                                      |
| `0001-F8` A-1, B-012                                                                  | `specht frontmatter --kind <kind> <path\|->` checks one document's frontmatter from a path or standard input and reports it as the check does | The precedent for candidate G: a document read from outside the tree, reported as the check reports. G cites it and does not duplicate it                                                                                                                                                    |
| `0001-F3` C-1, C-4                                                                    | The report's shape is one published schema, and within a tool major it changes only by addition                                               | Candidate B adds the violation identity as a field the schema names; a facts or delta document gets a schema of its own (OQ-3)                                                                                                                                                               |

## Open questions

None is resolved here. Each travels to the Feature specification it binds,
where it takes an `OQ-n` id of that Feature's and is resolved in place. OQ-1
to OQ-7 are the plan's; OQ-8 was raised while writing this file, and the plan
does not ask it.

| OQ   | Question                                                                                                                                                                                       | Who answers                                                                                                                                 |
| ---- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Is the candidate cut in "Features" the cut, candidate E included?                                                                                                                              | The owner                                                                                                                                   |
| OQ-2 | Is candidate A confirmed here or in epic `0132`? The epic that confirms it owns the Feature; the other depends on it                                                                           | The owner                                                                                                                                   |
| OQ-3 | Which schema version carries R5's rule and R6's gate, and do the facts and delta documents get published schemas as the report has (`0001-F3` C-1)?                                            | The owner for the version, under `0001-F7` C-11; the `spec-author` in candidates A's and B's specifications for the schemas, then the owner |
| OQ-4 | May `taskFiles` name this repository's YAML items, so that candidate F's rules run here, or do they run on the `<epic>-<nn>-<slug>.md` layout alone?                                           | The owner                                                                                                                                   |
| OQ-5 | Is a commit message within the tool's scope? It is not in the `.spec/` tree; the resolver is, and `0001-F8` is the precedent for a document read from outside it                               | The owner                                                                                                                                   |
| OQ-6 | Does `--only` filter the exit code, or only what is printed and counted?                                                                                                                       | The owner                                                                                                                                   |
| OQ-7 | What identifies a violation across two runs? The plan's members are the rule id, the file, the identifier and a hash of `expected`; which are the identity's?                                  | The `spec-author` in candidate B's specification, then the owner                                                                            |
| OQ-8 | `0055-F2` C-7 rules out a rule that compares two trees. Candidate B is a command over two documents and reads no tree. Does C-7 stand as written, or does it gain a decision naming the delta? | The owner                                                                                                                                   |

## Placement

No Feature specification exists, so none is placed. A specification sits
beside what it specifies: a command's under its folder in
`src/tool/Features/<Command>/`, as candidates A, B, G and C's command would;
`--only`'s beside the check command it extends, under
`src/tool/Features/Check/`; a rule's under `src/specht/Rules/<Name>/`, as
candidate D, F and C's rule would. Which folder each takes is the design's
choice.

This epic file lives at `epics/0228-review-surfaces/epic.md`, where the epic
glob `epics/**/epic.md` discovers it (`0001-F6` decision 0001).

## Status

`needs-decomposition`: the candidate cut is not confirmed and no child
exists. The epic is not `blocked`: nothing in "What this epic waits on" stops
a Feature specification being written, and the owner did not decide that the
epic waits. The next step is the owner's: confirm or change the candidate
cut, answer OQ-2, OQ-4 and OQ-5, and each confirmed Feature then begins with
its own specification.

## Out of this epic

| Item                                                                                           | Where it lives instead                                                                                        |
| ---------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| A § 5 exclusion the diff delivered, a test weakened, a scenario drifting from its claim's text | The reviewer's judgement: not a rule, and not deterministic                                                   |
| `Tier` traits, `*.Tests.cs` placement and an absolute path in a fixture                        | Epic `0201` (deferred): code concerns, with Roslyn as the home                                                |
| A baseline file of accepted violations                                                         | Never: a change's findings are candidate B's, and a repository's policy is the manifest's `rules` (brief § 5) |
| Section ownership                                                                              | `0101-F9` B-024 to B-026 (`SPEC015`); specified, not a candidate                                              |
| Anything MCP                                                                                   | Epic `0132`; this epic adds command-line answers MCP may later serve                                          |
| The GitHub Action and annotations on a pull request                                            | Epic `0142`                                                                                                   |
| Rule text, `--explain` and `expected`                                                          | `0001-F3`; candidate B hashes `expected` and defines none of it                                               |
| Building `0001-F12`, `0001-F13`, `0001-F10` or `0001-F8` here                                  | Their own items; this epic waits on them and builds none of them                                              |
