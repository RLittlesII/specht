---
title: "Decision 0003: run the Windows gates on a push to main only"
description: "On a pull request the windows-latest leg runs none of the build's targets and its check still reports and passes; a push to main runs every gate on both systems; B-015 and B-016, C-5 and § 5 #19 and #20 record it"
type: decision
---

# Decision 0003: run the Windows gates on a push to main only

**Date:** 2026-10-09
**Decided by:** the repository owner, 2026-10-09; recorded by spec-author

## The call

- On a pull request the `windows-latest` leg runs none of the build's
  targets (B-015), and its check still reports under its unchanged name and
  passes (B-016, B-007).
- On a push to `main` the `windows-latest` leg runs every gate, as before
  (B-002, B-004).
- The `ubuntu-latest` leg is unchanged on both events.
- The decision is made inside the leg, as B-012's is (C-5).

## Why

The owner, 2026-10-09: build minutes are not free, so a pull request stops
paying for the Windows gates.

- The last completed run before the decision was on pull request branch
  `0017/schema-file-names`, commit `e23bdfd`. Its `windows-latest` leg took
  about 219 s: `Restore` 65, `Format` 50, `IntegrationTest` 22, `Compile` 20,
  `AcceptanceTest` 19, `UnitTest` 12, `SpecCheck` 10, the rest about 21.
  GitHub bills a job per whole minute, so that is 4 billed minutes, at a
  higher per-minute rate than Linux on a private repository. The
  `ubuntu-latest` leg took about 113 s.
- The steps a skipped leg still runs - setting up the job, the checkout, the
  fetch and the decision step - take about 12 s, so the leg bills 1 minute in
  place of 4.
- The leg moves and is not removed. It is the only leg where the path
  separator is not `/`, so the only one where brief § 9's separator
  invariant is exercised, and it found the CRLF checkout defect on item
  0064's first run (§ 10).

## Rejected

**Running the Windows gates on every pull request, as before.** Cost of
rejecting: a Windows-only break is found one merge late, on `main`, after the
pull request has merged. That includes a dependency update merged without a
person (`0055-F4` B-005), whose required checks pass without the Windows
gates having run. The owner accepted this risk.

**Deferring one test tier to `main` and keeping the other Windows gates on a
pull request.** Cost of rejecting: none measured. The largest tier,
`IntegrationTest`, is 22 s of 219 s, so removing it changes no billed minute;
`Restore`, `Format` and `Compile` are 135 s, and the cost is fixed overhead.

**Leaving windows out of a pull request's run.** Cost of rejecting: the
skipped leg still starts a runner and bills 1 minute. `build (windows-latest)`
is a required check (`0055-F8` B-002), and a required check that never
reports blocks every merge, the failure C-5 exists to prevent. It would also
need the operating systems computed per event, against the literal check
names B-007 and B-010 rest on, and a settings change applied by hand
(`0055-F8` C-1).

## Affects

- B-015, B-016 (added).
- B-001, B-004, B-014 (Amended: a pull request builds on ubuntu); B-003
  (Source); A-1.
- C-5 (extended to B-015, and to rule out a run without windows).
- § 5 #19, #20 (added); § 1, § 2 #2 and #8, and the description.
- The `@B-004` and `@B-005` scenarios, the `@B-015` and `@B-016` scenarios
  and their `@boundary` scenarios.
- `0055-F3`: the `@B-002 @boundary` scenario for the windows leg's coverage.
- Item 0118.

## Reversal

None.
