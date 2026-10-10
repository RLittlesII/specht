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

The `id` values of the item files in the repository do not form, for each epic, the sequence `01`, `02`, `03` and so on with each number used once. The rule reports two conditions, each against an item file at the line of its `id` key:

| Condition                                              | Message                                                                                          |
| ------------------------------------------------------ | ------------------------------------------------------------------------------------------------ |
| An item's `id` is also the `id` of an earlier item     | `item id '{id}' is already used by {path} - ids are never reused`                                |
| An epic's item numbers do not run contiguously from 01 | `epic {epic}'s item sequence skips {number} - tasks are numbered per epic, contiguously from 01` |

Uniqueness is checked across every discovered item, whichever Feature it sits beside. The second and later files carrying an id are reported; `{path}` is the first.

Numbering is checked per epic, not per Feature. Items are grouped by the first four characters of their `id`, and within a group the ids of the form `<epic>-<nn>` are sorted by number; the first item whose number is not its position in that order is reported, with `{number}` the number expected there. Three things follow:

- one line is reported per epic, at the first break; a later gap shows after the first is closed;
- a sequence that starts at `02` is reported as skipping `01`;
- a repeated number breaks the sequence as a gap does, so two items with the id `0007-01` are reported as a reused id and as skipping `02`.

The `id` in the frontmatter is what is read, not the file name. An item with no `id` is not counted.

## Rule description

Item ids are allocated from one counter per epic: the next item in epic `0007` takes the next number, whichever of the epic's Features it is cut from. A contiguous sequence is the evidence that the counter was followed. A gap means a number was skipped or an item file was removed, and a reader cannot tell which without the history. A repeat means two pieces of work answer to one id, and every citation of that id is ambiguous.

Ids are never reused and never renumbered to tidy a sequence, because each one may be cited from a specification, a commit or another item.

### Example violation

Epic `0007` has two items, numbered `01` and `03`. The file `epics/0007-orders/F1-checkout/0007-03-sum-order-lines.md` opens with:

```yaml
---
id: "0007-03"
parent: "0007-F1"
type: task
---
```

```text
epics/0007-orders/F1-checkout/0007-03-sum-order-lines.md(2): error SPEC044: epic 0007's item sequence skips 02 - tasks are numbered per epic, contiguously from 01
```

### Corrected

The item that held `0007-02` had been deleted when its work was dropped. It is restored, and the sequence is whole:

```text
epics/0007-orders/F1-checkout/0007-01-validate-order-lines.md
epics/0007-orders/F1-checkout/0007-02-reject-negative-totals.md
epics/0007-orders/F1-checkout/0007-03-sum-order-lines.md
```

## How to fix violations

- **A skipped number, because an item was removed.** Restore the file. An item whose work is dropped keeps its file and its id, with its status saying where it stands; removing it is what opens the gap. Look in every Feature of the epic before concluding it is gone: the number may belong to an item beside another specification.
- **A skipped number, because a new item took the wrong one.** If the item the message names was just created and nothing cites its id yet, give it the number the message says was skipped, in both its `id` and its file name. If it is already cited, leave it and restore or write the item that holds the skipped number.
- **A reused id.** The later item takes the next unused number in the epic, in its `id` and its file name, and the entries that were meant for it in `children` or `spikes` are updated. The earlier item keeps the id.
