---
title: "Decision 0002: Require passing outcomes by default"
description: "Gate trace success on complete required links and current passing test outcomes"
type: decision
---

# Decision 0002: Require passing outcomes by default

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

`specht trace` succeeds only when required current inputs are complete, required
non-exempt links exist and the test cases supplying required test evidence have
passing outcomes. Missing, skipped, failed, aborted or not-run required outcomes
cannot satisfy that gate. Binding validity, link completeness and execution
outcomes remain separate reported states. No opt-in --require-passing flag is
needed. The command consumes exported evidence and does not use the analyzer
directly or launch tests.

## Why

A complete reference chain without passing required evidence is insufficient for
the owner's default delivery gate.

## Rejected

Link-completeness-only success by default with an optional passing-outcome flag.

## Affects

0201-F5 B-017; the report contract defines the CLI exit mapping.

## Reversal

None.

## Command-name addendum

The 2026-10-19 reversal of decision 0001 names this command `specht evidence`.
The passing-outcome gate in this decision is unchanged.
