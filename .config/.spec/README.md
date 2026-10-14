---
title: "Specification: Consuming the package"
description: "What a repository needs from this one to install specht.tool through its local tool manifest, or globally for a shell - the GitHub Packages feed, the package source entry, the token scope and the commands - written down once, and used by this repository itself after the first publish"
type: feature
id: "F7"
epic: "0055"
spec_status: approved
status: ready-for-architecture
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

| #   | Concern                                                        | Classification |
| --- | -------------------------------------------------------------- | -------------- |
| 1   | Published checker version and source routing                   | Both           |
| 2   | Environment credentials for package reads                      | Technical      |
| 3   | Development prerequisite selection without feed authentication | Technical      |
| 4   | Separate self-check and workflow adoption                      | Business       |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-14 -->

[Decision 0001](decisions/0001-isolate-package-consumption-from-development-tool-restore.md)
keeps all pins in [the root tool manifest](../dotnet-tools.json), adding the
published `specht.tool` version `0.1.90`. Root `nuget.config` uses the source
key `specht`, an exact package mapping for the checker, a public fallback
for all other packages and a public audit source. Credentials are supplied
through `NuGetPackageSourceCredentials_specht`, using NuGet's native
`Username=...;Password=...;ValidAuthenticationTypes=Basic` value format.

[`BuildToolManifest`](../../.build/BuildToolManifest.cs) derives the public
build-tool manifest without I/O. The build's `Restore` target writes it
atomically under `.artifacts/tool-restore/`, restores only those tools, then
restores the solution. Complete root `dotnet tool restore` still restores
the checker when the caller supplies its read credential. No committed
configuration is rewritten and no restore failure is swallowed.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-14 -->

B-002 and B-007 use synthetic .NET tools on local folder feeds, fresh tool
and package caches and scoped process environments. B-002 restores and
invokes the pinned synthetic checker through the committed manifest and
source policy with only endpoints replaced by folder feeds. B-007 runs the
actual compiled build's `Restore` target against a synthetic solution with
an unavailable checker feed, then builds that solution without a token.
The boundary scenario verifies the development commands are restored while
the checker is not. No scenario recurses into this repository's Test target.

`BuildToolManifestUnitTests` pins the selection mechanism: remove only the
published checker and preserve all other tool settings. Source mappings
and the package pin are declarations consumed by NuGet; the local-feed
acceptance proof exercises their effect instead of duplicating XML parsing
in a convention test. B-003 checks the committed pin against `0.1.90`, whose
actual publication is the `0055-F6` section 8 record. The actual GitHub read
is a separate manual proof; no automated test uses a real credential or
network. The documentation claims remain with 0088.

The selection unit test failed with the checker retained; both no-token
acceptance scenarios failed when Restore used the complete manifest. After
implementation, both unit cases and all four package-consumption scenarios
pass. Five documentation scenarios remain pending for 0088.

On 2026-10-14, root `dotnet tool restore` with the existing login's credential
supplied only through the process environment restored the real published
package, including a repeat with fresh NuGet and HTTP caches. `dotnet specht --version` printed `0.1.90+b213a0522a`, identifying
the first release commit. The root schema pin remains unchanged.

The complete build with package credentials removed passed: 906 unit tests,
304 integration tests and 318 acceptance scenarios; 214 scenarios are pending,
including the five documentation scenarios for 0088. Format passed and the
self-check reported zero errors and 157 existing coverage warnings. Workflow
regeneration changed no generated file.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-14 -->

| Claim ID | Scenario                                                                                          | Test                                             | Status  |
| -------- | ------------------------------------------------------------------------------------------------- | ------------------------------------------------ | ------- |
| B-001    | A new repository installs the tool from the feed                                                  | Missing                                          | Missing |
| B-002    | A clone restores the pinned tool                                                                  | `ConsumptionSteps`                               | Covered |
| B-003    | This repository installs its own published tool                                                   | `ConsumptionSteps`                               | Covered |
| B-004    | The document names the feed and the source entry                                                  | Missing                                          | Missing |
| B-005    | The document names the token and how CI supplies it                                               | Missing                                          | Missing |
| B-006    | The document names the install and restore commands                                               | Missing                                          | Missing |
| B-007    | A contributor without a token can build; Build prerequisites do not require the published checker | `ConsumptionSteps`; `BuildToolManifestUnitTests` | Covered |
| B-008    | The document names the global install and how each install is invoked                             | Missing                                          | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-14 -->

- 2026-10-11, C-2 amended in place and B-008 added on the owner's decision that a global install is documented as a second path, for a shell ([`0001-F2` decision 0009](../../src/tool/Features/Check/.spec/decisions/0009-a-global-install-is-a-second-documented-path.md)): a manifest install puts no command on PATH. C-2 ruled out a global install as the documented path; it now rules out one as the only path or as a prerequisite of a build. § 2 row 4 added. B-001 to B-007 are unchanged. The `@B-008` scenario is new and its § 9 row is `Missing`; owed to the `test-writer`.
- 2026-10-11, OQ-3 raised on a note from the `spec-reviewer`: nothing says which package source and credential a global install uses outside a repository that commits the source (`0055-F6` C-3, A-1). B-008's Source cites it. No claim or constraint changes; C-1 holds.

- 2026-10-14, decision 0001: record package-source isolation and development-only restore as the implementation of OQ-2. B-007 gains a boundary scenario proving an unavailable published checker does not prevent restoring build prerequisites. No claim or constraint changes; package consumption in the self-check and workflows remains separate work.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-11 (OQ-3 raised) -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Blocks              | Resolution                                                                                                                                     |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Where does the install document live: this repository's `README.md`, or a page under `docs/`, where `0002-F1` places usage documentation?                                                                                                                                                                                                                                                                                                                                                                                                                 | B-004, B-005, B-006 | Resolved 2026-10-08 by the repository owner: a section of `README.md`, moving into epic `0002`'s documentation later. C-4 and § 5 row 7 added. |
| OQ-2 | Once this repository's own sources include the authenticated feed (B-003), can a contributor without a GitHub token still restore and build the solution, with only the tool restore needing it? Proposed default: yes, as a claim.                                                                                                                                                                                                                                                                                                                       | B-003               | Resolved 2026-10-08: proposed default accepted by the repository owner. B-007 added.                                                           |
| OQ-3 | Which package source and credential does a global install use? `0055-F6` C-3 makes GitHub Packages the only feed and A-1 requires a token for every read, so `dotnet tool install -g specht.tool` run outside a repository that commits the source has neither. C-1 holds: no credential in a committed file or in a documented example. Raised by `spec-reviewer`, 2026-10-11. Proposed default: the document tells the reader to pass the feed with `--add-source` and to supply the token from the environment through user-level NuGet configuration. | B-008               | Open. Owner not asked. The item that delivers B-008 writes the proposed default; does not block it.                                            |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-14 (historical rows preserved) -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| ------- | ------ | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟢     | spec-reviewer | Round 2 (2026-10-08) - approved, reviewing `acf3cff`..`f32a31d`. C-5 states the owner's read token for this repository's own runs and agrees with `0055-F2` B-011 and `0055-F6` B-010; B-007 is one assertion; `0055-F1` B-010 now takes the token.                                                                                                                                                                                                                                                                                                                                                                                                                    |
| Tasks   | 🟢     | spec-reviewer | Item cut (2026-10-08) - approved, reviewing `6113181`..`0657b68`. 0087 and 0088 carry B-001-B-007 once; 0087 delivers B-003, and 0032, 0067, 0077 and 0085 wait on it.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| 1-5     | 🟢     | spec-reviewer | Round 3 (2026-10-11) - approved, reviewing the uncommitted diff on `60de077`. C-2 is amended in place with its id kept and agrees with `0001-F2` C-6 and its decision 0009; B-008 is a new id, with one § 9 row and one `@B-008` scenario that state the same three things; § 10 records the delta. Non-blocking (spec-author): `0055-F6` C-3 and A-1 make the feed GitHub Packages with a token, and no claim says where a global install outside a repository finds the source and the token, as B-001's Given does for the manifest install; brief § 6's install row and decision 0009 do not name the owner's 2026-10-09 decision in item 0111 that this reverses. |
| Tasks   | 🟢     | spec-reviewer | B-008 joined (2026-10-11) - approved, reviewing the same diff. 0086 and 0088 carry B-008 once each as parent and child, 0087 is unchanged, and no item stores `priority`, `rank` or `blocks`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |

| 6-10 | 🟢 | spec-reviewer | 2026-10-14, self-review of 0087: B-002, B-003 and B-007 trace to executable scenarios; development-only selection has a unit proof and an unavailable-checker boundary. The real published package restores and reports the release version. No credential is tracked. Documentation, self-check adoption and workflow authentication remain separate items. No independent review is claimed. |

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
