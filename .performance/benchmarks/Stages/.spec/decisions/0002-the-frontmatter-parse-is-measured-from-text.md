---
title: "Decision 0002: the frontmatter parse is measured from text"
description: "Once the model is built from text a store has read, B-003 measures parsing one specification's frontmatter from its text read before the measurement, C-3 rules out a disk read inside the measured parse, and A-1 names the reader, the parsers, discovery and the evaluator; ids kept"
type: decision
---

# Decision 0002: the frontmatter parse is measured from text

**Date:** 2026-11-02
**Decided by:** the repository owner, "A: parse from text (Recommended)",
on item `0166`'s sixth question (2026-11-02)

## The call

- B-003 measures parsing one generated specification's frontmatter from its
  text, read before the measurement. Its id is kept.
- C-3 states that input and rules out a disk read inside the measured parse,
  as C-4 does for the Markdown parse.
- A-1 names `SpechtModelReader` for the stages that read the disk, the
  manifest and schema load (B-001) and the model build (B-005), under C-10
  and C-11; the parser instances `FrontmatterReader` and
  `SpechtDocumentParser` for B-003 and B-004; and keeps `SpechtDiscovery`
  and `SpechtEvaluator.Evaluate`.

## Why

- ADR-0010 stage 6 builds the model from text: the tree store reads the
  files, and the frontmatter parser holds no file system. A frontmatter read
  that includes the disk is then two types measured as one stage, which
  C-11's "the entry point the engine calls it by" no longer describes.
- An item's and an epic's frontmatter are parsed apart from any document, so
  the frontmatter parse is a stage of its own and not inside B-004's.

## Rejected

**Withdraw B-003 and C-3**, the frontmatter parse taken as inside B-004's
document parse. Cost of rejecting: none measured; the item and epic parse
would have no benchmark of its own.

**Keep a disk read in B-003**: the store's read of one file and the parse in
one measured call. Cost of rejecting: the frontmatter's disk cost is not
measured per specification; it is inside B-005's model build, which reads
the disk (C-10).

## Affects

- `0109-F2` B-003, C-3 and A-1; the B-003 scenario.
- `0109-F2` § 9's B-003 row names the scenario, and follows by its owner.

## Reversal

None.
