---
title: "Epic 0194: Spectre.Console presentation"
description: "Spectre.Console widgets in the specht command line: the specification tree as a tree, a closed-set argument chosen from a selection list at a terminal, and the check's warnings and errors as a coloured table, each requested so that the MSBuild-shaped stream, the JSON document, the report file and the exit code stay what they are today"
id: "0194"
type: epic
status: needs-decomposition
priority: med
milestone: null
children: []
created: "2026-10-12"
updated: "2026-10-31"
github_issue: null
---

# Epic 0194: Spectre.Console presentation

## Summary

`specht` prints plain text: one MSBuild-shaped line per violation and the
summary lines, a tab-separated rule listing, a line per file. The owner asked
on 2026-10-12 for three Spectre.Console widgets in its place where a person
reads it: the specification tree shown as a `Tree`, command-line options
offered as drop-downs, and a table of warnings and errors, each in its proper
colour.

The widgets are Spectre.Console's; the command host they render in is
Spectre.Console.Cli (`spectre-cli` skill). A terminal has no drop-down: the
owner's "drop-down" is read here as Spectre.Console's selection prompt, which
`specht schema pin` already offers when no version is given (`0001-F7` B-057,
B-058, B-113).

The stream is a contract. The pre-commit hook, the `Specht` target and CI
carry the check's lines unchanged (`0001-F2` B-010), the build's annotation
step and epic `0142`'s action read them, and `0001-F2` C-3 rules out colour
markup on stdout. This epic adds presentation beside that contract and never
in place of it: how a run asks for the widgets, and where they go, is OQ-1 and
OQ-2.

This file records the owner's three lines of 2026-10-12, with the stub's typo
and its "Spectre Cli" naming corrected. Nothing here is designed: no option is
named, no command is chosen for the tree and no colour is fixed. The split
into Features is a candidate the owner has not confirmed, and no Feature
specification exists.

## Business Value

**The problem.** Every finding prints in one uncoloured shape, so an error and
a warning look alike until the severity word is read. The specification tree
has no view: a reader follows `children` from file to file, or reads
`specht facts` one line per fact (`0132-F2` B-024). A command that needs a
value from a closed set — a rule id and a level for `specht rules set`, a rule
id for `--explain`, a kind for `specht frontmatter` — requires the reader to
know the set; only `specht schema pin` offers its set as a list.

**The solved state.** A person at a terminal can ask for the tree as a tree,
the findings as a coloured table, and a missing closed-set value as a
selection list. A hook, a build target, CI or an agent that asks for none of
these gets what it gets today, byte for byte.

| Persona                                                    | Standing  | Goal                                                                                 |
| ---------------------------------------------------------- | --------- | ------------------------------------------------------------------------------------ |
| A developer reading the check at a terminal                | Primary   | Tell errors from warnings at a glance, with the counts beside them                   |
| A maintainer reading the specification tree                | Primary   | See epics, their Features and each Feature's state as one hierarchy                  |
| A person running a command that needs a closed-set value   | Primary   | Choose the value from the set the pinned schema version ships, without looking it up |
| A hook, a build target, CI, epic `0142`'s action, an agent | Secondary | The lines, the JSON document, the report file and the exit code unchanged            |

## Success criteria

`R1` to `R3` are the owner's three lines of 2026-10-12. `R4` is not the
owner's: it is the boundary AGENTS.md § CLI and § Invariants already set,
named so that a Feature specification can cite it. The owner gave no tier,
and none is assigned here.

| #   | Criterion                                                                                                                                                             | Notes                                                                |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------- |
| R1  | The specification tree can be shown as a Spectre.Console `Tree`                                                                                                       | Which command shows it is OQ-3; what its nodes are is OQ-4           |
| R2  | A command-line value from a closed set can be chosen from a selection list at an interactive terminal, as `specht schema pin` chooses a version                       | The precedent is `0001-F7` B-057, B-058, B-113; which values is OQ-6 |
| R3  | The check's warnings and errors can be printed as a table, each severity in its own colour                                                                            | The colours are OQ-8; the commands covered are OQ-9                  |
| R4  | A run that does not ask for R1 or R3 prints what it prints today; no request changes the verdict, the exit code, the `--json` document, the `--report` file or stderr | AGENTS.md § CLI, § Invariants; `0001-F2` B-009, B-010; OQ-1, OQ-2    |

## Features

**A candidate cut, awaiting the owner's confirmation.** Nothing below is
agreed. No Feature here has an id, a specification or a place in `children`:
a candidate takes its Feature id when its specification is written, and joins
`children` then, so that every child resolves to a file.

| Candidate | Name                        | Invariant it would carry                                                                                                                                | Serves |
| --------- | --------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ |
| A         | Presentation request        | Only a run that asks for a widget renders one, and the request changes nothing but how that run's stdout looks                                          | R4     |
| B         | Specification tree view     | The tree shows what the check discovers, read through the discovery and parse the check uses, and nothing it does not                                   | R1     |
| C         | Diagnostics table           | The table holds every violation the same run's lines hold, with the same path, line, rule id, message and identifier, and the counts the summary gives  | R3     |
| D         | Interactive argument choice | A prompt offers exactly the closed set the pin defines, only where a run would otherwise fail for the missing value, and never waits without a terminal | R2     |

What the owner is asked to confirm:

- **A is cut from B and C.** A can fail — a widget reaching the hook's run —
  while B and C render correctly, and B and C share it. A could instead be a
  level of `0001-F17`'s verbosity option (`0001-F17` OQ-6 asks for a louder
  level); `0001-F17` decision 0001 separates how much is printed from how it
  looks, which is why A is cut here.
- **D needs no A.** A prompt is input, not output, and the precedent selects
  it by the terminal and by the missing value (`0001-F7` B-057, B-113).
- **D is one Feature, not an amendment to each command.** `0001-F7` holds its
  selector in its own claims; D's claims could instead be deltas on
  `0001-F11` (`rules set`), `0001-F3` (`--explain`) and `0001-F8`
  (`frontmatter --kind`). One Feature carries one rule for every prompt.
- **The AGENTS.md "&" heuristic.** No candidate name joins two nouns. The
  epic joins three widgets; they are cut apart above because each can fail
  with the others correct.

## What this epic waits on

None of these stops a Feature specification being written. Each stops a
candidate being built as its default reads.

| Waits on                                                                    | Recorded in              | Blocks               |
| --------------------------------------------------------------------------- | ------------------------ | -------------------- |
| The facts the tree shows, read through the check's discovery; not yet built | `0132-F2` B-001 to B-004 | B, on OQ-3's default |
| Whether an epic file has facts: `0132-F2` gives none today                  | `0132-F2` B-026, OQ-6    | B's epic nodes       |

## Constraints

Every constraint is recorded elsewhere and still holds; each is listed so that
a Feature specification cut from this epic carries it forward.

| Constraint                                                                                                           | Source                                                 | What it rules out                                                                                                  |
| -------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------ |
| The hook, the `Specht` target and a shell carry the check's lines unchanged and in order                             | `0001-F2` B-010                                        | A widget, colour or reordering on the path a call site reads                                                       |
| Without `--json`, stdout carries only the violation lines and the summary, as plain text an MSBuild log parser reads | `0001-F2` B-009, C-3                                   | Colour markup or a table on stdout in a run that did not ask for one; see "Conflicts with recorded specifications" |
| The default lines go to the console's raw writer, not through a renderer                                             | `0001-F2` § 7 "The raw writer"; C-3                    | Rendering the default lines to the console's width, which wraps them at 80 columns when redirected                 |
| Presentation changes no finding, severity, ordering contract or exit code                                            | `0001-F15` C-2; `0001-F17` C-1; `0001-F2` B-003, B-004 | A table or tree that filters, regrades or drops a violation, or a widget that changes the verdict                  |
| Turning a rule down stays a rule level in the manifest                                                               | `0001-F17` C-2; `0001-F11`                             | A table that hides or regrades a rule's violations                                                                 |
| No output carries an absolute path                                                                                   | brief § 9; AGENTS.md § Invariants; `0001-F2` B-013     | A tree node, table cell or prompt choice naming a path that is not relative to the root                            |
| The same tree and the same invocation give the same content                                                          | AGENTS.md § Invariants; `0132-F2` B-022                | A path, message, count or node that varies with the terminal; whether styling may vary is OQ-1 and OQ-8            |
| The tree is read through the discovery and parse the check uses                                                      | `0132-F2` C-1; AGENTS.md § Architecture                | A second reader of specifications, frontmatter or `children`                                                       |
| A command parses, calls the engine and folds the result into an exit code                                            | `0001-F2` C-4                                          | A rule, a reader or a parse inside a widget                                                                        |
| Presentation adds no write and no network                                                                            | AGENTS.md § Invariants; brief § 9                      | A pager file, a cached layout, a fetched style or theme                                                            |
| A prompt without an interactive terminal writes nothing, names the explicit invocation on stderr and exits `2`       | `0001-F7` B-113; `0001-F7` decision 0012               | A prompt that waits for input in a hook or CI step                                                                 |
| An explicit argument skips the prompt                                                                                | `0001-F7` § 7                                          | A prompt confirming a value the invocation already gave                                                            |
| The tool learns nothing about GitHub                                                                                 | `0055-F2` C-6                                          | Selecting a presentation by detecting GitHub Actions or another CI                                                 |
| Presentation does not change the onboarding journey's states or next actions                                         | Epic `0196` § Candidate Features                       | A widget that adds, skips or reorders a journey state                                                              |
| `docs/cli.md` is held to the help by a test                                                                          | AGENTS.md § CLI; `0002-F1`                             | An option that the help names and `docs/cli.md` does not                                                           |

## Conflicts with recorded specifications

On the defaults below, R1 and R3 render on stdout for a run that asks, and two
records say stdout is plain text. Each is named with what changes. This file
amends neither: the amendment belongs to the Feature that adds the request.

| #   | Record                                           | What it says                                                                                    | What changes on OQ-2's default                                                         |
| --- | ------------------------------------------------ | ----------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| 1   | `0001-F2` C-3 (`src/tool/Features/Check/.spec/`) | Rules out colour markup on stdout                                                               | C-3 holds for every run that does not ask; a run that asks is excepted, as `--json` is |
| 2   | `0001-F3` § 5 row 6 (`src/specht/Report/.spec/`) | Colour, markup or a table rendering of the report is out of scope, because stdout is plain text | The exclusion stands for `0001-F3`; its reason points at candidate C instead           |

## Open questions

None is resolved here. Each carries its default where one is proposed, and
travels to the Feature specification it binds, where it is resolved in place.
Every default was proposed while writing this file on 2026-10-31; none was
put to the owner. OQ-1 is first because it sets the boundary for B and C.

| OQ    | Question                                                                                                                                                                                                    | Default                                                                                                                                                                                                                                                                                                          | Who answers |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------- |
| OQ-1  | How does a run ask for R1 or R3: automatically when stdout is an interactive terminal, by an option the invocation names, or by a separate command?                                                         | An option the invocation names. Terminal detection would make one command print two things, which `0001-F17` C-3 rules out for a verbosity level; a separate command would read the tree twice                                                                                                                   | The owner   |
| OQ-2  | Where does the requested output go: stdout in place of the lines and summary, as `--json` replaces them, or stderr beside unchanged stdout?                                                                 | Stdout, in place of the lines and summary, for the requesting run only; the two conflicts above follow                                                                                                                                                                                                           | The owner   |
| OQ-3  | Which command shows the tree?                                                                                                                                                                               | `specht facts` (`0132-F2`), which already gives each Feature's identity, statuses and `children` through the check's discovery (`0132-F2` C-1), gains the tree as a presentation                                                                                                                                 | The owner   |
| OQ-4  | What are the tree's nodes: epics, their Features by `children`, each Feature's statuses, its claims?                                                                                                        | Epics, then their Features by `children`, each with `spec_status` and `status`; a Feature no epic lists sits under a node of its own. Claims are not nodes                                                                                                                                                       | The owner   |
| OQ-5  | Does the request share an option with `--json`, and what happens when both are given?                                                                                                                       | None                                                                                                                                                                                                                                                                                                             | The owner   |
| OQ-6  | Which values are offered as a selection list?                                                                                                                                                               | Only a required value from a closed set the pin defines, where today's run fails without it: `rules set`'s rule id and level, `--explain`'s rule id, `frontmatter`'s kind. A value with a default, such as `--schema-source`, or an optional part of `init`, is never prompted, so no run that works today waits | The owner   |
| OQ-7  | Do a rule id and its level, missing together from `rules set`, give two prompts in turn or one?                                                                                                             | Two, the id first                                                                                                                                                                                                                                                                                                | The design  |
| OQ-8  | Which colour is each severity, and is colour dropped when the console reports no colour support?                                                                                                            | Error red and warning yellow; Spectre.Console's own capability detection decides whether colour is written, and the text is the same either way                                                                                                                                                                  | The owner   |
| OQ-9  | Does the table cover only the check, or also `specht frontmatter` and `specht evidence`, which print findings too?                                                                                          | The check only                                                                                                                                                                                                                                                                                                   | The owner   |
| OQ-10 | In what order are the table's rows, and does it repeat the summary's counts?                                                                                                                                | The engine's order (`0001-F2` B-001), and the counts below the table as the summary gives them (`0001-F2` B-002)                                                                                                                                                                                                 | The owner   |
| OQ-11 | A table and a tree render to the console's width, and a redirected console is 80 columns (`0001-F2` § 7). Does a cell wrap, truncate, or does the request fall back to the lines when stdout is redirected? | A cell wraps and is never truncated, so every path and message the line carries is in the table                                                                                                                                                                                                                  | The design  |

## Placement

No Feature specification exists, so none is placed. A specification sits
beside the code it governs: A and D beside the commands under
`src/tool/Features/`, in a folder of their own, as an invariant spanning
commands is placed by `0001-F17` decision 0001; B beside the command OQ-3
names; C beside the check, `src/tool/Features/Check/`.

This epic file lives at `epics/0194-spectre-cli-ui/epic.md`, where schema
version 1's epic glob `epics/**/epic.md` discovers it (`0001-F6` decision
0001).

## Status

`needs-decomposition`: the owner's three lines are recorded, the cut is a
candidate, and no child exists. The epic is not `blocked`: nothing in "What
this epic waits on" stops a Feature specification being written. The next
step is the owner's: answer OQ-1 and OQ-2, which decide whether any record is
amended, then confirm or change the candidate cut. Each confirmed Feature then
begins with its own specification.

## Out of this epic

| Item                                                                     | Where it lives instead                                                            |
| ------------------------------------------------------------------------ | --------------------------------------------------------------------------------- |
| How much a command prints                                                | `0001-F17`                                                                        |
| Explaining a finding, and the onboarding report's summary by rule        | `0001-F15`; that report may use candidate C's table                               |
| The `--json` document, the `--report` file and `--explain`'s page        | `0001-F3`                                                                         |
| The facts a tree node shows                                              | `0132-F2`; candidate B presents them                                              |
| A claim's coverage: § 9 rows, scenarios, tests                           | `0001-F13`                                                                        |
| Violations on the pull request diff and in the step summary              | Epic `0142`                                                                       |
| The onboarding journey, its states and next actions                      | Epic `0196`; it may present them with these widgets                               |
| JSON as a specification format                                           | Epic `0193`                                                                       |
| Setting a rule's level                                                   | `0001-F11`; no table or prompt changes the manifest                               |
| A table for `specht rules`, `schema list`, `trace`, `init` or `upgrade`  | Not asked for on 2026-10-12                                                       |
| Styling `--help`                                                         | Not asked for; `0002-F1` holds `docs/cli.md` to the help                          |
| A progress display, a live view, a watch mode or a full-screen interface | Not asked for; `0001-F2` C-3 rules out progress on stdout, § 5 row 9 a watch mode |
| A new rule id                                                            | Never: presentation is not a schema change                                        |
