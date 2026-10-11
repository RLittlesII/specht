---
title: "Decision 0001: specht manifest prints the effective manifest, and --defaults prints the shipped one"
description: "specht manifest prints the manifest on disk with every omitted key filled from the pinned version's default, what the engine reads; --defaults prints the default manifest the tool ships and needs no root or manifest; --json applies to both"
type: decision
---

# Decision 0001: specht manifest prints the effective manifest, and --defaults prints the shipped one

**Date:** 2026-10-11
**Decided by:** the repository owner, answering the second of three questions on the need for this Feature; recorded by `spec-author`

## The call

The owner's need: "we also want to provide `specht manifest` and
`specht rules` so they can be printed to the console or returned as json."

1. **`specht manifest` prints the effective manifest**: the manifest on disk
   with every omitted key filled from the pinned version's default, which is
   what the engine reads.
2. **`--defaults` prints the shipped default** and needs no root and no
   manifest.
3. **`--json` applies to both.**

## Why

- The manifest on disk is already readable; what is hidden is the value of a
  key it leaves out, and that is the question a maintainer asks after a
  verdict they did not expect.
- The shipped default answers the other question, "what can I set, and what
  is it when I do not", and it has no root to depend on.

## Rejected

**Print the manifest on disk as written.** It adds nothing to opening the
file. Cost of rejecting: the command depends on the loader's defaults
(`0001-F5` B-019), so it cannot be built ahead of them.

**Print the shipped default only, and leave the effective manifest to a
reader comparing two files.** Cost of rejecting: a second form and its
failure cases (B-013 to B-015).

**Require a root for `--defaults`.** It would let the default follow the
root's pin. Cost of rejecting: which version `--defaults` prints beside an
older pin is open (OQ-3).

## Affects

- `0001-F9` § 3 B-001 to B-012; § 2 A-1 to A-3.
- Not decided here, and left open: the text form and the key order (OQ-1);
  a key whose absence is a setting (OQ-2); a version for `--defaults` (OQ-3);
  a published schema for the JSON document (OQ-4).

## Reversal

None.
