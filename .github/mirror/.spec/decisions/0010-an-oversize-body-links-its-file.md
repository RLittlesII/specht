---
title: "Decision 0010: a body over GitHub's limit links its file instead of copying it"
description: "When an issue body would exceed GitHub's 65,536-character limit, the mirror replaces the verbatim copy of the file with a link to the file at the mirrored commit, then drops the rendered sections after Summary if it is still over; the marker, provenance, Delivered by, claimant and footer lines are always kept"
type: decision
---

# Decision 0010: a body over GitHub's limit links its file instead of copying it

**Date:** 2026-10-25
**Decided by:** adopted for item `0294` on 2026-10-25 as the default for the limit `implementer` reported in § 7; recorded by spec-author. Confirmed by the repository owner on 2026-10-25.

## The call

GitHub refuses an issue body over 65,536 characters. When the body B-021 or
B-048 would produce exceeds it:

1. The `## Source` section holds one line linking the file at the commit the
   run mirrors, in place of the verbatim copy.
2. If the body still exceeds the limit, the rendered sections after
   `## Summary` are left out too.
3. The marker, the `Mirrored from` line, the `Delivered by` line, the
   `is also claimed by` lines and the read-only footer are always kept.

A body within the limit is unchanged by this decision.

## Why

`epics/0201-spec-code-analysis/epic.md` is 73,534 bytes, and its rendered body
is 101,914 characters, so B-048's verbatim copy cannot be created and the run
stops on that epic. The next largest body, epic `0132`'s, is about 51,210
characters. The lines kept are the ones other claims depend on: the marker
finds the issue (B-001 to B-003, B-049), and the rest are claimed
unconditionally (B-016, B-017 to B-019, B-022, B-023). A link at a commit
still shows the file exactly as written.

## Rejected

- **Truncating the verbatim copy.** Cost of rejecting: the issue shows none of
  the file, only a link. Taking it would show a file that is not the file, and
  a cut fence can leave the rest of the body inside a code block.
- **Splitting the copy into comments.** Cost of rejecting: as above. Taking it
  would put mirror-owned text in comments, which B-022 says are not read, and
  every edit would rewrite several comments.
- **Skipping the oversize file.** Cost of rejecting: none beyond the link
  line. Taking it would break B-002 for that epic.

## Affects

- `0055-F9` B-021 and B-048 (amended), B-052 and B-053 (new); OQ-10 (new).

## Reversal

None.
