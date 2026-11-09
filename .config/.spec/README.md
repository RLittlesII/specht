---
title: "Specification: Consuming the package"
description: "What a repository needs from this one to install specht.tool through its local tool manifest, or globally for a shell - nuget.org, no source entry and no credential, the prerelease opt-in below 1.0.0 and the commands - written down once, and used by this repository itself after the first publish"
type: feature
id: "F7"
epic: "0055"
spec_status: in-review
status: ready-for-implementation
priority: high
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Build and release"
author: "spec-author"
milestone: null
children: []
depends_on: ["F6"]
blocks: []
spikes: []
created: "2026-10-08"
updated: "2026-11-05"
github_issue: null
synced_at: null
---

# Specification: Consuming the package

## 1. Business Goal

<!-- last written by: spec-author, 2026-11-01 (`0055-F6` decision 0007) -->

A consumer cannot install a package from a feed it does not know, with a credential nobody told it to create, and brief § 6 only says that consumers "carry a `nuget.config` source and a token with package read". So a consumer repository, Transporter and this repository's own self-check (`0001-F2` B-014) would each work out the feed, the source entry and the token on their own, differently. This Feature removes that failure state: one document says what a repository commits and what it supplies at run time to install `specht.tool` through its local tool manifest, and this repository installs its own published package the same way. Amended 2026-11-01 (`0055-F6` decision 0007): the feed is nuget.org, which needs no source entry and no credential, and below `1.0.0` an install opts in to prereleases.

## 2. User Needs

<!-- last written by: spec-author, 2026-11-01 (row 2, `0055-F6` decision 0007) -->

| #   | Persona                                 | Need                                                                                                          | Pain Point Today                                |
| --- | --------------------------------------- | ------------------------------------------------------------------------------------------------------------- | ----------------------------------------------- |
| 1   | The maintainer of a consumer repository | The feed, the source entry, the token and the commands, in one place                                          | Only brief § 6's one sentence                   |
| 2   | A consumer's CI                         | To restore the tool with no credential, and none committed (amended 2026-11-01: was with a token it is given) | Nothing says how                                |
| 3   | `0001-F2`                               | This repository's manifest naming the published package, for B-014                                            | N/A (internal dependency)                       |
| 4   | A developer at a workstation shell      | The bare `specht` command on PATH                                                                             | A manifest install runs only as `dotnet specht` |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                                      |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | Retired 2026-11-01 (`0055-F6` decision 0007): reading a NuGet package from GitHub Packages requires an authenticated token even when the package is public, so every consumer, this repository included, supplies one (brief § 6). The feed is nuget.org (A-3). |
| A-2 | A consumer's adoption work - a consumer repository (brief § 8 step 4) and Transporter (step 7) - is tracked in that repository; this Feature supplies only what they need from here (owner, 2026-10-08).                                                        |
| A-3 | nuget.org serves a public package to an unauthenticated read and is a package source every .NET SDK configures by default (`0055-F6` decision 0007).                                                                                                            |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-11-05 (README acceptance) -->

| ID    | Claim                                                                                                                                                                                                                                                                                                                                  | Source                                                                   | Status    |
| ----- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------ | --------- |
| B-001 | Given an empty repository that adds no package source and supplies no credential, `dotnet tool install --local specht.tool --prerelease` adds `specht.tool` to its local tool manifest. Amended 2026-11-01 (`0055-F6` decision 0007): was a repository that adds the documented package source and supplies a token with package read. | brief § 6; owner, 2026-10-08; owner, 2026-11-01, `0055-F6` decision 0007 | Amended   |
| B-002 | Given a clone whose committed local tool manifest names `specht.tool` at a version published to nuget.org, `dotnet tool restore` with no credential restores the tool. Amended 2026-11-01 (`0055-F6` decision 0007): was a committed package source that is the documented one, restored with a read token.                            | brief § 6; `0001-F2` C-6; owner, 2026-11-01, `0055-F6` decision 0007     | Amended   |
| B-003 | Given the first package has been published, this repository's committed local tool manifest names `specht.tool` at a published version.                                                                                                                                                                                                | `0001-F2` B-014; brief § 7                                               | Active    |
| B-004 | Given the install document, it names nuget.org as the feed the tool installs from and says a consumer commits no package source entry for it. Amended 2026-11-01 (`0055-F6` decision 0007): was the feed's address and the package source entry a consumer commits.                                                                    | owner, 2026-10-08; owner, 2026-11-01, `0055-F6` decision 0007            | Withdrawn |
| B-005 | Given the install document, it says installing and restoring the tool needs no credential. Amended 2026-11-01 (`0055-F6` decision 0007): was the token scope a consumer needs and how a CI run supplies the token.                                                                                                                     | owner, 2026-10-08; brief § 6; owner, 2026-11-01, `0055-F6` decision 0007 | Withdrawn |
| B-006 | Given the install document, it names the commands that install the tool locally and restore it in a clone.                                                                                                                                                                                                                             | owner, 2026-10-08                                                        | Withdrawn |
| B-007 | Given a clone and no GitHub token, the solution restores and builds.                                                                                                                                                                                                                                                                   | OQ-2 (owner, 2026-10-08)                                                 | Amended   |
| B-008 | Given the install document, it names the command that installs the tool globally, says that install puts `specht` on PATH, and says a manifest install runs as `dotnet specht`.                                                                                                                                                        | owner, 2026-10-11; `0001-F2` decision 0009; OQ-3                         | Withdrawn |
| B-009 | Given the install document while every published version is below `1.0.0`, each install command it names carries `--prerelease`.                                                                                                                                                                                                       | owner, 2026-11-01, `0055-F6` decision 0007                               | Withdrawn |
| B-010 | Given a `specht.tool` version published to nuget.org, this repository's committed local tool manifest names `specht.tool` at a version published there and usable through standard local tool restore and invocation.                                                                                                                  | owner, 2026-11-01, `0055-F6` decision 0007; B-003                        | Active    |
| B-011 | Given this repository's committed local tool manifest names a version published to nuget.org (B-010), its committed `nuget.config` names no GitHub Packages source.                                                                                                                                                                    | owner, 2026-11-01, `0055-F6` decision 0007                               | Active    |

## 4. Constraints

<!-- last written by: spec-author, 2026-11-05 (README acceptance) -->

| ID  | Constraint                                                                                                                                                                                                                                                                                     | Rules Out                                                                                                          |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| C-1 | A committed package source takes its credential from the environment.                                                                                                                                                                                                                          | A token, password or username-and-token pair in any committed `nuget.config`, here or in the document's examples.  |
| C-2 | The setup guide gives the local install through a committed tool manifest and the global install (`0001-F2` C-6 and its decision 0009).                                                                                                                                                        | A global install as the only documented path, or as a prerequisite of a build.                                     |
| C-3 | The document describes this repository's feed and nothing a consumer's own build does with the tool.                                                                                                                                                                                           | Consumer build wiring, such as a NUKE target, written here as if it were this repository's to maintain.            |
| C-4 | The README gives a local quick start. The setup guide covers both installation paths.                                                                                                                                                                                                          | Duplicating the full setup guide on the front page.                                                                |
| C-5 | This repository's CI, release and self-check runs supply no package-read credential. CI and release restore the pinned tool from nuget.org (B-010, `0055-F6` decision 0007). Amended 2026-11-09 when the pin moved to nuget.org. The former GitHub Packages pin used the workflow's own token. | A stored token, `NuGetPackageSourceCredentials_specht` or `packages: read` supplied to these runs for the tool.    |
| C-6 | nuget.org is the only feed the document tells a consumer to install the tool from (`0055-F6` C-10).                                                                                                                                                                                            | A documented GitHub Packages source, user-level source, `--add-source` or personal access token for `specht.tool`. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-11-05 (README acceptance) -->

| #   | Item                                                            | Exclusion Reason                                                                                                              |
| --- | --------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| 1   | a consumer repository adopting the tool                         | Tracked in a consumer repository (owner, 2026-10-08).                                                                         |
| 2   | Transporter installing the tool                                 | Tracked in Transporter (owner, 2026-10-08).                                                                                   |
| 3   | Publishing the package                                          | `0055-F6`.                                                                                                                    |
| 4   | Writing the schema and templates into a consumer after install  | `specht init`, `0001-F4`.                                                                                                     |
| 5   | What `SpecCheck` does once the manifest names the tool          | `0001-F2` B-014.                                                                                                              |
| 6   | Installing from GitHub Packages, or from any feed but nuget.org | C-6; `0055-F6` C-10. Amended 2026-11-01: the row was installing from NuGet.org, not published there (`0055-F6` C-3, retired). |
| 7   | Maintaining the setup guide                                     | Epic `0002` owns usage documentation.                                                                                         |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-14, 2026-11-01 (rows 5 and 6, item 0327, `0055-F6` decision 0007) -->

| #   | Concern                                                  | Classification |
| --- | -------------------------------------------------------- | -------------- |
| 1   | Published checker version and source routing             | Both           |
| 2   | Environment credentials for tool restore                 | Technical      |
| 3   | Native solution restore without tool-feed authentication | Technical      |
| 4   | Repeatable local and global installation guidance        | Both           |
| 5   | Opting in to a prerelease below 1.0.0                    | Business       |
| 6   | Ending this repository's own authenticated feed          | Both           |

## 7. Technical Design

<!-- last written by: implementer, 2026-11-05 (README acceptance) -->

**A consumer** installs from nuget.org, a default source of every SDK, so a
consumer commits no package source and supplies no credential for the tool
(B-001, B-002, C-6). Below `1.0.0` every published version is a `-preview`
prerelease (`0055-F5` B-009), which NuGet does not pick without
`--prerelease`, so each install command carries it (B-009). A committed tool
manifest pins an exact version, label included, so `dotnet tool restore`
needs no flag. The packages reach nuget.org through `0055-F6`'s release path.

**This repository** uses the same native commands. The local tool manifest
pins the published checker, and root `nuget.config` uses nuget.org for packages
and audit data. The pinned SDK in `global.json` supplies the runtime the
checker selects. Restore in NUKE, CI and release uses that configuration
without a package-read credential (B-010, B-011, C-5).

The README shows how to install the local tool, commit the manifest and restore
the tool in a clone. The setup guide covers both install paths and the public
NuGet feed. Review checks that the examples opt in to prereleases.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-11-05 (README acceptance) -->

`ConsumptionSteps` binds B-001, B-002, B-003, B-007, B-010 and B-011.
For B-003 and B-010, it uses the committed checker entry with a synthetic tool
at the pinned version and exercises local restore and invocation. B-011
reads the committed package sources and mappings. The live restore of the
full manifest and invocation of the real checker were verified on 2026-11-09
with fresh caches, .NET 11 RC3 and no feed credential. `dotnet specht --version`
printed `0.5.35-preview+cb12f05b68`.
`CodeAnalysisReleaseIntegrationTests.RepositoryWorkflows_ShouldRestoreTheCheckerWithoutAPackageReadCredential`
checks CI, main and release for the absence of package-read credentials (C-5).

A folder feed stands in for nuget.org. It sits above the consumer repository in
a `nuget.config` that clears every other source, mapping and audit source, as a
machine's default nuget.org source sits outside a repository. The consumer
repository itself carries no `nuget.config`. The feed holds one synthetic
`specht.tool` package at a prerelease version, `0.9.4-preview`, which prints its
informational version when run. Every child process runs with fresh caches,
without `GITHUB_ACTIONS`, and, under "no credential for any package feed" or
"no token for the package feed", without `NuGetPackageSourceCredentials_*`,
`GITHUB_TOKEN` or `GH_TOKEN`.

B-001 runs `dotnet new tool-manifest --output .config` and
`dotnet tool install --local specht.tool --prerelease` in the fixture. It checks
the package, version and command written to `.config/dotnet-tools.json`.

B-002 writes a manifest naming the synthetic package at its prerelease version,
restores it with no source of the clone's own and no credential, and runs it;
the output is the pinned version.

B-007 remaps the committed package and audit sources to a local folder feed
and restores and builds a synthetic solution. The 2026-10-16 verification
used the former GitHub Packages pin, which has since moved to nuget.org.
B-007 removes credential environment variables, supplies no checker package,
and runs native solution restore followed by build. No automated test uses a
network or real credential.

The fixture checks the native NuGet commands at the process boundary.

The real root tool restore and token-free native solution build were verified on
2026-10-14 for the existing consumption design. The installed package printed
`0.1.90+b213a0522a`. CI and release authentication are covered by their own
Features; self-check adoption and Renovate remain separate scope.

The synthetic application and checker projects target `net$(BundledNETCoreAppTargetFrameworkVersion)`, the framework of the SDK that builds them, where they named `net10.0`. Since `global.json` pins the .NET 11 SDK (`0001-F2` B-045, its decision 0011), that SDK bundles no `net10.0` targeting pack, and the fixture's isolated folder feeds cannot supply one, so a `net10.0` fixture failed to restore. No assertion changed; the fixture's target framework is no part of B-001 to B-008.

Full verification on 2026-10-14: `./build.sh` passed Format, Compile, 904 unit tests, 304 integration tests and 322 acceptance scenarios (209 pending elsewhere). The self-check reported zero errors and 152 existing warnings. The focused Feature run passed all eight scenarios with none skipped.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-11-05 (README acceptance) -->

| Claim ID | Scenario                                                              | Test                                                                            | Status    |
| -------- | --------------------------------------------------------------------- | ------------------------------------------------------------------------------- | --------- |
| B-001    | A new repository installs the tool from the feed                      | `ConsumptionSteps`                                                              | Covered   |
| B-002    | A clone restores the pinned tool                                      | `ConsumptionSteps`                                                              | Covered   |
| B-003    | This repository installs its own tool from nuget.org                  | `ConsumptionSteps`                                                              | Covered   |
| B-004    | The document names the feed and the source entry                      | None                                                                            | Withdrawn |
| B-005    | The document says no credential is needed                             | None                                                                            | Withdrawn |
| B-006    | The document names the install and restore commands                   | None                                                                            | Withdrawn |
| B-007    | A contributor without a token can build                               | `ConsumptionSteps`                                                              | Covered   |
| B-008    | The document names the global install and how each install is invoked | None                                                                            | Withdrawn |
| B-009    | The document opts in to prereleases below 1.0.0                       | None                                                                            | Withdrawn |
| B-010    | This repository installs its own tool from nuget.org                  | `ConsumptionSteps` and credential-free restore and invocation against nuget.org | Covered   |
| B-011    | This repository drops the authenticated feed                          | `ConsumptionSteps` and the workflow credential check in § 8 (C-5)               | Covered   |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-11-05 (README acceptance) -->

- 2026-10-11, C-2 amended in place and B-008 added on the owner's decision that a global install is documented as a second path, for a shell ([`0001-F2` decision 0009](../../src/tool/Features/Check/.spec/decisions/0009-a-global-install-is-a-second-documented-path.md)): a manifest install puts no command on PATH. C-2 ruled out a global install as the documented path; it now rules out one as the only path or as a prerequisite of a build. § 2 row 4 added. B-001 to B-007 are unchanged. The `@B-008` scenario is new and its § 9 row is `Missing`; owed to the `test-writer`.
- 2026-10-11, OQ-3 raised on a note from the `spec-reviewer`: nothing says which package source and credential a global install uses outside a repository that commits the source (`0055-F6` C-3, A-1). B-008's Source cites it. No claim or constraint changes; C-1 holds.

- 2026-10-14, decision 0001: use native solution and tool restore commands. OQ-2 permits tool restore to need a credential while solution restore and build remain token-free. No claim, constraint or scenario changes; no NUKE selection machinery is required.

- 2026-10-14, decision 0002 applies OQ-3's permitted default. B-001's scenario explicitly starts without a tool manifest; B-006 names its creation; B-008 names the global source and environment credential. Claim and constraint ids are unchanged.

- 2026-11-01, the owner moved the feed to nuget.org, with every version below `1.0.0` a `-preview` prerelease ([`0055-F6` decision 0007](../../.build/Releasing/.spec/decisions/0007-publish-prereleases-to-nuget-org-through-trusted-publishing.md)). A-1 retired and A-3 added; B-001, B-002, B-004 and B-005 amended in place: a consumer adds no source and supplies no credential; B-009 added, `--prerelease` below `1.0.0`; C-6 added; § 1, § 2 row 2 and § 5 row 6 amended; OQ-3 resolved in place; [decision 0002](decisions/0002-document-the-authenticated-global-install.md) reversed. Stated transition: this repository's own manifest pins `0.1.131`, published only to GitHub Packages, so its `specht` source and the `NuGetPackageSourceCredentials_specht` credential stay until the first nuget.org release. B-010 and B-011 state the end of it and C-5 is amended to it; their § 9 rows are `Missing` until item `0328` moves the pin. B-003 and B-007 are unchanged. `status` leaves `done`, since § 9 carries `Missing` rows, and `spec_status` returns to `in-review`. Owed to the `test-writer`: `ConsumptionSteps` for the reworded `@B-001`, `@B-002`, `@B-004`, `@B-005` and `@B-008` steps, and § 9's new rows. Owed to the `implementer`: § 7, which describes the GitHub Packages source and credential as the consumer path - delivered 2026-11-01 by the `implementer` in item 0327.

- 2026-11-05, the README quick start changed and acceptance failed because the fixture depended on the old section heading. The owner withdrew the prose checks B-004, B-005, B-006, B-008 and B-009. B-001 still exercises installation against the synthetic feed using explicit commands.

- 2026-11-09, B-010 records native restore and invocation with the public pin.
  C-5 records credential-free restore after the feed transition. The owner
  approved moving the SDK to .NET 11 RC3 so the normal local install can run
  the published tool's .NET 11 build.

## 11. Open Questions

<!-- last written by: spec-author, 2026-11-01 (OQ-3 resolved by the owner, `0055-F6` decision 0007) -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Blocks              | Resolution                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Where does the install document live: this repository's `README.md`, or a page under `docs/`, where `0002-F1` places usage documentation?                                                                                                                                                                                                                                                                                                                                                                                                                 | B-004, B-005, B-006 | Resolved 2026-10-08 by the repository owner: a section of `README.md`, moving into epic `0002`'s documentation later. C-4 and § 5 row 7 added.                                                                                                                                                                                                                                                                                                                                                                                                                              |
| OQ-2 | Once this repository's own sources include the authenticated feed (B-003), can a contributor without a GitHub token still restore and build the solution, with only the tool restore needing it? Proposed default: yes, as a claim.                                                                                                                                                                                                                                                                                                                       | B-003               | Resolved 2026-10-08: proposed default accepted by the repository owner. B-007 added.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| OQ-3 | Which package source and credential does a global install use? `0055-F6` C-3 makes GitHub Packages the only feed and A-1 requires a token for every read, so `dotnet tool install -g specht.tool` run outside a repository that commits the source has neither. C-1 holds: no credential in a committed file or in a documented example. Raised by `spec-reviewer`, 2026-10-11. Proposed default: the document tells the reader to pass the feed with `--add-source` and to supply the token from the environment through user-level NuGet configuration. | B-008               | Resolved 2026-11-01 by the repository owner ([`0055-F6` decision 0007](../../.build/Releasing/.spec/decisions/0007-publish-prereleases-to-nuget-org-through-trusted-publishing.md)): `dotnet tool install -g specht.tool --prerelease` from nuget.org, with no package source and no credential; decision 0002 reversed. Was resolved 2026-10-14 by applying the explicitly permitted default, not a new owner decision: named user-level source, environment credential and `--add-source` ([decision 0002](decisions/0002-document-the-authenticated-global-install.md)). |

## 12. Sign-off

<!-- last written by: spec-author, 2026-11-01 (`0055-F6` decision 0007 row added, awaiting spec-reviewer; historical rows preserved) -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| ------- | ------ | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟢     | spec-reviewer | Round 2 (2026-10-08) - approved, reviewing `acf3cff`..`f32a31d`. C-5 states the owner's read token for this repository's own runs and agrees with `0055-F2` B-011 and `0055-F6` B-010; B-007 is one assertion; `0055-F1` B-010 now takes the token.                                                                                                                                                                                                                                                                                                                                                                                                                    |
| Tasks   | 🟢     | spec-reviewer | Item cut (2026-10-08) - approved, reviewing `6113181`..`0657b68`. 0087 and 0088 carry B-001-B-007 once; 0087 delivers B-003, and 0032, 0067, 0077 and 0085 wait on it.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| 1-5     | 🟢     | spec-reviewer | Round 3 (2026-10-11) - approved, reviewing the uncommitted diff on `60de077`. C-2 is amended in place with its id kept and agrees with `0001-F2` C-6 and its decision 0009; B-008 is a new id, with one § 9 row and one `@B-008` scenario that state the same three things; § 10 records the delta. Non-blocking (spec-author): `0055-F6` C-3 and A-1 make the feed GitHub Packages with a token, and no claim says where a global install outside a repository finds the source and the token, as B-001's Given does for the manifest install; brief § 6's install row and decision 0009 do not name the owner's 2026-10-09 decision in item 0111 that this reverses. |
| Tasks   | 🟢     | spec-reviewer | B-008 joined (2026-10-11) - approved, reviewing the same diff. 0086 and 0088 carry B-008 once each as parent and child, 0087 is unchanged, and no item stores `priority`, `rank` or `blocks`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| 1-11    | 🟢     | spec-reviewer | 2026-10-14, self-review of installation delivery: all eight claims have one tagged scenario and one Covered row; all eight scenarios execute and pass. README setup satisfies C-1 to C-4; C-5's existing workflow configuration is preserved. Decision 0002 applies the permitted OQ-3 default. Full build and formatting pass; no production architecture or consumer build wiring was added. No independent review is claimed.                                                                                                                                                                                                                                       |

| 6-10 | 🟢 | spec-reviewer | 2026-10-14, self-review of 0087: native solution restore/build satisfies B-007 without credentials; native tool restore satisfies B-002 with an environment credential; the published pin satisfies B-003. Three executable scenarios pass. The Restore target is unchanged and workflow authentication uses the workflow token. No independent review is claimed. |
| 1-5, 9-11 (`0055-F6` decision 0007) | 🟡 | spec-reviewer | Feed moved to nuget.org (2026-11-01) - awaiting review. |

### Review history

Earlier review rows are preserved verbatim; later approvals and the current record supersede them.

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| ------- | ------ | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Draft; not reviewed                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| 1-5     | 🟡     | spec-reviewer | Round 1 (2026-10-08) - reviewed in draft, reviewing commit `04fdc28`; no blocking findings. OQ-1 and OQ-2 are claimed, C-1 keeps every credential out of tracked files, and B-003 agrees with `0001-F2` B-014 and A-2. Non-blocking (spec-author): the Business Goal has this repository install its own package the same way, but no claim here or in `0055-F2` or `0055-F6` says how this repository's own runs get the read token A-1 requires; B-007's second clause restates A-1 rather than asserting anything new; `0055-F1` B-010 is contradicted after B-003 (recorded there). Left `draft`: the spec-author has not submitted it, and epic `0055` says its eight-Feature split awaits the owner's confirmation. |

## Tasks

Cut 2026-10-08 into [`../.issue/`](../.issue/): `0086` (the Feature), with `0087` to `0088`. 2026-10-11: B-008 joined `0088`. 2026-11-01 (`0055-F6` decision 0007): B-001, B-002, B-004, B-005, B-008 and B-009 joined `0327`, in [`../../.build/Releasing/.issue/`](../../.build/Releasing/.issue/); `0328` cut for B-010 and B-011.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
