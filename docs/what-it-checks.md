---
title: "What specht checks"
description: "The rule groups, where a specification is found, and what runs without work items"
type: reference
---

# What it checks

Under schema `0.3.0` a rule id is `SPEC` and four digits, the thousands digit
naming the group; under `0.1.0` to `0.2.1` it is the three-digit id in
brackets below. No version accepts the other form.
[brief § 4](../.spec/brief.md#4-the-rule-vocabulary-fixed-versioned-with-the-schema)
summarises them, each has a page under the pinned version's folder, such as
[`rules/v0.3.0/`](rules/v0.3.0/), and each finding says what the rule expected.

A specification is a `<area>/.spec/README.md` beside the code it covers, with
its `.feature` file in the same folder. The repository-root `.spec/` holds the
schema, the templates and repo-wide records, and is never checked as a
specification. No rule reads `adr/`, `lessons/` or `decisions/`. A `.spec/`
under `.git`, `.artifacts`, `.claude`, `.skillfile`, `graphify-out`, `bin`,
`obj` or `node_modules` is skipped.

| Rules                                                              | Checks                                                                                                                                                                                                   |
| ------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `SPEC1001`–`SPEC1003` (`SPEC001`–`SPEC003`)                        | Frontmatter. Every specification and item file opens with YAML frontmatter that satisfies its schema.                                                                                                    |
| `SPEC2001`, `SPEC2002` (`SPEC010`, `SPEC013`)                      | Sections. Each contracted section is present once and in order, and a table the manifest names has the headers it names.                                                                                 |
| `SPEC3002` (`SPEC012`)                                             | Identity. No `<epic>-<id>` is claimed by two specifications.                                                                                                                                             |
| `SPEC4001`, `SPEC4002` (`SPEC020`, `SPEC021`)                      | Companion. Exactly one `.feature` sits beside the specification, and every claim tag in it resolves to a § 3 claim.                                                                                      |
| `SPEC4003`, `SPEC4004` (`SPEC030`, `SPEC031`)                      | Claims. Each § 3 claim id matches the claim grammar and is unique in its specification, and § 9 has exactly one row per claim and none for anything else.                                                |
| `SPEC4009`                                                         | Tests. From schema `0.3.0`, with `tests` declared, every test name a § 9 Test cell gives in inline code is held by a declared test file, and a Test cell that is not `Missing` names one in inline code. |
| `SPEC5001`–`SPEC5004` (`SPEC040`, `SPEC041`, `SPEC043`, `SPEC044`) | Items. Each id under `children` or `spikes` resolves to one item file, an item's `id` and `parent` agree with its file name and its specification, and item ids are unique and contiguous per epic.      |
| `SPEC6001`–`SPEC6003` (`SPEC050`–`SPEC052`)                        | Dependencies. Every `depends_on` and `blocks` edge names a discovered specification, is declared from both ends, and forms no cycle.                                                                     |
| `SPEC7001`, `SPEC7002` (`SPEC060`, `SPEC061`)                      | Approval. A done specification has no `Missing` cell in § 9, and an approved one no draft or blocked row in § 12. Before schema `0.3.0`, `SPEC060` reads an approved specification, not a done one.      |

§ 3, § 9 and § 12 are the sections the manifest's `roles` name: by default
`3. Acceptance Criteria`, `9. Traceability Matrix` and `12. Sign-off`. A
`Missing` cell in § 9 is not a violation until the specification's delivery
`status` is `done`, the manifest's `markers.done`; before schema `0.3.0`, until
its `spec_status` is approved.

## Missing coverage on a done specification

`SPEC7001` (`SPEC060`) is an error by default: a done Feature says its coverage is known.
From schema `0.3.0` a `Missing` row does not block `spec_status` reaching
approved; it is reported once `status` is done. Under `0.1.0` to `0.2.1` it is
reported once the specification is approved, so a repository on those versions
whose policy is that a `Missing` row blocks a work item reaching done, and not a
specification reaching approved, lowers the rule in its manifest:

```json
"rules": { "SPEC060": "warning" }
```

That is the sanctioned way to choose otherwise. `warning` still fails a run
under `--strict`; `off` stops the rule. This repository pins `0.3.0` and
sets no level.
`specht rules set <id> <level>` sets one rule's level
([command line](cli.md#specht-rules-set)), or edit `rules` by hand
([configuration](configuration.md)).

## Work items are optional

`specht` never reads `.issue/`. The item rules run only on
`<epic>-<nn>-<slug>.md` files in a specification's folder, and `SPEC5001` and
`SPEC5002` only when a specification's frontmatter lists an id under `children`
or `spikes`. A repository with specifications, companions and a manifest, and no
work items, gets every other check.
