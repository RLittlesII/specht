---
title: "SPEC051: Dependency edges are declared from both ends"
description: One Feature declares a depends_on or blocks edge to another, and the other does not declare the matching edge back.
type: rule
---

# SPEC051: Dependency edges are declared from both ends

## Metadata

| Property         | Value        |
| ---------------- | ------------ |
| Rule ID          | SPEC051      |
| Family           | Dependencies |
| Default severity | Error        |
| Schema version   | 1            |

## Cause

A Feature specification's frontmatter declares an edge to another Feature, and that Feature's frontmatter does not declare the reverse. The rule reports once per one-sided edge, against the specification that declares it, at the line of the key that holds it, with the message:

`'{identity}' declares {key} '{other}', but '{other}' does not declare {opposite} '{identity}'`

The two keys mirror each other:

| This Feature declares | The other Feature must declare |
| --------------------- | ------------------------------ |
| `depends_on: [other]` | `blocks: [this]`               |
| `blocks: [other]`     | `depends_on: [this]`           |

Entries are compared as qualified identities, so `F2` in a Feature of epic `0007` and `0007/F2` in a Feature of another epic are the same edge.

Only an edge that resolves is checked here. An entry naming a Feature that was not discovered is SPEC050's, and an entry naming the Feature itself is SPEC052's; neither is also reported as one-sided.

## Rule description

A dependency is one fact with two readers. The Feature that waits records it as `depends_on`, so its author knows what must land first. The Feature waited on records it as `blocks`, so its author knows what a slip will hold up. `blocks` is derived: it is the reverse of some other Feature's `depends_on`, and nothing recomputes it. Checking that the two ends agree is what keeps the derived side true.

When only one end is declared, one of the two readers has a wrong picture: a Feature is thought free to change while another waits on it, or is thought to be holding up work that no longer depends on it.

### Example violation

`src/billing/.spec/README.md`, Feature `0007-F2`, depends on `0007-F1`:

```yaml
---
id: "F2"
epic: "0007"
type: feature
depends_on: ["F1"]
blocks: []
---
```

`src/orders/.spec/README.md`, Feature `0007-F1`, does not say it blocks anything:

```yaml
---
id: "F1"
epic: "0007"
type: feature
depends_on: []
blocks: []
---
```

```text
src/billing/.spec/README.md(5): error SPEC051: '0007-F2' declares depends_on '0007-F1', but '0007-F1' does not declare blocks '0007-F2'
```

### Corrected

`src/orders/.spec/README.md` declares the other end:

```yaml
---
id: "F1"
epic: "0007"
type: feature
depends_on: []
blocks: ["F2"]
---
```

## How to fix violations

Two fixes are valid, and which applies depends on whether the dependency is real.

- **The dependency is real.** Add the missing end to the other Feature, in the key the message names after `does not declare`. This is the usual fix when the message is about `depends_on`: the waiting Feature's author recorded the dependency, and the derived `blocks` on the other side was not updated.
- **The dependency is not real, or no longer is.** Remove the entry from the Feature the line is reported against. This is the usual fix when the message is about `blocks`: the other Feature dropped its `depends_on`, and the derived entry was left behind.

Whichever end changes, write the entry in a form that resolves from that file: the short form `F2` within one epic, the qualified form `0007/F2` across epics. Then run the check again; a dependency declared in both directions is a cycle, which SPEC052 reports.
