---
title: "Lesson 0001: Validate provenance before emission"
description: "A root-relative output promise needs a supplied-input boundary claim so unchecked provenance cannot flow into binding results"
type: lesson
kind: product
---

# Lesson 0001: Validate provenance before emission

**Date:** 2026-10-17
**Kind:** product

## Symptom

Final implementation review of binding identity found that supplied Feature
texts and Feature or ADR catalog declarations accepted arbitrary provenance,
which resolution could carry unchanged into binding results. The existing
claim tests passed without proving the repository-relative output invariant.

## Root cause

The global invariant and the technical design required root-relative paths,
but the agreement had no falsifiable claim at the supplied-text and catalog
boundary. Identity matching correctly ignored paths while provenance values
remained unguarded.

## Spec delta

0201-F1 B-022 rejects non-repository-relative or root-escaping provenance before
result emission with a fixed error that does not echo the rejected value.
The scenarios describe POSIX, Windows drive, UNC and backslash roots and
escaping traversal under either separator without storing absolute fixtures.
Nested relative paths and harmless internal parent segments remain usable.

This guard applies to F1's supplied text, catalog and results. It requires no
filesystem or network access, makes no attempt to relativize paths against an
unknown root, and does not change legacy engine parser APIs.

## Claim

- [0201-F1 B-022](../README.md#9-traceability-matrix) — the `@B-022`
  unsafe-provenance and contained-provenance scenarios pass through
  `BindingSteps`, alongside the passing unit tests
  `BindingProvenanceUnitTests.UnsafeProvenanceAtEachInputBoundary_WhenPrepared_ShouldRejectWithFixedPathFreeError`
  and `BindingProvenanceUnitTests.ContainedProvenanceAtEachInputBoundary_WhenResolved_ShouldRetainTheSuppliedSpelling`.
  Both tiers exercise supplied Feature text and Feature and ADR catalog inputs;
  § 9 records the claim as Covered.
