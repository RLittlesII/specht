---
title: "Decision 0002: annotate only the pull request's own violations"
description: "B-006 annotates only violations in a pull request's changed files, from the ubuntu-latest leg only, written by the build's SpecCheck target beside its existing output; C-6 and § 5 #13 to #16 record it"
type: decision
---

# Decision 0002: annotate only the pull request's own violations

**Date:** 2026-10-09
**Decided by:** the repository owner, 2026-10-09; recorded by spec-author

## The call

- A violation is annotated only when its file is among the pull request's
  changed files: the same
  `git diff --name-only --no-renames <base.sha>...<head.sha>` range the
  `changes` step uses (B-012). A push to `main` annotates nothing.
- Only the `ubuntu-latest` leg annotates, so no violation is annotated twice:
  the same single-leg choice the owner made for the Codecov upload (item
  0112, `0055-F3` B-002).
- The build's `SpecCheck` target writes the annotations as GitHub workflow
  commands, in addition to its existing output and never instead of it.
  Local runs, push runs and the windows leg print exactly what they print
  today. The `specht` tool never learns about GitHub.
- An annotation never fails the step; C-4 alone decides whether `SpecCheck`
  gates.

## Why

- On CI run 37972407964 (`main`, commit `534940a`), `SpecCheck` printed 251
  errors: 225 `SPEC060` and 26 `SPEC061`. GitHub Actions keeps at most 10
  error and 10 warning annotations per step, and 50 per job. Annotating every
  violation in the tree would hide a pull request's own violation behind
  pre-existing ones.
- No problem matcher annotates today. NUKE relays each `specht` line through
  Serilog with ANSI colour and an `HH:mm:ss [DBG]` prefix, and setup-dotnet's
  `csc` matcher requires a trailing `[project]` that `specht` never prints.
  The annotation has to be written deliberately, and the build is the one
  place that knows it runs on GitHub.
- Two legs annotating the same line would show every violation twice.

## Rejected

**Annotating every violation in the tree.** Cost of rejecting: a violation
already on `main` is not annotated on a pull request that does not touch its
file; it stays in the step's log. The same filter also leaves unannotated a
violation the pull request causes in a file it does not change: removing a
claim leaves an unchanged `.feature` tag dangling, and a `depends_on` edit
breaks symmetry in the other, unchanged specification. The owner chose
file-scoped annotation knowing pre-existing debt stays silent; this causal
case is part of that accepted cost, and its line stays in the step's log.

**Falling back to every violation when the changed files are none or cannot
be determined**, as B-014 falls back to building. Cost of rejecting: such a
pull request gets no annotation (C-6). This rule is derived from the owner's
chosen range by spec-author, 2026-10-09, not stated by the owner: an empty or
undeterminable set of changed files contains no violation's file.

**Annotating from both legs.** Cost of rejecting: a violation only the windows
leg reports would not be annotated. Both legs check the same tree, so none is
expected.

**A problem matcher over the existing output.** Cost of rejecting: the build
carries the annotation code instead of a matcher file; the matcher would have
to strip NUKE's prefix and colour, which are not the tool's to fix.

**Annotations beyond the per-step cap.** Accepted as lost: GitHub drops them,
and the step's log keeps the full list.

## Affects

- B-006 (Amended to a pull request that changes a file carrying a violation,
  annotated from the `ubuntu-latest` leg only).
- C-6 (added); C-4 (unchanged, still governs gating).
- § 5 #13, #14, #15, #16 (added); § 1 and the description narrowed.
- The `@B-006` scenario's Given, and the `@B-006 @boundary` scenario for a
  violation in an unchanged file.
- Item 0066.

## Reversal

None.
