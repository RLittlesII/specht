---
title: "SPEC030: Claim ids are well formed and unique"
description: A claim id in § 3 does not match the claim id grammar, or is declared more than once in the same specification.
type: rule
---

# SPEC030: Claim ids are well formed and unique

## Metadata

| Property         | Value   |
| ---------------- | ------- |
| Rule ID          | SPEC030 |
| Family           | Claims  |
| Default severity | Error   |
| Schema version   | 1       |

## Cause

The first cell of a row in the claims table is not a usable claim id. The claims section is the one the manifest's `claims` role names, `## 3. Acceptance Criteria` in the default manifest; the message says § 3 whatever its title. Reported at the row:

| Condition                             | Message                                                                                    |
| ------------------------------------- | ------------------------------------------------------------------------------------------ |
| The id does not match the grammar     | `claim id '{id}' does not match {grammar} - claim ids are permanent, so the form is fixed` |
| The id is also that of an earlier row | `claim '{id}' is declared twice in § 3 (also at line {line})`                              |

`{grammar}` is the manifest's `identifiers.claim` pattern, `^B-[0-9]{3}[a-z]?$` in the default manifest.

## Rule description

Each claim id matches the grammar - by default `B-`, three digits and an optional lowercase letter - and is declared once in its specification.

### Example violation

```markdown
| B-2 | An order total is the sum of its lines. | need 1 | Active |
```

```text
src/orders/.spec/README.md(43): error SPEC030: claim id 'B-2' does not match ^B-[0-9]{3}[a-z]?$ - claim ids are permanent, so the form is fixed
```

### Corrected

```markdown
| B-002 | An order total is the sum of its lines. | need 1 | Active |
```

## How to fix violations

- **Malformed.** Rewrite the id in the grammar's form, and its scenario tags and § 9 row with it.
- **Declared twice.** The later claim takes the next unused number; the earlier one keeps its id.
