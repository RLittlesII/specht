---
title: "Decision 0002: Required-case scope per project"
description: "Each selected test project owns the case keys its declared tag expression matches, apart from discovery and run selection"
type: decision
---

# Decision 0002: Required-case scope per project

**Date:** 2026-10-23
**Decided by:** Repository owner

## The call

Each selected test project under `codeAnalysis` declares a required-case scope:
a Gherkin tag expression over case categories, such as `not @visual` for one
project and `@visual` for another. A category is a Reqnroll scenario tag mapped
to a `Category` trait or an xUnit `Trait("Category", ...)` value; both match the
same way. With no declared scope, a project's scope is all its cases.

A project owns the discovered case keys its scope matches. Ownership is the
required scope; it is separate from what the project discovered and from what
its run actually selected. Case keys stay per project, target framework and
assembly, so the same scenario compiled into two projects gives two keys and
the evidence never merges them.

A run whose recorded selection does not cover its project's owned keys gets an
incomplete-run diagnosis.

## Why

Two projects compiling the same claim files discover the same cases under
distinct keys, and complementary run filters execute each case in only one of
them. Gating on discovery fails the keys a project deliberately does not run;
gating on actual selection excuses a case that a run silently skipped. A
declared scope states which keys each project answers for, so a partition can
pass without either outcome.

## Rejected

- Require every discovered key: fails valid partitioned runs.
- Require only what a run selected: a narrowed or omitted run passes silently.
- Merge keys across projects by scenario: breaks the per-assembly case identity
  of the version-one contract.

## Affects

B-020, B-021, B-025, B-026; 0201-F5 decision 0003; 0201-F6 B-026.

## Reversal

None.
