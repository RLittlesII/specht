---
title: "SPEC001: Specification artifacts open with frontmatter"
description: A Feature specification, an item file or an epic file has no YAML frontmatter the tool can read.
type: rule
---

# SPEC001: Specification artifacts open with frontmatter

## Metadata

| Property         | Value       |
| ---------------- | ----------- |
| Rule ID          | SPEC001     |
| Family           | Frontmatter |
| Default severity | Error       |
| Schema version   | 1           |

## Cause

A file the tool reads frontmatter from has none. Three kinds of file are read:

- every Feature specification, in both layouts: `<area>/.spec/README.md` and `epics/<epic>/<feature>/spec.md`;
- every item file beside a specification, named `<epic>-<nn>-<slug>.md`;
- every `epic.md` under `epics/`.

A file has no frontmatter when any of these holds:

- its first line is not `---`;
- no later line closes the block with `---`;
- the text between the two delimiters is not valid YAML;
- the YAML is valid but is not a mapping of keys to values, such as a list or a single scalar.

The rule reports once per file, always at line 1, with the message:

`no YAML frontmatter - every tracked specification artifact opens with a '---' delimited mapping`

## Rule description

Every other frontmatter rule reads keys from the mapping at the top of the file: the identity of a Feature, its dependencies, its children, an item's parent. With no mapping there is nothing to read, so the file is reported here and its frontmatter is not checked against its schema (SPEC002, SPEC003, SPEC004).

The block must be the first thing in the file. A heading, a blank line or a comment above the opening `---` means the file does not open with frontmatter.

### Example violation

A specification that starts at its heading:

```markdown
# Specification: Orders

## 1. Business Goal

An order is never accepted with no lines.
```

```text
src/orders/.spec/README.md(1): error SPEC001: no YAML frontmatter - every tracked specification artifact opens with a '---' delimited mapping
```

### Corrected

```markdown
---
id: "F1"
epic: "0007"
type: feature
spec_status: draft
---

# Specification: Orders

## 1. Business Goal

An order is never accepted with no lines.
```

The block above is shortened. The keys a Feature must carry are the ones its frontmatter schema requires; a block that is present but incomplete is reported by SPEC002.

## How to fix violations

1. Open the file the line names and look at its first line.
2. If the file has no frontmatter, add a `---` delimited block as the first thing in it. For a Feature, start from `.spec/templates/feature.md`, which carries every required key.
3. If the file has a block, find what stops it being read: text above the opening `---`, a missing closing `---`, or YAML that does not parse - most often a value containing `: ` or starting with `[`, `{` or `@` that needs quoting.
4. Run the check again. Any key the schema requires and the block lacks is then reported by SPEC002, SPEC003 or SPEC004.
