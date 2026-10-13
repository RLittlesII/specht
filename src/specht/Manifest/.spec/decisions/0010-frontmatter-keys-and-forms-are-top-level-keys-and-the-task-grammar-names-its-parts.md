---
title: "Decision 0010: frontmatter keys and forms are top-level keys and the task grammar names its parts"
description: "Ratified October 13 2026 with keys grouped by document kind, identity and edge forms, named task groups and a schema 0.2.0 compatibility boundary; the earlier flat-key proposal remains as history"
type: decision
---

# Decision 0010: frontmatter keys and forms are top-level keys and the task grammar names its parts

**Date:** 2026-10-11
**Decided by:** the repository owner, 2026-10-13, in the resumed design and decision pass; the original proposal was authored on 2026-10-11

## Status

**Ratified with changes, 2026-10-13.** The owner chose grouped keys instead of
the proposed flat map. The current agreement is in the appended ratification;
the October 11 proposal and its reasoning below are retained as history.
Production code and tests are not delivered by this decision pass.

## Original proposal of October 11

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

2026-10-13: the owner rejected the flat key map and chose keys grouped by
document kind. The original rejection of grouped keys above is reversed:
independent Feature/item spellings justify the larger configuration shape.
The original per-name fill becomes per-kind and per-role fill. The schema
boundary, validation, diagnostic behavior and order are settled below.

## Ratification of October 13

The owner answered the concrete alternatives in this design pass. These calls
replace the unratified proposal wherever they differ.

| Concern                     | Agreed rule                                                                                                                                                                                   | Alternative rejected and its cost                                                                                                                                                   |
| --------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Public key shape, OQ-1      | `keys.feature` names `id`, `epic`, `specStatus`, `children`, `spikes`, `dependsOn`, `blocks`; `keys.task` names `id`, `parent`, `type`. Values default to the existing frontmatter spellings. | A flat map: smaller configuration, but a Feature and item cannot rename their `id` independently.                                                                                   |
| Forms, OQ-1                 | `forms.identity`, `forms.localEdge`, `forms.qualifiedEdge`, defaulting to `{epic}-{id}`, `{id}`, `{epic}/{id}`. Placeholders name semantic parts, independent of the key spelling.            | Separators alone cannot express forms with another ordering. Per-kind grouping changes keys, not these three form names.                                                            |
| Version boundary, OQ-16     | First ship the new interpretation in schema `0.2.0`; pins `0.1.0` and `0.1.1` retain existing parsing and validation.                                                                         | Applying stricter validation or changed diagnostic identifiers to older pins changes their verdicts despite frozen resource bytes.                                                  |
| Omission defaults, OQ-23    | Fill omissions from the pinned version's shipped manifest, per kind and role; explicit consumer settings win; rule levels are never inherited.                                                | One newest default for every pin allows updating the tool to change an older tree's verdict.                                                                                        |
| Task groups, OQ-6           | Require named `sequence`; also require named `epic` when the epic grammar is declared. Reject missing groups before rules under the new contract.                                             | A fallback keeps a second task expression or character-count grouping, or silently skips the sequence check. Older pins keep their existing behavior at the compatibility boundary. |
| Forms, OQ-21                | Require each expected placeholder exactly once and fill omitted forms from the pinned defaults. Reject malformed forms before rules.                                                          | Fallback behavior for incomplete declarations would require another agreement and can silently normalize unrelated edges to the same identity.                                      |
| Malformed edges, OQ-20      | Under `0.2.0`, `SPEC050` identifies an edge matching neither form by its original text.                                                                                                       | Expanding malformed identifiers under every pin obscures what the consumer wrote. Older pins retain expansion.                                                                      |
| Gap width, OQ-19            | Under `0.2.0`, pad the missing sequence number to the captured width of the reported item's sequence.                                                                                         | Two-digit padding under every custom grammar prints `03` when the item grammar uses `003`.                                                                                          |
| Implementation order, OQ-17 | Follow ADR-0010's rule-split and model/parser stages `0164` and `0166`; pinned-default work `0191` also precedes the implementation.                                                          | Building `0016` first delivers configuration sooner but requires the refactor branches to adapt its reads a second time.                                                            |

The first release condition is met: package tag `v0.1.90` was published on
2026-10-13. Package numbering does not choose the schema version.

`0001-F7` C-4 is amended: the pin selects manifest interpretation as well as
frontmatter schemas and rule vocabulary. The consumer's on-disk manifest
remains authoritative; selecting defaults never replaces explicit consumer
configuration or changes the frontmatter schema source. Versioned resources
alone do not preserve behavior: the parser and model build must preserve the
older interpretation too.

Claims B-048 to B-054 record these defaults, compatibility, rejections and
diagnostics. B-004 and B-019 are amended for grouped keys and omitted forms;
A-6 scopes the new contract to `0.2.0`. Item `0191` carries B-048; item `0016`
carries B-049 to B-054 alongside B-004 to B-006 and B-014. No new rule id,
schema-version file, dependency package or engine implementation is added by
the design pass.

OQ-18 remains open for the separate epic opt-in scope and its delivery order.
Nothing in this ratification disables epics in the default manifest, changes
absent-epic identity behavior, or decides the claim-tag names owned by `0134`.
The exact grammar-composition mechanics must be checked against the source
after the prerequisite stages, including anchor and capture interactions;
ratification is not evidence that an untested composition algorithm works.
