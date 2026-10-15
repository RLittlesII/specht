---
title: "Decision 0011: The tool targets net10.0 and net11.0"
description: "specht.tool carries the tool for net10.0, long-term support, and for net11.0, standard-term support, from one package; the long-term-support target stays, so ADR-0009's gate on moving off it is untouched"
type: decision
---

# Decision 0011: The tool targets net10.0 and net11.0

**Date:** 2026-10-14
**Decided by:** the repository owner; recorded by spec-author

## The call

`specht.tool` carries the tool for two target frameworks: `net10.0` and
`net11.0` (B-045).

- **Built for both:** the engine, `src/specht`, and the tool, `src/tool`,
  and the test projects that exercise them: `test/tests`, `test/acceptance`
  and `test/comparison.tests`.
- **Left on `net10.0`:** the build, `.build`; `test/build.tests`; and the
  projects under `.performance/`. None of them ships in the package.
- **The SDK:** `global.json` pins the .NET 11 release candidate 2 SDK,
  `11.0.100-rc.2.26504.105`, with prerelease SDKs allowed. Continuous
  integration installs that SDK and `10.0.401`, because a `net10.0` test run
  needs the .NET 10 runtime.

## Why

- **The long-term-support target stays.** .NET 10 is long-term support and
  .NET 11 standard-term; a consumer on a machine or a runner with only the
  .NET 10 runtime still runs the tool. A tool targeting `net11.0` alone
  would need the .NET 11 runtime everywhere it runs, the cost ADR-0009 names.
- **.NET 11 is reached now, not at general availability.** General
  availability is 2026-11-10. Building and testing both targets from the
  release candidate finds what breaks on .NET 11 before then.
- **A specification claim, not a chore.** What the package carries is
  observable to every consumer, so it is a claim of this Feature, beside
  B-017 to B-036, and not an exemption.

## Rejected

- **`net11.0` alone.** Leaves long-term support and needs the .NET 11
  runtime on every consumer's machine and CI. That is gate 3 of
  [ADR-0009](../../../../../../.spec/adr/0009-input-failures-stay-exceptions-until-a-native-union-can-carry-them.md),
  which stays a separate owner decision (§ 5 row 29).
- **Any other target, such as `net9.0`.** Not asked for (§ 5 row 28).
- **Every project on both targets.** The build, its tests and the
  benchmarks ship nothing in the package; building them twice costs time and
  proves nothing about it.
- **Waiting for general availability.** Defers what a release candidate can
  already show.

## Cost accepted

- **A release-candidate SDK on `main` until general availability.** Moving
  `global.json` to the released SDK may need a pull request by hand: the
  dependency bot may not propose a release candidate to release move.
- **Every multi-targeted test project runs twice,** once per target.
- **Test modules run one at a time.** The acceptance tests pack the solution
  once per run, and two packs running together collide.

## Affects

B-045. § 5 rows 28 and 29. Item `0217`. `0055-F6` § 7 and § 10, which quote
the continuous-integration step that installs the SDK. ADR-0009, gate 3:
unchanged, and neither met nor waived.

## Reversal

None.

## Addendum - 2026-10-14

**Decided by:** the repository owner; recorded by spec-author

The project scope under "The call" is superseded; the text above stays as
it was decided.

- **Ratified.** The implementer moved `.performance/benchmarks` to both
  targets, beyond the scope above: `test/tests` references it, and its
  `net10.0` runtime configuration for the tool overwrote the `net11.0` one
  in the `net11.0` test output (`0001-F2` § 7, "Two target frameworks").
  The owner ratified the move.
- **Widened.** The owner then directed: "put it in the directory props so
  _all_ the things do it". `Directory.Build.props` sets `TargetFrameworks`
  `net10.0;net11.0` for every project, and no project is selected by name.
  `test/build.tests`, `.performance/comparison` and `.performance/benchmarks`
  build for both with the engine, the tool and the other test projects.
- **The one exception: `.build` keeps `net10.0`.** Measured: with two
  targets NUKE's static constructor fails its check that the build assembly
  runs from where it was built, because NUKE turns off the target-framework
  folder in the output path and both targets write one `bin/Debug/`; with
  that folder turned back on, NUKE refuses the suffixed location. The one
  target is `net10.0`, not `net11.0`, because `test/acceptance` and
  `test/build.tests` reference `.build` and build for both. NUKE therefore
  still runs on the .NET 10 runtime, and the build still clears the MSBuild
  variables it would hand to the .NET 11 SDK.
- **The benchmarks run their `net10.0` build.** The `Benchmark` target
  builds and runs them for `net10.0`, so what a benchmark measures is
  unchanged.
- **Verified.** The full `./build.sh` passed on both targets, 2026-10-14.

Rejected: "Every project on both targets", above, no longer holds; its
reason, that building twice proves nothing about the package, is outweighed
by one rule in one file with no list of project names to keep current.

The cross-project references that let one project's targets break
another's are item `0218`.

Affects: `0001-F2` A-2, § 6, § 7 and § 10; item `0217`; `.build/.spec`
A-2 and § 7; `0109-F1` § 7; `0055-F3` § 7. B-045, § 5 rows 28 and 29 and
ADR-0009 gate 3: unchanged.
