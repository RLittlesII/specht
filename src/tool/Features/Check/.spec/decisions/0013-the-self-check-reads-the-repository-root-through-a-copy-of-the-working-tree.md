---
title: "Decision 0013: The self-check reads the repository root through a copy of the working tree"
description: "B-011's repository root is the root ./build.sh Specht finds over this repository's working tree, read through a file-for-file copy of that working tree, because a child NUKE run cannot start beside a parent build over the same root; B-011, its scenario and item 0031's second criterion stand as written. Narrows decision 0012's last rejected alternative"
type: decision
---

# Decision 0013: The self-check reads the repository root through a copy of the working tree

**Date:** 2026-10-14
**Decided by:** the repository owner, 2026-10-14, choosing from a
multiple-choice question put on the `spec-reviewer`'s round-14 finding (§ 12);
recorded by spec-author.

## The call

1. **B-011's repository root is the root `./build.sh Specht` finds over this
   repository's working tree.** The self-check reads it through a file-for-file
   copy of that working tree: every file
   `git ls-files --cached --others --exclude-standard` lists, as it stands on
   disk, copied into a temporary root, where the copy's own `build.sh Specht`
   runs with no `--root` and finds its root by `.nuke/`.
2. **B-011's text, the `@B-011` scenario and item `0031`'s second acceptance
   criterion stand as written.** Each is read through this decision; none is
   amended.
3. **Decision 0012's last rejected alternative is narrowed.** Its reason, "the
   root it would check is not the repository root B-011 names", still rejects an
   export of a commit or of the index. It does not reach a copy of the working
   tree as it stands, which reads as that root for B-011.

## Why

- **A child NUKE run cannot start beside a parent build over the same root.**
  NUKE fixes its log at `<root>/.nuke/temp/build.log` once the root is fixed;
  the parent build holds that file open, and a child over the same root stops at
  it before any target runs. `./build.sh AcceptanceTest`, the build the
  acceptance tier runs under, is such a parent, so the self-check failed over
  the repository root itself (§ 8, "Seen red under the parent build").
- **A copy of the working tree is the tree B-011 reads.** It holds the same
  specifications, manifest and build as the checkout, uncommitted edits and new
  files included, and its `build.sh Specht` finds its root as
  `./build.sh Specht` does and runs the command A-2 quotes against it (§ 12,
  round 14).
- **Decision 0012 rejected a root that is not the checkout as it stands.** An
  export of a commit leaves out what is not committed, and an export of the
  index what is not staged. A file-for-file copy of the working tree leaves out
  neither, so that reason does not apply to it.
- **C-7 holds.** The self-check is still the one test that reads this
  repository's real tree, through the copy.

## Rejected

- **The repository root itself, as the self-check's root.** Fails under the
  parent build, as above. Cost of rejecting: the self-check's root is a
  temporary directory, not the checkout's path; a file the working tree ignores
  is not in it; and the copy's `.build` and tool are built cold in it.
- **Amending B-011, its scenario and item `0031`'s second criterion to name a
  copy**, the alternative round 14 named. Cost of rejecting: B-011's
  "repository root" is read through this decision, not through its own text.
- **An export of a commit or of the index**, as the pre-commit hook makes.
  Rejected in decision 0012, and still rejected: it is not the checkout as it
  stands. Cost of rejecting: none.
- **A wiring change that lets a child NUKE run share the parent's root**: a log
  directory apart from the root, or a host for the child that writes no log.
  The owner made no wiring change; § 7 records why neither is available without
  one. Cost of rejecting: the cost of the copy, above.

## Affects

- B-011: text unchanged; its repository root read through this decision.
- The `@B-011` scenario: unchanged.
- C-7 and A-2: unchanged.
- Decision 0012: its last rejected alternative narrowed; its Reversal records
  this, and nothing above it is edited.
- § 7, "The B-011 run": cites this decision for the copy.
- § 10. Item `0031`: its second acceptance criterion unchanged; its
  `decisions` records this decision.

## Reversal

None.
