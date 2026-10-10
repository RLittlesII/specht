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

The dependency edges of the discovered Features do not form an order. The rule reports two conditions:

| Condition                                                              | Line reported                  | Message                                       |
| ---------------------------------------------------------------------- | ------------------------------ | --------------------------------------------- |
| An entry in `depends_on` or `blocks` resolves to the Feature itself    | The key that holds the entry   | `frontmatter {key} names this Feature itself` |
| Following `depends_on` from a Feature returns to a Feature on the path | The Feature's `depends_on` key | `dependency cycle: {path}`                    |

`{path}` is the walk that found the cycle: the identities in the order they were followed, joined by `->`, ending at the first one met twice.

Cycles are followed through `depends_on` only; `blocks` is its mirror (SPEC051) and is not walked. Each Feature is walked from in turn, so the rule reports one line for every Feature whose dependencies lead into a cycle: each member of the cycle, and also any Feature outside it that depends on a member. For a Feature outside the cycle, the path starts at that Feature and the repeated identity is further along.

A Feature that names itself in `depends_on` is reported twice, once under each condition, because a self-dependency is also a cycle of length one. An entry naming a Feature that was not discovered is SPEC050's and takes no part in a cycle.

## Rule description

`depends_on` orders work: a Feature is ready when everything it depends on is delivered. That reading needs the edges to have a start. In a cycle every member waits on another member, so none can be the first delivered, and the order the edges were written to give does not exist.

A cycle is rarely meant. It usually records that two Features share something neither owns, or that one of them is two Features, with one part needed by the other and one part needing it.

### Example violation

`src/orders/.spec/README.md`, Feature `0007-F1`:

```yaml
---
id: "F1"
epic: "0007"
type: feature
depends_on: ["F2"]
blocks: ["F2"]
---
```

`src/billing/.spec/README.md`, Feature `0007-F2`:

```yaml
---
id: "F2"
epic: "0007"
type: feature
depends_on: ["F1"]
blocks: ["F1"]
---
```

```text
src/billing/.spec/README.md(5): error SPEC052: dependency cycle: 0007-F2 -> 0007-F1 -> 0007-F2
src/orders/.spec/README.md(5): error SPEC052: dependency cycle: 0007-F1 -> 0007-F2 -> 0007-F1
```

### Corrected

Billing waits on orders, and orders waits on nothing:

```yaml
---
id: "F1"
epic: "0007"
type: feature
depends_on: []
blocks: ["F2"]
---
```

```yaml
---
id: "F2"
epic: "0007"
type: feature
depends_on: ["F1"]
blocks: []
---
```

## How to fix violations

- **The Feature names itself.** Remove the entry. If it was meant for another Feature, correct it: the short form `F1` always means a Feature of the same epic, so an edge to `F1` of another epic is written `<epic>/F1`.
- **A cycle.** Read the path in the message and find the edge that is not a real dependency - the one where the first Feature could be delivered without the second. Remove that entry from `depends_on`, and the matching entry from the other Feature's `blocks`.
- **A cycle in which every edge is real.** The Features are cut wrongly. Move the part both need into a Feature of its own that the others depend on, or split the Feature that sits on both sides of the cycle in two.

Fix the cycle's members first. Lines reported for Features outside the cycle clear when the cycle does.
