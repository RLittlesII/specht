---
title: "SPEC013: Contracted tables carry their headers"
description: A section the manifest gives a table to has no table, or a table whose headers differ from the contracted ones.
type: rule
---

# SPEC013: Contracted tables carry their headers

## Metadata

| Property         | Value     |
| ---------------- | --------- |
| Rule ID          | SPEC013   |
| Family           | Structure |
| Default severity | Error     |
| Schema version   | 1         |

## Cause

A section the manifest's `tables` gives headers to has no table, or its first table has other headers. The default manifest contracts one table: `Claim ID`, `Scenario`, `Test`, `Status` in the `matrix` role's section, `## 9. Traceability Matrix`. Reported at the section's heading:

| Condition         | Message                                                                            |
| ----------------- | ---------------------------------------------------------------------------------- |
| No table          | `section '## {title}' carries no table - expected a table with headers {expected}` |
| Different headers | `section '## {title}' has headers {headers} - expected {expected}`                 |

## Rule description

The header cells equal the contracted ones: the same text and case, the same number of columns, in the same order.

### Example violation

```markdown
## 9. Traceability Matrix

| Claim | Scenario | Test | State |
| ----- | -------- | ---- | ----- |
```

```text
src/orders/.spec/README.md(69): error SPEC013: section '## 9. Traceability Matrix' has headers Claim | Scenario | Test | State - expected Claim ID | Scenario | Test | Status
```

### Corrected

```markdown
| Claim ID | Scenario | Test | Status |
| -------- | -------- | ---- | ------ |
```

## How to fix violations

- **Different headers.** Rewrite the header row to the text after `expected`, cell for cell.
- **No table.** Add a table with those headers as the first table under the heading.
