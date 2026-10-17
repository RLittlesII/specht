---
title: "Decision 0006: Rule defaults use the working schema"
description: "Apply Init's explicit rule defaults to working schema 0.2.0 under the owner's clarification, retaining 0.1.0 and consumer overrides"
type: decision
---

# Decision 0006: Rule defaults use the working schema

**Date:** 2026-10-16
**Decided by:** the repository owner for the working-version boundary; recorded by implementer for its application to Init

## The call

The owner clarified: "we haven't shipped anything to 0.2.0 schema is still valid."
Schema `0.2.0` remains the working version under `0001-F7` decision 0014.
The earlier interpretation of the v0.1.131 package asset as a freeze of that
schema is withdrawn. The asset and its differing lock remain factual history;
they do not block this working-schema change under the owner's clarification.
This is a specific version ruling, not a general exception for prereleases.

Apply decision 0002 to the working `0.2.0` embedded manifest: list every ID of
its current vocabulary at that rule's own default level. Retain `0.1.0` bytes
and digest. Relock `0.2.0` using decision 0014's reviewed removal and lock-target
procedure; create no `0.3.0` and change no package version or tag.

The following design application resolves OQ-5: the live manifest remains
repository-specific consumer configuration. `rules` is excluded from B-004's
shipping-copy comparison by name. Its levels are not the source of Init's
defaults. The parser reads each version's embedded manifest for omitted
structural keys and reads rule overrides only from the consumer's manifest.

## Why

The existing writer copies the selected embedded manifest and never overwrites
an existing one. An explicit embedded map satisfies B-003, B-024 to B-026 and
decision 0002 without a runtime synthesis path or architecture migration.
The live repository can keep SPEC060 at warning without changing that rule's
own default in a newly initialized repository.

## Rejected

- Keep the freeze blocker after the owner's correction: retains a withdrawn
  inference and prevents the authorized working-schema delivery.
- Generate only at runtime: contradicts B-003/B-004 and decision 0002's embedded
  map without an agreed product amendment.
- Modify `0.1.0`: violates the retained version's immutable bytes and digest.
- Create `0.3.0` for this map: spends a number without the owner's authorization.
- Copy live rule overrides into the embedded map: changes a fresh consumer's
  verdict instead of exposing each rule's own default.

## Affects

- `0001-F4` B-004, B-024 to B-026, OQ-5, and sections 6-9.
- Working `src/tool/manifest/v0.2.0/manifest.json` and its lock entry.
- No rule ID, retained-version resource, package version or tag changes.

## Reversal

A later working-version boundary is recorded by its owning versioning decision;
this record does not redefine publication for any other schema version.
