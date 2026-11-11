---
title: "Decision 0015: The tool starts faster, and a consumer changes nothing"
description: "Proposed: specht.tool turns Tiered PGO off and ships ReadyToRun code in platform-specific packages behind the one specht.tool id, framework-dependent, with a portable fallback; the package id, the command and the local tool manifest pin stay. Awaiting the owner's agreement."
type: decision
---

# Decision 0015: The tool starts faster, and a consumer changes nothing

**Date:** 2026-11-10
**Decided by:** proposed by spec-author, not yet agreed. The repository
owner chose on 2026-11-10, in session, to have "TieredPGO + ReadyToRun"
specified; the scope below is spec-author's proposal and awaits the owner.

## The call

- **Built:** Tiered PGO off in every package that carries the tool (B-049); a
  top-level `specht.tool` package pointing at one `specht.tool.<rid>` package
  per listed runtime identifier and at `specht.tool.any` (B-050, B-051,
  B-053); ReadyToRun code in each runtime identifier's package, for `net10.0`
  and `net11.0` (B-052); no package carries the .NET runtime (B-054).
- **Unchanged for a consumer:** a local tool manifest pin of `specht.tool`
  restores, `dotnet tool run specht` runs, and `dotnet tool list` names
  `specht.tool` and `specht`, on a listed platform and on one that is not
  (B-055 to B-059; C-12).
- **Not built:** native AOT, trimming, self-contained packages, any change
  to `specht.mcp`, any change to the package id, command or install paths
  (§ 5 rows 31 to 35).
- **Evidence:** the delivering pull request records the Startup benchmark's
  rows against `main` (C-15;
  [ADR-0035](../../../../../../.spec/adr/0035-specht-tool-ships-readytorun-platform-packages-with-tiered-pgo-off.md)).

## Why

- Every call site starts a new process, `specht-mcp` once per tool call, and
  a long-lived `specht` is not a direction (ADR-0031 decision 8). The cost
  that remains to cut is the start.
- What a package carries is observable to every consumer, so it is a claim
  of this Feature beside B-017 to B-036 and B-045 (decision 0011).
- Holding the consumer contract of C-5 and C-6 keeps the change out of every
  consumer's repository: nobody edits a manifest or a call site.

## Rejected

The options, with their cost, are ADR-0035's: the status quo, Tiered PGO off
alone, ReadyToRun alone, native AOT (deferred) and a long-lived `specht`
(excluded).

## Affects

B-049 to B-059; C-12 to C-15; § 2 row 8; § 5 rows 31 to 37; OQ-10 to OQ-12;
`0055-F6` B-003, owed an amendment for the packages a release pushes.

## Reversal

ADR-0035 § "What would reverse it".
