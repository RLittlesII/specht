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
[Multiple projects](multiple-projects.md) says what one run reads when a
repository holds several.

Use `specht schema list` to see the shipped versions and the selected pin.
Run `specht schema pin <version>` to change only that pin, or `specht schema pin`
to select with arrow keys and Enter at an interactive terminal. Pinning preserves
all other manifest settings and files. A downgrade can leave newer settings
that the next check rejects; edit those settings after pinning.

## What is configurable

| Manifest key        | Read today                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| ------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `schemaVersion`     | Which embedded version supplies the rule vocabulary and, with neither `spechtSchema` nor a disk selection, the frontmatter schemas.                                                                                                                                                                                                                                                                                                                                                |
| `codeAnalysis`      | Analyzer policy, including selected scope, required links, reasoned exemptions and explicit mappings to consumer configuration. Accepted on existing pins without a schema version bump. The standalone checker recognizes this section; the compiler component validates it using the actual supplied consumer inputs. See [native Gherkin claim input](native-gherkin-claims.md) for tagged `.feature` claims and the [analysis agreement](../src/codeanalysis/.spec/README.md). |
| `schemaSource`      | Under schema `0.2.0` or later, `embedded` or `disk` selects the frontmatter schemas. The check argument `--schema-source` overrides a valid manifest selection for one run. Omission keeps the automatic source choice. Older pins reject the key.                                                                                                                                                                                                                                 |
| `spechtSchema`      | The public upstream source fetched by `init --schemas` or `init --all`, and by `upgrade` for a present local copy that differs from its hash: `location` is an HTTPS base address, `version` is a nonempty label, and `hashes` records SHA-256 by kind. The check verifies and reads the local copy under `.specht/schema/frontmatter/`, and reports `schemaSource: "upstream"`.                                                                                                   |
| `layouts`           | Where specifications are found: an ordered list, each entry a name and a path glob, which discovery matches and the summary and the report name each entry by. An entry's optional `identity` gives the zero-based path segments `SPEC011` checks for the epic and the Feature id; without one `SPEC011` skips the entry, and a malformed one exits `3`.                                                                                                                           |
| `exclusions`        | The directories discovery never looks in: a bare name at any depth, or a path from the root when the entry begins with `/`.                                                                                                                                                                                                                                                                                                                                                        |
| `taskFiles`         | The file-name shapes of an item beside a specification, a list of globs, `{task}` standing for `identifiers.task`. A file matching any one is an item the item rules read; an empty list exits `3`.                                                                                                                                                                                                                                                                                |
| `epicFiles`         | The path globs of an epic file, a list. A file matching any one is an epic file; an empty list exits `3`.                                                                                                                                                                                                                                                                                                                                                                          |
| `companionFiles`    | The file-name globs of a companion beside a specification, a list. A file matching any one is a companion `SPEC020` and `SPEC021` read; an empty list exits `3`.                                                                                                                                                                                                                                                                                                                   |
| `sections`          | The `##` headings `SPEC010` requires, in order.                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| `tables`            | The header cells `SPEC013` expects in a role's section, by role name; a key that is not a role exits `3`.                                                                                                                                                                                                                                                                                                                                                                          |
| `roles`             | The `sections` title that holds the claims, the matrix and the sign-off, which `SPEC013`, `SPEC021`, `SPEC030`, `SPEC031`, `SPEC060` and `SPEC061` look up.                                                                                                                                                                                                                                                                                                                        |
| `markers`           | The text of a missing-test cell, a draft and a blocked sign-off row and the approved status, which `SPEC060` and `SPEC061` look for.                                                                                                                                                                                                                                                                                                                                               |
| `rules`             | Each rule id's level: `error` or `warning` re-grades its violations, `off` drops them. A rule left out keeps its own severity; an id outside the pinned vocabulary, or any other level, exits `3`.                                                                                                                                                                                                                                                                                 |
| `identifiers.claim` | The claim id grammar `SPEC030` enforces in § 3, and the § 3 ids a tag may resolve to under `SPEC021`.                                                                                                                                                                                                                                                                                                                                                                              |

Under schema `0.2.0`, `markers` also names `covered`, `partial`, `waived`, `withdrawn` and `superseded`, default `Covered`, `Partial`, `Waived`, `Withdrawn` and `Superseded`. Four more keys are accepted and no rule reads them yet; none is filled from a default, and older pins reject them:

- `tests`: `{ "files": [<glob>], "declarations": [<form>] }`, `declarations` optional; each form holds `{test}` exactly once.
- `matrix`: `{ "scenarioTitles": <bool> }`, `false` when left out.
- `owners`: a `sections` title mapped to a non-empty list of roles.
- `requiredSpecifications`: a list of directory globs, empty when left out.

Under schema `0.2.0`, `claim.tag` is literal text around exactly one `{claim}` placeholder, default `@{claim}`. `claim.tagIgnoreCase` is Boolean, default `false`; when true it covers the entire tag and resolution uses ordinal comparison. Missing values come from the pinned version's shipped manifest. Older pins reject `claim`; flat tag keys are rejected at every pin. Each missing member of `claim` uses the pinned default, including a partially declared object.

A key left out takes its value from the default manifest, except `rules`, which
is never filled: a manifest without it sets no level. A key the engine does
not know exits `3`; a key starting with `$` is ignored. The other `identifiers`
entries are accepted and no rule reads them yet.

Without `spechtSchema` or an explicit selection, the check uses the pinned version's embedded frontmatter schemas. Select `disk` to maintain your own local files:

```json
{
  "schemaVersion": "0.2.0",
  "schemaSource": "disk",
  "frontmatterSchemas": {
    "feature": "feature.schema.json",
    "task": "task.schema.json"
  }
}
```

Place those files under `.specht/schema/frontmatter/`. The Feature and task files
are required, and epic only when the original manifest declares `identifiers.epic`.
Names must be plain filenames. A missing or invalid selected schema exits `3`,
with a relative file diagnostic and no stdout. Local schemas need no URL or hash.
For a one-run selection with any shipped pin, use `specht --schema-source disk`.
The JSON report says `"schemaSource": "disk"`. Source selection changes only
frontmatter schemas; the manifest, version pin and rule vocabulary stay the same.

A recorded `spechtSchema` takes the verified upstream path even under explicit
`disk`; explicit `embedded` contradicts that record and exits `3`.

With a recorded source, the check reads the local files named by `frontmatterSchemas`
under `.specht/schema/frontmatter/`. The recording requires a nonempty `location`
and `version`, plus a hash for `feature` and `task`, and for `epic` when the
manifest explicitly declares `identifiers.epic`. Each hash is `sha256:` followed
by 64 lowercase hexadecimal digits, computed over the original file bytes.
The upstream version is independent of the pinned rule vocabulary.

A missing or changed local file exits `3`, naming its relative path on stderr.
For a missing file, the message names `specht init`; use `specht init --schemas`
to fetch missing schema files from the recorded source. `specht upgrade`
replaces a changed file with the verified source copy and leaves a missing one
missing, and does the same for each template the record hashes under
`templates`, except one named in `consumerTemplates`, which it leaves untouched. The check never fetches, and it never writes
into the root.

Under schema `0.2.0`, companion claim tags follow `identifiers.claim` inside the literal tag form. For example, `@req:{claim}` recognizes `@req:B-001`; punctuation in the form is literal. SPEC021 quotes an unresolved tag exactly as written and carries the claim id alone as its identifier. Claim and matrix ids remain case-sensitive even when tags ignore case. Full-line comments are skipped.

Pin `0.1.0` retains the original `@B-nnn` scanner with an optional lowercase suffix. The frontmatter keys remain literals pending the separate frontmatter contract.

## Default manifest

It lives at `<root>/.specht/manifest.json`. Every `specht init` selects it;
`--schemas`, `--templates` and `--all` add the other parts. The block leaves out the five discovery keys
(`layouts`, `exclusions`, `taskFiles`, `epicFiles` and `companionFiles`): the
file carries each at its default, and a manifest without them is read the same
way. The block also omits this repository's `rules` setting: consumers that leave a rule unset keep that rule's own severity. The shipping manifest currently has no `rules` key; the complete default map is specified in `0001-F5` decision 0009 and is not built yet.

<!-- prettier-ignore -->
```json
{
  "schemaVersion": "0.2.0",
  "$comment": "Not a JSON Schema. The ordered section manifest Specht rule SPEC010 reads, kept as data so the section contract - the twelve numbered sections plus Tasks and Scoring - lives in one place rather than as a string array in C#.",
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
    "feature": "feature.schema.json",
    "task": "task.schema.json",
    "epic": "epic.schema.json"
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
    "approved": "approved",
    "covered": "Covered",
    "partial": "Partial",
    "waived": "Waived",
    "withdrawn": "Withdrawn",
    "superseded": "Superseded"
  },
  "claim": {
    "tag": "@{claim}",
    "tagIgnoreCase": false
  }
}
```

A root without a manifest can record this source with `init --upstream`,
`--upstream-version` and repeated `--upstream-hash kind=hash` options. Hashes
are required before first use. Fetching still requires `--schemas` or `--all`;
source options beside an existing manifest are refused. See [the CLI reference](cli.md#specht-init).
