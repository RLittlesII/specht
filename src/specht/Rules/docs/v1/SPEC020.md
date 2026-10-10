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

The directory that holds a Feature specification does not hold exactly one file whose name ends in `.feature`. The rule reports once per specification, against the specification file as a whole and with no line, with the message:

`found {count} '.feature' files beside this specification - expected exactly one companion`

`{count}` is `0` when there is none and the number found when there are several.

Only the specification's own directory is searched: `<area>/.spec/` for a co-located specification, `epics/<epic>/<feature>/` for a legacy one. A `.feature` file in a subdirectory, or elsewhere in the repository, is not a companion. The file may have any name.

While a specification is reported here, the claim tags of its `.feature` files are not checked (SPEC021): with no file there are none to check, and with several the tool does not choose between them.

## Rule description

A specification's § 3 states its claims; the Gherkin file beside it states each claim as scenarios someone can run. The two are one agreement in two forms, so they live in one folder and are found together: the scenario tag `@B-003` means claim `B-003` of the specification in the same directory.

One file, not several, is what makes that resolution certain. With two `.feature` files beside one specification, a tag in either could be the claim's scenario, and a reader cannot tell which file is current.

### Example violation

A specification with no Gherkin file beside it:

```text
src/orders/.spec/README.md
src/orders/.spec/decisions/0001-totals-are-integers.md
```

```text
src/orders/.spec/README.md: error SPEC020: found 0 '.feature' files beside this specification - expected exactly one companion
```

### Corrected

```text
src/orders/.spec/README.md
src/orders/.spec/orders.feature
src/orders/.spec/decisions/0001-totals-are-integers.md
```

## How to fix violations

- **None found.** Create one `.feature` file in the specification's directory and write a scenario for each § 3 claim, tagged with the claim's id. If the file exists somewhere else, such as a test project, move it beside the specification and point the test project at it there.
- **Several found.** Merge the scenarios into one file and delete the rest. If the extra file belongs to another Feature, move it beside that Feature's specification instead.

Then run the check again: once exactly one file is found, its tags are checked against § 3, and SPEC021 reports any that resolve to no claim.
