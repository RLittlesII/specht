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

Dropped by the owner: `upgrade` moving one version at a time (C-3, B-008), and
the boundary against moving down (§ 5 row 2). Their cost is under Rejected.

Not decided, and inferred by the author to follow from the integer going: the
folder name `schema/v<n>/` and the `v<n>` in C-6's `$id` have no replacement
yet (OQ-22). Deferred by the owner: whether a published version is frozen
forever (OQ-10), behind "rethink shipping it with the tool and hosting it from
the web" (OQ-9).

## Why

Stated by the owner:

- An integer "does not allow for minor version bumps; not every change is a
  breaking change if you respect semver".
- With an integer the consumer bears the cost of every schema change, and the
  owner cannot try one on this repository's own tree first.
- Pinning is configuration, set by a command: list the versions, pin one, up
  or down; an interactive pick is "even better".
- The pin is exact; "latest-patch or latest-minor configuration would be fine".
- The schema version is independent of the tool version.

The author's inference, not stated by the owner:

- Numbering by verdict tells a consumer, from the number alone, whether moving
  the pin can fail its build.
- An exact pin keeps "the same tree gives the same report" (brief § 9) true
  across tool releases unless a consumer opts out.
- A pin command that only edits the manifest makes moving down the same act as
  moving up.

## Rejected

Each cost below is the author's assessment unless it quotes the owner.

**Keep the integer `n`.** What the Feature said before this decision; rejected
by the owner for the reason quoted under Why. Cost of rejecting: item `0042`'s
integer pin is converted, and every claim worded in `n` and `n+1` is amended.

**`upgrade` moves one version at a time (C-3, B-008).** Dropped by the owner.
Cost of dropping: a repository can jump several versions in one move and meet
every version's violations in one report, where C-3 gave them one version at a
time.

**No moving down (§ 5 row 2).** Dropped by the owner: the pin moves "up and
down". Cost of dropping: a manifest can carry a key only a higher version
defines after the pin moves down, which nothing yet answers (OQ-14).

**An absent key is invalid, exit `3`.** An option offered in the session; the
owner chose "the first version". Cost of rejecting: a manifest that never pins
reads as `0.1.0` for good. The author's reason for the choice, not the
owner's: `hooked`'s manifest has no key and must keep checking unchanged (A-1,
Should-7).

**An absent key is the newest shipped version.** An option offered in the
session; not chosen. Cost of rejecting: as above. The author's reason: a tool
release would change an unpinned repository's verdict.

**The pin command replaces `upgrade`.** An option offered in the session; the
owner chose that the pin command only edits the manifest and `upgrade` rewrites
the files to match. Cost of rejecting: moving a repository is two commands.

**Both the pin command and `upgrade` rewrite the files.** An option offered in
the session; not chosen. Cost of rejecting: after a pin and before an `upgrade`
the files are at another version than the pin.

**Another bump rule.** The owner chose the verdict rule from options offered in
the session; the options not chosen are not in the record this decision was
written from, and are not reconstructed here.

**Float by default.** The author's inference from "exact pin"; the owner was
not offered it as an option. A tool release could change a verdict without a
commit in the consumer. Cost of rejecting: a consumer who wants patches opts in
with a pin policy.

**The schema version is the tool's package version.** The author's inference
from "independent of the tool version"; the owner was not offered it as an
option. Every tool release would be a schema version. Cost of rejecting: two
version numbers to read; C-12 keeps them apart.

## Affects

- `0001-F7` § 1; § 2 need 2 and need 7, A-1 to A-3; § 3 B-001, B-002, B-004,
  B-005, B-006, B-012 to B-014, B-016, B-017, B-021 to B-023, B-029, B-031
  (amended), B-008 (withdrawn), B-037 to B-057 (added); § 4 C-1, C-6
  (amended), C-3 (retired), C-11 to C-13 (added); § 5 row 2 (withdrawn), row 3
  (amended), rows 7 to 12 (added); § 11 OQ-9 to OQ-24 (raised), OQ-2, OQ-3 and
  OQ-6 (resolutions amended), OQ-13 (resolved by the item cut).
- Decision 0001 narrowed and decision 0002 amended; see their Reversal
  sections.
- `0001-F1` A-3, C-8 and § 5 row 10: "schema version 1" reads as `0.1.0`, and
  epic `0101`'s version number is OQ-18.
- `0001-F4` A-2, B-005 and § 5 row 6; `0001-F8` § 5 rows 3 and 6 and the
  version its scenarios pin.
- brief § 2, § 5, § 6 and § 9; AGENTS.md § Configuration, § CLI and
  § Invariants; README.md.
- The `specht-conventions` skill (its settled answers and both write lists) and
  the `dotnet-tool` skill's vertical-slice reference.

## Reversal

None.
