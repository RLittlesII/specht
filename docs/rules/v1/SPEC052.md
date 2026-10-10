---
title: "SPEC052: A Feature does not depend on itself or on a cycle"
description: A Feature names itself in depends_on or blocks, or its depends_on edges lead back to a Feature already on the path.
type: rule
---

# SPEC052: A Feature does not depend on itself or on a cycle

## Metadata

| Property         | Value        |
| ---------------- | ------------ |
| Rule ID          | SPEC052      |
| Family           | Dependencies |
| Default severity | Error        |
| Schema version   | 1            |

## Cause

A Feature names itself in an edge, or its `depends_on` edges lead into a cycle:

| Condition                                                  | Line reported                | Message                                       |
| ---------------------------------------------------------- | ---------------------------- | --------------------------------------------- |
| An entry in `depends_on` or `blocks` is the Feature itself | The key that holds the entry | `frontmatter {key} names this Feature itself` |
| Following `depends_on` returns to a Feature already met    | The `depends_on` key         | `dependency cycle: {path}`                    |

`{path}` is the identities in the order followed, joined by `->`. A cycle is reported once for each Feature whose `depends_on` leads into it, members or not. `blocks` is not followed.

## Rule description

Following `depends_on` from any Feature ends; it never returns to a Feature already passed, the Feature itself included.

### Example violation

```yaml
---
id: "F1"
epic: "0007"
type: feature
depends_on: ["F1"]
```

```text
src/orders/.spec/README.md(5): error SPEC052: frontmatter depends_on names this Feature itself
src/orders/.spec/README.md(5): error SPEC052: dependency cycle: 0007-F1 -> 0007-F1
```

### Corrected

```yaml
depends_on: []
```

## How to fix violations

- **The Feature names itself.** Remove the entry, or write `<epic>/<id>` if a Feature of another epic was meant.
- **A cycle.** Remove the `depends_on` entry on the path that is not a real dependency, and the matching `blocks` entry. If every edge is real, move what the Features share into a Feature of its own.
