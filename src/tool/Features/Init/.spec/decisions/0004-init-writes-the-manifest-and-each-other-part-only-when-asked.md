---
title: "Decision 0004: init writes the manifest, and each other part only when asked"
description: "The owner's decision of 2026-10-11: a bare specht init writes only the manifest; --schemas adds the frontmatter schemas, --templates adds the templates and --all adds both; --epics stays as specified. Decision 0002 holds, since the manifest alone carries every default"
type: decision
---

# Decision 0004: `init` writes the manifest, and each other part only when asked

**Date:** 2026-10-11
**Decided by:** the repository owner, answering one of four questions on the need; recorded by `spec-author`

## The call

The owner's need: "the init needs flags. not everyone will want the templates.
not everyone will want all the things."

What `init` can write is three parts (A-1). The owner chose opt-in:

| Command                   | Parts in the run's selection                         |
| ------------------------- | ---------------------------------------------------- |
| `specht init`             | the manifest                                         |
| `specht init --schemas`   | the manifest and the frontmatter schemas             |
| `specht init --templates` | the manifest and the templates                       |
| `specht init --all`       | the manifest, the frontmatter schemas, the templates |

`--epics` is unchanged (`0001-F5` decision 0004): into a root with no manifest
it alone decides the epic tier, and beside a manifest the manifest's epic
grammar does (C-5, C-6).

Three points were not put to the owner as questions, and none was stated by
the owner. Each is derived by spec-author: a default spec-author proposed,
contained in a plan the owner approved on 2026-10-11.

- **`--all` is `--schemas --templates`** and nothing more (B-032). It does not
  decide the epic tier.
- **`init` fetches only when the frontmatter schemas are in the selection**
  (B-014 to B-016, B-034). Without them nothing fetched could be written.
- **The stdout listing names the files of the selection and no other** (B-006,
  B-033).

**Decision 0002 holds.** The owner's need there was that `init` provide "all
default configuration". The manifest is that configuration: it carries
`schemaVersion` (`0001-F7` B-012), a level for every rule (B-024, B-025) and
every key the default manifest declares (B-003). A check needs no other file:
it validates frontmatter with the embedded schemas of the pinned version
(`0001-F5` C-9). The frontmatter schemas and the templates are copies for a
reader, an editor or an agent, not defaults a run reads.

## Why

- The owner's reason: not every consumer wants the templates, or every file.
- A repository that only checks reads the manifest alone, so the smallest
  useful `init` writes one file.

## Rejected

**Every part by default, with an option to leave one out.** The alternative the
owner's choice excludes. Cost of rejecting: a consumer who wants what `init`
wrote before this decision types `--all`.

Not put to the owner, and left out of scope on spec-author's reading (§ 5 rows
13 to 15): an option that selects one file; a prompt.

## Affects

- § 2 needs 1 and 7, A-1; § 3 B-002, B-006, B-010 to B-012, B-014 to B-016,
  B-018 to B-020, B-023 and B-027 amended, B-029 to B-034 added; § 4 C-7
  added; § 5 rows 8 and 12 amended, rows 13 to 16 added; § 11 OQ-8 narrowed,
  OQ-9 raised.
- Decision 0002: unchanged, as above.
- `0001-F7` § 5 row 18; `0001-F7` OQ-51, on what `upgrade` does with a part
  that was not asked for.
- AGENTS.md § Configuration and § CLI.

## Reversal

None.
