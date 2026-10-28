---
title: "Lesson 0002: Input rejections are not rule faults"
description: "Fault isolation must preserve the existing typed input-failure contract, including schema references rejected during evaluation"
type: lesson
kind: product
---

# Lesson 0002: Input rejections are not rule faults

**Date:** 2026-10-27
**Kind:** product

## Symptom

The full integration tier found that an unresolved reference in a selected disk
schema returned a JSON error report and exit 1, where `0001-F7` B-105 requires
empty stdout, a relative stderr diagnostic and exit 3.

## Root cause

ADR-0005's original fault wrapper caught every exception. The later schema-source
contract resolves references when a schema is used, and frontmatter evaluation
translates a failed reference into a typed input rejection. The wrapper therefore
mistook an expected input failure for a rule crash.

## Spec delta

`0001-F1` A-4 and B-012 to B-016 distinguish rule faults from the existing typed
root and manifest input failures. Decision 0005 and ADR-0005's status retain the
later input contract. The B-013 boundary scenario proves that the caller receives
the original rejection. Reference resolution stays at the existing use boundary.

## Claim

- `0001-F1` B-013 — `FaultIsolatedRuleUnitTests.ATypedInputFailure_WhenARuleRaisesIt_ShouldReachTheCallerUnchanged` and the input-rejection scenario in `EngineSteps`.
- `0001-F7` B-105 — `SchemaSourceIntegrationTests.AFaultySelectedDiskSchema_WhenChecked_ShouldExitThreeWithRelativeStderrAndEmptyStdout`.
