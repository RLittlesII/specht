---
title: "Decision 0001: upgrade rewrites tool-owned files and keeps manifest config"
description: "specht upgrade rewrites the frontmatter schemas and the templates with the next version's; in the manifest it changes only schemaVersion and the keys the next version adds, so every consumer setting survives"
type: decision
---

# Decision 0001: upgrade rewrites tool-owned files and keeps manifest config

**Date:** 2026-10-07
**Decided by:** the repository owner, answering `0001-F7` OQ-2 (2026-10-07); recorded by spec-author

## The call

`specht upgrade`, under a root pinned to `n` where the tool ships `n+1`:

- **Rewrites** the three frontmatter schemas under `.spec/schema/` and every
  file under `.spec/templates/` with version `n+1`'s. These files are the
  tool's; a hand edit to one is overwritten.
- **In the manifest**, changes only `schemaVersion` (to `n+1`) and the keys
  version `n+1` adds. Every key the consumer set - a severity override, a
  disabled rule, a renamed role, a section title, a grammar - keeps its value.
- Prints each file it rewrote.

## Why

- The manifest is the consumer's configuration (`0001-F5` B-010, B-011;
  AGENTS.md § Configuration). Replacing it with the next version's default
  discards that configuration on every upgrade, which makes upgrading a
  reason to lose work.
- The frontmatter schemas and templates are the generation contract of one
  version (README § 6). A consumer who wants a different schema selects the
  on-disk source deliberately (B-009); the copy under `.spec/schema/` is not a
  place to keep a fork across versions.
- Printing every rewritten file makes an overwritten hand edit visible in the
  output and in version control.

## Rejected

**Rewrite the manifest whole, to `n+1`'s default.** What B-005 said before this
decision. Cost of rejecting: `upgrade` has to know which manifest keys each
version adds. Taken; C-5 already puts that knowledge in the tool.

**Skip a hand-edited file and report it.** Leaves a tree half at `n` and half
at `n+1`, which C-3 rules out. Cost of rejecting: a consumer's hand edit to a
tool-owned schema is lost, recoverable from version control. Taken.

**Refuse the whole upgrade when any file was hand-edited.** Makes the on-disk
fork a lock against upgrading. Cost of rejecting: as above. Taken.

**Never overwrite any existing file** (the earlier README § 9 wording). Makes
`upgrade` a no-op on every initialised root. Rejected; README § 9 and
AGENTS.md § Configuration were amended to match this decision.

## Affects

- `0001-F7` § 3 B-005, B-016, B-017; § 11 OQ-2 (resolved), OQ-6 (raised).
- `0001-F7` § 3 B-005 (amended), B-029 to B-031; § 4 C-2 (amended); § 11
  OQ-8: narrowed 2026-10-08, see Reversal.
- README § 6 ("What a schema version is in the file") and § 9; AGENTS.md § Configuration (amended by the coordinator).

## Reversal

**Narrowed 2026-10-08 by the repository owner (OQ-8).** With the on-disk
source selected and no upstream source recorded, `upgrade` does not rewrite
the frontmatter schemas: they are the consumer's own. It prints each one it
skipped and why, and still rewrites and lists the templates. Considered and
not chosen: rewriting them anyway (overwrites the consumer's schemas), and
refusing the upgrade (the templates could not move either).
