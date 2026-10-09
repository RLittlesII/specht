---
title: "Decision 0001: specht format reorders rows and keys, and is the fourth writer"
description: "Fixing exists as a separate command, specht format, which moves whole table rows and frontmatter keys and never edits text or renumbers an id; it reverses brief § 2's autofix exclusion for reordering only and joins init, upgrade and --report as a writer into a consumer's tree"
type: decision
---

# Decision 0001: `specht format` reorders rows and keys, and is the fourth writer

**Date:** 2026-10-08
**Decided by:** the repository owner, in the epic `0101` requirements session (2026-10-08); recorded by spec-author

## The call

Fixing exists, as a separate command: `specht format`. The check stays
read-only and gains no `--fix`.

`format` reorders only. It moves whole table rows and whole frontmatter keys
into the order `0101-F1` and `0101-F4` declare. It never edits a cell's text,
a heading or a value, and never renumbers an id. Every `0101-F3` finding -
heading text, table form - is therefore report-only, and every `0101-F2`
finding is restored by the author.

`format --check` writes nothing and exits `1` when it would change a file, as
`dotnet format --verify-no-changes` does.

This reverses, for reordering only, two lines of the brief:

- brief § 2 **Out of scope**, "Autofix: no `--fix` …". The check keeps no
  `--fix`, and `upgrade` still never rewrites a document; `format` is the one
  command that changes a document, and only its order.
- brief § 6, "Who migrates documents across schema versions": still the
  agent, never the tool. Its reason, "Autofix is out of scope", now reads
  with this exception.

It extends brief § 9, "never a write into a consumer's tree except through
`init`, `upgrade` and the `--report` file": `format` is the fourth writer, and
writes only the specification and epic files it reorders.

## Why

- Moving a row by hand is where a row gets dropped, a cell trimmed or an id
  tidied. A mechanical move removes the chance; a reviewer sees a pure
  reordering.
- Reordering is the one fix the tool can make without deciding what a
  document says. Rewording a heading, filling a gap or re-padding a cell is a
  judgement about content, and the stable-id rule forbids the tool from ever
  renumbering.
- A separate command keeps the check what it is: a read-only oracle with one
  verdict at three call sites (Must-1). A hook or CI step that runs the check
  never writes.

## Rejected

**`--fix` on the check.** The check would write when an option is passed,
and a call site that passes it changes the tree it is judging. Cost of
rejecting: a second command to learn. Taken.

**`format` fixing headings and table form as well.** It would edit text: a
heading's characters, a cell's content. Cost of rejecting: `0101-F3`
findings stay manual. Taken.

**`format` closing gaps by renumbering.** Breaks every citation to every
moved id (AGENTS.md § "Stable IDs"). Cost of rejecting: none.

**No fixer at all; the agent reorders from the warnings.** Every reorder is
re-typed rows and a diff a reviewer has to read cell by cell. Cost of
rejecting: one more writer into a consumer's tree. Taken.

## Affects

- `0101-F5` all of § 3, § 4 and § 5.
- `0101-F3` C-1; `0101-F1` C-4; `0101-F2` C-3, § 5 row 2; `0101-F4` C-2.
- `0001-F1` § 5 row 10; epic `0001` "Out of this epic" (the autofix row).
- brief § 2 (Out of scope), § 6 (who migrates documents), § 9 (writers);
  AGENTS.md § Invariants ("Never a write into a consumer's tree").

## Reversal

None.
