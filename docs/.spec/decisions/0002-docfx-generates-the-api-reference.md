---
title: "Decision 0002: DocFX generates the API reference"
description: "DocFX generates the API reference from the XML doc comments and is also the framework 0002-F2 builds the documentation site with; this Feature's target still publishes nothing"
type: decision
---

# Decision 0002: DocFX generates the API reference

**Date:** 2026-11-06
**Decided by:** the repository owner; what is marked spec-author's below was not put to the owner

## The call

The repository owner ruled, 2026-11-06, that DocFX is the documentation
framework for epic `0002`:

- It generates the API reference from the XML doc comments. This resolves
  OQ-1's generator half.
- It builds the documentation site from the existing `docs/*.md` pages. That
  half resolves `0002-F2` OQ-2 and is that Feature's; its own decision 0001
  records the rest of the owner's rulings of the same date.

Spec-author's, on that ruling:

- **B-004 is amended, not withdrawn.** Its boundary still holds: this
  Feature's documentation target publishes nothing. What it no longer says is
  that no site framework exists in the repository, because DocFX is now this
  Feature's generator and `0002-F2`'s site framework at once.
- **C-2 keeps the site build out of this Feature's target.** The site and its
  publish step are `0002-F2` B-001 and B-002.
- **The output format OQ-1 also asked about stays `implementer`'s**, recorded
  in the ADR that delivers B-002, as OQ-1 already said. The owner named the
  generator, not the format.

## Why

The rationale offered to the owner, and accepted:

- One .NET tool generates the reference and builds the site.
- It runs as a NUKE target (C-1).
- The documentation needs no Node toolchain.

## Rejected

- **Astro Starlight.** It needs Node, and a separate generator to turn the XML
  doc comments into Markdown before it can show the API reference: two tools
  where DocFX is one.

## Affects

OQ-1 (resolved), B-002, B-004 (amended), C-2 (amended), § 1, § 5 rows 1 and 2,
the `@B-004` scenario; `0002-F2` OQ-2, B-001 and C-2.

## Reversal

None.
