---
title: "Decision 0001: A renamed key is renamed in the manifest and the schema"
description: "A key name or id grammar the frontmatter schemas also fix is renamed by the consumer in both files, under the on-disk source; the engine derives neither from the other"
type: decision
---

# Decision 0001: A renamed key is renamed in the manifest and the schema

**Date:** 2026-10-07
**Decided by:** spec-author, on spec-reviewer's finding 3 (2026-10-07), reading README §§ 5-6

## The call

The manifest names the frontmatter keys and id grammars the rules read. The
frontmatter schemas fix some of the same names and grammars on their own
(`required`, `additionalProperties: false`, `pattern`). A consumer who renames
one of them, such as `children` to `tasks` or the task id to a three-digit
sequence, changes it in the manifest **and** in its on-disk frontmatter schema
files, and checks with the on-disk source. The engine does not generate or patch
a schema from the manifest, and does not read a key name or a grammar out of a
schema.

## Why

README § 6 already makes the consumer's on-disk `.spec/schema/` files the way
to bring your own schema, selected by a manifest field or a CLI argument
(Should-8). The schema files are the agent's generation contract (Should-5), so
they have to say what the tree is held to; a schema rewritten at run time would
hold the tree to a contract nobody can read. Keeping the two files separate and
both in the consumer's hands is the only reading where README § 5 ("every
literal moves into the manifest") and § 6 ("on-disk schemas when selected")
both hold.

## Rejected

- **The engine derives the schema from the manifest.** That would mean a
  schema patched in memory, which the agent cannot read, and a second place
  where the embedded set is no longer what was shipped (`0001-F7` C-1).
- **The engine derives the manifest's names from the schema.** That would put
  the grammar in two places that are read, against C-2.
- Cost of the call: a rename is two edits, and a rename made in the manifest
  alone is reported by `SPEC002`/`SPEC003`. That report is the signal, not a
  defect.

## Affects

C-2, C-8, C-9, B-004, B-005, B-006, B-008; the scenarios for B-004 to B-006 and
B-008 select the on-disk source.

## Reversal

None.
