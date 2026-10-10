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

The `## ` headings of a Feature specification do not match the `sections` list of the manifest, `.spec/schema/spec-structure.schema.json`:

| Condition                       | Reported at                    | Message                                                                                                 |
| ------------------------------- | ------------------------------ | ------------------------------------------------------------------------------------------------------- |
| A section has no heading        | The file, with no line         | `missing section '## {title}' - the contracted sections are in .spec/schema/spec-structure.schema.json` |
| A heading appears twice or more | The file, with no line         | `section '## {title}' appears {count} times - each contracted section appears exactly once`             |
| The sections are out of order   | The first heading out of place | `section '## {title}' is out of order - '## {expected}' is expected at this position`                   |

A heading matches by its exact text, number included. Only the first section out of place is reported.

## Rule description

A specification carries each section the manifest lists, once, as a `## ` heading, in the manifest's order.

### Example violation

```markdown
## 4. Constraints

## 6. Concern Separation
```

```text
src/orders/.spec/README.md: error SPEC010: missing section '## 5. Out of Scope' - the contracted sections are in .spec/schema/spec-structure.schema.json
```

### Corrected

```markdown
## 4. Constraints

## 5. Out of Scope

## 6. Concern Separation
```

## How to fix violations

- **Missing.** Add the heading as the message spells it, in its place; correct a heading that is there under another title or level.
- **Appears several times.** Merge the content under one heading and delete the others.
- **Out of order.** Move the section the message names to its place, then run the check again.
