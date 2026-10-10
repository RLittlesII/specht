---
title: "Specification: Package versioning"
description: "Nerdbank.GitVersioning computes the package version from version.json and the git height, public on main and v* tags and suffixed elsewhere, the release tag is created from it with nbgv tag, and the package version never moves the schema version"
type: feature
id: "F5"
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
depends_on: ["F1"]
blocks: ["F6"]
spikes: []
created: "2026-10-08"
updated: "2026-10-09"
github_issue: null
synced_at: null
---

# Specification: Package versioning

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-08 -->

Nothing sets the package's version, so every `Pack` would produce the same default number, two builds of different commits would be indistinguishable to a consumer's local tool manifest, and the first release would carry whatever someone typed on the day. This Feature removes that failure state: the version is a function of the commit, computed by Nerdbank.GitVersioning from `version.json` and the git height, public on `main` and on `v*` tags and visibly a prerelease everywhere else, and the release tag is made from the computed version rather than typed. The package version moves independently of the schema version a repository pins.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Persona                               | Need                                                   | Pain Point Today                            |
| --- | ------------------------------------- | ------------------------------------------------------ | ------------------------------------------- |
| 1   | A consumer pinning the tool           | One version means one commit                           | Every pack carries the same default version |
| 2   | The maintainer cutting a release      | A tag that cannot disagree with what is built          | A tag would be typed by hand                |
| 3   | The maintainer testing a branch build | A branch package that cannot be mistaken for a release | Nothing marks a build as a prerelease       |
| 4   | `0055-F1`, `0055-F6`                  | The version to pack and to publish                     | N/A (internal dependency)                   |

### Assumptions

| ID  | Assumption                                                                                                                                                                 |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | A release is cut by running `nbgv tag` on a commit of `main`, which creates the `v*` tag from the computed version; nobody types a version into a tag (owner, 2026-10-08). |
| A-2 | `version.json`'s `publicReleaseRefSpec` names `main` and `v*` tags, and only those (owner, 2026-10-08).                                                                    |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                                                | Source                                         | Status  |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------- | ------- |
| B-001 | Given one commit built from the same ref, with its full history, on any two machines, the build computes the same package version on both.                                           | owner, 2026-10-08; decision 0001; B-003; B-004 | Amended |
| B-002 | Given two commits on `main` where the second follows the first and `version.json` is unchanged, the second's computed version is higher than the first's.                            | owner, 2026-10-08; decision 0001               | Active  |
| B-003 | Given a commit built from `main` or from a `v*` tag, the computed version carries no prerelease suffix.                                                                              | owner, 2026-10-08; A-2                         | Active  |
| B-004 | Given a commit built from any other ref, the computed version carries a prerelease suffix naming the commit.                                                                         | owner, 2026-10-08; A-2                         | Active  |
| B-005 | Given the `Pack` target, the version inside the package equals the version computed for the commit.                                                                                  | owner, 2026-10-08; `0055-F1` B-009             | Active  |
| B-006 | Given a commit that changes the version in `version.json` and nothing else, the schema versions the tool embeds and the `schemaVersion` `specht init` writes are unchanged.          | owner, 2026-10-08; `0001-F7` B-012; C-1        | Active  |
| B-007 | Given a fresh clone after the local tools are restored, `dotnet nbgv tag` on a commit of `main` creates the tag `v<version>` at that commit, `<version>` being its computed version. | owner, 2026-10-08; A-1                         | Active  |
| B-008 | Given `version.json` as first committed, the version computed for a commit of `main` begins `0.1.`.                                                                                  | OQ-1 (owner, 2026-10-08)                       | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-08 -->

| ID  | Constraint                                                                                                                               | Rules Out                                                                                                                                   |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | The package version and the schema version are independent numbers; neither is computed from the other (`0001-F7` owns `schemaVersion`). | A package major tied to the schema version; a schema version bumped to mark a tool release; a tool release forced by a schema change alone. |
| C-2 | The version is computed from the commit's full history.                                                                                  | A shallow checkout in any build that packs (`0055-F2` C-2, `0055-F6`).                                                                      |
| C-3 | Nerdbank.GitVersioning is the only source of the version.                                                                                | A `<Version>` in `Directory.Build.props` or a project file; a version passed in by a workflow variable or typed into a tag.                 |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Item                                                  | Exclusion Reason                                                                       |
| --- | ----------------------------------------------------- | -------------------------------------------------------------------------------------- |
| 1   | Schema versions, `schemaVersion` and `specht upgrade` | `0001-F7`.                                                                             |
| 2   | Publishing on a tag                                   | `0055-F6`.                                                                             |
| 3   | A `--version` option on the command                   | The command's surface is epic `0001`'s; not asked for.                                 |
| 4   | When the first tag is cut                             | `0055-F6` C-2.                                                                         |
| 5   | Versioning any assembly other than the packed tool's  | Only `specht.tool` packs (`0001-F2` C-5); the rest takes the same version, unasserted. |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-09 -->

| #   | Concern                                                                 | Classification |
| --- | ----------------------------------------------------------------------- | -------------- |
| 1   | Which version line the package is on                                    | Business       |
| 2   | Which refs build a public version, and which build a prerelease         | Business       |
| 3   | Computing the version from the commit and its height                    | Technical      |
| 4   | Giving every build the full history the height is counted from          | Technical      |
| 5   | Cutting the release tag from the computed version                       | Both           |
| 6   | Keeping one source of the version                                       | Both           |
| 7   | Getting the `nbgv` tool onto a fresh clone, and keeping its pin current | Technical      |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-09 -->

Delivered so far by item 0079. Stamping the computed version into the assemblies and the package with the Nerdbank.GitVersioning MSBuild package (B-005) is 0080; the schema version staying put when `version.json` moves (B-006) is 0081. Until 0080 lands, `Pack` still carries the SDK's default version, and the version below is what `dotnet nbgv get-version` reports.

- **The source** is [`version.json`](../../../version.json) at the repository root: `version` `0.1` (OQ-1), `publicReleaseRefSpec` `^refs/heads/main$` and `^refs/tags/v\d+(?:\.\d+)*$` (A-2), and `release.tagName` `v{version}`. It carries no comments; the reasons live here.
- **The tool** is `nbgv` 3.10.94 in the local tool manifest, [`.config/dotnet-tools.json`](../../../.config/dotnet-tools.json), `rollForward: false`. The build's `Restore` target restores it with `DotNetToolRestore` (item 0057), so a fresh clone has it after `./build.sh` or `dotnet tool restore`. Renovate keeps the pin current through the manifest.
- **The version** is `<version>.<height>`, the height counting commits since `version.json`'s `version` last changed. The commit that introduces `version.json` is therefore `0.1.1` (B-008), and each later commit on `main` is one higher (B-002).
- **Public or prerelease** is the ref matched against `publicReleaseRefSpec`. Locally `nbgv` reads the checked-out branch. On GitHub Actions it reads `GITHUB_REF`, so a push to `main` (`refs/heads/main`) or a `v*` tag builds a public version (B-003), and a pull request (`refs/pull/<n>/merge`, built detached on the head commit) builds a prerelease such as `0.1.1-g7a88c23105`, the suffix being `g` and the commit id (B-004).
- **One commit, one version** (B-001): the same commit from the same ref with full history computes the same version on any machine. CI has that history without a workflow change: [`.build/Build.GitHubActions.cs`](../../Build.GitHubActions.cs) `Middleware` sets the checkout's `FetchDepth = 0` and adds a `git fetch --prune` step, so the generated [`ci.yml`](../../../.github/workflows/ci.yml) checks out with `fetch-depth: 0` (C-2). A shallow clone has no version at all: `nbgv` stops with `GitException: Shallow clone lacks the objects required to calculate version height`, so C-2 is enforced by the build failing, not by a wrong number.
- **The release tag** (A-1, B-007): `dotnet nbgv tag` on a commit of `main` creates `v<version>` at that commit from `release.tagName`. It creates the tag locally only; pushing it is the maintainer's step, and publishing on the tag is `0055-F6`.
- **One source** (C-3): no `<Version>` is set in `Directory.Build.props` or any project file, and no workflow passes one in.
- **Verified by hand** when 0079 was built, 2026-10-09. Nothing here has a test yet (§ 8).

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-09 -->

- **The build has no tests (owner, 2026-10-08), and versioning is the build's.** That decision is recorded in [`0055-F2` § 8](../../ContinuousIntegration/.spec/README.md) and applies here unchanged. `versioning.feature` is not linked into `test/specht.acceptance`, and no step class or unit test exists for it. Every § 9 row is therefore `Missing`: nothing pins these claims.
- **Mechanisms, per claim.** Read in [`version.json`](../../../version.json) and [`.config/dotnet-tools.json`](../../../.config/dotnet-tools.json). None has a test, by the owner's decision above.
  - B-001, B-002: no mechanism of their own. Nerdbank.GitVersioning derives the version from `version.json`'s `version` and the git height of the commit, so both follow from the tool, not from code here.
  - B-003, B-004: a literal setting. `publicReleaseRefSpec` names `^refs/heads/main$` and `^refs/tags/v\d+(?:\.\d+)*$` and nothing else; every other ref gets the `-g<commit>` suffix.
  - B-007: a literal setting. `release.tagName` is `v{version}`, and the local tool manifest pins `nbgv` 3.10.94 with `rollForward: false`.
  - B-008: a literal setting. `version` is `0.1`.
  - B-005 and B-006: items 0080 and 0081; not verified here.
- **Verified by hand on 2026-10-09, for 0079.**
  - B-004, B-008: on branch `0079/nbgv-version`, `dotnet nbgv get-version` gave NuGetPackageVersion `0.1.1-g7a88c23105`.
  - B-003: with `GITHUB_ACTIONS=true` and `GITHUB_REF=refs/heads/main`, and again with `refs/tags/v0.1.1`, it gave `0.1.1`. With `refs/pull/9/merge` and `refs/heads/feature/x` it gave `0.1.1-g7a88c23105`.
  - B-001: two fresh full clones of the branch both gave `0.1.1-g7a88c23105` at `7a88c23`.
  - B-002: in a throwaway clone, an empty commit on a local `main` took the version from `0.1.1` to `0.1.2`.
  - B-007: `dotnet nbgv tag` there created `v0.1.2` at HEAD, equal to the computed SimpleVersion. The clone was deleted and nothing was pushed.
  - Those results describe the code as it was then. Nothing re-checks them.
- **Verdict.** Every claim built here rests on a literal setting in `version.json` or on Nerdbank.GitVersioning itself; there is no code in this repository beneath them to unit-test. Each is testable as it stands, the way the hand checks ran: a temporary git repository with `version.json` committed, `dotnet nbgv get-version` run under a set `GITHUB_REF`. No claim has a test because the owner decided the build needs none, not because the design prevents it.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-08 -->

| Claim ID | Scenario                                           | Test    | Status  |
| -------- | -------------------------------------------------- | ------- | ------- |
| B-001    | One commit has one version everywhere              | Missing | Missing |
| B-002    | A later commit has a higher version                | Missing | Missing |
| B-003    | A main-branch build is a public version            | Missing | Missing |
| B-004    | A branch build is a prerelease                     | Missing | Missing |
| B-005    | The package carries the computed version           | Missing | Missing |
| B-006    | A package version change leaves the schema version | Missing | Missing |
| B-007    | The release tag is made from the computed version  | Missing | Missing |
| B-008    | The first version line is 0.1                      | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-08 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                           | Blocks       | Resolution                                                       |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------ | ---------------------------------------------------------------- |
| OQ-1 | What version does `version.json` start at? The first publish freezes schema version 1 (`0001-F7` decision 0002) whatever the package version is (C-1); whether that publish is `0.x` or `1.0` is the owner's call. | B-002, B-007 | Resolved 2026-10-08 by the repository owner: `0.1`. B-008 added. |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-09 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| ------- | ------ | ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Draft; not reviewed                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| 1-5     | 🔴     | spec-reviewer | Round 1 (2026-10-08) - blocked, reviewing commit `04fdc28`. Decision 0001, OQ-1 and C-1 are stated faithfully, and C-1 agrees with `0001-F7`. Blocking (spec-author): B-001 says one commit computes the same version on any two machines, with no condition on the ref, while B-003 and B-004 give the same commit a public version from `main` and a prerelease from any other ref; state B-001 for the same commit built from the same ref. Non-blocking (spec-author): B-005 tests `0055-F1`'s `Pack` but `depends_on` is empty; decision 0001's Affects stops at B-007.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| 1-5     | 🟢     | spec-reviewer | Round 2 (2026-10-08) - approved, reviewing `acf3cff`..`f32a31d`. B-001 is now the same commit from the same ref, agreeing with B-003 and B-004; `depends_on` names `0055-F1` (B-005); decision 0001's Affects is complete.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| Tasks   | 🟢     | spec-reviewer | Item cut (2026-10-08) - approved, reviewing `6113181`..`0657b68`. 0079-0081 carry B-001-B-008 once. 0081 on 0049 is real: B-006 asserts the `schemaVersion` `specht init` writes, which `0001-F7` B-012 (0049) defines and B-006 cites; holding only the test keeps the release path (0080, 0083) off epic 0001.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| Tasks   | 🟢     | spec-reviewer | Round 3 (2026-10-09) - approved, reviewing `main`...`6edec11` (item `0079`). Every commit cites `Delivers 0079`, the two behaviour commits B-001-B-004, B-007, B-008. Re-checked at `6edec11`: `version` `0.1` (B-008); under `GITHUB_REF` `refs/heads/main` and `refs/tags/v0.1.2` the version is `0.1.2`, under `refs/pull/9/merge`, `refs/heads/feature/x`, `refs/heads/mainline` and `refs/tags/version1` it is `0.1.2-g6edec1167f`, so `publicReleaseRefSpec` names `main` and `v*` tags and nothing else (A-2, B-003, B-004); `release.tagName` `v{version}` (A-1, B-007); no `<Version>` in any props, project or workflow (C-3); `ci.yml` checks out with `fetch-depth: '0'` (C-2). B-005, B-006 and C-1 are left to 0080 and 0081, as § 7 and the item say; nothing in § 5 is delivered. The § 9 rows stay `Missing` and the item's § 9 criterion is superseded by the owner's call (`0055-F2` § 8), as for 0073. Non-blocking (implementer): § 7 and `specht-conventions` say a shallow clone computes a different version, but `nbgv get-version` on a `--depth 1` clone of this branch throws `GitException: Shallow clone lacks the objects required to calculate version height` - C-2 is enforced by a failure, not a wrong number; say so in both. Non-blocking (implementer): the diff drops `.config/dotnet-tools.json`'s final newline against `.editorconfig` `insert_final_newline = true`. Non-blocking: the frontmatter `updated` stays `2026-10-08` though §§ 6-8 and 12 changed on 2026-10-09. |

## Tasks

Cut 2026-10-08 into [`../.issue/`](../.issue/): `0078` (the Feature), with `0079` to `0081`.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
