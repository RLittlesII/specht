---
title: "SPEC020: A specification has one companion feature file"
description: A Feature specification has no Gherkin file beside it, or more than one.
type: rule
---

# SPEC020: A specification has one companion feature file

## Metadata

| Property         | Value        |
| ---------------- | ------------ |
| Rule ID          | SPEC020      |
| Family           | Feature file |
| Default severity | Error        |
| Schema version   | 1            |

## Cause

The directory that holds a Feature specification does not hold exactly one `.feature` file. Subdirectories are not searched. Reported once per specification, with no line:

`found {count} '.feature' files beside this specification - expected exactly one companion`

## Rule description

One Gherkin file, of any name, sits in the same directory as the specification.

### Example violation

```text
src/orders/.spec/README.md
```

```text
src/orders/.spec/README.md: error SPEC020: found 0 '.feature' files beside this specification - expected exactly one companion
```

### Corrected

```text
src/orders/.spec/README.md
src/orders/.spec/orders.feature
```

## How to fix violations

- **None found.** Create the file beside the specification, or move it there.
- **Several found.** Merge the scenarios into one file, or move a file that belongs to another Feature beside that Feature's specification.
