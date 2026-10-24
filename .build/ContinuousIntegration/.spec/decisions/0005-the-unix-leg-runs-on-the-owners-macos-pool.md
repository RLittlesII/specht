---
title: "Decision 0005: the Unix leg runs on the owner's macOS runner pool"
description: "Every job that ran on GitHub-hosted ubuntu-latest in the integration and release workflows runs on the owner's self-hosted pool, runs-on [self-hosted, macOS, ARM64], in place of the Linux leg; windows-latest and the Renovate workflow stay on hosted runners; every pool job starts from a clean checkout"
type: decision
---

# Decision 0005: the Unix leg runs on the owner's macOS runner pool

**Date:** 2026-10-23
**Decided by:** the repository owner, 2026-10-23; recorded by spec-author

## The call

- The owner's self-hosted runner pool is three runners, `vitruvius-1` to
  `vitruvius-3`, each labelled `self-hosted`, `macOS` and `ARM64`. A job
  selects the pool by all three labels, `runs-on: [self-hosted, macOS, ARM64]`,
  and never by a runner's name.
- The pool **replaces** the GitHub-hosted Linux leg; it is not added beside it.
  Every job that ran on `ubuntu-latest` in the `ci`, `main` and `release`
  workflows runs on the pool: the pull request's build, the Unix leg of a push
  to `main`, and the release workflow's `build` and `release` jobs.
- The Windows leg stays on GitHub-hosted `windows-latest`.
- The Renovate workflow stays on GitHub-hosted `ubuntu-latest`, as
  [`.github/.spec` decision 0001](../../../../.github/.spec/decisions/0001-the-renovate-workflow-starts-by-hand-too-and-runs-on-a-hosted-runner.md)
  decided. This decision does not overturn it.
- Everything the Linux leg alone did, the macOS leg now does: it runs a pull
  request's gates (B-001, B-014), annotates (B-006), uploads coverage
  (`0055-F3` B-002), runs the merge-tree check (B-017, B-022 to B-024, C-10) and
  runs the benchmarks on a push to `main` (`0109-F1` B-017).
- A decision that keyed on the Linux runner keys on the macOS runner:
  `runner.os == 'macOS'` in a step, `RUNNER_OS` equal to `macOS` in a target.

Derived, not stated by the owner, from facts the coordinator supplied on
2026-10-23:

- A pool runner keeps its working directory between jobs, and the generated
  checkout runs with `clean: 'false'`, so a previous job's untracked and ignored
  files - `.artifacts/` coverage, benchmark results, packages - would reach the
  next job. Every job on the pool therefore starts from a clean checkout, with
  untracked and ignored files removed (C-11). A GitHub-hosted job starts on a
  fresh image and needs nothing.
- The generator writes the `main` matrix as
  `os: [[self-hosted, macOS, ARM64], windows-latest]` with
  `runs-on: ${{ matrix.os }}`, and a workflow with one leg - `ci`, `release` -
  as `runs-on: [self-hosted, macOS, ARM64]`.

## Check names

B-007 keeps each check's name across runs. This decision changes one name, once:

| Workflow  | Event               | Before                   | After                                |
| --------- | ------------------- | ------------------------ | ------------------------------------ |
| `ci`      | pull request        | `build`                  | `build` (unchanged: no matrix)       |
| `main`    | push to `main`      | `build (ubuntu-latest)`  | `build (self-hosted, macOS, ARM64)`  |
| `main`    | push to `main`      | `build (windows-latest)` | `build (windows-latest)` (unchanged) |
| `release` | `v*` tag or by hand | `build`, `release`       | `build`, `release` (unchanged)       |

The `main` name is the one GitHub derives from the matrix value
`[self-hosted, macOS, ARM64]`; the first push to `main` after delivery shows it,
and item 0264 records the name observed there.

What the owner changes by hand in branch protection (`0055-F8` C-1): wherever a
required status check names `build (ubuntu-latest)`, replace it with the `main`
name observed above. A required `build` from `ci` stays as it is. The checklist
`0055-F8` B-002 describes names the checks the workflows produce, so it follows
without a claim change.

## Why

The owner, 2026-10-23: move this repository's CI off GitHub-hosted Linux onto
the new self-hosted pool. No further reason was stated.

## Rejected

**Adding the pool beside the Linux leg.** The owner said replace. Cost of
rejecting: no run proves the build on Linux any more. The path invariants of
brief § 9 are still proven on Windows, the leg whose separator differs, and
macOS is a Unix as Linux is; a Linux-only break is found by a consumer, not by
this repository.

**Moving the Renovate workflow to the pool.** Rejected by `.github/.spec`
decision 0001, which the owner did not overturn: the Renovate container needs
Docker, which the hosted image carries, and that decision uses no machine of
the owner's.

**Moving the Windows leg.** The pool has no Windows runner.

## Consequences

- § 5 #8 ("Running the build on macOS") is withdrawn: its reason - macOS billed
  at ten times the Linux rate - does not hold on the owner's own machines.
- `0055-F3`: coverage reaching Codecov is measured on macOS ARM64, where
  `0055-F3` § 7 already records the static instrumentation as working.
- `0109-F1`: the results artifact is named for `macOS` in place of `Linux`, so
  a comparison of results across this change compares different hardware.
- `0109-F4` (Tool cold start): its ADR-0001 measured a Linux observer on a
  hosted runner; CI no longer offers a Linux host. The ADR is not edited here.

## Affects

- B-001, B-003, B-006, B-007, B-014, B-017, B-022, B-023, B-024 (Amended); A-1
  (Amended); A-5, C-11, C-12 (added); C-10 (Amended); § 5 #8 (Withdrawn),
  #28 to #30 (added); § 1 and the description.
- The `@B-003`, `@B-004`, `@B-006`, `@B-010`, `@B-013`, `@B-022`, `@B-023` and
  `@B-024` scenarios, and a `@B-003 @boundary` scenario added.
- `0055-F3` A-2, B-002, § 5 #6 and the `@B-002` scenario.
- `0055-F6` C-9 (added).
- `0109-F1` B-017 and its scenarios, and the `@B-019` scenario.
- `0055-F4` and `0055-F8` § 10 entries; the `0055` epic's Feature table.
- Item 0264.

## Reversal

None.
