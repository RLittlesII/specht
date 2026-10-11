---
title: "Decision 0010: frontmatter keys and forms are top-level keys and the task grammar names its parts"
description: "Proposed and not ratified: the manifest names each frontmatter key a rule reads in a top-level keys object, writes the identity and the two edge forms as templates in a top-level forms object, and gives the default task grammar the named groups epic and sequence; no claim is written against any of it until the repository owner ratifies it"
type: decision
---

# Decision 0010: frontmatter keys and forms are top-level keys and the task grammar names its parts

**Date:** 2026-10-11
**Decided by:** nobody yet. Proposed by `implementer` in § 7 on item `0016`'s design pass, recorded by `spec-author`, awaiting the repository owner's ratification

## Status

**Proposed. Not ratified.** Nothing below is a decision until the repository
owner ratifies it (§ 2 A-3, § 11 OQ-1). No claim, constraint or scenario is
written against any name here, no code reads one, and no manifest carries one.
Ratification is recorded in this file, dated, with what the owner changed.

## The call

Three proposals, for B-004, B-005 and B-006.

**1. `keys`.** A top-level object mapping a name to the frontmatter key a rule
reads. An omitted name is filled from the default manifest, name by name.

| Name         | Default       |
| ------------ | ------------- |
| `id`         | `id`          |
| `epic`       | `epic`        |
| `type`       | `type`        |
| `specStatus` | `spec_status` |
| `parent`     | `parent`      |
| `children`   | `children`    |
| `spikes`     | `spikes`      |
| `dependsOn`  | `depends_on`  |
| `blocks`     | `blocks`      |

**2. `forms`.** A top-level object mapping a name to a template of literal
text and placeholders.

| Name            | Default       | Read as                                     |
| --------------- | ------------- | ------------------------------------------- |
| `identity`      | `{epic}-{id}` | A Feature's identity                        |
| `localEdge`     | `{id}`        | An edge to a Feature of the declaring epic  |
| `qualifiedEdge` | `{epic}/{id}` | An edge to a Feature of the epic it carries |

- A placeholder is a `keys` name in braces.
- A form holds no regular expression.
- In an edge form `{epic}` stands for the epic grammar and `{id}` for the
  Feature grammar.
- An edge is tried against the qualified form first.

**3. The task grammar.** The default `identifiers.task` becomes
`^(?<epic>[0-9]{4})-(?<sequence>[0-9]{2})$`. It matches the ids it matches
today, and `SPEC043` and `SPEC044` read an item's epic and sequence from the
groups `epic` and `sequence`.

```json
{
  "keys": { "specStatus": "spec_status", "dependsOn": "depends_on" },
  "forms": {
    "identity": "{epic}-{id}",
    "localEdge": "{id}",
    "qualifiedEdge": "{epic}/{id}"
  },
  "identifiers": { "task": "^(?<epic>[0-9]{4})-(?<sequence>[0-9]{2})$" }
}
```

## Why

- `keys` and `forms` take the shape decision 0006 gave `roles` and `markers`:
  a flat top-level object of texts, filled per name.
- A name is camel case, as `signOff` and `openQuestion` are, and its value is
  the key as a specification writes it.
- A placeholder in braces is how B-024 already writes the identity form and
  how B-036 writes the claim tag form.
- An edge form built from the epic and Feature grammars adds no second grammar
  (C-2), and is the first read of those two grammars by any rule.
- Named groups replace a prefix read by character count and a second task
  expression, both of which C-2 rules out.

## Rejected

Each was turned down by the `implementer` in § 7. The owner may take any of
them at ratification.

**Keys grouped by kind**, `keys.feature.id` beside `keys.task.id`. Cost of
rejecting: `id` is renamed for a Feature and an item together, though the
frontmatter schemas are per kind (C-8).

**A `keys` name spelled as its default**, `spec_status` and `depends_on`. Cost
of rejecting: two names, `specStatus` and `dependsOn`, differ from the keys a
specification writes. Spelled as its default, a renamed key reads
`"spec_status": "status"`, where the name on the left is a key nobody writes.

**A separator in place of a form**, such as `"edgeSeparator": "/"`. Cost of
rejecting: a larger key than the default needs. A separator cannot say
`{id}@{epic}`, and B-005 and B-024 ask for a form.

**A placeholder standing for any text.** Cost of rejecting: an edge that fits
neither form changes the text `SPEC050` reports (§ 11 OQ-20). It leaves the
epic and Feature grammars unread by every rule (C-2), and without the epic
grammar it reads `0002/F1` as qualified, which B-029 rules out.

**A fallback for a task grammar without its groups.** Cost of rejecting: such
a grammar is rejected or goes unchecked (§ 11 OQ-6). The fallback is the
character count and the second expression kept, which C-2 rules out.

The rejections about the code's shape are the `implementer`'s and stay in § 7:
compiled expressions carried on the structure, an item's file-name id read by
matching the grammar at the start of the name, and a rule per commit by moving
the reads into the rules.

## Calls left to the owner

1. **An edge that fits neither form.** Today it is still resolved, so `F2x`
   is reported under `SPEC050` as `0001-F2x`. The proposal reports it as
   written, `F2x`. That changes a reported identifier; the golden report holds
   no such edge. § 11 OQ-20.
2. **A task grammar without its groups.** Whether it is rejected, and from
   which schema version. The rejection would reach every manifest
   `specht init` writes today, which declares the grammar without groups.
   § 11 OQ-6 and OQ-16.

## Claims owed and not written

Each waits on ratification: a claim written on an unratified name would have
to be withdrawn.

- A rejection of a task grammar that names no `sequence` group, or no `epic`
  group while the manifest declares the epic grammar (B-032), with its place
  in B-022 and B-023.
- A rejection of a form that does not hold each of its placeholders exactly
  once, with its place in B-022 and B-023 (§ 11 OQ-21).
- B-019 gaining "a form" among the omitted values (§ 11 OQ-21).
- A-1's "this repository's live copy is amended in that change", restated
  once schema `0.1.0` is published (§ 11 OQ-16).

## Affects

Nothing until ratified. On ratification:

- § 11 OQ-1, for frontmatter keys and forms; OQ-6.
- B-004, B-005 and B-006: the names their tests are written on. Their texts
  are unchanged.
- B-019, B-022, B-023 and A-1, as "Claims owed" lists.
- § 6 and § 7, which call every name here proposed.
- Not decided here: the schema version that first carries the keys (OQ-16);
  the order against ADR-0010 (OQ-17); the item for the epics opt-in (OQ-18);
  the width in the `SPEC044` message (OQ-19).

## Reversal

None.
