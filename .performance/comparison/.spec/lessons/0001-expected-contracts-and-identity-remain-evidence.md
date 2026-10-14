---
title: Expected contracts and identity remain evidence
description: Independent review found that matching keys and earlier timestamps could hide incomplete or contradictory evidence
type: lesson
kind: product
---

# Lesson 0001: Expected contracts and identity remain evidence

**Date:** 2026-10-14
**Kind:** product

## Symptom

Independent review of PR 181 reproduced three misleading results: required allocation vanished when the observed contract supplied timing only; contradictory expected fixture and boundary contracts still paired as comparable; unmapped renamed records appeared Added/Removed. A saved pair could also name the current immutable release as its own predecessor by supplying an earlier timestamp.

## Root cause

Grouping treated an operation/parameter key as the entire contract and discarded matching session expectations. The one-sided branch inferred workload absence before checking audited identity. Saved-pair validation checked publication ordering without checking distinct immutable release identities.

## Spec delta

The existing agreement already required explicit expected inventories, known one-sided identities and validated durable pairs. Boundary scenarios now make those cases executable under the existing claims. The technical design records full contract reconciliation, retained metric requirements and distinct predecessor identity; no new claim or architecture rule is introduced.

## Claim

- `0109-F5 B-005`, `0109-F5 B-006` — `IndependentReviewUnitTests` and `ComparisonSteps` prove expected metrics, conflicting native contracts and unknown renamed identities.
- `0109-F5 B-002` — `IndependentReviewUnitTests.SavedPairNamesCurrentReleaseAsEarlierPredecessor_WhenRead_ShouldRejectItsImmutableIdentity`, `InvalidSavedPairIntegrationTests` and `ComparisonSteps` prove rejection without reselection or overwrite.
