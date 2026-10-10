---
title: "SPEC040: Declared children resolve to one item file"
description: A Feature's children frontmatter names an item id that no file, or more than one file, beside the specification carries.
type: rule
---

# SPEC040: Declared children resolve to one item file

## Metadata

| Property         | Value   |
| ---------------- | ------- |
| Rule ID          | SPEC040 |
| Family           | Items   |
| Default severity | Error   |
| Schema version   | 1       |

## Cause

An entry in the `children` list of a Feature specification's frontmatter does not resolve to exactly one item file in the specification's own directory. The rule reports two conditions, each once per entry, at the line of the `children` key:

| Condition                               | Message                                                                |
| --------------------------------------- | ---------------------------------------------------------------------- |
| No item file's name starts with the id  | `frontmatter children names '{id}', but no '{id}-*.md' was discovered` |
| Several item files' names start with it | `frontmatter children names '{id}', which resolves to {count} files`   |

An item file is a Markdown file beside the specification whose name starts `<epic>-<nn>-`, such as `0007-02-sum-order-lines.md`. An entry resolves by the file's name: its first seven characters, `0007-02`, are compared with the entry. The `id` inside the file is not read here; SPEC043 checks that it agrees with the name.

Children resolve beside the specification only. An item with the right name in another Feature's directory does not satisfy the entry: `children` states which items this Feature owns, and an owned item sits with its owner. Spikes are the opposite case and resolve across the repository (SPEC041).

The rule checks one direction. An item file beside a specification that `children` does not list is not reported.

## Rule description

`children` is a Feature's list of the work cut from it. Each entry is a promise that a file exists, and a reader following the list, or a tool computing what is left to do, opens the file the entry names. An entry with no file is work that is tracked nowhere; an entry with two files is one id with two descriptions.

### Example violation

A specification at `epics/0007-orders/F1-checkout/spec.md` that lists two children, in a directory holding only `0007-01-validate-order-lines.md`:

```yaml
---
id: "F1"
epic: "0007"
type: feature
children: ["0007-01", "0007-02"]
---
```

```text
epics/0007-orders/F1-checkout/spec.md(5): error SPEC040: frontmatter children names '0007-02', but no '0007-02-*.md' was discovered
```

### Corrected

The item is written beside the specification:

```text
epics/0007-orders/F1-checkout/spec.md
epics/0007-orders/F1-checkout/0007-01-validate-order-lines.md
epics/0007-orders/F1-checkout/0007-02-sum-order-lines.md
```

## How to fix violations

- **No file, and the work is real.** Create `<id>-<slug>.md` in the specification's directory, with frontmatter whose `id` is the entry and whose `parent` is this Feature.
- **No file, because it is somewhere else.** If the item was written in another Feature's directory, decide which Feature owns it. Move the file beside its owner and list it in that Feature's `children` only.
- **No file, because the entry is wrong.** Correct a mistyped id. Remove the entry only if the item never existed; an item that was started and dropped keeps its file and its entry, with its status saying so.
- **Several files.** Two files' names start with the same id. Keep the one the id belongs to and give the other the next unused id in the epic, in both its name and its `id`. The second file is reported in its own right as well, and by which rule depends on its frontmatter: if both files carry the same `id`, SPEC044 reports the reused id; if the second carries a different `id`, SPEC043 reports that it does not match the file's name, and SPEC044 reports nothing.
