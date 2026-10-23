---
title: "Decision 0003: Gate on owned case keys"
description: "The default gate requires every case key a declared project owns to pass in that project's own run"
type: decision
---

# Decision 0003: Gate on owned case keys

**Date:** 2026-10-23
**Decided by:** Repository owner

## The call

The default gate evaluates case keys by project ownership, as 0201-F3 decision
0002 defines it. Every key a declared project owns must pass in that project's
own run; an owned key that is missing or did not pass fails. Keys are never
merged across projects, so a pass in one project never satisfies another
project's key. A discovered key its project does not own never counts as
passed and never fails the gate. A declared project with no current evidence
fails, so an omitted owning run is caught.

## Why

Case keys carry project, target framework and assembly. With two projects
partitioning the same claim files, gating every discovered key fails a valid
partition, and gating on a passing selected case elsewhere would merge
identities. Ownership keeps the fail-closed rule for genuinely missing cases.

## Rejected

- Gate on every discovered key: fails the keys a project deliberately leaves to
  another project.
- Let a pass in one project satisfy the same scenario in another: merges
  distinct case identities.
- Gate only on the cases a run selected: an omitted or narrowed run passes.

## Affects

B-017, B-020, B-021, B-022; extends decision 0002.

## Reversal

None.
