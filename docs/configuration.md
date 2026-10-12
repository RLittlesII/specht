---
title: "Configuring specht"
description: "The manifest keys specht reads, the default manifest, and how the schema version is chosen"
type: reference
---

# Configuration

The schema is per repository however the tool is installed. `specht` reads the
manifest from the root it is given, embeds every schema version it ships, and
checks with the pinned one, so one install can check a `0.1.0` repository and
a `1.0.0` one. A pin the tool does not ship exits `3`, naming the pin and the
versions it ships. A build installs through the local tool manifest, which
pins the tool's version; a global install puts `specht` on PATH.

## What is configurable

| Manifest key        | Read today                                                                                                                                                                                                                                                                                                                                               |
| ------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `schemaVersion`     | Which embedded version checks the tree: its frontmatter schemas and its rule vocabulary.                                                                                                                                                                                                                                                                 |
| `layouts`           | Where specifications are found: an ordered list, each entry a name and a path glob, which discovery matches and the summary and the report name each entry by. An entry's optional `identity` gives the zero-based path segments `SPEC011` checks for the epic and the Feature id; without one `SPEC011` skips the entry, and a malformed one exits `3`. |
| `exclusions`        | The directories discovery never looks in: a bare name at any depth, or a path from the root when the entry begins with `/`.                                                                                                                                                                                                                              |
| `taskFiles`         | The file-name shapes of an item beside a specification, a list of globs, `{task}` standing for `identifiers.task`. A file matching any one is an item the item rules read; an empty list exits `3`.                                                                                                                                                      |
| `epicFiles`         | The path globs of an epic file, a list. A file matching any one is an epic file; an empty list exits `3`.                                                                                                                                                                                                                                                |
| `companionFiles`    | The file-name globs of a companion beside a specification, a list. A file matching any one is a companion `SPEC020` and `SPEC021` read; an empty list exits `3`.                                                                                                                                                                                         |
| `sections`          | The `##` headings `SPEC010` requires, in order.                                                                                                                                                                                                                                                                                                          |
| `tables`            | The header cells `SPEC013` expects in a role's section, by role name; a key that is not a role exits `3`.                                                                                                                                                                                                                                                |
| `roles`             | The `sections` title that holds the claims, the matrix and the sign-off, which `SPEC013`, `SPEC021`, `SPEC030`, `SPEC031`, `SPEC060` and `SPEC061` look up.                                                                                                                                                                                              |
| `markers`           | The text of a missing-test cell, a draft and a blocked sign-off row and the approved status, which `SPEC060` and `SPEC061` look for.                                                                                                                                                                                                                     |
| `rules`             | Each rule id's level: `error` or `warning` re-grades its violations, `off` drops them. A rule left out keeps its own severity; an id outside the pinned vocabulary, or any other level, exits `3`.                                                                                                                                                       |
| `identifiers.claim` | The claim id grammar `SPEC030` enforces in § 3, and the § 3 ids a tag may resolve to under `SPEC021`.                                                                                                                                                                                                                                                    |

A key left out takes its value from the default manifest, except `rules`, which
is never filled: a manifest without it sets no level. A key the engine does
not know exits `3`; a key starting with `$` is ignored. The other `identifiers`
entries are accepted and no rule reads them yet. The frontmatter schemas a check
uses are the tool's embedded copy of the pinned version, not the files under
`.spec/schema/`.

Claim tags in a `.feature` file are read as `@B-nnn`, with an optional
lowercase letter suffix, whatever `identifiers.claim` says. A configurable tag
form and letter case are specified
([`src/specht/Manifest/.spec/README.md`](../src/specht/Manifest/.spec/README.md)
B-007, B-036–B-038) and not built yet. The frontmatter keys the rules read are
still literals in the engine; they move into the manifest in
[brief § 8](../.spec/brief.md#8-order-of-work) step 5.

## Default manifest

It lives at `<root>/.spec/schema/spec-structure.schema.json`. `specht init`
writes it when the file is absent. The block leaves out the five discovery keys
(`layouts`, `exclusions`, `taskFiles`, `epicFiles` and `companionFiles`): the
file carries each at its default, and a manifest without them is read the same
way. `init` writes no `rules` key; the empty one below is read the same way.

<!-- prettier-ignore -->
```json
{
  "$comment": "Not a JSON Schema. The ordered section manifest Specht rule SPEC010 reads, kept beside the schemas so the section contract - the twelve numbered sections plus Tasks and Scoring - lives in one place rather than as a string array in C#.",
  "sections": [
    "1. Business Goal",
    "2. User Needs",
    "3. Acceptance Criteria",
    "4. Constraints",
    "5. Out of Scope",
    "6. Concern Separation",
    "7. Technical Design",
    "8. Testing Strategy",
    "9. Traceability Matrix",
    "10. Lessons / Spec Deltas",
    "11. Open Questions",
    "12. Sign-off",
    "Tasks",
    "Scoring"
  ],
  "tables": {
    "matrix": [
      "Claim ID",
      "Scenario",
      "Test",
      "Status"
    ]
  },
  "identifiers": {
    "claim": "^B-[0-9]{3}[a-z]?$",
    "constraint": "^C-[0-9]+$",
    "openQuestion": "^OQ-[0-9]+$",
    "task": "^[0-9]{4}-[0-9]{2}$",
    "feature": "^F[0-9]+[a-z]?$",
    "epic": "^[0-9]{4}$"
  },
  "frontmatterSchemas": {
    "feature": "feature-spec.frontmatter.schema.json",
    "task": "task.frontmatter.schema.json",
    "epic": "epic.frontmatter.schema.json"
  },
  "roles": {
    "claims": "3. Acceptance Criteria",
    "matrix": "9. Traceability Matrix",
    "signOff": "12. Sign-off"
  },
  "markers": {
    "missing": "Missing",
    "draft": "🟡",
    "blocked": "🔴",
    "approved": "approved"
  },
  "rules": {}
}
```
