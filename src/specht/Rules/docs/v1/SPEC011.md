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

A Feature specification in the legacy layout, `epics/<epic-directory>/<feature-directory>/spec.md`, declares an `epic` or an `id` its directories do not start with. The rule reports two conditions:

| Condition                                                | Line reported  | Message                                                                           |
| -------------------------------------------------------- | -------------- | --------------------------------------------------------------------------------- |
| The epic directory's name does not start with `<epic>-`  | The `epic` key | `frontmatter epic '{epic}' does not match the containing directory '{directory}'` |
| The feature directory's name does not start with `<id>-` | The `id` key   | `frontmatter id '{id}' does not match the containing directory '{directory}'`     |

The epic directory is the one directly under `epics/`, and the feature directory is the one directly under that. The comparison is on the prefix up to and including the hyphen, so `F1-checkout` matches `id: "F1"` and `F10-checkout` does not.

The rule applies to the legacy layout only. A co-located specification, `<area>/.spec/README.md`, has no directory that names its identity, and is never reported. A legacy specification is skipped when its frontmatter has no `epic` or no `id`, which SPEC002 reports, and when it sits fewer than two directories below `epics/`.

## Rule description

In the legacy layout a Feature has two statements of who it is: the path, `epics/0007-orders/F1-checkout/`, and the frontmatter, `epic: "0007"` and `id: "F1"`. Citations use the frontmatter identity, `0007-F1`; people find the file by the path. When the two disagree, a citation leads to a directory that says it is something else, and a copied directory keeps the identity of the Feature it was copied from.

### Example violation

A specification at `epics/0007-orders/F2-checkout/spec.md` that still carries the id of the Feature it was copied from:

```yaml
---
id: "F1"
epic: "0007"
type: feature
---
```

```text
epics/0007-orders/F2-checkout/spec.md(2): error SPEC011: frontmatter id 'F1' does not match the containing directory 'F2-checkout'
```

### Corrected

```yaml
---
id: "F2"
epic: "0007"
type: feature
---
```

## How to fix violations

Decide which of the two is right, then change the other.

- **The directory is right** - the usual case after copying a Feature's folder to start a new one. Change `id` or `epic` in the frontmatter to match the directory.
- **The frontmatter is right** - the identity is already cited elsewhere and the folder was misnamed. Rename the directory so it starts with `<epic>-` or `<id>-`. An identity is permanent once cited, so prefer this to changing an id that other files name.

After either change, check the Feature's edges: other Features that name it in `depends_on` or `blocks`, and items whose `parent` names it, must use the identity that remains (SPEC050, SPEC043).
