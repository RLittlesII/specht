---
title: "SPEC050: Dependency edges name a discovered Feature"
description: A Feature's depends_on or blocks frontmatter names a Feature the tool did not discover.
type: rule
---

# SPEC050: Dependency edges name a discovered Feature

## Metadata

| Property         | Value        |
| ---------------- | ------------ |
| Rule ID          | SPEC050      |
| Family           | Dependencies |
| Default severity | Error        |
| Schema version   | 1            |

## Cause

An entry in the `depends_on` or `blocks` list of a Feature specification's frontmatter resolves to an identity no discovered specification has. The rule reports once per entry, at the line of the key that holds it, with the message:

`frontmatter {key} names '{identity}', which is not a discovered Feature`

`{key}` is `depends_on` or `blocks`. `{identity}` is the entry in its qualified form, which is how it is resolved:

| Entry as written | Meaning                             | Resolves to                         |
| ---------------- | ----------------------------------- | ----------------------------------- |
| `F2`             | Feature `F2` of this Feature's epic | `<this epic>-F2`, such as `0007-F2` |
| `0002/F1`        | Feature `F1` of epic `0002`         | `0002-F1`                           |

A discovered Feature is a specification in either layout whose frontmatter carries both `epic` and `id`; its identity is the two joined by a hyphen. A specification with either key absent has no identity, cannot be the target of an edge, and has no edges checked itself.

## Rule description

`depends_on` says which Features this one waits on, and `blocks` says which wait on it. Both are read to order work: what can start, and what a change will hold up. An edge to a Feature that does not exist orders work against nothing. It usually means the target was renumbered, moved to another epic, or never written, and the dependency it stood for is now recorded nowhere.

The short form is relative to the Feature's own epic, so the same entry `F2` means different Features in different epics. An edge that crosses epics is always written in the qualified form.

### Example violation

A specification in epic `0007` that depends on a Feature of epic `0002`, written in the short form:

```yaml
---
id: "F1"
epic: "0007"
type: feature
depends_on: ["F3"]
blocks: []
---
```

```text
src/orders/.spec/README.md(5): error SPEC050: frontmatter depends_on names '0007-F3', which is not a discovered Feature
```

The message shows how the entry was read: `F3` became `0007-F3`, and epic `0007` has no `F3`.

### Corrected

```yaml
---
id: "F1"
epic: "0007"
type: feature
depends_on: ["0002/F3"]
blocks: []
---
```

## How to fix violations

Read the identity in the message, then find out why no specification has it.

- **The target is in another epic.** Write the entry in the qualified form, `<epic>/<id>`.
- **The entry is mistyped, or the target's id changed.** Correct the entry to the identity the target's frontmatter declares.
- **The target is not written yet.** Write its specification first, or take the entry out until it exists; record the intended dependency in the specification's prose so it is not lost.
- **The target exists but was not discovered.** A specification is found at `<area>/.spec/README.md` or `epics/<epic>/<feature>/spec.md`, and needs `epic` and `id` in its frontmatter. Check its path and those two keys; SPEC001 and SPEC002 report a file whose frontmatter cannot supply them.

Once the edge resolves, the target must declare it from its own end (SPEC051).
