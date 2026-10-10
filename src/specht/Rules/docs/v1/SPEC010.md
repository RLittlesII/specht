---
title: "SPEC010: Contracted sections appear once and in order"
description: A Feature specification is missing a contracted section, carries one more than once, or carries them out of order.
type: rule
---

# SPEC010: Contracted sections appear once and in order

## Metadata

| Property         | Value     |
| ---------------- | --------- |
| Rule ID          | SPEC010   |
| Family           | Structure |
| Default severity | Error     |
| Schema version   | 1         |

## Cause

The `## ` headings of a Feature specification do not match the `sections` list in the manifest, `.spec/schema/spec-structure.schema.json`. The rule reports three conditions:

| Condition                                                     | Line reported                             | Message                                                                                                 |
| ------------------------------------------------------------- | ----------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| A contracted section has no heading                           | None; the file as a whole                 | `missing section '## {title}' - the contracted sections are in .spec/schema/spec-structure.schema.json` |
| A contracted section's heading appears more than once         | None; the file as a whole                 | `section '## {title}' appears {count} times - each contracted section appears exactly once`             |
| The contracted sections that are present are in another order | The heading of the first one out of place | `section '## {title}' is out of order - '## {expected}' is expected at this position`                   |

Only second-level headings are read, and a heading matches by its exact text, number included: `## 5. Out of Scope`, not `## Out of Scope` and not `### 5. Out of Scope`.

The order check compares the contracted sections that are present, so a missing section is reported once as missing and does not also put every later section out of order. It reports the first section out of place in a specification and stops; fixing that one may reveal the next. A second-level heading the manifest does not list is ignored.

## Rule description

A specification is read by position as much as by name: claims are cited as § 3, constraints as § 4, the traceability matrix as § 9, and the rules that check those sections find them by their exact titles. One fixed set of sections in one fixed order is what lets a reader, a reviewer and the tool open any specification and find the same thing in the same place.

The contracted sections are the twelve numbered ones, `1. Business Goal` to `12. Sign-off`, then `Tasks` and `Scoring`. The manifest's `sections` list is the authority.

### Example violation

A specification that goes from § 4 straight to § 6:

```markdown
## 4. Constraints

| ID  | Constraint                        | Rules Out          |
| --- | --------------------------------- | ------------------ |
| C-1 | An order total is never negative. | A negative refund. |

## 6. Concern Separation
```

```text
src/orders/.spec/README.md: error SPEC010: missing section '## 5. Out of Scope' - the contracted sections are in .spec/schema/spec-structure.schema.json
```

The line carries no line number: no one line of the file is at fault.

### Corrected

```markdown
## 4. Constraints

| ID  | Constraint                        | Rules Out          |
| --- | --------------------------------- | ------------------ |
| C-1 | An order total is never negative. | A negative refund. |

## 5. Out of Scope

| #   | Item    | Exclusion Reason              |
| --- | ------- | ----------------------------- |
| 1   | Refunds | Owned by the billing Feature. |

## 6. Concern Separation
```

## How to fix violations

- **Missing.** Add the heading exactly as the message spells it, in its place in the order. A section with nothing to say yet still appears, with a line saying so; leaving it out is what the rule reports. If the section is there under another title or at another heading level, correct the heading; do not add a second one.
- **Appears several times.** Merge the content under one heading and delete the others. The usual cause is a section pasted from another specification.
- **Out of order.** Move the section the message names, with everything under it, to the position the manifest gives it. Then run the check again, because only the first section out of place is reported.
