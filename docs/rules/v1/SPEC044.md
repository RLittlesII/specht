---
title: "SPEC044: Item ids are unique and contiguous per epic"
description: Two items carry the same id, or an epic's item numbers do not run from 01 with no number skipped.
type: rule
---

# SPEC044: Item ids are unique and contiguous per epic

## Metadata

| Property         | Value   |
| ---------------- | ------- |
| Rule ID          | SPEC044 |
| Family           | Items   |
| Default severity | Error   |
| Schema version   | 1       |

## Cause

The `id`s of an epic's item files, across all its Features, are not `<epic>-01`, `<epic>-02` and so on, each used once. Reported in an item file, at its `id` key:

| Condition                          | Message                                                                                          |
| ---------------------------------- | ------------------------------------------------------------------------------------------------ |
| The `id` is also an earlier item's | `item id '{id}' is already used by {path} - ids are never reused`                                |
| The epic's numbers skip one        | `epic {epic}'s item sequence skips {number} - tasks are numbered per epic, contiguously from 01` |

Only the first skip in an epic is reported. A reused id also breaks the sequence, so its epic gets a skip line too.

## Rule description

Item numbers are allocated per epic, from `01`, with no number skipped and none used twice.

### Example violation

Epic `0007` has two items:

```text
epics/0007-orders/F1-checkout/0007-01-validate-order-lines.md
epics/0007-orders/F1-checkout/0007-03-sum-order-lines.md
```

```text
epics/0007-orders/F1-checkout/0007-03-sum-order-lines.md(2): error SPEC044: epic 0007's item sequence skips 02 - tasks are numbered per epic, contiguously from 01
```

### Corrected

```text
epics/0007-orders/F1-checkout/0007-01-validate-order-lines.md
epics/0007-orders/F1-checkout/0007-02-sum-order-lines.md
```

## How to fix violations

- **A skipped number.** Restore the item that held it, or give an item nothing cites yet that number, in its `id` and its file name.
- **A reused id.** The later item takes the next unused number in the epic, in its `id` and its file name.
