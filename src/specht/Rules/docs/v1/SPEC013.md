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

The manifest, `.spec/schema/spec-structure.schema.json`, gives under `tables` the headers a section's table must have. Each entry is keyed by a role, and the manifest's `roles` give the title of the section that role is. A Feature specification has such a section whose table does not match. The rule reports two conditions, both at the line of the section's heading:

| Condition                                  | Message                                                                            |
| ------------------------------------------ | ---------------------------------------------------------------------------------- |
| The section has no table                   | `section '## {title}' carries no table - expected a table with headers {expected}` |
| The table's headers are not the contracted | `section '## {title}' has headers {headers} - expected {expected}`                 |

`{headers}` and `{expected}` are the header cells in order, with `|` between them.

The table read is the first pipe table after the section's heading and before the next `## ` heading. Its header cells must equal the contracted ones exactly: the same text, the same case, the same number of columns, in the same order. Text in a code span counts as its content, so `` `Status` `` reads as `Status`.

A specification that lacks the section altogether is not reported here; the missing section is SPEC010's. The default manifest contracts one table, the `matrix` role's: `roles` names that section `9. Traceability Matrix`, and `tables` gives it the headers `Claim ID`, `Scenario`, `Test` and `Status`. `{title}` in the message is the title `roles` gives, so a manifest that names another section for the role is checked, and reported, under that title.

## Rule description

The rules that check coverage read a table by position: the first cell of each row of the traceability matrix is the claim the row is for (SPEC031), and a cell holding the manifest's `missing` marker is what an approved specification may not carry (SPEC060). Fixed headers are what make the columns mean the same thing in every specification, to the tool and to a reader comparing two of them.

### Example violation

A traceability matrix with two headers renamed:

```markdown
## 9. Traceability Matrix

| Claim | Scenario                   | Test    | State   |
| ----- | -------------------------- | ------- | ------- |
| B-001 | An empty order is rejected | Missing | Missing |
```

```text
src/orders/.spec/README.md(69): error SPEC013: section '## 9. Traceability Matrix' has headers Claim | Scenario | Test | State - expected Claim ID | Scenario | Test | Status
```

### Corrected

```markdown
## 9. Traceability Matrix

| Claim ID | Scenario                   | Test    | Status  |
| -------- | -------------------------- | ------- | ------- |
| B-001    | An empty order is rejected | Missing | Missing |
```

## How to fix violations

- **Wrong headers.** Rewrite the header row to the text after `expected` in the message, cell for cell. If a column was added, move what it held into one of the contracted columns or into prose below the table; if columns were reordered, reorder every row with them.
- **No table.** Add a table with the contracted headers under the section's heading. A specification with no claims yet still carries the header row and its delimiter row.
- **A table that is there but not read.** The first table after the heading is the one checked. If another table sits above the contracted one in the same section, move the contracted one first.
