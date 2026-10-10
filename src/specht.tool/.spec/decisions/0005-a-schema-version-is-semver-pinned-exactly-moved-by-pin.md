---
title: "Decision 0005: a schema version is semver, pinned exactly, moved by pin"
description: "A schema version is major.minor.patch, numbered by whether a tree's verdict can change and independently of the tool's package version; the first is 0.1.0 and an absent key means it; a pin is exact unless a pin policy floats it; a pin command moves the pin up or down and upgrade syncs the files to the pin"
type: decision
---

# Decision 0005: a schema version is semver, pinned exactly, moved by pin

**Date:** 2026-10-09
**Decided by:** the repository owner, in a requirements session (2026-10-09), confirmed accurate; recorded by spec-author

## The call

- A schema version is `major.minor.patch`. It is numbered on its own, not from
  the `specht.tool` package version, and one tool ships many schema versions.
- The first schema version is `0.1.0`. A manifest with no `schemaVersion` key
  means `0.1.0`.
- The bump follows the verdict. Patch: no tree's verdict changes. Minor: the
  version accepts more and no tree that passed fails. Major: a tree that passed
  can fail, and that includes any new `SPEC###`.
- A pin is exact by default: a manifest pinning `0.1.0` is checked with exactly
  `0.1.0`. A pin policy may let it float to the newest patch (`latest-patch`)
  or the newest minor (`latest-minor`); that is a Could.
- A command lists the schema versions the tool ships.
- A command pins any listed version, up or down, and changes only
  `schemaVersion` in the manifest. With no argument it may offer an arrow-key
  selection list; that is a Could.
- `upgrade` rewrites the frontmatter schemas and the templates to match the
  pinned version, not the next one. When they already match, it says the root
  is current and writes nothing.

Dropped: `upgrade` moving one version at a time (C-3, B-008); the boundary
against moving down (§ 5 row 2); the integer version `n`; the folder name
`schema/v<n>/` and the `v<n>` in C-6's `$id`.

## Why

- An integer cannot tell a breaking change from a non-breaking one. Every
  schema change then forces each consumer into a deliberate upgrade, so the
  owner cannot try a schema change on this repository's own tree without
  making it everyone's.
- Numbering by verdict tells a consumer, from the number alone, whether moving
  the pin can fail its build.
- Keeping the version off the package version lets a tool release ship no
  schema change, and lets one tool check repositories pinned to different
  versions.
- An exact pin keeps "the same tree gives the same report" (brief § 9) true
  across tool releases unless a consumer opts out.
- Separating the pin from the file sync makes each command do one thing, and
  makes moving down the same act as moving up.

## Rejected

**Keep the integer `n`.** What the Feature said before this decision. Cost of
rejecting: item `0042`'s integer pin is converted, and every claim worded in
`n` and `n+1` is amended.

**Float by default.** A tool release could change a verdict without a commit
in the consumer. Cost of rejecting: a consumer who wants patches opts in with a
pin policy.

**The schema version is the tool's package version.** Every tool release would
be a schema version, and a tool fix would force a pin move. Cost of
rejecting: two version numbers to read; C-12 keeps them apart.

**An absent key is invalid.** `hooked`'s manifest has no key and must keep
checking unchanged (A-1, Should-7). Cost of rejecting: a manifest that never
pins reads as `0.1.0` for good.

**An absent key is the newest version.** A tool release would change an
unpinned repository's verdict. Cost of rejecting: as above.

## Affects

- `0001-F7` § 1; § 2 need 2 and need 7, A-1 to A-3; § 3 B-001, B-002, B-004,
  B-005, B-006, B-012 to B-014, B-016, B-017, B-021 to B-023, B-029, B-031
  (amended), B-008 (withdrawn), B-037 to B-056 (added); § 4 C-1, C-6
  (amended), C-3 (retired), C-11 to C-13 (added); § 5 row 2 (withdrawn), row 3
  (amended), rows 7 to 12 (added); § 11 OQ-9 to OQ-20 (raised), OQ-2 and OQ-6
  (resolutions amended).
- Decision 0001 narrowed and decision 0002 amended; see their Reversal
  sections.
- `0001-F4` A-2, B-005 and § 5 row 6; brief § 2, § 5, § 6 and § 9; AGENTS.md
  § Configuration, § CLI and § Invariants; README.md.

## Reversal

None.
