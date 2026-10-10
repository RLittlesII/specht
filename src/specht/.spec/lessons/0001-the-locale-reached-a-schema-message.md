---
title: "Lesson 0001: The locale reached a schema message"
description: "Under the tr-TR culture a SPEC002 message named the expected type 'ınteger', because JsonSchema.Net lowercases it with the current culture; B-006 was false until the engine evaluated schemas under the invariant culture."
type: lesson
kind: product
---

# Lesson 0001: The locale reached a schema message

**Date:** 2026-10-09
**Kind:** product

## Symptom

Item 0024's determinism test ran the baseline tree under the `tr-TR` culture
and compared the report with the golden report. It failed at index 15: two
`SPEC002` messages read `Value is "null" but should be "ınteger"`, with a
dotless `ı`, where the golden report reads `"integer"`. B-006 was false on the
copied engine.

## Root cause

JsonSchema.Net 9.4.0, the latest release, fills the expected type in its `type`
keyword's message with the culture-sensitive `ToLower()`. Under a culture whose
lowercase of `I` is not `i`, such as `tr-TR` or `az`, the current culture
changes the message. `EvaluationOptions.Culture` and `ErrorMessages.Culture`
choose only the message template and do not reach that call. The engine
evaluated schemas under the caller's culture, and no test had varied it.

## Spec delta

- No new claim: B-006 already said the locale does not change a violation, and
  the engine was wrong.
- `FrontmatterSchemaRule` evaluates each schema under the invariant culture and
  restores the caller's culture afterwards (§ 7).
- Decision 0006 amended: the engine changes once, and the reference guard
  exempts that one site.
- C-9 amended: the verdicts it preserves are those the engine gave at
  `e7dba24` under the invariant culture.

## Claim

- B-006 — `SpechtRunnerDeterminismIntegrationTests` (the baseline tree at a
  nested root under `tr-TR` with an extra environment variable, held to the
  golden report), and the `SpechtRunnerDeterminismUnitTests` § 9 names: the
  reference guard, its one pinned exemption, the invariant message under
  `tr-TR`, and the restore of the caller's culture.
