---
title: "Decision 0021: pinPolicy enters schema 0.3.0, and upgrade names the version in use"
description: "The owner's rulings of 2026-11-02 on the pin policy: schema 0.3.0, unpublished, is the first version that defines pinPolicy, added in place; upgrade's version line names the version the check uses; every other command, `specht frontmatter` among them, keeps the pinned version, as built"
type: decision
---

# Decision 0021: pinPolicy enters schema 0.3.0, and upgrade names the version in use

**Date:** 2026-11-02
**Decided by:** the repository owner, through question cards on item `0124`,
choosing the recommended option on each: "0.3.0, in place", "Version in use"
and "Stay on pin, record OQ-73"; recorded by `spec-author`

## The call

1. **The first version that defines `pinPolicy` (OQ-70, reading (a)).** Schema
   `0.3.0` defines the key, added in place: no published package carries
   `0.3.0`, the ground on which item `0169` renumbered its rule ids in place
   (ADR-0029). The gate is the engine's, as `schemaSource`'s is at `0.2.0`
   (B-102), and no shipped default manifest gains the key, so no shipped file
   and no lock entry changes. A manifest pinned below `0.3.0` that declares
   `pinPolicy` is rejected as naming a key that version does not know (B-153).
2. **`upgrade`'s version line (B-017).** Under a `pinPolicy` that has the check
   use a version above the pin, the line names the version in use, the version
   the files are brought to (C-18).
3. **Every other command (OQ-73).** Under a floating `pinPolicy`,
   `specht rules`, `specht --explain`, `specht rules set`, `specht manifest`,
   `specht init` beside an existing manifest and `specht schema pin`'s
   validation keep the pinned version, as built. No claim states it; § 5 row
   33 rules out the version in use for them.

   Amended 2026-11-02, the same day: the owner ruled "Stay on the pin" for
   `specht frontmatter`, which joins the commands above and keeps the pinned
   version under a floating `pinPolicy`. `0001-F8` B-006, as amended the same
   day, states it;
   `FrontmatterCommandPinPolicyIntegrationTests.ARootPinnedWithLatestPatchBesideAPatchWhoseSchemaDiffers_WhenADocumentIsChecked_ShouldValidateWithThePinnedVersionsSchema`
   holds it.

## Why

Stated with the rulings:

- `0.3.0` is unpublished, so adding a key to it changes no published version's
  verdict (C-1).
- The line names the version whose files `upgrade` wrote, as C-18 has it read
  every other `upgrade` claim (call 2).

The author's reading, not stated by the owner: with the gate in the engine and
no default manifest changed, call 1 touches no file a lock entry covers; and
`upgrade` adds no `pinPolicy`, so the key stays one a consumer writes.

## Rejected

The other options the cards offered. Each cost is the author's assessment.

**A new schema `0.4.0` cut for the key, with its folders, its vocabulary arm,
its rule pages and its lock entry.** Cost of rejecting: none to a consumer,
who moves its pin to `0.3.0` to reach the key as it would to `0.4.0`; `0.3.0`'s
default manifest does not show the key.

**B-017 printing the pin.** Cost of rejecting: the line no longer names the
version the manifest states; the list's ` (in use)` mark and the check's report
name the same version.

**The other commands following the version in use.** Cost of rejecting: under
a floating pin, `specht rules` and `specht --explain` describe the pin's rules
while the check reports the version in use's.

## Affects

- B-017 amended; B-153 added, with its scenario and a `Missing` § 9 row.
- C-18 amended.
- The scenarios of B-053, B-056 and B-146 to B-150 move their example versions
  to `0.3.0` and above; their claims are unchanged.
- OQ-70 resolved; OQ-73 raised and resolved; § 5 row 33 added.
- Call 3 amended 2026-11-02: `specht frontmatter` keeps the pin; OQ-73's
  resolution and § 5 row 33 name it.
- `0001-F8` B-006 amended 2026-11-02 for call 3 as amended: the frontmatter
  command checks against the version the manifest pins, whatever its
  `pinPolicy`, from the source the check selects
  ([`0001-F8` § 10](../Features/Frontmatter/.spec/README.md#10-lessons--spec-deltas)).
- Item `0124`.

## Reversal

None.
