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

A Feature declares `depends_on` another Feature that does not declare `blocks` it, or `blocks` another that does not declare `depends_on` it. Reported in the declaring specification, at the key that holds the entry:

`'{identity}' declares {key} '{other}', but '{other}' does not declare {opposite} '{identity}'`

## Rule description

Every edge is written at both ends: `depends_on` in the Feature that waits, `blocks` in the Feature it waits on.

### Example violation

`src/billing/.spec/README.md`, Feature `0007-F2`, opens with the lines below. Feature `0007-F1` has `blocks: []`.

```yaml
---
id: "F2"
epic: "0007"
type: feature
depends_on: ["F1"]
```

```text
src/billing/.spec/README.md(5): error SPEC051: '0007-F2' declares depends_on '0007-F1', but '0007-F1' does not declare blocks '0007-F2'
```

### Corrected

In Feature `0007-F1`:

```yaml
blocks: ["F2"]
```

## How to fix violations

- **The dependency is real.** Add the entry the message says is not declared to the other Feature.
- **The dependency is not real.** Remove the entry from the Feature the line names.
