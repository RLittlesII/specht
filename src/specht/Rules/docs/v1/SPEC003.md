---
title: "SPEC003: Item frontmatter satisfies its schema"
description: The frontmatter of a task, test, bug or spike file beside a specification fails the item frontmatter schema.
type: rule
---

# SPEC003: Item frontmatter satisfies its schema

## Metadata

| Property         | Value       |
| ---------------- | ----------- |
| Rule ID          | SPEC003     |
| Family           | Frontmatter |
| Default severity | Error       |
| Schema version   | 1           |

## Cause

The frontmatter of an item file does not validate against the schema version's item frontmatter schema, `task.frontmatter.schema.json`. A copy sits in `.spec/schema/`.

An item file is a Markdown file in the same directory as a Feature specification whose name starts `<epic>-<nn>-`, four digits, a hyphen and two digits, such as `0007-01-validate-order-lines.md`. Files with any other name, and files in directories that hold no specification, are not items and are not read.

The rule reports one line for each failure the schema validator gives, so one mistake usually produces two lines:

| Where the failure is | Line reported                   | Message                             |
| -------------------- | ------------------------------- | ----------------------------------- |
| The mapping itself   | The first line of the file      | `frontmatter {errors}`              |
| One key              | The line the key is declared on | `frontmatter '{location}' {errors}` |

`{location}` is the key, or the key and an index into it. `{errors}` is the validator's own text, with several errors at one location joined by `; `: a required key that is absent, a key the schema does not name, a value outside the listed values, a value of the wrong type, or a string that does not match its pattern or format.

A file with no frontmatter at all is SPEC001's, and is not validated.

## Rule description

An item is a unit of work cut from a Feature: a task, a test, a bug or a spike. Its frontmatter says which it is, which Feature it belongs to, and where it stands. The schema closes that record so that the item rules (SPEC040, SPEC041, SPEC043, SPEC044) read an `id`, a `parent` and a `type` of a known shape.

The schema file is the authority on the keys and their values. Read it there; this page does not repeat it.

### Example violation

An item whose `status` is not one of the schema's values:

```yaml
---
id: "0007-01"
parent: "0007-F1"
type: task
status: started
priority: med
---
```

```text
epics/0007-orders/F1-checkout/0007-01-validate-order-lines.md(1): error SPEC003: frontmatter Some properties did not match the required schema: ["status"]
epics/0007-orders/F1-checkout/0007-01-validate-order-lines.md(5): error SPEC003: frontmatter 'status' Value should match one of the values specified by the enum
```

The first line is the mapping's and says which keys failed. The second is the key's own, at the line it is declared on.

### Corrected

```yaml
---
id: "0007-01"
parent: "0007-F1"
type: task
status: in-progress
priority: med
---
```

Both blocks are shortened to the keys the example is about.

## How to fix violations

1. Start with the lines that name a key in quotes. Each gives the key, the line it is on, and what the schema expected of it.
2. Open `.spec/schema/task.frontmatter.schema.json` at that key and write a value it allows.
3. For `Required properties [...] are not present`, add each key listed.
4. For `All values fail against the false schema`, the key is not one the schema names. Remove it, or correct its spelling.
5. The line reported at the first line of the file needs no fix of its own; it clears when the keys it lists do.
6. If the file is not an item at all - a note that happens to be named like one - rename it so its name no longer starts `<epic>-<nn>-`, or move it out of the specification's directory.
