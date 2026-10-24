---
title: "ADR-0001: VSTest logger and artifact version three"
description: "A VSTest test logger copied beside the test assembly collects case identity, categories, selection and ownership, and writes them at test-evidence format version 3"
type: adr
---

# ADR-0001: VSTest logger and artifact version three

## Status

Proposed — 2026-10-23, `implementer`, from item 0260. The repository owner
decides. OQ-3 already fixes the mechanism, a VSTest test logger registered by
the package build integration; this record settles how it is built, registered
and versioned. Section 7 states the contract in full.

## Context

0201-F3 B-010 to B-016 need per-case identity and outcome from xUnit v2 under
VSTest; B-020, B-025 and B-026 need each case's categories, whether its run
selected it, and whether its project owns it (decision 0002). Case records are
closed objects at versions 1 and 2, so a reader rejects an unknown member as
invalid. A VSTest logger runs inside `vstest.console`, not in the test process,
so it cannot read the compilation export the way the MTP collector does. In a
run request VSTest raises only result events; discovered cases reach a logger
only from a discovery request.

## Considered options

1. A logger assembly named `*TestLogger.dll` copied into the test output and
   enabled through `VSTestLogger`; categories, `owned` and `selected` as new
   required case members at format version 3; discovery from a discovery
   request merged with the following run's results into one artifact.
2. The same logger, with the three members optional at version 1 or 2.
3. A logger that writes separate discovery and run artifacts for one project
   and target framework.
4. A logger that evaluates the run's VSTest filter string against discovered
   cases to decide selection.

## Decision

Option 1.

- The logger is `src/testevidence.vstest`, assembly
  `Specht.TestEvidence.VsTest.TestLogger`, friendly name `specht`. VSTest loads
  loggers whose file name ends in `TestLogger.dll` from the test source's
  directory; the package targets copy it there and append `specht` to
  `VSTestLogger`. `Microsoft.TestPlatform.ObjectModel` is a compile-only
  reference; `vstest.console` supplies it at run time.
- The logger reads the compilation export constant from the test assembly's
  metadata with `System.Reflection.Metadata` and never loads the assembly.
- Format version 3 is version 2 plus `categories`, `owned` and `selected` on
  every case record. Versions 1 and 2 are unchanged and still read.
- Selection is what VSTest reports: a case is selected when its run reported a
  result for it or named it explicitly. The logger never evaluates a filter.
- A run reads the current discovery artifact at its output path and adds its
  selection and outcomes to it, so one project and target framework keeps one
  artifact.

## Consequences

- A tree with a VSTest project gains no new manifest key and no new command;
  `dotnet test --list-tests` followed by `dotnet test` is the sequence.
- A consumer who names its own `--logger` on the command line replaces the
  `VSTestLogger` property, a global MSBuild property, and must add
  `--logger specht`; otherwise that run writes no evidence, which reads as
  unavailable, never as passing.
- An older tool reading a version-3 artifact reports `unsupportedContract`.
- The MTP collector still writes version 1; categories and ownership under MTP
  are later work with their own claim.
- A case whose method metadata the logger cannot translate, such as a generic
  parameter or a nested test class, stays unresolved, so discovery is
  unavailable (B-004). Adapters that set no managed-name properties give the
  same result.

## Rejected

- Optional members at version 1 or 2: version-1 and version-2 objects are
  closed, and an existing reader would call a new artifact invalid instead of
  unsupported (the same reasoning as 0201-F7 ADR-0001 for `catalog`).
- Separate discovery and run artifacts: `specht evidence` treats two current
  exports for one project and target framework that disagree about discovery
  as invalid input.
- Evaluating the VSTest filter: it would be a second implementation of the
  platform's filter grammar, and its answer could differ from what the adapter
  ran.
- A runsettings file to enable the logger: `--settings` replaces
  `RunSettingsFilePath` the same way `--logger` replaces `VSTestLogger`, and it
  adds a generated file to the consumer's output.
- TRX ingestion: rejected by the owner under OQ-3.

## Reversal

None.
