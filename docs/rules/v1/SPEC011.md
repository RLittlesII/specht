---
title: "SPEC011: Frontmatter identity matches the containing directory"
description: In the legacy layout, a Feature specification's epic or id disagrees with the directories it sits in.
type: rule
---

# SPEC011: Frontmatter identity matches the containing directory

## Metadata

| Property         | Value    |
| ---------------- | -------- |
| Rule ID          | SPEC011  |
| Family           | Identity |
| Default severity | Error    |
| Schema version   | 1        |

## Cause

A Feature specification in the legacy layout, `epics/<epic-directory>/<feature-directory>/spec.md`, declares an `epic` or an `id` its directories do not start with. A co-located specification is never reported.

| Condition                                         | Line reported  | Message                                                                           |
| ------------------------------------------------- | -------------- | --------------------------------------------------------------------------------- |
| The epic directory does not start with `<epic>-`  | The `epic` key | `frontmatter epic '{epic}' does not match the containing directory '{directory}'` |
| The feature directory does not start with `<id>-` | The `id` key   | `frontmatter id '{id}' does not match the containing directory '{directory}'`     |

## Rule description

The path and the frontmatter name the same Feature: `epics/0007-orders/F1-checkout/spec.md` declares `epic: "0007"` and `id: "F1"`.

### Example violation

`epics/0007-orders/F2-checkout/spec.md` opens with:

```yaml
---
id: "F1"
```

```text
epics/0007-orders/F2-checkout/spec.md(2): error SPEC011: frontmatter id 'F1' does not match the containing directory 'F2-checkout'
```

### Corrected

```yaml
id: "F2"
```

## How to fix violations

- **The directory is right.** Change `epic` or `id` in the frontmatter to match it.
- **The frontmatter is right.** Rename the directory to start with `<epic>-` or `<id>-`. Prefer this when the identity is already cited.
