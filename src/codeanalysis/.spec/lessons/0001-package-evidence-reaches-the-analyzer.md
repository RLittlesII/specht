---
title: "Lesson 0001: Package evidence reaches the analyzer"
description: "Evidence declared through the package was delivered under an input kind no analyzer reader consumed, so no test method was recognised"
type: lesson
kind: product
---

# Lesson 0001: Package evidence reaches the analyzer

**Date:** 2026-10-23
**Kind:** product

## Symptom

A consumer declaring a test evidence file through the `SpechtTestEvidence` item
saw no test method recognised from it. Nothing reported the evidence as
missing or unread; it was silently ignored (RLittlesII/specht#256, gap 4).

## Root cause

The package delivered `SpechtTestEvidence` files as the `evidence` input kind,
while the analyzer's test-method recognition reads only `test-evidence`. Both
halves were specified by the item name alone; no claim stated that the
package's delivery reaches the analyzer. Analyzer tests supplied `test-evidence`
directly and the package test declared no evidence, so each half passed in
isolation.

## Spec delta

0201-F6 B-025 requires evidence declared through the `SpechtTestEvidence` item
to reach the analyzer's test-method recognition, with an `@B-025` scenario that
exercises the package route end to end. § 7 is to state the delivered input
kinds completely, including `scenario` and the evidence kind.

## Claim

- [0201-F6 B-025](../README.md#9-traceability-matrix) — no test exists yet; § 9
  records the claim as Missing until the package-to-analyzer test lands with
  item 0257.
