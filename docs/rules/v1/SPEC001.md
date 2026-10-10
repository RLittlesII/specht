---
title: "SPEC001: Specification artifacts open with frontmatter"
description: A Feature specification, an item file or an epic file has no YAML frontmatter the tool can read.
type: rule
---

# SPEC001: Specification artifacts open with frontmatter

## Metadata

| Property         | Value       |
| ---------------- | ----------- |
| Rule ID          | SPEC001     |
| Family           | Frontmatter |
| Default severity | Error       |
| Schema version   | 1           |

## Cause

A Feature specification, an item file (`<epic>-<nn>-<slug>.md` beside a specification) or an `epic.md` under `epics/` does not open with a `---` delimited YAML mapping. Reported once per file, at line 1:

`no YAML frontmatter - every tracked specification artifact opens with a '---' delimited mapping`

## Rule description

The block is the first thing in the file, is closed by a second `---`, and parses as a mapping of keys to values.

### Example violation

```markdown
# Specification: Orders
```

```text
src/orders/.spec/README.md(1): error SPEC001: no YAML frontmatter - every tracked specification artifact opens with a '---' delimited mapping
```

### Corrected

```markdown
---
id: "F1"
epic: "0007"
---

# Specification: Orders
```

## How to fix violations

- **No block.** Add one as the first thing in the file, with the keys the file's schema requires; for a Feature, start from `.spec/templates/feature.md`.
- **A block that is not read.** Remove any text above the opening `---`, add the closing `---`, or quote the value that stops the YAML parsing.
