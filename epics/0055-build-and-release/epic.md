---
id: "0055"
type: epic
status: needs-decomposition
priority: high
milestone: null
children:
  [
    "0055-F1",
    "0055-F2",
    "0055-F3",
    "0055-F4",
    "0055-F5",
    "0055-F6",
    "0055-F7",
    "0055-F8",
  ]
created: "2026-10-08"
updated: "2026-10-08"
github_issue: null
---

# Epic 0055: Build, versioning, release and consumption

## Summary

The infrastructure that turns this repository into one that ships: the NUKE
build and its entry scripts, the local tool manifest and the pre-commit hook,
continuous integration on three operating systems, the coverage gate,
dependency updates, the package version, the release that publishes
`specht.tool` to GitHub Packages, what a consumer needs to install it, and the
GitHub repository settings that make the checks binding.

The need is the owner's (2026-10-08): everything a pull request needs must be
in place **before** the repository is first pushed to GitHub, so that the
first push leaves no infrastructure work behind. README § 8 steps 1 and 3 name
this work and no specification owned it; `0001-F2` § 5 row 7 hands the release
workflow to "repository tooling", which this epic is. Approved claims already
assume it: `0001-F2` B-010, B-011 and B-014 (the `SpecCheck` target, the
pre-commit call site, the tool reached through the local tool manifest), and
`0001-F7` decision 0002 (schema version 1 freezes at the first publish).

## Business Value

Without this epic, the first pull request on GitHub is also the first time
anything checks it: no workflow runs, no coverage is measured, no check is
required before a merge, and the first release is improvised on the day it
matters most, because that release freezes schema version 1 for every
consumer. With it delivered, a pull request is built, tested, formatted,
spec-checked and coverage-gated on every operating system the tool runs on,
dependencies arrive as reviewed pull requests, and a release is one deliberate
tag.

## Features

Decomposed by capability dimension, not by file. Each Feature has its own
invariant, and the cut is made where the invariant changes.

| Feature   | Name                          | Specification                                  | Invariant                                                                    |
| --------- | ----------------------------- | ---------------------------------------------- | ---------------------------------------------------------------------------- |
| `0055-F1` | The build                     | `.build/.spec/README.md`                       | Every gate is a build target; the hook and CI call targets, never `dotnet`   |
| `0055-F2` | Continuous integration        | `.build/ContinuousIntegration/.spec/README.md` | Every pull request and every push to `main` runs the build on three systems  |
| `0055-F3` | The coverage gate             | `test/.spec/README.md`                         | New code under the patch target fails the pull request; the total never does |
| `0055-F4` | Dependency updates            | `.github/.spec/README.md`                      | An update is a pull request; only a green minor or patch merges itself       |
| `0055-F5` | Package versioning            | `.build/Versioning/.spec/README.md`            | The package version is a function of the commit, never of a hand edit        |
| `0055-F6` | Release                       | `.build/Release/.spec/README.md`               | Only a `v*` tag publishes, and only after the full build passes              |
| `0055-F7` | Consuming the package         | `.config/.spec/README.md`                      | A repository installs the tool from the feed through its local tool manifest |
| `0055-F8` | Repository settings checklist | `.github/settings/.spec/README.md`             | The settings that make a check binding are written down and applied by hand  |

### Where "&" was checked

The owner's own phrasing joined four verbs - building, versioning, releasing,
consuming - and the starting hypothesis joined three pairs. Each pair was
checked against the slicing rule (a Feature delivers on its own, fills its own
claims, constraints and out-of-scope list, and survives a sibling being cut):

- **CI and coverage: split.** CI is valuable with no coverage service at all;
  the coverage gate has its own target, its own service configuration and its
  own failure (a pull request that passes every test and still fails). It
  depends on CI; it does not restate it.
- **Versioning and release: split.** "The version is a function of the commit"
  holds on every local `Pack`, long before anything is released; "only a tag
  publishes" holds whatever the version scheme is. Release depends on
  versioning.
- **Consumption and repository settings: split.** One is what a consumer
  repository needs from this one; the other is this repository's own GitHub
  configuration. They share no claim.
- **The build and the pre-commit hook: kept together.** The hook is a call
  site of two build targets with no behaviour of its own beyond choosing
  which; cut from the build it has nothing to call.

Eight Features is a hypothesis for the owner to confirm, not a settled split.

## Placement

A Feature's specification sits beside what it specifies. Two rules decide the
homes that have no obvious code folder:

- **A generated file is specified beside its generator.** `.github/workflows/`
  holds NUKE's output, never its source (`0055-F2` C-1), so CI and release are
  specified under `.build/`, where `Build.GitHubActions.cs` will declare them.
- **Hand-written GitHub configuration is specified under `.github/`.**
  `0055-F4` sits beside `.github/renovate.json`; `0055-F8` beside the checklist
  it specifies.

`0055-F3` sits under `test/`, whose projects produce the coverage it gates;
`0055-F5` under `.build/`, which reads the computed version; `0055-F7` under
`.config/`, beside the local tool manifest. Any of them may move with its code
(`git mv`): a specification's identity is its frontmatter `epic` and `id`
(`SPEC012`).

## Before the first push, and after

| When                       | What exists                                                                                                                                                                                                                                                                                    |
| -------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Before the first push      | `0055-F1` the build, tool manifest and hook; `0055-F2` the CI workflow; `0055-F3` the coverage configuration; `0055-F4` the Renovate configuration; `0055-F5` the version file; `0055-F6` the release workflow; `0055-F7` the feed configuration and install document; `0055-F8` the checklist |
| Once, after the first push | The checklist applied by hand (`0055-F8`), and the Renovate and Codecov installations it names                                                                                                                                                                                                 |
| The first publish          | The first `v*` tag (`0055-F6`). A later, deliberate act: schema version 1 freezes at it (`0001-F7` decision 0002), so it waits for `0001-F5` and `0001-F6` to land and for `0001-F7` OQ-4 to be answered                                                                                       |
| After the first publish    | This repository's own manifest names the published package (`0055-F7`; `0001-F2` B-014)                                                                                                                                                                                                        |

Nothing in the first two rows publishes a package.

## Out of this epic

| Item                                                                    | Where it lives instead                                                                 |
| ----------------------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| `hooked` adopting the tool (README § 8 step 4)                          | `hooked`, tracked in that repository (owner, 2026-10-08)                               |
| Transporter installing the tool (README § 8 step 7)                     | Transporter, tracked in that repository (owner, 2026-10-08)                            |
| What the `SpecCheck` target's run does, and its three-call-site verdict | `0001-F2` B-010, B-011, B-014                                                          |
| Schema versions and `schemaVersion`                                     | `0001-F7`; the package version is independent of it (`0055-F5` B-006)                  |
| Publishing to NuGet.org                                                 | README § 2 constraint (Soft): NuGet.org is the reversal of GitHub Packages, not a plan |
| The documentation site's publish pipeline                               | `0002-F2`                                                                              |
| GitHub issues, labels and milestones as a tracker                       | Not used: `github_mode: false` (AGENTS.md § SDLC); `0055-F8` § 5                       |
