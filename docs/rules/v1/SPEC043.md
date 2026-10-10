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

An item file's frontmatter disagrees with where the file sits. Reported in the item file:

| Condition                                                      | Line reported    | Message                                                                              |
| -------------------------------------------------------------- | ---------------- | ------------------------------------------------------------------------------------ |
| `id` is not the file name's `<epic>-<nn>` prefix               | The `id` key     | `frontmatter id '{id}' does not match the file name prefix '{prefix}'`               |
| `parent` is not the Feature whose specification it sits beside | The `parent` key | `frontmatter parent '{parent}' does not match the Feature it sits in ('{identity}')` |

## Rule description

An item's `id` is the start of its file name, and its `parent` is the identity, `<epic>-<id>`, of the Feature in the same directory.

### Example violation

`0007-01-validate-order-lines.md`, beside the specification of `0007-F1`, opens with:

```yaml
---
id: "0007-01"
parent: "0007-F2"
```

```text
epics/0007-orders/F1-checkout/0007-01-validate-order-lines.md(3): error SPEC043: frontmatter parent '0007-F2' does not match the Feature it sits in ('0007-F1')
```

### Corrected

```yaml
parent: "0007-F1"
```

## How to fix violations

- **Parent differs.** Change `parent` to the Feature the file sits beside, or move the file beside the Feature `parent` names.
- **Id differs.** Change `id` to the file name's prefix, or rename the file to start with the `id`; keep whichever is already cited.
