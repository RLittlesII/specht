---
title: "Decision 0005: init fetch failures write nothing"
description: "The owner's decisions of 2026-10-12: init skips fetching when every selected schema already exists, writes nothing if any fetched schema fails its hash, and treats authentication and missing-file responses as upstream-source failures; upgrade remains undecided"
type: decision
---

# Decision 0005: init fetch failures write nothing

**Date:** 2026-10-12
**Decided by:** the repository owner, answering the questions about `init`; recorded by `spec-author`

## The call

- When every selected frontmatter schema already exists, `init` skips
  fetching and preserves those files (OQ-8).
- When one fetched schema fails its recorded hash, `init` writes nothing,
  including other fetched schemas that matched and requested templates
  (`0001-F7` OQ-44, for `init`).
- When the source requires authentication or does not serve the requested
  file, `init` names the source on stderr, writes nothing and exits `3`
  (`0001-F7` OQ-45, for `init`). HTTP `401` and `404` are covered explicitly.

These answers concern `init`. Neither resolves `upgrade`'s portions of
`0001-F7` OQ-44 and OQ-45.

## Why

The accepted choices preserve existing files and make an unsuccessful
initialisation leave the tree as it was. The check is responsible for
verifying existing schema bytes against their recorded hashes
(`0001-F7` B-080).

## Rejected

**Fetching and verifying when every selected schema exists.** The owner chose
to skip. Cost of rejecting: `init` does not verify the existing files; the
check does. Fetching anyway could fail a run with no schema file to write.

**Writing matching files when another fetched file fails its hash.** The
owner chose to write nothing. Cost of rejecting: no successfully fetched
file is retained after the failure; a later run fetches it again. Writing
those files would leave a partial initialisation.

**A separate outcome for authentication and missing-file responses.** The
owner chose the same result as an unreachable source. Cost of rejecting:
there is no distinct exit code for these responses.

## Affects

- B-014 requires every fetched absent schema to match its recorded hash.
- B-015 requires an absent selected schema and covers any failed hash.
- B-016 includes authentication and missing-file responses.
- OQ-8; `0001-F7` OQ-44 and OQ-45, for `init` only.
- The B-002/B-014 all-present boundary scenario, B-015 mixed-result boundary
  scenario and B-016 HTTP-refusal scenarios.

## Reversal

None.
