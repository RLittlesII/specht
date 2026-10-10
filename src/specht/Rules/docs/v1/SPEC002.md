---
title: "SPEC002: Feature frontmatter satisfies its schema"
description: The frontmatter of a Feature specification fails the Feature frontmatter schema.
type: rule
---

# SPEC002: Feature frontmatter satisfies its schema

## Metadata

| Property         | Value       |
| ---------------- | ----------- |
| Rule ID          | SPEC002     |
| Family           | Frontmatter |
| Default severity | Error       |
| Schema version   | 1           |

## Cause

The frontmatter of a Feature specification - `<area>/.spec/README.md`, or `epics/<epic>/<feature>/spec.md` in the legacy layout - does not validate against the schema version's Feature frontmatter schema, `feature-spec.frontmatter.schema.json`. A copy sits in `.spec/schema/`.

The rule reports one line for each failure the schema validator gives, so one mistake usually produces two lines:

| Where the failure is | Line reported                   | Message                             |
| -------------------- | ------------------------------- | ----------------------------------- |
| The mapping itself   | The first line of the file      | `frontmatter {errors}`              |
| One key              | The line the key is declared on | `frontmatter '{location}' {errors}` |

`{location}` is the key, or the key and an index into it such as `children/0`. `{errors}` is the validator's own text, with several errors at one location joined by `; `. The failures the schema can give include:

- a required key is absent: `Required properties ["author"] are not present`;
- a key the schema does not name is present: `All values fail against the false schema`, at that key;
- a value is not one the schema lists: `Value should match one of the values specified by the enum`;
- a value has the wrong type: `Value is "string" but should be "integer"`;
- a string does not match its pattern or its format;
- a conditional part of the schema fails: a specification with `scored_by` set needs `value` and `risk` of at least 1, and one with `github_issue` set needs `synced_at`.

A file with no frontmatter at all is SPEC001's, and is not validated.

## Rule description

A Feature's frontmatter is its tracking record: its identity (`id`, `epic`), its two statuses, its scoring, and its edges to other Features and to its items. The schema closes that record - every required key present, no key the schema does not name, each value of the stated type and vocabulary - so that the rules which read those keys read values of a known shape.

The schema file is the authority on the keys and their values. Read it there; this page does not repeat it.

### Example violation

A specification whose `spec_status` is not one of the schema's values and which has no `author`:

```yaml
---
id: "F1"
epic: "0007"
type: feature
spec_status: accepted
status: ready
priority: med
---
```

```text
src/orders/.spec/README.md(1): error SPEC002: frontmatter Required properties ["author"] are not present; Some properties did not match the required schema: ["spec_status"]
src/orders/.spec/README.md(5): error SPEC002: frontmatter 'spec_status' Value should match one of the values specified by the enum
```

The first line is the mapping's: it names the missing key and says which keys failed. The second is the key's own, at the line it is declared on.

### Corrected

```yaml
---
id: "F1"
epic: "0007"
type: feature
spec_status: in-review
status: ready
priority: med
author: "spec-author"
---
```

Both blocks are shortened to the keys the example is about.

## How to fix violations

1. Start with the lines that name a key in quotes. Each gives the key, the line it is on, and what the schema expected of it.
2. Open `.spec/schema/feature-spec.frontmatter.schema.json` at that key and write a value it allows.
3. For `Required properties [...] are not present`, add each key listed. The blank at `.spec/templates/feature.md` carries every required key with a starting value.
4. For `All values fail against the false schema`, the key is not one the schema names. Remove it, or correct its spelling if it was meant to be a key the schema has.
5. The line reported at the first line of the file needs no fix of its own; it clears when the keys it lists do.
