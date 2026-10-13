---
title: "What specht checks"
description: "The rule groups, where a specification is found, and what runs without work items"
type: reference
---

# What it checks

Rules are `SPEC001`–`SPEC061`.
[brief § 4](../.spec/brief.md#4-the-rule-vocabulary-fixed-versioned-with-the-schema)
summarises them, each has a page under [`rules/v0.1.0/`](rules/v0.1.0/), and each
finding says what the rule expected.

A specification is a `<area>/.spec/README.md` beside the code it covers, with
its `.feature` file in the same folder. The repository-root `.spec/` holds the
schema, the templates and repo-wide records, and is never checked as a
specification. No rule reads `adr/`, `lessons/` or `decisions/`. A `.spec/`
under `.git`, `.artifacts`, `.claude`, `.skillfile`, `graphify-out`, `bin`,
`obj` or `node_modules` is skipped.

| Rules                                      | Checks                                                                                                                                                                                              |
| ------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `SPEC001`–`SPEC003`                        | Frontmatter. Every specification and item file opens with YAML frontmatter that satisfies its schema.                                                                                               |
| `SPEC010`, `SPEC013`                       | Sections. Each contracted section is present once and in order, and a table the manifest names has the headers it names.                                                                            |
| `SPEC012`                                  | Identity. No `<epic>-<id>` is claimed by two specifications.                                                                                                                                        |
| `SPEC020`, `SPEC021`                       | Companion. Exactly one `.feature` sits beside the specification, and every claim tag in it resolves to a § 3 claim.                                                                                 |
| `SPEC030`, `SPEC031`                       | Claims. Each § 3 claim id matches the claim grammar and is unique in its specification, and § 9 has exactly one row per claim and none for anything else.                                           |
| `SPEC040`, `SPEC041`, `SPEC043`, `SPEC044` | Items. Each id under `children` or `spikes` resolves to one item file, an item's `id` and `parent` agree with its file name and its specification, and item ids are unique and contiguous per epic. |
| `SPEC050`–`SPEC052`                        | Dependencies. Every `depends_on` and `blocks` edge names a discovered specification, is declared from both ends, and forms no cycle.                                                                |
| `SPEC060`, `SPEC061`                       | Approval. A specification with `spec_status: approved` has no `Missing` cell in § 9 and no draft or blocked row in § 12.                                                                            |

§ 3, § 9 and § 12 are the sections the manifest's `roles` name: by default
`3. Acceptance Criteria`, `9. Traceability Matrix` and `12. Sign-off`. A
`Missing` cell in § 9 is not a violation until the specification is approved.

## Work items are optional

`specht` never reads `.issue/`. The item rules run only on
`<epic>-<nn>-<slug>.md` files in a specification's folder, and `SPEC040` and
`SPEC041` only when a specification's frontmatter lists an id under `children`
or `spikes`. A repository with specifications, companions and a manifest, and no
work items, gets every other check.
