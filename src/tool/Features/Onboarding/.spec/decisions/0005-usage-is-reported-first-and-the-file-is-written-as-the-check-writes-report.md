---
title: "Decision 0005: usage is reported first, and the file is written as the check writes --report"
description: "Several onboard input failures report the first in command-line order; the --output file is written as the check writes its --report file"
type: decision
---

# Decision 0005: usage is reported first, and the file is written as the check writes `--report`

**Date:** 2026-11-27
**Decided by:** the repository owner, answering two multiple-choice questions (2026-11-27); recorded by `spec-author`

## The call

1. OQ-4. Asked "when a `specht onboard` run has more than one input failure,
   for example no `--output` under a root with no manifest, which failure is
   reported?", the owner selected "(a) Usage first (Recommended)". Its
   description read: "Report in command-line order: the missing `--output`
   usage error first, then the root, then the manifest, each with its own exit
   code. This follows 0001-F8 B-027, and the command touches no files for a
   request that cannot run." B-013 states it.
2. OQ-5. Asked "how should `specht onboard` write the file at `--output`?",
   the owner selected "(a) Same as --report (Recommended)". Its description
   read: "Behave as the check does for `--report` (0001-F3 B-023, B-024, B-044
   to B-046): create missing folders; replace an existing file; resolve a
   relative path from the working directory. A run that exits 2 or 3 creates
   no folder and removes any earlier file. B-010 (exit 4) then covers only a
   write the file system refuses." B-014 to B-018 state it, and B-010 is
   narrowed to a write the file system refuses.

## Why

The owner selected the recommended options and gave no reason. The options'
own: the order is the one `0001-F8` B-027 uses for `frontmatter`, and the two
caller-named files the tool writes, `--report` and `--output`, then behave
alike.

## Rejected

Not chosen: OQ-4's reading (b), the root and manifest first, then the usage;
and OQ-5's reading (b), a missing directory as a B-010 failure and an input
failure leaving an earlier file as it is. Which other options the questions
put to the owner is not recorded here.

## Affects

- `0001-F15` OQ-4 and OQ-5, resolved; B-013 to B-018, added with their
  scenarios and § 9 rows; B-010, amended in place; items `0384`, `0385`
  (B-013) and `0386` (B-010, B-014 to B-018).

## Reversal

None.
