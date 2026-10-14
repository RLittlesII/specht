---
title: "Decision 0011: Explicit schema selection is versioned and command line first"
description: "The owner ratified schemaSource and --schema-source, CLI precedence, a schema 0.2.0 manifest boundary, retained upstream verification and relative disk-schema failures on October 13 2026"
type: decision
---

# Decision 0011: Explicit schema selection is versioned and command line first

**Date:** 2026-10-13  
**Decided by:** the repository owner, answering the architecture pass questions.

## Status

Ratified. This records the public contract and its architecture; implementation,
automated tests and the schema `0.2.0` snapshot remain downstream work.

## Decision

1. `schemaSource` is the optional manifest key and
   `--schema-source <embedded|disk>` the optional check argument. An explicit
   CLI value overrides a valid manifest's selection. Validate the manifest
   before applying that override; it cannot repair a rejected manifest.
2. The manifest key first applies under schema `0.2.0`. Older pins keep
   rejecting it. The CLI option works with every shipped pin and moves none.
3. A recorded `spechtSchema` keeps its verified local copy authoritative.
   Explicit `disk` keeps hash verification and reports `upstream`. Explicit
   `embedded` contradicts the record and exits `3`, with no stdout (B-036).
4. Without a recorded source, explicit `disk` reads the named frontmatter
   schema files under `.specht/schema/frontmatter/` and reports `disk`.
   Missing required files and files that cannot compile as JSON Schema exit
   `3`, with a root-relative file diagnostic and no stdout. This answers
   Manifest OQ-2(b) for the selected disk path.

An absent selection preserves the existing automatic behavior: verified
upstream when recorded, embedded otherwise. Only frontmatter schemas switch;
the consumer manifest, pin, defaults, vocabulary and templates do not.
Feature and task schemas are required, and the epic schema only with a declared
epic grammar (`0001-F5` B-008, C-10). This is schema loading, not the broader
epic opt-in identity and sequence slice.

## Why

The names match the report's vocabulary and the earlier proposal in decision 0008. CLI precedence gives a one-run choice without a manifest write. The
version boundary preserves older manifest interpretations. Keeping upstream
verification prevents a source-selection flag from weakening a recorded
content guarantee. Controlled disk failures preserve the tool's exit contract
and root-relative diagnostics before rules run.

## Rejected

- Rejecting different valid manifest and CLI selections: the owner chose CLI
  precedence. Cost: the invocation must be retained to reproduce that run.
- Adding the manifest key to `0.1.x`: the owner chose `0.2.0`, consistent with
  the earlier later-minor ruling. Cost: persisting the choice needs a new pin.
- Requiring a `0.2.0` pin for the CLI option: rejected by the owner. Cost:
  command-line source selection and manifest interpretation have distinct
  compatibility boundaries.
- Treating explicit `disk` as an unverified read beside `spechtSchema`, or
  allowing `embedded` to ignore it: rejected by the owner. Cost: changing a
  recorded schema by hand requires changing/removing its source record too.
- Leaving a missing or invalid selected disk schema as an unhandled exception:
  rejected by the owner. Cost: the loader must translate failures at the point
  where their root-relative file identity is known.

## Affects

`0001-F7` B-009 to B-011, B-036, B-101 to B-107; OQ-1 and OQ-40;
§ 6 to § 9; the `0043` architecture handoff and `0177` contradiction guard.
Manifest OQ-2(b)'s owning row awaits its current specification writer; the
owner's answer is authoritative in this record. Report OQ-6's upstream
address/version fields and `upgrade` behavior are not decided here.

## Reversal

None.
