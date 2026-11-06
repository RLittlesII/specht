---
title: "Decision 0002: the report is specht onboard, and its file is Markdown at --output"
description: "A new command, specht onboard, prints the onboarding report in the terminal and writes it as Markdown at the caller-named --output path; the check command is untouched"
type: decision
---

# Decision 0002: the report is `specht onboard`, and its file is Markdown at `--output`

**Date:** 2026-11-06
**Decided by:** the repository owner, answering a multiple-choice question (2026-11-06); recorded by `spec-author`

## The call

1. The onboarding report is requested by a new command,
   `specht onboard [--root <dir>] --output <path>`. It prints the report in the
   terminal and writes the standalone file at the path `--output` names. The
   check command is untouched. This resolves OQ-1's syntax.
2. The standalone file is Markdown. This resolves OQ-1's file format.
3. B-001 to B-006 are approved for cutting into work items.

## Why

spec-author's reading; the owner chose among the options and gave no reason:

- A command of its own leaves the check's options, output and exit code as
  they are, which decision 0001 and B-006 require, with no flag to keep apart
  from the check's own.
- A Markdown file reads as text in a terminal, an editor and a code host, and
  can be committed or attached as it is.

## Rejected

The owner was offered these and chose against them; the cost after each is
spec-author's reading:

- **An HTML file.** It would need a browser to read, and a page layout the
  report does not otherwise have.
- **A flag on the check, `specht --onboarding <path>`.** It would add an option
  to the check command whose output and exit code integrations rely on, and the
  check's help, which `docs/cli.md` is held to, would change with it.

## Affects

- `0001-F15` OQ-1, resolved; B-001 and B-003, amended in place; § 5 rows 4 and
  5, added; OQ-2, raised; items `0384` to `0386`, cut.
- AGENTS.md § "Invariants": the caller-named `onboard --output` file joins the
  writes allowed into a consumer's tree.

## Reversal

None.
