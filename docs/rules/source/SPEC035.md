---
title: "SPEC035: A retired claim is no longer carried"
description: A withdrawn or superseded claim still has a companion tag or a matrix row with another status.
type: rule
---

# SPEC035: A retired claim is no longer carried

## Metadata

| Property         | Value   |
| ---------------- | ------- |
| Rule ID          | SPEC035 |
| Family           | Claims  |
| Default severity | Off     |
| Schema version   | {{schemaVersion}}   |

## Cause

A claim marked with the manifest's withdrawn or superseded marker still has a matching companion tag, or an existing matrix row has a different status. One diagnostic identifies the claim at its declaration line, even when both conditions hold.

## Rule description

Keep the retired claim and its stable id in the claims table. Its matrix rows retain the same retirement marker, and no companion tag carries it. Matching uses the configured claim-tag form and case setting. Active claims are outside this rule.

### Example violation

Claim `B-004` is `Withdrawn`, and a companion scenario still carries `@B-004`.

```text
src/sample/.spec/README.md(24): error SPEC035: retired claim must have no companion tag and its § 9 status must be 'Withdrawn' [B-004]
```

### Corrected

Remove the retired claim's scenario tag and mark its matrix row `Withdrawn`, retaining the claim and its id.

## How to fix violations

Retire the companion reference and align the matrix status with the claim's configured retirement marker.
