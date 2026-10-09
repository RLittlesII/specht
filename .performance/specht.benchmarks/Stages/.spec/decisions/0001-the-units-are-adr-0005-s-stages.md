---
title: "Decision 0001: the units are ADR-0005's stages"
description: "A stage benchmark measures one stage ADR-0005 names, through the entry point the engine calls it by, with every stage that reads the disk - evaluation included - measured against a generated tree on disk; container resolution is not a unit; benchmarks of internal methods, of the libraries beneath a stage, and of every stage against a substitute file system were turned down"
type: decision
---

# Decision 0001: the units are ADR-0005's stages

**Date:** 2026-10-09
**Decided by:** spec-author, from the owner's scope "engine stages"
(2026-10-09) and the accepted ADR-0005; the owner may reverse it

## The call

- One benchmark per stage ADR-0005 § Decision (a) names: the manifest and
  schema load, discovery, the model build with its two readers - the
  frontmatter reader and the Markdown parse - each rule, and evaluation.
- Each is measured through the entry point the engine calls it by (C-11).
- A stage that reads the disk is measured on disk, and says so (C-10). That
  includes the rules and evaluation: the companion-file rule reads each
  `.feature` file while it evaluates (`src/specht/Rules/FeatureFileRule.cs:41`;
  ADR-0005 (a)), and C-11 forbids changing `src/` to avoid it. The model and
  any parsed text are built before the measurement (C-9).
- **Container resolution is not a unit** (B-008 and C-8 withdrawn,
  2026-10-09). ADR-0005 states no resolution cost, and the engine's types are
  singletons, so a second resolve from one provider measures a cached lookup.
  Building a provider and resolving per operation would measure composition,
  a cost a process pays once, which `0109-F4`'s start-to-exit finding already
  includes.
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

**Keeping container resolution as a unit** - with the benchmarker's example
row as its source, and building a provider plus resolving per operation as
the measured call. No ADR or constraint states the cost, and the composition
it would measure is already inside `0109-F4`'s finding. Cost of rejecting:
the composition cost is not separated from the rest of the tool's start.

**Every stage against an in-memory file system.** Removes disk noise and
measures a path nobody runs. Cost of rejecting: the disk-reading stages carry
file-system noise, which their finding states.

## Affects

- `0109-F2` B-001 to B-009 (B-008 withdrawn); C-8 (withdrawn) to C-11; § 5 rows 8 and 9.

## Reversal

None.
