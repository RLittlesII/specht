---
title: "Decision 0007: Explain prints the entire rule page"
description: "The rule page printed by --explain includes its YAML frontmatter"
type: decision
---

# Decision 0007: Explain prints the entire rule page

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

`--explain` prints the entire authoritative page for the pinned schema version,
including YAML frontmatter. The owner's answer to OQ-3 was “Print the entire page.”

## Why

The printed text is the rule page decision 0003 chose. Keeping the whole page
preserves its metadata alongside the explanation and avoids a second rendering.
This is the delivery interpretation of the owner's answer, not an additional requirement.

## Rejected

OQ-3's proposed default of stripping the YAML frontmatter and starting at the
heading. It would omit part of the page the owner selected.

## Affects

`0001-F3` B-019, OQ-3, and item `0040`. C-6 still holds: text is held once in
the engine and versioned with the schema.
