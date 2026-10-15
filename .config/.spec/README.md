---
title: "Specification: Consuming the package"
description: "What a repository needs from this one to install specht.tool through its local tool manifest, or globally for a shell - the GitHub Packages feed, the package source entry, the token scope and the commands - written down once, and used by this repository itself after the first publish"
type: feature
id: "F7"
epic: "0055"
spec_status: approved
status: done
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
updated: "2026-10-14"
github_issue: null
synced_at: null
---

# Specification: Consuming the package

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-08 -->

A consumer cannot install a package from a feed it does not know, with a credential nobody told it to create, and brief § 6 only says that consumers "carry a `nuget.config` source and a token with package read". So a consumer repository, Transporter and this repository's own self-check (`0001-F2` B-014) would each work out the feed, the source entry and the token on their own, differently. This Feature removes that failure state: one document says what a repository commits and what it supplies at run time to install `specht.tool` through its local tool manifest, and this repository installs its own published package the same way.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-11 (row 4) -->

| #   | Persona                                 | Need                                                                 | Pain Point Today                                |
| --- | --------------------------------------- | -------------------------------------------------------------------- | ----------------------------------------------- |
| 1   | The maintainer of a consumer repository | The feed, the source entry, the token and the commands, in one place | Only brief § 6's one sentence                   |
| 2   | A consumer's CI                         | To restore the tool with a token it is given, not one committed      | Nothing says how                                |
| 3   | `0001-F2`                               | This repository's manifest naming the published package, for B-014   | N/A (internal dependency)                       |
| 4   | A developer at a workstation shell      | The bare `specht` command on PATH                                    | A manifest install runs only as `dotnet specht` |

### Assumptions

| ID  | Assumption                                                                                                                                                                                               |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | Reading a NuGet package from GitHub Packages requires an authenticated token even when the package is public, so every consumer, this repository included, supplies one (brief § 6).                     |
| A-2 | A consumer's adoption work - a consumer repository (brief § 8 step 4) and Transporter (step 7) - is tracked in that repository; this Feature supplies only what they need from here (owner, 2026-10-08). |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-11 (B-008; its Source cites OQ-3) -->

| ID    | Claim                                                                                                                                                                                      | Source                                           | Status  |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------ | ------- |
| B-001 | Given an empty repository that adds the documented package source and supplies a token with package read, the documented install command adds `specht.tool` to its local tool manifest.    | brief § 6; owner, 2026-10-08                     | Active  |
| B-002 | Given a clone whose committed local tool manifest names `specht.tool` and whose committed package source is the documented one, `dotnet tool restore` with a read token restores the tool. | brief § 6; `0001-F2` C-6                         | Active  |
| B-003 | Given the first package has been published, this repository's committed local tool manifest names `specht.tool` at a published version.                                                    | `0001-F2` B-014; brief § 7                       | Active  |
| B-004 | Given the install document, it names the feed's address and the package source entry a consumer commits.                                                                                   | owner, 2026-10-08                                | Active  |
| B-005 | Given the install document, it names the token scope a consumer needs and how a CI run supplies the token.                                                                                 | owner, 2026-10-08; brief § 6                     | Active  |
| B-006 | Given the install document, it names the commands that install the tool locally and restore it in a clone.                                                                                 | owner, 2026-10-08                                | Active  |
| B-007 | Given a clone and no GitHub token, the solution restores and builds.                                                                                                                       | OQ-2 (owner, 2026-10-08)                         | Amended |
| B-008 | Given the install document, it names the command that installs the tool globally, says that install puts `specht` on PATH, and says a manifest install runs as `dotnet specht`.            | owner, 2026-10-11; `0001-F2` decision 0009; OQ-3 | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-11 (C-2 amended) -->

| ID  | Constraint                                                                                                                                                                        | Rules Out                                                                                                         |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| C-1 | A committed package source takes its credential from the environment.                                                                                                             | A token, password or username-and-token pair in any committed `nuget.config`, here or in the document's examples. |
| C-2 | The document gives the local install, through a committed tool manifest, and the global install (`0001-F2` C-6 and its decision 0009).                                            | A global install as the only documented path, or as a prerequisite of a build.                                    |
| C-3 | The document describes this repository's feed and nothing a consumer's own build does with the tool.                                                                              | Consumer build wiring, such as a NUKE target, written here as if it were this repository's to maintain.           |
| C-4 | The install document is a section of this repository's `README.md` (owner, 2026-10-08).                                                                                           | A second install document elsewhere in this repository while that section exists.                                 |
| C-5 | This repository's own CI, release and self-check runs read the feed with the workflow's own token granted `packages: read` (owner, 2026-10-08; `0055-F2` B-011, `0055-F6` B-010). | A stored read token as a repository secret for this repository's own runs.                                        |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Item                                                           | Exclusion Reason                                                                |
| --- | -------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| 1   | a consumer repository adopting the tool                        | Tracked in a consumer repository (owner, 2026-10-08).                           |
| 2   | Transporter installing the tool                                | Tracked in Transporter (owner, 2026-10-08).                                     |
| 3   | Publishing the package                                         | `0055-F6`.                                                                      |
| 4   | Writing the schema and templates into a consumer after install | `specht init`, `0001-F4`.                                                       |
| 5   | What `SpecCheck` does once the manifest names the tool         | `0001-F2` B-014.                                                                |
| 6   | Installing from NuGet.org                                      | Not published there (`0055-F6` C-3).                                            |
| 7   | Moving the install document into `docs/`                       | Later, under epic `0002`, which owns usage documentation; C-4 holds until then. |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-14 -->

| #   | Concern                                                  | Classification |
| --- | -------------------------------------------------------- | -------------- |
| 1   | Published checker version and source routing             | Both           |
| 2   | Environment credentials for tool restore                 | Technical      |
| 3   | Native solution restore without tool-feed authentication | Technical      |
| 4   | Repeatable local and global installation guidance        | Both           |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-14 -->

[Decision 0001](decisions/0001-use-native-package-and-tool-restore.md) uses
native restore commands. The root tool manifest adds published `specht.tool`
0.1.90. Root `nuget.config` maps exactly that package to the `specht` source,
GitHub Packages; all other packages and audit data use nuget.org. NuGet reads
`NuGetPackageSourceCredentials_specht` from the process environment, in its
native `Username=...;Password=...;ValidAuthenticationTypes=Basic` format.

`dotnet tool restore` restores all tools with a read credential. Without one,
`dotnet restore specht.slnx` and `dotnet build specht.slnx` still restore and
build the solution. NUKE retains its existing tool restore followed by solution
restore, so full NUKE invocations need the tool-feed credential. The generated CI and release build jobs receive the native NuGet environment
credential from their workflow token, granted package read (0067 and 0085).
No custom manifest generation or restore-target change is required.

The install section of `README.md` owns the source XML, runtime credential, local manifest creation/install/restore commands and the global path. Decision 0002 applies OQ-3's permitted default. Documentation scenarios read that section; the empty-repository scenario executes its native manifest-creation and install commands against synthetic folder feeds.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-14 (the synthetic projects' target framework, `0001-F2` B-045) -->

`ConsumptionSteps` binds all eight scenarios. The five installation scenarios
were run before the README change: five failed for missing guidance and the
three existing scenarios passed. After the README change, all eight pass and
none are skipped.

B-001 reads the README's source XML and executes its manifest-creation and local
install commands against synthetic folder feeds with fresh caches. It starts
without a manifest and checks the installed package, version and command in
`.config/dotnet-tools.json`. The explicit `--output .config` makes the documented
manifest destination independent of the SDK template's default. B-004 to B-006
and B-008 read the same install section for the named source, runtime credential,
CI setup, install/restore commands and authenticated global path.

B-002 restores and invokes a synthetic checker at the committed pin. B-003
checks that pin against 0.1.90, whose publication is recorded in `0055-F6` § 8.
B-007 removes credential environment variables, supplies no checker package,
and runs native solution restore followed by build. No automated test uses a
network or real credential.

No new unit tier is needed: `README.md`, `nuget.config`, the tool manifest and
`ConsumptionSteps` were read; the change adds documentation and its acceptance
harness, not a production decision, derivation or selection algorithm. The native
NuGet commands are observed at the process boundary.

The real root tool restore and token-free native solution build were verified on
2026-10-14 for the existing consumption design. The installed package printed
`0.1.90+b213a0522a`. CI and release authentication are covered by their own
Features; self-check adoption and Renovate remain separate scope.

The synthetic application and checker projects target `net$(BundledNETCoreAppTargetFrameworkVersion)`, the framework of the SDK that builds them, where they named `net10.0`. Since `global.json` pins the .NET 11 SDK (`0001-F2` B-045, its decision 0011), that SDK bundles no `net10.0` targeting pack, and the fixture's isolated folder feeds cannot supply one, so a `net10.0` fixture failed to restore. No assertion changed; the fixture's target framework is no part of B-001 to B-008.

Full verification on 2026-10-14: `./build.sh` passed Format, Compile, 904 unit tests, 304 integration tests and 322 acceptance scenarios (209 pending elsewhere). The self-check reported zero errors and 152 existing warnings. The focused Feature run passed all eight scenarios with none skipped.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-14 -->

| Claim ID | Scenario                                                              | Test               | Status  |
| -------- | --------------------------------------------------------------------- | ------------------ | ------- |
| B-001    | A new repository installs the tool from the feed                      | `ConsumptionSteps` | Covered |
| B-002    | A clone restores the pinned tool                                      | `ConsumptionSteps` | Covered |
| B-003    | This repository installs its own published tool                       | `ConsumptionSteps` | Covered |
| B-004    | The document names the feed and the source entry                      | `ConsumptionSteps` | Covered |
| B-005    | The document names the token and how CI supplies it                   | `ConsumptionSteps` | Covered |
| B-006    | The document names the install and restore commands                   | `ConsumptionSteps` | Covered |
| B-007    | A contributor without a token can build                               | `ConsumptionSteps` | Covered |
| B-008    | The document names the global install and how each install is invoked | `ConsumptionSteps` | Covered |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-14 -->

- 2026-10-11, C-2 amended in place and B-008 added on the owner's decision that a global install is documented as a second path, for a shell ([`0001-F2` decision 0009](../../src/tool/Features/Check/.spec/decisions/0009-a-global-install-is-a-second-documented-path.md)): a manifest install puts no command on PATH. C-2 ruled out a global install as the documented path; it now rules out one as the only path or as a prerequisite of a build. § 2 row 4 added. B-001 to B-007 are unchanged. The `@B-008` scenario is new and its § 9 row is `Missing`; owed to the `test-writer`.
- 2026-10-11, OQ-3 raised on a note from the `spec-reviewer`: nothing says which package source and credential a global install uses outside a repository that commits the source (`0055-F6` C-3, A-1). B-008's Source cites it. No claim or constraint changes; C-1 holds.

- 2026-10-14, decision 0001: use native solution and tool restore commands. OQ-2 permits tool restore to need a credential while solution restore and build remain token-free. No claim, constraint or scenario changes; no NUKE selection machinery is required.

- 2026-10-14, decision 0002 applies OQ-3's permitted default. B-001's scenario explicitly starts without a tool manifest; B-006 names its creation; B-008 names the global source and environment credential. Claim and constraint ids are unchanged.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-14 (OQ-3 default applied) -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Blocks              | Resolution                                                                                                                                                                                                                                     |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Where does the install document live: this repository's `README.md`, or a page under `docs/`, where `0002-F1` places usage documentation?                                                                                                                                                                                                                                                                                                                                                                                                                 | B-004, B-005, B-006 | Resolved 2026-10-08 by the repository owner: a section of `README.md`, moving into epic `0002`'s documentation later. C-4 and § 5 row 7 added.                                                                                                 |
| OQ-2 | Once this repository's own sources include the authenticated feed (B-003), can a contributor without a GitHub token still restore and build the solution, with only the tool restore needing it? Proposed default: yes, as a claim.                                                                                                                                                                                                                                                                                                                       | B-003               | Resolved 2026-10-08: proposed default accepted by the repository owner. B-007 added.                                                                                                                                                           |
| OQ-3 | Which package source and credential does a global install use? `0055-F6` C-3 makes GitHub Packages the only feed and A-1 requires a token for every read, so `dotnet tool install -g specht.tool` run outside a repository that commits the source has neither. C-1 holds: no credential in a committed file or in a documented example. Raised by `spec-reviewer`, 2026-10-11. Proposed default: the document tells the reader to pass the feed with `--add-source` and to supply the token from the environment through user-level NuGet configuration. | B-008               | Resolved 2026-10-14 by applying the explicitly permitted default, not a new owner decision: named user-level source, environment credential and `--add-source` ([decision 0002](decisions/0002-document-the-authenticated-global-install.md)). |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-14 (historical rows preserved) -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| ------- | ------ | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟢     | spec-reviewer | Round 2 (2026-10-08) - approved, reviewing `acf3cff`..`f32a31d`. C-5 states the owner's read token for this repository's own runs and agrees with `0055-F2` B-011 and `0055-F6` B-010; B-007 is one assertion; `0055-F1` B-010 now takes the token.                                                                                                                                                                                                                                                                                                                                                                                                                    |
| Tasks   | 🟢     | spec-reviewer | Item cut (2026-10-08) - approved, reviewing `6113181`..`0657b68`. 0087 and 0088 carry B-001-B-007 once; 0087 delivers B-003, and 0032, 0067, 0077 and 0085 wait on it.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| 1-5     | 🟢     | spec-reviewer | Round 3 (2026-10-11) - approved, reviewing the uncommitted diff on `60de077`. C-2 is amended in place with its id kept and agrees with `0001-F2` C-6 and its decision 0009; B-008 is a new id, with one § 9 row and one `@B-008` scenario that state the same three things; § 10 records the delta. Non-blocking (spec-author): `0055-F6` C-3 and A-1 make the feed GitHub Packages with a token, and no claim says where a global install outside a repository finds the source and the token, as B-001's Given does for the manifest install; brief § 6's install row and decision 0009 do not name the owner's 2026-10-09 decision in item 0111 that this reverses. |
| Tasks   | 🟢     | spec-reviewer | B-008 joined (2026-10-11) - approved, reviewing the same diff. 0086 and 0088 carry B-008 once each as parent and child, 0087 is unchanged, and no item stores `priority`, `rank` or `blocks`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| 1-11    | 🟢     | spec-reviewer | 2026-10-14, self-review of installation delivery: all eight claims have one tagged scenario and one Covered row; all eight scenarios execute and pass. README setup satisfies C-1 to C-4; C-5's existing workflow configuration is preserved. Decision 0002 applies the permitted OQ-3 default. Full build and formatting pass; no production architecture or consumer build wiring was added. No independent review is claimed.                                                                                                                                                                                                                                       |

| 6-10 | 🟢 | spec-reviewer | 2026-10-14, self-review of 0087: native solution restore/build satisfies B-007 without credentials; native tool restore satisfies B-002 with an environment credential; the published pin satisfies B-003. Three executable scenarios pass. The Restore target is unchanged and workflow authentication uses the workflow token. No independent review is claimed. |

### Review history

Earlier review rows are preserved verbatim; later approvals and the current record supersede them.

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| ------- | ------ | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Draft; not reviewed                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| 1-5     | 🟡     | spec-reviewer | Round 1 (2026-10-08) - reviewed in draft, reviewing commit `04fdc28`; no blocking findings. OQ-1 and OQ-2 are claimed, C-1 keeps every credential out of tracked files, and B-003 agrees with `0001-F2` B-014 and A-2. Non-blocking (spec-author): the Business Goal has this repository install its own package the same way, but no claim here or in `0055-F2` or `0055-F6` says how this repository's own runs get the read token A-1 requires; B-007's second clause restates A-1 rather than asserting anything new; `0055-F1` B-010 is contradicted after B-003 (recorded there). Left `draft`: the spec-author has not submitted it, and epic `0055` says its eight-Feature split awaits the owner's confirmation. |

## Tasks

Cut 2026-10-08 into [`../.issue/`](../.issue/): `0086` (the Feature), with `0087` to `0088`. 2026-10-11: B-008 joined `0088`.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
