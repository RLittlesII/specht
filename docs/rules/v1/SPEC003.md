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

The frontmatter of an item file - a Markdown file beside a specification, named `<epic>-<nn>-<slug>.md` - fails `task.frontmatter.schema.json`, a copy of which sits in `.spec/schema/`. One line is reported for each failure the validator records:

| Failure at  | Line reported  | Message                             |
| ----------- | -------------- | ----------------------------------- |
| The mapping | 1              | `frontmatter {errors}`              |
| One key     | The key's line | `frontmatter '{location}' {errors}` |

`{errors}` is the validator's own text.

## Rule description

The frontmatter carries every key the schema requires, no key it does not name, and values of the type and vocabulary it states. The schema file is the authority.

### Example violation

The first lines of `0007-01-validate-order-lines.md`:

```yaml
---
id: "0007-01"
parent: "0007-F1"
type: task
status: started
```

```text
epics/0007-orders/F1-checkout/0007-01-validate-order-lines.md(1): error SPEC003: frontmatter Some properties did not match the required schema: ["status"]
epics/0007-orders/F1-checkout/0007-01-validate-order-lines.md(5): error SPEC003: frontmatter 'status' Value should match one of the values specified by the enum
```

### Corrected

```yaml
status: in-progress
```

## How to fix violations

- **A line naming a key.** Write a value the schema allows at that key.
- **`Required properties [...] are not present`.** Add each key listed.
- **`All values fail against the false schema`.** Remove the key, or correct its spelling.
- **A mapping line.** It clears when the keys it lists do.
- **The file is not an item.** Rename it so its name does not start `<epic>-<nn>-`.
