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

The frontmatter of an `epic.md` under `epics/` fails `epic.frontmatter.schema.json`, a copy of which sits in `.spec/schema/`. One line is reported for each failure the validator records:

| Failure at  | Line reported  | Message                             |
| ----------- | -------------- | ----------------------------------- |
| The mapping | 1              | `frontmatter {errors}`              |
| One key     | The key's line | `frontmatter '{location}' {errors}` |

`{errors}` is the validator's own text.

## Rule description

The frontmatter carries every key the schema requires, no key it does not name, and values of the type and vocabulary it states. The schema file is the authority.

### Example violation

The first lines of an `epic.md`:

```yaml
---
id: "0007"
type: epic
status: ready
priority: urgent
```

```text
epics/0007-orders/epic.md(1): error SPEC004: frontmatter Some properties did not match the required schema: ["priority"]
epics/0007-orders/epic.md(5): error SPEC004: frontmatter 'priority' Value should match one of the values specified by the enum
```

### Corrected

```yaml
priority: critical
```

## How to fix violations

- **A line naming a key.** Write a value the schema allows at that key.
- **`Required properties [...] are not present`.** Add each key listed.
- **`All values fail against the false schema`.** Remove the key, or correct its spelling.
- **A mapping line.** It clears when the keys it lists do.
