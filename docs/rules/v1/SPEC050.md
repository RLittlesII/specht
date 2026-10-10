---
title: "SPEC050: Dependency edges name a discovered Feature"
description: A Feature's depends_on or blocks frontmatter names a Feature the tool did not discover.
type: rule
---

# SPEC050: Dependency edges name a discovered Feature

## Metadata

| Property         | Value        |
| ---------------- | ------------ |
| Rule ID          | SPEC050      |
| Family           | Dependencies |
| Default severity | Error        |
| Schema version   | 1            |

## Cause

An entry in a Feature specification's `depends_on` or `blocks` names a Feature no discovered specification declares. Reported once per entry, at the key that holds it:

`frontmatter {key} names '{identity}', which is not a discovered Feature`

`{identity}` is the entry as it was resolved: `F2` is Feature `F2` of this Feature's own epic, and `0002/F1` is Feature `F1` of epic `0002`.

## Rule description

Each entry names a Feature whose specification exists and declares that `epic` and `id`. An entry for a Feature of another epic is written `<epic>/<id>`.

### Example violation

A specification in epic `0007` that waits on `F3` of epic `0002`:

```yaml
---
id: "F1"
epic: "0007"
type: feature
depends_on: ["F3"]
```

```text
src/orders/.spec/README.md(5): error SPEC050: frontmatter depends_on names '0007-F3', which is not a discovered Feature
```

### Corrected

```yaml
depends_on: ["0002/F3"]
```

## How to fix violations

- **The target is in another epic.** Write the entry as `<epic>/<id>`.
- **The entry is mistyped.** Correct it to the `id` the target declares.
- **The target is not written.** Write its specification, or remove the entry.
