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

Use `specht schema list` to see the shipped versions, the selected pin and, under a floating `pinPolicy`, the version in use.
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
| `pinPolicy`         | Under schema `0.3.0` or later, `exact` (the default), `latest-patch` or `latest-minor` (not below `1.0.0`) selects the version the check uses: the pin, or the newest shipped patch or minor of it. `upgrade` writes that version's files and `schema list` marks it ` (in use)`; every other command keeps the pin. Any other value exits `3`; older pins reject the key.                                                                                                         |
| `spechtSchema`      | The public upstream source fetched by `init --schemas` or `init --all`, and by `upgrade` for a present local copy that differs from its hash: `location` is an HTTPS base address, `version` is a nonempty label, and `hashes` records SHA-256 by kind. The check verifies and reads the local copy under `.specht/schema/frontmatter/`, and reports `schemaSource: "upstream"`.                                                                                                   |
| `consumerTemplates` | Template file names under `.specht/templates/` that are the consumer's, such as `["feature.md"]`. `upgrade` never reads or writes one the pinned version ships, whatever source is selected or recorded, and lists it as skipped. A name matches a file name exactly, case included; one the pinned version does not ship has no effect. Nothing the check reports depends on the names.                                                                                           |
| `layouts`           | Where specifications are found: an ordered list, each entry a name and a path glob, which discovery matches and the summary and the report name each entry by. An entry's optional `identity` gives the zero-based path segments `SPEC3001` checks for the epic and the Feature id; without one `SPEC3001` skips the entry, and a malformed one exits `3`. From schema `0.3.0` an `identity` member starting with `$` is ignored.                                                  |
| `exclusions`        | The directories discovery never looks in: a bare name at any depth, or a path from the root when the entry begins with `/`. Inside a git work tree discovery asks `git` for the tracked and untracked files, so it also skips what git ignores; outside one, or without `git`, it walks the tree. Neither way follows a symbolic link to a directory.                                                                                                                              |
| `taskFiles`         | The file-name shapes of an item beside a specification, a list of globs, `{task}` standing for `identifiers.task`. A file matching any one is an item the item rules read; an empty list exits `3`.                                                                                                                                                                                                                                                                                |
| `epicFiles`         | The path globs of an epic file, a list. A file matching any one is an epic file; an empty list exits `3`.                                                                                                                                                                                                                                                                                                                                                                          |
| `companionFiles`    | The file-name globs of a companion beside a specification, a list. A file matching any one is a companion `SPEC4001` and `SPEC4002` read; an empty list exits `3`.                                                                                                                                                                                                                                                                                                                 |
| `sections`          | The `##` headings `SPEC2001` requires, in order.                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| `tables`            | The header cells `SPEC2002` expects in a role's section, by role name; a key that is not a role exits `3`.                                                                                                                                                                                                                                                                                                                                                                         |
| `roles`             | The `sections` title that holds the claims, the matrix and the sign-off, which `SPEC2002`, `SPEC4002`, `SPEC4003`, `SPEC4004`, `SPEC7001` and `SPEC7002` look up.                                                                                                                                                                                                                                                                                                                  |
| `markers`           | The text of a missing-test cell, a draft and a blocked sign-off row and the approved status, which `SPEC7001` and `SPEC7002` look for.                                                                                                                                                                                                                                                                                                                                             |
| `rules`             | Each rule id's level: `error` or `warning` re-grades its violations, `off` drops them. A rule left out keeps its own severity; an id outside the pinned vocabulary, or any other level, exits `3`.                                                                                                                                                                                                                                                                                 |
| `identifiers.claim` | Below schema `0.4.0`, the claim id grammar `SPEC4003` enforces in § 3, and the § 3 ids a tag may resolve to under `SPEC4002`.                                                                                                                                                                                                                                                                                                                                                      |
| `ids`               | From schema `0.4.0`, one record per id type that shapes its ids; `ids.claim` is the claim grammar. See [Id records](#id-records).                                                                                                                                                                                                                                                                                                                                                  |

Under schema `0.2.0`, `markers` also names `covered`, `partial`, `waived`, `withdrawn` and `superseded`, default `Covered`, `Partial`, `Waived`, `Withdrawn` and `Superseded`. From schema `0.4.0`, `SPEC4008` reads `missing`, `waived`, `withdrawn` and `superseded`. Four more keys are accepted; none is filled from a default, and older pins reject them:

- `tests`: `{ "files": [<glob>], "declarations": [<form>] }`, `declarations` optional; each form holds `{test}` exactly once. From schema `0.3.0`, `SPEC4009` resolves each § 9 test name against these files.
- `matrix`: `{ "scenarioTitles": <bool> }`, `false` when left out. From schema `0.3.0`, `true` has `SPEC4007` match each § 9 Scenario cell title to a companion scenario; no rule reads `owners` or `requiredSpecifications` yet.
- `owners`: a `sections` title mapped to a non-empty list of roles.
- `requiredSpecifications`: a list of directory globs, empty when left out.

Schema `0.3.1` reads every key and reports every rule `0.3.0` does. It also accepts `mint`, the id settings, an object keyed by record type, and older pins reject it. The check reads nothing from it, so a manifest with it gets the report it gets without it; [Minted ids](#minted-ids) describes it.

Schema `0.3.1` also names `keys.feature.claimPrefix`, default `claim_prefix`: the Feature frontmatter field holding the Feature's own claim prefix, text with at least one non-white-space character, which `SPEC1002` checks. A Feature that declares one, such as `claim_prefix: CI-AC-`, reads its claim ids as that literal, case-sensitive prefix, three ASCII digits and an optional lowercase letter, `CI-AC-001a`; the declaration takes precedence over `identifiers.claim`, which still governs every Feature that declares none. Its matrix and companion tags resolve the complete id under `claim.tag` and `claim.tagIgnoreCase`. Older pins give the field no meaning.

Schema `0.4.0` adds `ids` and moves the claim, constraint and open-question grammars out of `identifiers`, and `mint`, into it; [Id records](#id-records) describes it. It reports every rule `0.3.1` does.

Under schema `0.3.0`, `markers` also names `done`, default `done`: the delivery `status` at which `SPEC7001` reports a `Missing` cell. An empty one exits `3`. From schema `0.3.0` any marker of white space alone exits `3` too.

Under schema `0.3.0`, two more keys are filled name by name from the pinned default manifest, and older pins reject them:

- `keys`: the frontmatter key each rule reads, by kind. `feature` names `id`, `epic`, `specStatus`, `status`, `children`, `spikes`, `dependsOn` and `blocks`, default `id`, `epic`, `spec_status`, `status`, `children`, `spikes`, `depends_on` and `blocks`; `task` names `id`, `parent` and `type`, each its own default. Rename a key in the frontmatter schemas too.
- `forms`: `identity`, default `{epic}-{id}`, a Feature's identity and an item's `parent`; `localEdge`, default `{id}`, an edge to a Feature of the same epic; `qualifiedEdge`, default `{epic}/{id}`, an edge to another epic's Feature. `identity` and `qualifiedEdge` hold `{epic}` and `{id}` once each, and `localEdge` holds `{id}` once, or the run exits `3`.

Under schema `0.3.0` every grammar must compile, and `identifiers.task` must name a `sequence` group, and an `epic` group when the manifest declares `identifiers.epic`; otherwise the run exits `3`. The default is `^(?<epic>[0-9]{4})-(?<sequence>[0-9]{2})$`. A manifest moved from `0.2.x` that kept `^[0-9]{4}-[0-9]{2}$` exits `3` until the groups are named.

With `identifiers.epic` declared, edges resolve through the forms, each part read by the `epic` and `feature` grammars; an edge fitting neither form is reported by `SPEC6001` as written. Items are sequenced per captured epic, and a gap is written at the captured sequence's width. A Feature with no `epic` is reported by `SPEC3001` and keeps the identity its form renders, `-F2`.

Without `identifiers.epic`, under schema `0.3.0`, a Feature's identity is its `id`, a local edge resolves by `id`, and a qualified edge names no Feature. Items are sequenced per Feature, each under the Feature whose folder holds it, whatever groups `identifiers.task` names; an item in no Feature's folder is not sequenced. No epic file is read. The shipped default declares `identifiers.epic`; `specht init` without `--epics` writes a manifest without it. Under older pins a manifest without `identifiers.epic` is read as before.

Under schema `0.2.0`, `claim.tag` is literal text around exactly one `{claim}` placeholder, default `@{claim}`. `claim.tagIgnoreCase` is Boolean, default `false`; when true it covers the entire tag and resolution uses ordinal comparison. Missing values come from the pinned version's shipped manifest. Older pins reject `claim`; flat tag keys are rejected at every pin. Each missing member of `claim` uses the pinned default, including a partially declared object.

Schema `0.4.0` adds the `claim` members that select native claim input; older pins leave them unread. `claim.source` is `markdown`, the default, or `gherkin`; any other value exits `3`. Under `gherkin`, `claim.files` lists the claim file sets, each with a text `glob` and `feature`, an `epic` exactly when the manifest declares `identifiers.epic`, and an optional `record`; a missing, empty or malformed list exits `3`, and so does `claim.files` beside any other source. `claim.plannedTags`, default `["@ignore"]`, `claim.withdrawnTags` and `claim.supersededTags`, default empty, are tag lists, and `claim.lifecycleKey`, default `spec_status`, names a record's lifecycle key. `claim.scopes.featureTag` and `claim.scopes.epicTag` are optional tag forms. No rule reads Gherkin claims yet.

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
Names must be plain filenames; from schema `0.3.0` an empty name or `.` exits `3`. A missing or invalid selected schema exits `3`,
with a relative file diagnostic and no stdout. Local schemas need no URL or hash.
For a one-run selection with any shipped pin, use `specht --schema-source disk`.
The JSON report says `"schemaSource": "disk"`. Source selection changes only
frontmatter schemas; the manifest, version pin and rule vocabulary stay the same.

### Shared frontmatter schemas

A recorded `spechtSchema` takes the verified upstream path even under explicit
`disk`; explicit `embedded` contradicts that record and exits `3`.

With a recorded source, the check reads the local files named by `frontmatterSchemas`
under `.specht/schema/frontmatter/`. The recording requires a nonempty `location`
and `version`, plus a hash for `feature` and `task`, and for `epic` when the
manifest explicitly declares `identifiers.epic`. Each hash is `sha256:` followed
by 64 lowercase hexadecimal digits, computed over the original file bytes.
The upstream version is independent of the pinned rule vocabulary.

For example, a team can share custom Feature and task frontmatter schemas.
Merge these keys into the repository manifest, substitute your public HTTPS
directory and version label, and replace each placeholder with the SHA-256
of the corresponding file. The [setup guide](setup.md#host-your-schemas-on-github)
shows how to compute the hashes and fetch the files.

```json
{
  "$comment.frontmatterSchemas": "Use these filenames in both the source directory and .specht/schema/frontmatter/.",
  "frontmatterSchemas": {
    "feature": "feature-spec.frontmatter.schema.json",
    "task": "task.frontmatter.schema.json"
  },
  "$comment.spechtSchema": "Init and upgrade fetch from this directory. The check verifies the files already on disk.",
  "spechtSchema": {
    "location": "https://schemas.example.org/frontmatter/",
    "version": "team-schemas-v1",
    "hashes": {
      "feature": "sha256:<feature-file-sha256>",
      "task": "sha256:<task-file-sha256>"
    }
  }
}
```

With epics enabled, add the epic filename and hash. Set `epicFiles` to the
paths that hold epic records, such as `["**/.spec/epic.md"]`.

A missing or changed local file exits `3`, naming its relative path on stderr.
For a missing file, the message names `specht init`; use `specht init --schemas`
to fetch missing schema files from the recorded source. `specht upgrade`
replaces a changed file with the verified source copy and leaves a missing one
missing, and does the same for each template the record hashes under
`templates`, except one named in `consumerTemplates`, which it leaves untouched. The check never fetches, and it never writes
into the root.

Under schema `0.2.0`, companion claim tags follow `identifiers.claim` inside the literal tag form. For example, `@req:{claim}` recognizes `@req:B-001`; punctuation in the form is literal. SPEC4002 (SPEC021 below `0.3.0`) quotes an unresolved tag exactly as written and carries the claim id alone as its identifier. Claim and matrix ids remain case-sensitive even when tags ignore case. Full-line comments are skipped.

Pin `0.1.0` retains the original `@B-nnn` scanner with an optional lowercase suffix. Pins below `0.3.0` read the frontmatter keys by their default names.

## Id records

From schema `0.4.0` the manifest's `ids` holds one record per id type:
`claim`, `constraint`, `openQuestion`, `scenario`, `decision`, `adr` and
`lesson`. A record has three members:

| Member  | Does                                                                                                                        |
| ------- | --------------------------------------------------------------------------------------------------------------------------- |
| `plain` | The id where no claim prefix applies: literal text ending with its only `{id}`, no other brace.                             |
| `form`  | The id in a Feature that declares a claim prefix: holds `{prefix}` once and ends with `{id}`. `adr` and `lesson` take none. |
| `id`    | A regular expression the text in the `{id}` place matches whole. It must compile and not be empty.                          |

A type or member left out takes the shipped record's value. These are the
shipped records, with `[0-9a-hjkmnp-tv-z]` written `b32`:

| Type           | `plain`   | `form`         | `id`                              |
| -------------- | --------- | -------------- | --------------------------------- |
| `claim`        | `B-{id}`  | `{prefix}{id}` | `[0-9]{3}[a-z]?\|b32{6}\|b32{13}` |
| `constraint`   | `C-{id}`  |                | `[0-9]+\|b32{6}\|b32{13}`         |
| `openQuestion` | `OQ-{id}` |                | `[0-9]+\|b32{6}\|b32{13}`         |
| `scenario`     | `S-{id}`  |                | `[0-9]+\|b32{6}\|b32{13}`         |
| `decision`     | `{id}`    |                | `[0-9]{4}\|b32{6}\|b32{13}`       |
| `adr`          | `{id}`    |                | `[0-9]{4}\|b32{8}\|b32{13}`       |
| `lesson`       | `{id}`    |                | `[0-9]{4}\|b32{8}\|b32{13}`       |

`SPEC4003` reads a claim id as its head, then a whole match of `ids.claim.id`,
case-sensitive. The head is `plain` without its `{id}`, `B-`; in a Feature that
declares a claim prefix it is `form` with the prefix in place of `{prefix}`, so
`claim_prefix: CI-AC-` reads `CI-AC-001a` and `CI-AC-m7p2qr`. A prefix whose
claim head equals its scenario head, rendered the same way from `ids.scenario`,
is reported and read as none. `ids.claim` and `ids.scenario` must not give the
same head, compared as `claim.tagIgnoreCase` compares tags.

Under schema `0.4.0` these exit `3`, each message naming where the setting goes:

- `identifiers.claim`, `identifiers.constraint`, `identifiers.openQuestion`,
  and an `identifiers` entry for any other of the seven types. `task`, `feature`
  and `epic` stay in `identifiers`.
- The top-level `mint`; its entries go in `ids.<type>.mint`. The check accepts
  that member and reads nothing from it; `specht mint` reads it
  ([Minted ids](#minted-ids)).
- An `ids` member that is not one of the seven or `$`-led, a record member
  other than the three above, `mint` or a `$`-led one, and a `plain`, `form` or
  `id` the rules above reject.

To move a manifest to `0.4.0`, run `specht schema pin 0.4.0`, then
`specht upgrade`, which moves those settings into `ids` once. An `identifiers`
entry equal to a shipped default below `0.4.0` is dropped, and the type reads
the shipped record. A customised one goes into `ids.<type>`: `plain` is the
literal head after `^` followed by `{id}`, and `id` is the rest of the pattern
before `$`, so `"claim": "^REQ-[0-9]{4}$"` becomes
`"ids": { "claim": { "plain": "REQ-{id}", "id": "[0-9]{4}" } }`. Each top-level
`mint` entry for one of the seven types goes into `ids.<type>.mint` without its
`prefix`. `upgrade` writes nothing and names the setting when it cannot move one
exactly, such as a grammar with no anchors; move that one by hand, the same way,
and run it again. A `mint` entry for any other name, such as `mint.epic`, has
no record to go into; remove it and run it again.
`specht schema pin 0.3.1` takes a manifest that still holds the old settings
back down. Without the old settings `upgrade` adds no `ids`: a manifest without
it reads the shipped records.

A manifest that maps the claim grammar to `Specht.CodeAnalysis` maps
`"/ids/claim": "/ids/claim"` in place of `identifiers.claim`, keeping
`/identifiers` for the Feature grammar; the analyzer and `specht evidence` read
the claim record's `plain` and `id` from the manifest as written, and fill
neither from the shipped record, so that manifest writes both.

## Minted ids

[`specht mint <type>`](cli.md#specht-mint) draws a new id from the type's
settings: `ids.<type>.mint` from schema `0.4.0`, the top-level `mint` entry
under `0.3.1`. A type with no settings keeps its own numbering. From `0.4.0`
the id is the record's head, its `form` with the Feature's claim prefix or else
its `plain`, then the draw, and the record's `id` must accept the draw; a
claim or scenario id that would begin with the other type's longer head is
refused. This record mints claims such as `ACTIVITY-k3x9ab` in a Feature
declaring `claim_prefix: ACTIVITY`, and `B-k3x9ab` in one declaring none:

```json
"ids": {
  "claim": { "form": "{prefix}-{id}", "mint": { "strategy": "nanoid" } }
}
```

Under `0.3.1` the type also needs a grammar in `identifiers` that accepts every
id the entry can draw. This entry mints eight-character ADR numbers from digits
and lower-case letters:

```json
"identifiers": { "adr": "^[0-9a-z]{8}$" },
"mint": {
  "adr": {
    "strategy": "nanoid",
    "nanoid": { "alphabet": "0123456789abcdefghijklmnopqrstuvwxyz", "size": 8 }
  }
}
```

| Member            | Default                                                                         | Does                                                                                                                         |
| ----------------- | ------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| `strategy`        | None                                                                            | `nanoid` or `snowflake`. An entry without one mints nothing.                                                                 |
| `nanoid.alphabet` | `0123456789abcdefghjkmnpqrstvwxyz`                                              | The characters each one is drawn from. A non-empty string.                                                                   |
| `nanoid.size`     | `8` for `adr` and `lesson`, `6` for per-Feature types                           | How many characters are drawn. A positive whole number.                                                                      |
| `snowflake`       | Epoch `2026-01-01`, `datacenterBits` `5`, `workerBits` `5`, `sequenceBits` `12` | `epoch` as `yyyy-MM-dd`, the three widths, and `datacenter`, `worker` and `sequenceStart`, each drawn at random when absent. |
| `prefix`          | `B-`, `C-`, `OQ-`, `S-` for claims, constraints, open questions, scenarios      | Below `0.4.0`, text written before the draw; empty for any other type. From `0.4.0` it exits `3`: the record gives the head. |
| `encoding`        | `base32` for Snowflake, `none` otherwise                                        | `base32` is offered to Snowflake alone.                                                                                      |
| `attempts`        | `8`                                                                             | Candidates drawn before the request is refused because the tree holds each one.                                              |

A setting no id can be drawn from, such as an empty alphabet, exits `3` and
prints no id.

## C# traceability

When a project uses `Specht.CodeAnalysis`, add `codeAnalysis` to select the
projects and Feature identities it checks. In this example the checkout
project implements claims from Feature `F1`, and repository ADRs live under
`.spec/adr/`. Replace the project path, target framework and Feature ID with
the values in your repository. If epics are enabled, include the epic ID in
each Feature selection.

```json
{
  "$comment.contract": "Declare the values mapped into the analyzer. It reads these values from the consumer manifest.",
  "schemaVersion": "0.3.0",
  "identifiers": {
    "feature": "^F[0-9]+[a-z]?$",
    "claim": "^B-[0-9]{3}[a-z]?$"
  },
  "roles": { "claims": "3. Acceptance Criteria" },
  "markers": {
    "approved": "approved",
    "withdrawn": "Withdrawn",
    "superseded": "Superseded"
  },
  "$comment.codeAnalysis": "Map the shared manifest values into the analyzer and set the C# scope.",
  "codeAnalysis": {
    "mappings": {
      "/schemaVersion": "/schemaVersion",
      "/identifiers": "/identifiers",
      "/claimsSection": "/roles/claims",
      "/markers": "/markers",
      "/policy": "/codeAnalysis"
    },
    "projects": [
      {
        "path": "src/checkout/checkout.csproj",
        "targetFrameworks": ["net10.0"]
      }
    ],
    "features": [{ "feature": "F1" }],
    "enforcement": [
      {
        "project": "src/checkout/checkout.csproj",
        "include": ["src/checkout/**/*.cs"],
        "exclude": ["src/checkout/**/*.g.cs"],
        "role": "implements",
        "declarationKinds": ["type", "constructor", "method"]
      }
    ],
    "records": {
      "adrProfiles": [
        {
          "glob": ".spec/adr/*.md",
          "owner": "repository",
          "identity": { "headingLevel": 1, "separator": ": " },
          "lifecycle": { "section": "Status", "firstWord": true }
        }
      ]
    }
  }
}
```

The `enforcement` entry requires implementation links on the selected C#
declarations. `include` is required and nonempty; `exclude` is optional, and an
absent `exclude` excludes nothing. A `verifies` entry takes only `testMethod`
in `declarationKinds`. The ADR profile tells the analyzer which files to read and
where to find each record's identity and lifecycle status. See the
[analyzer guide](../src/codeanalysis/README.md) for package setup.

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
