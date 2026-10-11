---
title: "Decision 0004: the first release waits for a schema sourced at a URL"
description: "The owner ruled that the first v* tag is not pushed until a consumer can initialise a repository to a schema sourced at a URL and have the check validate with it, and that a setup guide is needed; which claims make up that ability, and whether the guide also holds the tag, are proposed and not ratified"
type: decision
---

# Decision 0004: the first release waits for a schema sourced at a URL

**Date:** 2026-10-11
**Decided by:** the repository owner, 2026-10-11; recorded by spec-author

## The call

On learning that `specht` cannot `init` a repository to a schema sourced at a
URL, the owner said:

> "Okay, this is a HUGE gap. We can't release without this. We need to solve
> this asap."

> "We also need a "setup guide" proper. covering. initialization, manifest,
> setting up your own schema using github."

> "Just update the specs and items (if needed), with a pr. we will implement
> these in separate sessions."

Two things are decided.

1. **The first `v*` tag waits.** It is not pushed until a consumer can
   initialise a repository to a schema sourced at a URL and have the check
   validate with it (C-8). The tag is item `0084`'s.
2. **A setup guide is required.** It covers initialisation, the manifest, and
   setting up your own schema using GitHub.

The third quotation orders this change: specifications and items only, with
the building left to later sessions.

Nothing else is decided. The source's shape is
[`0001-F7` decision 0008](../../../../src/tool/.spec/decisions/0008-an-upstream-schema-source-is-one-manifest-key-with-a-base-address-a-label-and-a-hash-per-file.md)
and
[`0001-F4` decision 0003](../../../../src/tool/Features/Init/.spec/decisions/0003-init-records-an-upstream-schema-source-named-on-the-command-line.md),
both proposed and not ratified.

## Read by the author from the ruling, not stated by the owner

Each point below is a proposal. None binds until the owner confirms it.

**What "can initialise to a schema at a URL and the check validates with it"
is made of (OQ-17).** Proposed, the smallest set:

| Part                                                 | Claims                                     |
| ---------------------------------------------------- | ------------------------------------------ |
| The manifest records the source                      | None yet; `0001-F7` OQ-1 and decision 0008 |
| A root with no manifest comes to record one          | None yet; `0001-F4` OQ-6 and decision 0003 |
| `init` fetches, verifies and writes the schema files | `0001-F4` B-014, B-015, B-016, B-027       |
| The check validates with the local copy, and offline | `0001-F7` B-024, B-025, B-028, B-032       |

Proposed as not holding the tag:

| Part                                                     | Claims                                                        |
| -------------------------------------------------------- | ------------------------------------------------------------- |
| `upgrade`, and its fetch of a recorded source            | `0001-F7` B-005 to B-007, B-026, B-027, B-033 to B-035, B-067 |
| Selecting the on-disk source explicitly                  | `0001-F7` B-009, B-010                                        |
| Rejecting a recorded source beside an embedded selection | `0001-F7` B-036; `0001-F4` B-017; `0001-F3` B-030             |
| Naming the recorded source and version in the report     | `0001-F7` B-011; `0001-F3` B-005, OQ-6                        |

What that split costs:

- Without `upgrade`, a consumer who edits the recorded version or hash
  refreshes the local copy only by deleting the local schema files and running
  `init` again.
- A manifest key that lands after the first tag is not part of the published
  `0.1.0`. The key that selects a source explicitly would be a later `0.minor`
  (`0001-F7` C-11, OQ-40).
- The first published report says the schemas came from an upstream copy and
  does not say which one.

**Whether the setup guide also holds the first tag (OQ-18).** Proposed: no.
The guide is a page in this repository and is not in the package, so it can
land after the tag without changing what was published. It is on the product
goal: the guide's last item is named in `.issue/.goal`. That edit is the
author's reading of "we also need", and one line reverses it.

## Why

Stated by the owner: the quotations above.

The author's reasoning for the split, not the owner's:

- The owner named `init` and a schema at a URL. `upgrade` is a second command.
- Each claim in the smallest set is needed under any reading of the ruling.
  Depending on them cannot be wrong; stopping at them can be.

## Rejected

Nothing was put to the owner as an alternative, so nothing was rejected by the
owner. Considered by the author and not proposed:

**Holding the tag for every upstream claim, `upgrade` included.** Cost of not
proposing it: the three costs listed above. Cost of proposing it: the first
release waits for the whole `upgrade` command, which the ruling does not name.

**Holding the tag for the guide.** Cost of not proposing it: a release whose
setup is undocumented for as long as the guide's last part takes.

## Affects

- § 4 C-8 (added); § 5 row 11 (added); § 11 OQ-17 and OQ-18 (raised).
- C-2 stands: the tag still does not wait for `0001-F5` and `0001-F6`.
- [`0001-F7` decision 0007](../../../../src/tool/.spec/decisions/0007-the-freeze-starts-at-schema-version-1-0-0.md),
  ruling 3, "publish now!": redirected, see its Reversal.
- Item `0084`: waits on items `0092`, `0095` and `0178`.
- `.issue/.goal`: item `0180` added.
- Epic `0055`'s epic file, on when the first publish happens.

## Reversal

None.
