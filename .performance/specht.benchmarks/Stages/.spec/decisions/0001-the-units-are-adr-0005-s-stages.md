---
title: "Decision 0001: the units are ADR-0005's stages"
description: "A stage benchmark measures one stage ADR-0005 names, through the entry point the engine calls it by, with disk-reading stages measured on disk and the rest in memory; benchmarks of internal methods, of the libraries beneath a stage, and of every stage against a substitute file system were turned down"
type: decision
---

# Decision 0001: the units are ADR-0005's stages

**Date:** 2026-10-09
**Decided by:** spec-author, from the owner's scope "engine stages"
(2026-10-09) and the accepted ADR-0005; the owner may reverse it

## The call

- One benchmark per stage ADR-0005 § Decision (a) names: the manifest and
  schema load, discovery, the model build with its two readers - the
  frontmatter reader and the Markdown parse - each rule, and evaluation. Plus
  the coordinator's resolution, which ADR-0005 makes a cost of its own.
- Each is measured through the entry point the engine calls it by (C-11).
- A stage that reads the disk is measured on disk, and says so (C-10); the
  readers, the rules and evaluation take their input in memory (C-9).
- The rules are measured from the engine's own rule set, so a new rule is
  measured without a new benchmark (B-009).

## Why

- ADR-0005 drew the stage boundaries; a cost stated per stage is a cost an
  ADR or a § 4 constraint can name, and so one the `benchmarker` can measure.
- A benchmark of a private method measures something no specification states
  and couples the benchmark to code the `implementer` may rewrite freely.
- The disk is part of what discovery and the model build do; a substitute file
  system would measure a path no user runs.

## Rejected

**Benchmarks of internal methods** - the frontmatter key-line scan, the table
reader. Finer, and not a unit any constraint names. Cost of rejecting: a cost
inside a stage is located by profiling, out of this epic.

**Benchmarking Markdig, YamlDotNet and JsonSchema.Net directly.** Their cost
is inside each stage's finding; their own numbers are their maintainers'. Cost
of rejecting: none.

**Every stage against an in-memory file system.** Removes disk noise and
measures a path nobody runs. Cost of rejecting: the disk-reading stages carry
file-system noise, which their finding states.

## Affects

- `0109-F2` B-001 to B-009; C-9 to C-11.

## Reversal

None.
