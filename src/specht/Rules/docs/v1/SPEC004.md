---
title: "SPEC004: Epic frontmatter satisfies its schema"
description: The frontmatter of an epic file fails the epic frontmatter schema.
type: rule
---

# SPEC004: Epic frontmatter satisfies its schema

## Metadata

| Property         | Value       |
| ---------------- | ----------- |
| Rule ID          | SPEC004     |
| Family           | Frontmatter |
| Default severity | Error       |
| Schema version   | 1           |

## Cause

The frontmatter of an epic file does not validate against the schema version's epic frontmatter schema, `epic.frontmatter.schema.json`. A copy sits in `.spec/schema/`.

An epic file is any file named `epic.md` under the `epics/` directory at the root the tool was given, at any depth. A repository with no `epics/` directory has no epic files, and this rule has nothing to report there.

The rule reports one line for each failure the schema validator gives, so one mistake usually produces two lines:

| Where the failure is | Line reported                   | Message                             |
| -------------------- | ------------------------------- | ----------------------------------- |
| The mapping itself   | The first line of the file      | `frontmatter {errors}`              |
| One key              | The line the key is declared on | `frontmatter '{location}' {errors}` |

`{location}` is the key, or the key and an index into it. `{errors}` is the validator's own text, with several errors at one location joined by `; `: a required key that is absent, a key the schema does not name, a value outside the listed values, a value of the wrong type, or a string that does not match its pattern or format.

A file with no frontmatter at all is SPEC001's, and is not validated.

## Rule description

An epic is the container a set of Features is delivered under. Its frontmatter carries its four-digit id, its status and priority, and the Features it holds. The schema closes that record: every required key present, no key the schema does not name, each value of the stated type and vocabulary.

The schema file is the authority on the keys and their values. Read it there; this page does not repeat it.

### Example violation

An epic whose `priority` is not one of the schema's values:

```yaml
---
id: "0007"
type: epic
status: ready
priority: urgent
milestone: null
children: []
---
```

```text
epics/0007-orders/epic.md(1): error SPEC004: frontmatter Some properties did not match the required schema: ["priority"]
epics/0007-orders/epic.md(5): error SPEC004: frontmatter 'priority' Value should match one of the values specified by the enum
```

The first line is the mapping's and says which keys failed. The second is the key's own, at the line it is declared on.

### Corrected

```yaml
---
id: "0007"
type: epic
status: ready
priority: critical
milestone: null
children: []
---
```

Both blocks are shortened to the keys the example is about.

## How to fix violations

1. Start with the lines that name a key in quotes. Each gives the key, the line it is on, and what the schema expected of it.
2. Open `.spec/schema/epic.frontmatter.schema.json` at that key and write a value it allows.
3. For `Required properties [...] are not present`, add each key listed.
4. For `All values fail against the false schema`, the key is not one the schema names. Remove it, or correct its spelling.
5. The line reported at the first line of the file needs no fix of its own; it clears when the keys it lists do.
