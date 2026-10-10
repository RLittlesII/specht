---
title: "SPEC012: A Feature identity is specified in one place"
description: Two or more Feature specifications declare the same epic and id.
type: rule
---

# SPEC012: A Feature identity is specified in one place

## Metadata

| Property         | Value    |
| ---------------- | -------- |
| Rule ID          | SPEC012  |
| Family           | Identity |
| Default severity | Error    |
| Schema version   | 1        |

## Cause

More than one discovered Feature specification has the same identity. A Feature's identity is its frontmatter `epic` and `id` joined by a hyphen, such as `0007-F1`.

The rule reports once for each file that shares the identity, at the line of its `id` key, with the message:

`'{identity}' is specified in {count} places ({paths}) - a migration removes the old tree in the same change`

`{paths}` lists every file that declares the identity, sorted, so each line names all of them.

Specifications in both layouts are compared together: a legacy `epics/<epic>/<feature>/spec.md` and a co-located `<area>/.spec/README.md` with the same `epic` and `id` are a duplicate. A specification whose frontmatter has no `epic` or no `id` is left out of the comparison; SPEC002 reports it.

## Rule description

An identity is what every citation resolves to: a claim is cited as `0007-F1 B-003`, a dependency as `F1`, an item's parent as `0007-F1`. With two files answering to one identity, a citation has two targets and the two can say different things.

The case the rule exists for is a migration from the legacy layout to the co-located one. Moving a Feature means creating `<area>/.spec/README.md` and removing the `epics/` folder in the same change. A copy left behind gives one specification two homes, and the next edit lands in only one of them. The second case is a new Feature started by copying an existing one and not given its own id.

### Example violation

A Feature migrated to `src/orders/.spec/README.md` while `epics/0007-orders/F1-checkout/spec.md` still exists. Both files open with:

```yaml
---
id: "F1"
epic: "0007"
type: feature
---
```

```text
epics/0007-orders/F1-checkout/spec.md(2): error SPEC012: '0007-F1' is specified in 2 places (epics/0007-orders/F1-checkout/spec.md, src/orders/.spec/README.md) - a migration removes the old tree in the same change
src/orders/.spec/README.md(2): error SPEC012: '0007-F1' is specified in 2 places (epics/0007-orders/F1-checkout/spec.md, src/orders/.spec/README.md) - a migration removes the old tree in the same change
```

### Corrected

One file declares `0007-F1`. The legacy folder is removed, with its `.feature` file and its items moved beside the co-located specification:

```text
src/orders/.spec/README.md
src/orders/.spec/orders.feature
```

## How to fix violations

Which fix applies depends on whether the files are one Feature or two.

- **One Feature in two places** - a migration left the old tree behind. Compare the two files, carry anything the old one has that the new one lacks into the one being kept, and delete the other together with its folder.
- **Two Features with one identity** - one was copied from the other. Give the newer one the next unused `id` in its epic, and in the legacy layout rename its directory to match (SPEC011). Do not renumber the older one: its identity may already be cited.

Then run the check again. Edges and item parents that named the identity now resolve to the one file that keeps it.
