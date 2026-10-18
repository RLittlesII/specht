---
title: "Decision 0004: Consumer inputs and analysis completeness"
description: "Read actual consumer records through build-supplied AdditionalFiles and require current declared inputs for complete analysis"
type: decision
---

# Decision 0004: Consumer inputs and analysis completeness

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Build integration discovers the consumer's selected records and supplies the
pinned manifest, actual specification and ADR contents, and configured schema
files through Roslyn AdditionalFiles, consistently in editor and build. The
analyzer reads and parses those supplied contents. Schema files define structure;
consumer records supply identities, ownership, lifecycle and other record data.
The analyzer stays offline and does not independently crawl the repository.

When analysis is enabled, missing, invalid or incompatible required inputs
produce error diagnostics by default and make analysis unavailable or incomplete,
never clean. Standard severity/suppression controls remain in force without
changing the unavailable/incomplete state. A genuinely absent referenced record
in an otherwise complete inventory is still an unresolved-binding finding;
this policy does not turn every unresolved reference into an input error.

Changes to source, specification or configuration inputs invalidate affected
analysis. Completeness requires current required inputs for every declared
project/target-framework combination; one compilation cannot certify repository
coverage. The consumer's declared set is the aggregation boundary.

Discovery and result evidence carry source, assembly and configuration identity
and are checked against current inputs. Stale or mismatched artifacts remain
visibly stale or mismatched and cannot establish current discovery or execution.
Absent results never count as passing. Binding validity remains a separate
verdict; stale execution artifacts do not rewrite a declaration's binding.

The input-delivery mechanism is supported by Roslyn's [AdditionalFiles contract](https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Using%20Additional%20Files.md),
which provides file contents as analyzer inputs. The build layer must locate
actual consumer files; supplying schemas alone does not satisfy this contract.

Detailed discovery configuration, MSBuild properties/items, evidence metadata
fields and their versioned encoding remain to be designed in their owning
Features. This decision does not choose test-runner versions or a report surface,
launch tests or permit network lookups during analysis.

## Why

Supplying actual consumer records keeps resolution grounded in the values the
consumer owns. A shared input contract supports consistent editor/build verdicts
without ambient analyzer file discovery. Declared compilation boundaries and
freshness checks prevent partial or stale inputs from looking like complete,
passing evidence while preserving independent binding validity.

## Rejected

- Supply schemas as the only non-code data: cannot resolve consumer claim or
  ADR identities, owners and lifecycle values.
- Let the analyzer independently crawl or fetch records: makes its inputs depend
  on ambient filesystem or network state rather than the supplied contract.
- Treat missing required inputs as an empty clean inventory: hides unavailable
  analysis and missing projects.
- Infer repository completeness from one compilation: omits other declared
  projects or target frameworks.
- Accept old discovery/results without matching current identities: can certify
  tests that no longer correspond to the analyzed source or configuration.

The accepted cost is build integration for consumer record discovery and explicit
compilation/evidence identities, with unavailable or incomplete states when the
required current inputs are not supplied.

## Affects

0201-F6 B-016 through B-018 and OQ-1; 0201-F3 B-006 and OQ-2; 0201-F5 B-011
through B-013 and OQ-2; epic 0201 OQ-6, OQ-8 and OQ-9's input-failure severity.
Exact configuration/evidence encoding and adapter/report contracts remain open.

## Reversal

None.
