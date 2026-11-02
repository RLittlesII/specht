---
title: "Decision 0020: the pin policy is pinPolicy, and a float uses the version it selects"
description: "The owner's rulings of 2026-11-01 on the pin policy: the manifest key is pinPolicy with exact, latest-patch and latest-minor; upgrade brings the files to the version the check uses; an unshipped pin is rejected under every policy; latest-minor is rejected below 1.0.0; and schema list marks the version pinned and the version in use apart"
type: decision
---

# Decision 0020: the pin policy is pinPolicy, and a float uses the version it selects

**Date:** 2026-11-01
**Decided by:** the repository owner, through question cards, choosing to keep
the pin policy and answer its open questions now, then answering OQ-11, OQ-16,
OQ-29 and OQ-50; recorded by `spec-author`

## The call

1. **Keep the pin policy.** Item `0124` stays in milestone `0.4.0` and its
   open questions are answered now.
2. **The key (OQ-11).** The manifest key is `pinPolicy`, with the values
   `exact`, `latest-patch` and `latest-minor`. A manifest with no
   `pinPolicy` is `exact` (B-055 unchanged). Any other value is invalid
   configuration, exit `3` (B-056).
3. **What `upgrade` writes (OQ-16 (a)).** Under a policy that floats,
   `upgrade` brings the files to the version the check uses, so the files on
   disk match the verdict.
4. **An unshipped pin (OQ-16 (b)).** A pin the tool does not ship is rejected
   by B-003 under every policy, even when a shipped version is in the
   policy's range.
5. **`latest-minor` below `1.0.0` (OQ-29, reading (c)).** On a `0.x.y` pin,
   `latest-minor` is invalid configuration, exit `3`, and the message points
   to `latest-patch`.
6. **The list's marks (OQ-50).** `specht schema list` marks the version
   pinned ` (pinned)` and the version in use ` (in use)`. When they are the
   same version it marks ` (pinned)` only, so the list under `exact` is
   unchanged (B-084).

## Why

Stated with the rulings:

- The files on disk match the verdict (call 3).
- Below `1.0.0` a `0.minor` step may fail a tree that passed (C-11), so a
  `latest-minor` float there lets a tool release change a verdict, the reason
  § 5 row 8 gives for ruling out a wider float; rejecting it keeps that reason
  intact (call 5).
- One mark when the two versions agree keeps B-084's output under `exact`
  (call 6).

The author's reading, not stated by the owner: an explicit `exact` lets a
manifest state the default rather than imply it, and rejecting an unshipped
pin keeps B-003 one rule, with no range to resolve.

## Rejected

The readings each open question recorded. The other options the cards
offered are not in the record this decision was written from, and are not
reconstructed here. Each cost is the author's assessment.

**No `exact` value; only an absent key is exact (OQ-11).** Cost of rejecting:
the tool accepts two spellings of one setting.

**`upgrade` brings the files to the pin (OQ-16 (a)).** Cost of rejecting: the
files on disk name no version the manifest states; a reader learns it from the
check's report or the list's ` (in use)` mark.

**An unshipped pin resolves within its range (OQ-16 (b)).** Cost of
rejecting: a consumer pinning a patch the tool does not ship yet gets exit `3`
until it pins a shipped one.

**`latest-minor` floats below `1.0.0` (OQ-29 (a)), or floats within the
pinned minor there (OQ-29 (b)).** Cost of rejecting: a `0.x.y` consumer who
wants new minors moves the pin by hand; (b) would have been `latest-patch`
under a second name.

**Mark only the version pinned, or only the version in use (OQ-50).** Cost of
rejecting: the list carries a second suffix to read.

## Affects

- B-053, B-054, B-056 and B-084 amended.
- B-145 to B-150 added, with their scenarios and `Missing` § 9 rows.
- C-18 added.
- B-003's source cites OQ-16 (b).
- OQ-11, OQ-16, OQ-29 and OQ-50 resolved; OQ-70 and OQ-71 raised.
- Item `0124`.

## Reversal

None.
