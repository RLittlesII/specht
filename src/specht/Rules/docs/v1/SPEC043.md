---
title: "SPEC043: An item's id and parent match where it sits"
description: An item's frontmatter id disagrees with its file name, or its parent disagrees with the Feature whose directory holds it.
type: rule
---

# SPEC043: An item's id and parent match where it sits

## Metadata

| Property         | Value   |
| ---------------- | ------- |
| Rule ID          | SPEC043 |
| Family           | Items   |
| Default severity | Error   |
| Schema version   | 1       |

## Cause

The frontmatter of an item file says one thing about the item and its location says another. The rule reports two conditions, each against the item file:

| Condition                                                         | Line reported    | Message                                                                              |
| ----------------------------------------------------------------- | ---------------- | ------------------------------------------------------------------------------------ |
| `id` is not the first seven characters of the file's name         | The `id` key     | `frontmatter id '{id}' does not match the file name prefix '{prefix}'`               |
| `parent` is not the identity of the Feature in the same directory | The `parent` key | `frontmatter parent '{parent}' does not match the Feature it sits in ('{identity}')` |

An item file is a Markdown file beside a specification whose name starts `<epic>-<nn>-`; its file name prefix is that `<epic>-<nn>`, such as `0007-02`. The Feature it sits in is the one whose specification is in the same directory, and that Feature's identity is its `epic` and `id` joined by a hyphen, such as `0007-F1`.

Both comparisons are exact. An item with no `id` is skipped entirely, and the parent comparison is skipped for an item with no `parent`; SPEC003 reports either as a missing required key.

## Rule description

An item is found in three ways: by its file name, by the `id` other files cite, and by the Feature that lists it. The rule keeps the three in agreement. `children` and `spikes` resolve an entry by file name (SPEC040, SPEC041) while uniqueness and numbering read the `id` (SPEC044), so an item whose name and `id` differ is one item to the first pair of rules and another to the third.

`parent` is the item's own statement of which Feature it was cut from. An item that sits beside one Feature and names another is either in the wrong folder or was copied and not updated, and a reader cannot tell which Feature's claims it delivers.

### Example violation

The file `epics/0007-orders/F1-checkout/0007-01-validate-order-lines.md`, beside the specification of `0007-F1`:

```yaml
---
id: "0007-01"
parent: "0007-F2"
type: task
---
```

```text
epics/0007-orders/F1-checkout/0007-01-validate-order-lines.md(3): error SPEC043: frontmatter parent '0007-F2' does not match the Feature it sits in ('0007-F1')
```

### Corrected

```yaml
---
id: "0007-01"
parent: "0007-F1"
type: task
---
```

## How to fix violations

- **Parent differs.** Decide which Feature the item belongs to. If it belongs to the Feature it sits beside, change `parent` to that Feature's identity. If `parent` is right, move the file into that Feature's directory, and move its entry from one Feature's `children` to the other's.
- **Id differs from the file name.** Decide which is the item's id by looking at what cites it: a Feature's `children` or `spikes`, and other items. If the name is right, change `id` to match it. If the `id` is right, rename the file so it starts with the `id`. An id that is already cited does not change.

After renaming a file or changing an `id`, run the check again: the epic's numbering is read from the `id` (SPEC044), and the Feature's `children` resolve by the file name (SPEC040).
