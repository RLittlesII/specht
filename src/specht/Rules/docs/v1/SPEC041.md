---
title: "SPEC041: Declared spikes resolve to one spike"
description: A Feature's spikes frontmatter names an item id that no file or several files carry, or that belongs to an item that is not a spike.
type: rule
---

# SPEC041: Declared spikes resolve to one spike

## Metadata

| Property         | Value   |
| ---------------- | ------- |
| Rule ID          | SPEC041 |
| Family           | Items   |
| Default severity | Error   |
| Schema version   | 1       |

## Cause

An entry in the `spikes` list of a Feature specification's frontmatter does not resolve to exactly one item file whose type is `spike`. The rule reports three conditions, each once per entry:

| Condition                               | File and line reported                 | Message                                                                          |
| --------------------------------------- | -------------------------------------- | -------------------------------------------------------------------------------- |
| No item file's name starts with the id  | The specification, at its `spikes` key | `frontmatter spikes names '{id}', but no '{id}-*.md' was discovered`             |
| Several item files' names start with it | The specification, at its `spikes` key | `frontmatter spikes names '{id}', which resolves to {count} files`               |
| The one file found is not a spike       | The item file, at its `type` key       | `'{id}' is declared in frontmatter spikes but its type is '{type}', not 'spike'` |

An entry resolves by file name, as a child does: the first seven characters of an item file's name, such as `0007-02`, are compared with the entry.

Spikes resolve across the repository. The files searched are the item files beside every discovered specification, in any Feature and any epic, not only the ones beside the specification that declares the spike. Children are the opposite case and resolve beside their own specification only (SPEC040).

## Rule description

A spike is an investigation whose answer a Feature waits on. Unlike a task it is not owned by the Features that cite it: one spike can answer a question for several Features, and it sits beside whichever one it was cut from. That is why `spikes` is a reference that may cross Features and epics, where `children` is ownership and may not.

The reference still has to land. An entry with no file is a dependency on an investigation nobody can find. An entry that lands on a task or a test says the Feature is waiting on research when the file is ordinary work, and the two are planned differently.

### Example violation

A specification that cites `0007-02` as a spike:

```yaml
---
id: "F1"
epic: "0007"
type: feature
spikes: ["0007-02"]
---
```

The file `epics/0007-orders/F2-refunds/0007-02-rounding-rules.md` declares itself a task:

```yaml
---
id: "0007-02"
parent: "0007-F2"
type: task
---
```

```text
epics/0007-orders/F2-refunds/0007-02-rounding-rules.md(4): error SPEC041: '0007-02' is declared in frontmatter spikes but its type is 'task', not 'spike'
```

The line is reported against the item, not the specification: the item's `type` is what disagrees.

### Corrected

```yaml
---
id: "0007-02"
parent: "0007-F2"
type: spike
---
```

## How to fix violations

- **Not a spike.** Two fixes are valid. If the item is an investigation, change its `type` to `spike`. If it is ordinary work that this Feature happens to need, leave its type alone and take the entry out of `spikes`; a Feature that owns the work lists it in `children`.
- **No file.** Correct a mistyped id, or write the spike as `<id>-<slug>.md` beside the specification it is cut from, with `type: spike`. The file must sit in a directory that holds a specification, or it is not discovered.
- **Several files.** Two files' names start with the same id. Keep the one the id belongs to and give the other the next unused id in its epic, in both its name and its `id`. The second file is reported in its own right as well, and by which rule depends on its frontmatter: if both files carry the same `id`, SPEC044 reports the reused id; if the second carries a different `id`, SPEC043 reports that it does not match the file's name, and SPEC044 reports nothing.
