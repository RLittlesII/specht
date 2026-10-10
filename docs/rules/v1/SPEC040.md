---
title: "SPEC040: Declared children resolve to one item file"
description: A Feature's children frontmatter names an item id that no file, or more than one file, beside the specification carries.
type: rule
---

# SPEC040: Declared children resolve to one item file

## Metadata

| Property         | Value   |
| ---------------- | ------- |
| Rule ID          | SPEC040 |
| Family           | Items   |
| Default severity | Error   |
| Schema version   | 1       |

## Cause

An entry in a Feature specification's `children` does not resolve to exactly one item file - a file named `<id>-<slug>.md` - in the specification's own directory. Reported once per entry, at the `children` key:

| Condition                   | Message                                                                |
| --------------------------- | ---------------------------------------------------------------------- |
| No file starts with the id  | `frontmatter children names '{id}', but no '{id}-*.md' was discovered` |
| Several files start with it | `frontmatter children names '{id}', which resolves to {count} files`   |

## Rule description

Each entry in `children` is the name prefix, such as `0007-02`, of one item file beside the specification.

### Example violation

`epics/0007-orders/F1-checkout/` holds `spec.md` and `0007-01-validate-order-lines.md`. `spec.md` opens with:

```yaml
---
id: "F1"
epic: "0007"
type: feature
children: ["0007-01", "0007-02"]
```

```text
epics/0007-orders/F1-checkout/spec.md(5): error SPEC040: frontmatter children names '0007-02', but no '0007-02-*.md' was discovered
```

### Corrected

```text
epics/0007-orders/F1-checkout/0007-02-sum-order-lines.md
```

## How to fix violations

- **No file.** Write the item beside the specification, move it there, or correct the entry.
- **Several files.** Keep the file the id belongs to and give the other the next unused id, in its name and its `id`.
