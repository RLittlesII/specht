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

The frontmatter of a Feature specification fails `feature-spec.frontmatter.schema.json`, a copy of which sits in `.spec/schema/`. One line is reported for each failure the validator records:

| Failure at  | Line reported  | Message                             |
| ----------- | -------------- | ----------------------------------- |
| The mapping | 1              | `frontmatter {errors}`              |
| One key     | The key's line | `frontmatter '{location}' {errors}` |

`{errors}` is the validator's own text. When `scored_by` or `github_issue` is `null`, any other failure also prints two lines for that key: a mapping line listing only it, and `Value is "null" but should be "string"` (or `"integer"`). `null` is valid for both keys; ignore those lines, which go when the real failure is fixed.

## Rule description

The frontmatter carries every key the schema requires, no key it does not name, and values of the type and vocabulary it states. The schema file is the authority.

### Example violation

The first lines of a specification whose `scored_by` and `github_issue` are set:

```yaml
---
id: "F1"
epic: "0007"
type: feature
spec_status: accepted
```

```text
src/orders/.spec/README.md(1): error SPEC002: frontmatter Some properties did not match the required schema: ["spec_status"]
src/orders/.spec/README.md(5): error SPEC002: frontmatter 'spec_status' Value should match one of the values specified by the enum
```

### Corrected

```yaml
spec_status: in-review
```

## How to fix violations

- **A line naming a key.** Write a value the schema allows at that key.
- **`Required properties [...] are not present`.** Add each key listed; `.spec/templates/feature.md` carries them all.
- **`All values fail against the false schema`.** Remove the key, or correct its spelling.
- **A mapping line.** It clears when the keys it lists do.
