---
title: "Decision 0018: upgrade fetch failures write nothing, and absent files stay absent"
description: "The owner's rulings of 2026-10-29 on upgrade under a recorded upstream schema source: one failed hash writes nothing, a 401 or 404 refusal fails as an unreachable source does, and upgrade rewrites only the files present"
type: decision
---

# Decision 0018: upgrade fetch failures write nothing, and absent files stay absent

**Date:** 2026-10-29
**Decided by:** the repository owner, answering OQ-44, OQ-45 and OQ-51 for `upgrade`; recorded by `spec-author`

## The call

- When any fetched frontmatter schema fails its recorded hash, `upgrade`
  writes nothing, including schemas that matched. Every fetch completes
  before the first write, as `0001-F4` decision 0005 rules for `init`
  (OQ-44, for `upgrade`).
- A source that answers and refuses, HTTP `401` or `404`, is treated as a
  source that cannot be reached (B-035): the source named on stderr,
  nothing written, exit `3` (OQ-45, for `upgrade`). This settles B-133 for
  templates as well.
- `upgrade` rewrites only the files present. An absent frontmatter schema or
  template stays absent; `init --schemas` and `init --templates` add them
  (OQ-51, reading (a)).

## Why

The same rules as `init` (`0001-F4` decision 0005): a failed run leaves the
tree as it was, and a refusal has the result an unreachable source already
has. Which parts a root holds is the consumer's choice at `init` (`0001-F4`
decision 0004); `upgrade` keeps that choice.

## Rejected

**Writing the matching files when another fetched file fails its hash.**
Cost of rejecting: a schema that matched is fetched again on the next run.
Writing it would leave a partial upgrade.

**A separate outcome for a credential request or a missing file.** Cost of
rejecting: no distinct exit code tells a refusal from an unreachable source;
stderr names the source in both.

**`upgrade` writing every file the pinned version ships (OQ-51 (b)).** Cost
of rejecting: a consumer who wants a part must run `init` for it. Writing it
would undo the choice `init` offered.

**`upgrade` taking the part options `init` takes (OQ-51 (c)).** Cost of
rejecting: adding a part and upgrading are two commands, not one run.

## Affects

- B-027 amended: at least one fetched schema failing its hash.
- B-035 amended: a source that requires authentication or does not serve the
  requested file.
- B-139 to B-141 added: an absent file is not created, with no upstream source
  recorded, for a recorded kind's schema, and for a template the source hashes.
- B-130, B-132 and B-133: their sources cite this decision in place of the
  proposed defaults.
- OQ-44 and OQ-45 resolved for `upgrade`; OQ-51 resolved.
- `0001-F4` § 5 row 16.

## Reversal

None.
