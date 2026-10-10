---
title: "SPEC041: Declared spikes resolve to one spike"
description: A Feature's spikes frontmatter names an item id that no file or several files carry, or that belongs to an item that is not a spike.
type: rule
---

# SPEC041: Declared spikes resolve to one spike

## Metadata

| Property         | Value   |
| ---------------- | ------- |
| Rule ID          | SPEC041 |
| Family           | Items   |
| Default severity | Error   |
| Schema version   | 1       |

## Cause

An entry in a Feature specification's `spikes` does not resolve to exactly one item file - a file named `<id>-<slug>.md` beside any specification in the repository - whose `type` is `spike`. Reported once per entry:

| Condition                   | Reported at                      | Message                                                                          |
| --------------------------- | -------------------------------- | -------------------------------------------------------------------------------- |
| No file starts with the id  | The specification's `spikes` key | `frontmatter spikes names '{id}', but no '{id}-*.md' was discovered`             |
| Several files start with it | The specification's `spikes` key | `frontmatter spikes names '{id}', which resolves to {count} files`               |
| The one file is not a spike | The item file's `type` key       | `'{id}' is declared in frontmatter spikes but its type is '{type}', not 'spike'` |

## Rule description

Each entry in `spikes` is the name prefix, such as `0007-02`, of one item file of type `spike`. The file may sit beside another Feature's specification.

### Example violation

A specification declares `spikes: ["0007-01"]`. `0007-01-rounding-rules.md` opens with:

```yaml
---
id: "0007-01"
parent: "0007-F1"
type: task
```

```text
epics/0007-orders/F1-checkout/0007-01-rounding-rules.md(4): error SPEC041: '0007-01' is declared in frontmatter spikes but its type is 'task', not 'spike'
```

### Corrected

```yaml
type: spike
```

## How to fix violations

- **Not a spike.** Change the item's `type` to `spike`, or take the entry out of `spikes`.
- **No file.** Correct the entry, or write the spike beside the specification it is cut from.
- **Several files.** Keep the file the id belongs to and give the other the next unused id, in its name and its `id`.
