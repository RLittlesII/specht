---
title: "Epic 0101: Specification linting"
description: "Style and ordering rules for a specification - table rows in id order, retired ids kept as marked rows, exact heading and pipe-table form, frontmatter keys in declared order - warnings by default; id uniqueness rules, errors by default; and a specht format command that reorders rows and keys and edits nothing else"
id: "0101"
type: epic
status: blocked
priority: med
milestone: null
children: ["0101-F1", "0101-F2", "0101-F3", "0101-F4", "0101-F5", "0101-F6"]
created: "2026-10-08"
updated: "2026-10-09"
github_issue: null
---

# Epic 0101: Specification linting

## Summary

Schema version 1 proves a specification is complete: the sections are there,
every claim has a § 9 row, every tag resolves. It says nothing about whether
the document is in order. Claims land out of sequence, § 9 drifts from § 3, a
withdrawn constraint is deleted instead of marked, a heading carries markup, a
row grows an empty trailing cell, frontmatter keys wander. Each is harmless to
the verdict and costly to the reader and to every diff that follows.

This epic adds those checks as rules of schema version 2, each of these style
and ordering rules a warning by default, and adds `specht format`, a separate
command that fixes the ordering findings by moving whole table rows and
frontmatter keys. The check stays read-only.

It also holds the uniqueness rules: an id declared twice is not harmless to
the verdict, because a citation of it names two rows. Those rules are errors
by default (`0101-F6` decision 0001). Style and ordering Features warn;
uniqueness Features error.

The need and its scope were decided by the repository owner on 2026-10-08,
in a requirements session; the decisions are recorded beside the Features
they bind (`0101-F1` decisions 0001 and 0002, `0101-F5` decision 0001).
Uniqueness was added by the owner on 2026-10-09 (`0101-F6` decisions 0001
and 0002).

## Business Value

Four repositories write specifications on one model, by hand and by agent. A
document that is complete but out of order still costs every reader a scan
and every reviewer a diff of moved rows. A document that declares an id twice
costs more: every citation of that id names two rows, and only a reader
finds it. With this epic delivered, a repository is told where a document
drifted from the form, can choose which of those findings it enforces, can
put the ordering right with one command that never touches what a row or key
says, and fails its check on an id declared twice until it lowers or
disables that rule.

## Features

Decomposed by capability dimension: four style and ordering rule Features
and one uniqueness rule Feature, each with its own finding, and the one
command that writes.

| Feature   | Name                           | Specification                                       |
| --------- | ------------------------------ | --------------------------------------------------- |
| `0101-F1` | Row ordering rules             | `src/specht/Rules/Ordering/.spec/README.md`         |
| `0101-F2` | Retired-id rule                | `src/specht/Rules/RetiredIds/.spec/README.md`       |
| `0101-F3` | Heading and table form         | `src/specht/Rules/Form/.spec/README.md`             |
| `0101-F4` | Frontmatter key order          | `src/specht/Rules/FrontmatterOrder/.spec/README.md` |
| `0101-F5` | The format command             | `src/specht.tool/Features/Format/.spec/README.md`   |
| `0101-F6` | In-specification id uniqueness | `src/specht/Rules/DuplicateIds/.spec/README.md`     |

Where the invariant changes, the Feature is cut:

- `0101-F1` judges the order of rows that are all present. `0101-F2` judges
  whether a row is missing. A table can pass one and fail the other.
- `0101-F3` is report-only: `format` never edits cell or heading text
  (`0101-F5` decision 0001), so nothing it finds is fixable by the tool.
  `0101-F1` and `0101-F4` are the two findings `format` fixes.
- `0101-F5` is the only Feature that writes into a consumer's tree.
- `0101-F6` judges whether an id is declared twice in one table. It is the
  one rule Feature here that errors by default, and `format` fixes none of
  it: a fix is a new id, and `format` renumbers nothing.

The AGENTS.md "&" heuristic was applied to `0101-F3`, "Heading and table
form". Both halves share one invariant - a finding about the text of the
document's form that `format` may not fix - and both are narrow once the
version 1 rules already cover most of the ground (`0101-F3` OQ-1). They stay
one Feature; if OQ-1 drops the heading half, the name drops with it.

Every Feature depends on `0001-F7`: a new rule is a new schema version
(brief § 9; `0001-F1` C-5). The five rule Features also depend on `0001-F5`,
whose per-rule severity and disable they are defaulted and switched off by
(`0101-F1` decision 0001; `0101-F6` decision 0001).

## Placement

The rule specifications sit under `src/specht/Rules/`, one folder per
Feature, beside the rule classes they will become. The command's sits in
`src/specht.tool/Features/Format/`, the folder its command will occupy. The
`implementer` may move a specification with its code (`git mv`), because a
specification's identity is its frontmatter `epic` and `id` (`SPEC012`).

This epic file lives at `epics/0101-specification-linting/epic.md`, where
schema version 1's epic glob `epics/**/epic.md` discovers it (`0001-F6`
decision 0001).

## Out of this epic

| Item                                                       | Where it lives instead                                                          |
| ---------------------------------------------------------- | ------------------------------------------------------------------------------- |
| The order of ids inside prose - bullets and sentences      | Not checked (`0101-F1` decision 0002).                                          |
| Editing any cell, heading or value, or renumbering any id  | Never: `format` moves whole rows and keys only (`0101-F5` decision 0001).       |
| A `--fix` on the check                                     | The check stays read-only; fixing is `specht format` (`0101-F5` decision 0001). |
| `upgrade` rewriting a document                             | Still out (brief § 2; `0001-F7` C-2).                                           |
| Per-rule severity and disable                              | `0001-F5` B-010 and B-011; this epic only uses them.                            |
| The schema version mechanism, and the content of version 1 | `0001-F7`, `0001-F1`.                                                           |
| Cell padding and column alignment of a pipe table          | The repository's Markdown formatter, not a rule (`0101-F3` § 5).                |
