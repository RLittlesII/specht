---
title: "Specification: Repository settings checklist"
description: "A checked-in checklist of the GitHub repository settings that make the checks binding - branch protection, required checks, merge methods, auto-merge, secrets and app installations - applied by hand once after the first push"
type: feature
id: "F8"
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
depends_on: ["F2", "F3", "F4"]
blocks: []
spikes: []
created: "2026-10-08"
updated: "2026-10-25"
github_issue: null
synced_at: null
---

# Specification: Repository settings checklist

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-08 -->

A workflow that runs is not a gate until the repository requires it: without branch protection a red pull request merges, without the merge methods AGENTS.md § SDLC relies on a squash drops the commit-message citations rule 3 depends on, and without the secret and app installations the coverage gate and dependency updates never report. None of that lives in a file, so after the first push it would be set from memory, or not at all. This Feature removes that failure state: one checked-in checklist names every repository setting the other Features need, it is applied by hand once after the first push, and a test keeps its required checks in step with the checks CI produces.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Persona                                    | Need                                                                   | Pain Point Today              |
| --- | ------------------------------------------ | ---------------------------------------------------------------------- | ----------------------------- |
| 1   | The maintainer, right after the first push | Every setting to apply, in one list, in one sitting                    | Settings exist only in memory |
| 2   | A reviewer of a settings change            | The settings as a reviewable file                                      | A settings page has no diff   |
| 3   | `0055-F2`, `0055-F3`, `0055-F4`            | Their checks required, their secret present, their app installed       | N/A (internal dependency)     |
| 4   | AGENTS.md rule 3                           | Squash built from commit messages, and rebase, as the only merge paths | Not configured                |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                                                                                      |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The checklist sits beside this specification under `.github/settings/`; the `implementer` may move both together.                                                                                                                                                                                               |
| A-2 | `github_mode` stays `false`: GitHub issues are not the tracker, and the issue templates under `.github/ISSUE_TEMPLATE/` stay for outside reports only (owner, 2026-10-08). The read-only mirror of the work items as issues is `0055-F9`, and it is not a tracker (owner, 2026-10-24; `0055-F9` decision 0001). |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                                                                        | Source                                                                        | Status  |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------- | ------- |
| B-001 | Given the checklist, it requires a pull request before any change reaches `main`.                                                                                                                            | owner, 2026-10-08                                                             | Active  |
| B-002 | Given the checklist and the CI workflow, the status checks the checklist requires from CI are exactly the per-operating-system checks the workflow produces.                                                 | owner, 2026-10-08; `0055-F2` B-003, B-007                                     | Active  |
| B-003 | Given the checklist, it requires no coverage status, neither the patch status nor the project status.                                                                                                        | owner, 2026-10-08; `0055-F2` OQ-2 (owner, 2026-10-08); `0055-F3` B-003, B-005 | Amended |
| B-004 | Given the checklist, it enables squash merging with the commit messages as the squashed body.                                                                                                                | AGENTS.md § Specification-Driven Development rule 3                           | Active  |
| B-005 | Given the checklist, it enables rebase merging and disables merge commits.                                                                                                                                   | AGENTS.md rule 3; `specht-conventions` § Delivery                             | Active  |
| B-006 | Given the checklist, it enables merging a pull request automatically once its required checks pass.                                                                                                          | owner, 2026-10-08; `0055-F4` B-005                                            | Active  |
| B-007 | Given the checklist and the committed workflows, every secret a workflow reads - the Codecov token and the Renovate workflow's token among them - is named in the checklist with the workflow that reads it. | owner, 2026-10-08; `0055-F3` C-2; `0055-F4` C-8                               | Amended |
| B-008 | Given the checklist, it names the Codecov installation and no Renovate app installation, because Renovate runs as this repository's own workflow.                                                            | `0055-F3`; `0055-F4` OQ-6 (owner, 2026-10-08)                                 | Amended |
| B-009 | Given the checklist, it keeps GitHub issues enabled, for outside reports through the issue templates.                                                                                                        | owner, 2026-10-08; A-2                                                        | Active  |
| B-010 | Given the checklist, it sets the repository's visibility to public.                                                                                                                                          | OQ-1 (owner, 2026-10-08)                                                      | Active  |
| B-011 | Given the checklist, its protection of `main` requires no approving review; the required checks are the gate.                                                                                                | OQ-2 (owner, 2026-10-08)                                                      | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-08 -->

| ID  | Constraint                                                                             | Rules Out                                                                                                  |
| --- | -------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| C-1 | The settings are a checked-in document applied by hand (decision 0001).                | A script calling the GitHub API; a settings-as-code app; a setting recorded nowhere but the settings page. |
| C-2 | The checklist names secrets, never their values.                                       | A token, or any part of one, in the checklist or its history.                                              |
| C-3 | The checklist is the record of the settings: a setting changes in the checklist first. | A setting changed on GitHub that the checklist does not show.                                              |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-25 (item 0300) -->

| #   | Item                                                                     | Exclusion Reason                                                                                                                                           |
| --- | ------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | GitHub issues, labels and milestones as a tracker                        | `github_mode: false` (AGENTS.md § SDLC); work is tracked in `.issue/` items. GitHub issues and milestones are `0055-F9`'s read-only mirror, not a tracker. |
| 2   | Turning an outside report into work                                      | A maintainer writes a `.issue/` item from it by hand; nothing here automates that.                                                                         |
| 3   | Producing the checks, the coverage statuses and the update pull requests | `0055-F2`, `0055-F3`, `0055-F4`.                                                                                                                           |
| 4   | Verifying that GitHub's live settings match the checklist                | C-1: no call to the GitHub API; the tool and its tests stay offline (AGENTS.md § Invariants).                                                              |
| 5   | Package feed permissions for consumers                                   | `0055-F7`.                                                                                                                                                 |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-28 (item 0090) -->

| #   | Concern                                                                                                         | Classification |
| --- | --------------------------------------------------------------------------------------------------------------- | -------------- |
| 1   | What makes a check binding on `main`: a pull request first, the required checks, no approving review            | Business       |
| 2   | Which statuses are reported but not required: the two coverage statuses                                         | Business       |
| 3   | The merge paths rule 3 relies on: squash built from the commit messages, rebase, no merge commit                | Business       |
| 4   | Merging a pull request automatically once its required checks pass                                              | Business       |
| 5   | Naming each setting as GitHub's settings pages name it, so the maintainer applies it by hand in one sitting     | Both           |
| 6   | Recording every live setting in the checklist's area, claimed or not, so none lives only on GitHub (C-3)        | Both           |
| 7   | A shape a test reads without interpretation: one field and value per row, one row per required check            | Technical      |
| 8   | Deriving the checks a pull request's run produces from the generated integration workflow, as GitHub names them | Technical      |
| 9   | Keeping the checklist out of the specification tree the self-check reads                                        | Technical      |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-28 (item 0090) -->

Delivered by item `0090`. Item `0091` adds the secrets, installations, issues and visibility sections (B-007 to B-010) to the same file; `0090` writes no heading or placeholder for them. There is no production code: the deliverable is one document, and the test that reads it is the `test-writer`'s.

- **The file.** [`../checklist.md`](../checklist.md), beside this specification (A-1; neither moves). Its frontmatter is `title`, `description` and `type: reference`, the type the reference pages under `docs/` carry, and nothing else. It is not a specification: the manifest's layouts discover only `**/.spec/README.md` and `epics/**/spec.md`, and its item shape `{task}-*.md` needs an item id, so `./build.sh Specht` never reads it.
- **Protection is a ruleset.** `main` is protected by the repository ruleset `main`, not by classic branch protection, which reports the branch unprotected (observed 2026-10-28). The checklist records the ruleset as the ruleset page names its parts.
- **Order and sections.** The ruleset first, then Settings › General › Pull Requests, so the ruleset's claims come first: B-001, B-011, B-002, B-003, then B-004 to B-006. A short preamble says that order, that a setting changes in the checklist first (C-3), and how `Field` names a setting. One `##` section per claim, headed with the setting's name as GitHub shows it and the claim id in parentheses, then one section for what C-3 records without a claim:

  | Section                                           | Rows (`Field` = `Value`)                                                                                                                                                                                                                                                                                                     |
  | ------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
  | Require a pull request before merging (B-001)     | `ruleset.pull_request` = `true`                                                                                                                                                                                                                                                                                              |
  | Required approvals (B-011)                        | `ruleset.pull_request.required_approving_review_count` = `0`; one line says the required checks are the gate                                                                                                                                                                                                                 |
  | Require status checks to pass (B-002)             | `ruleset.required_status_checks` = `true`, and the required-checks table                                                                                                                                                                                                                                                     |
  | Coverage statuses are not required (B-003)        | none; prose naming `codecov/patch` and `codecov/project` as reported by the coverage service (`0055-F3` B-003) and never added to the required checks                                                                                                                                                                        |
  | Allow squash merging (B-004)                      | `allow_squash_merge` = `true`; `squash_merge_commit_title` = `COMMIT_OR_PR_TITLE`; `squash_merge_commit_message` = `COMMIT_MESSAGES`. The dropdown's option labels are not in GitHub's documentation (checked 2026-10-28), so the section describes the format to pick by what it produces instead of naming an option label |
  | Allow rebase merging; Allow merge commits (B-005) | `allow_rebase_merge` = `true`; `allow_merge_commit` = `false`                                                                                                                                                                                                                                                                |
  | Allow auto-merge (B-006)                          | `allow_auto_merge` = `true`                                                                                                                                                                                                                                                                                                  |

  The last section, Recorded without a claim (C-3), holds the rest of the ruleset and one repository field: `ruleset.name` = `main`; `ruleset.target` = `branch`; `ruleset.enforcement` = `active`; `ruleset.conditions.ref_name.include` = `~DEFAULT_BRANCH`; the one bypass actor as `ruleset.bypass_actors.actor_type` = `RepositoryRole`, `ruleset.bypass_actors.actor_id` = `5` (Admin) and `ruleset.bypass_actors.bypass_mode` = `always`; the rules `ruleset.deletion`, `ruleset.non_fast_forward`, `ruleset.required_linear_history` and `ruleset.required_signatures` = `true`; the other `ruleset.pull_request.*` parameters (stale-review dismissal, code-owner review and last-push approval `false`, required reviewers `none`, thread resolution and extra approval for unattributed changes `true`, allowed merge methods `squash, rebase`); `ruleset.required_status_checks.strict_required_status_checks_policy` = `true` and `ruleset.required_status_checks.do_not_enforce_on_create` = `false`; and `delete_branch_on_merge` = `true`.

  Every value is what the repository showed on 2026-10-28 through read-only API calls, and every claimed value already holds there, so applying the checklist changes nothing today. The last section is C-3's record, not a claim: no test reads it, a reviewer sees it change, and promoting or changing one of its settings is the `spec-author`'s and the owner's call.

- **The settings table.** Each section's settings are one pipe table with the header `Setting | Field | Value`. `Setting` is the label on GitHub's page, for the hand that applies it, or a short description where the page gives a field no label of its own (the squash title and body, each part of the bypass actor). `Field` is the REST name in a code span: a repository field bare (`allow_squash_merge`); a ruleset field as `ruleset.<field>`; a rule as `ruleset.<rule type>`, whose `Value` `true` means the ruleset holds that rule; a rule parameter as `ruleset.<rule type>.<parameter>`. `Value` is the REST value in a code span, a list as its items joined by `, `, and an empty list the plain text `none`. A `Field` appears once in the file, so every settings table folds into one map.
- **The required-checks table.** B-002's section holds the one pipe table with the header `Check | Source | Integration id`, one row per required check: the context in a code span, the source app as the ruleset page names it, and the REST `integration_id`. Today it has one row: `build`, `GitHub Actions`, `15368`. Binding the check to the app means a status another app posts under the same name does not satisfy it.
- **B-002's derivation: the checks a pull request's run produces.** They come from [`ci.yml`](../../workflows/ci.yml) alone, read with YamlDotNet's representation model as the mirror workflow's tests read theirs. It is the only workflow a pull request starts: `main` and `mirror` run on a push to `main`, `release` on a tag or by hand, `renovate` on a schedule or by hand (observed 2026-10-28). A check only `main.yml` produces never reports on a pull request, and requiring it would block every merge, so `build (self-hosted, macOS, ARM64)` and `build (windows-latest)` are not candidates. For each entry under `jobs`, the check is named by the job's `name` when it has one, and otherwise by its key, without the workflow's name: the pull request page shows `ci / build (pull_request)`, and the context is `build`. Under `strategy.matrix` GitHub appends ` (<v1>, <v2>, …)`, the combination's values in axis order with a list value's items joined by `, `; `main.yml`'s `os` axis gives `build (self-hosted, macOS, ARM64)` and `build (windows-latest)`, observed on `main` at `99a2d228`. `ci.yml` has no matrix today, its `strategy` holding only `fail-fast: false`, so the derivation gives exactly `build`, the one check from the integration app on pull request #599 beside `codecov/patch` and `codecov/project`. The derivation handles that case and no other. A job with a `matrix`, or a `name` holding `${{`, fails the test naming the job: the file alone would not settle the check's name, and a guess could pass while the required check never reports. Expanding a matrix waits for a `ci` with one (`0055-F2` OQ-5).
- **The comparison.** B-002: the `Check` values of the rows whose `Source` is `GitHub Actions` equal the derived set. B-003: no `Check` is `codecov/patch` or `codecov/project`. B-001, B-004 to B-006 and B-011 read their fields from the map.
- **The test seam.** The path `.github/settings/checklist.md`; Markdig with `UseYamlFrontMatter()` and `UsePipeTables()`, the pipeline [`RulePage`](../../../test/tests/Rules/RulePage.cs) builds; tables recognised by their header cells; a cell's value is its code span's content, or its text where it has none. `build.tests` reaches Markdig only through its reference to `src/tool`.
- **When the test runs.** A pull request that changes only the checklist changes one `*.md` file outside any `.spec/` folder, so the integration workflow runs no gate on it (`0055-F2` B-012): a checklist edit that breaks B-002 is caught by the build on the push to `main`, not before the merge. A change to a workflow is not Markdown, so it is built and the test runs before the merge. That path rule is `0055-F2`'s; this design records it and changes nothing.
- **Applying it.** The maintainer applies the checklist by hand (decision 0001), top to bottom: Settings › Rules › Rulesets › `main` first, then Settings › General › Pull Requests. Nothing here reads or writes GitHub (§ 5 #4).

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-28 (item 0090) -->

- **What the tests read.** The claims are values written in a committed document, [`../checklist.md`](../checklist.md), and B-002 compares it with the committed integration workflow, [`ci.yml`](../../workflows/ci.yml). The tests read both files and run nothing. [`SettingsChecklistIntegrationTests`](../../../test/build.tests/SettingsChecklist.Integration.Tests.cs), tier Integration because it reads committed files, follows `MirrorWorkflowIntegrationTests` in `test/build.tests` and [`0055-F9` § 8](../../mirror/.spec/README.md) "The generated workflow": one test per claim, named for it, each parsing the files it needs. No owner ruling removes this Feature's tests: § 1 says a test keeps the required checks in step with the checks CI produces, and item `0090` requires a § 9 row naming a test per claim.
- **No Reqnroll binding.** `repository-settings.feature` is not linked into `test/acceptance`. The tool's acceptance tier runs the `specht` engine and commands over synthetic trees; these scenarios exercise neither, and a step class would only call the same readers the tests call, a second test of each claim through a different arrangement. The workflow Features follow the same choice (`0055-F9` § 8). Each test is named for its claim, and § 9 cites it.
- **The seam the tests read** (§ 7): the path `.github/settings/checklist.md`; Markdig with `UseYamlFrontMatter()` and `UsePipeTables()`; a table recognised by its header row, `Setting | Field | Value` or `Check | Source | Integration id`; a cell's value its first code span's content, or its text where it has none. Every settings table folds into one `Field` to `Value` map, and the test fails if a `Field` repeats. The checklist holds exactly one required-checks table. The derivation of B-002 reads `ci.yml` with YamlDotNet's representation model: for each entry under `jobs`, the job's `name` or else its key; a job whose `strategy` holds `matrix`, or whose `name` holds `${{`, fails the test naming the job. The parsing lives in the test class, used by it alone; there is no production code to reference.
- **Mechanism and test, per claim.** No claim has a mechanism in code: the checklist is a document applied by hand (C-1), and the generator in [`SpechtBuild.GitHubActions.cs`](../../../.build/SpechtBuild.GitHubActions.cs) (the `ci` `GitHubActionsSteps` attribute, one image, no matrix) emits the one job `build` with no `name`. What each test pins is the value the claim states.
  - B-001: the field `ruleset.pull_request` is `true`. `B001_Checklist_WhenRead_ShouldRequireAPullRequestBeforeAnyChangeReachesMain`.
  - B-002: a set equality, the one derivation in the Feature. The `Check` values of the rows whose `Source` is `GitHub Actions` are unique and equal the checks derived from `ci.yml`, today `build`. `B002_ChecklistAndIntegrationWorkflow_WhenCompared_ShouldRequireExactlyTheChecksTheWorkflowProduces`. The derivation is test code, so it has no unit test of its own; a matrix or an expression fails the test rather than pass on a guess (§ 7).
  - B-003: no row of the required-checks table, whatever its `Source`, has `Check` `codecov/patch` or `codecov/project`. `B003_Checklist_WhenRead_ShouldRequireNoCoverageStatus`.
  - B-004: `allow_squash_merge` is `true` and `squash_merge_commit_message` is `COMMIT_MESSAGES`. The commit title is not claimed and not asserted. `B004_Checklist_WhenRead_ShouldEnableSquashMergingWithTheCommitMessagesAsTheBody`.
  - B-005: `allow_rebase_merge` is `true` and `allow_merge_commit` is `false`. `B005_Checklist_WhenRead_ShouldEnableRebaseMergingAndDisableMergeCommits`.
  - B-006: `allow_auto_merge` is `true`. `B006_Checklist_WhenRead_ShouldEnableAutoMerge`.
  - B-011: `ruleset.required_status_checks` is `true` and `ruleset.pull_request.required_approving_review_count` is `0`, the scenario's two steps. `B011_Checklist_WhenRead_ShouldRequireTheChecksToPassAndNoApprovingReview`.
  - B-007 to B-010: item `0091`'s; no test yet.
- **Red for the right reason, then green.** Before the checklist existed, all seven tests failed on the assertion that `.github/settings/checklist.md` exists; B-002 first derived `build` from `ci.yml` without failing. Run on `net11.0`, the project's highest target: 7 run, 7 fail, 0 error. With the checklist written, 7 run and 7 pass on `net11.0` and `net10.0`.
- **When they run.** In the build's Integration tier, on every pull request that builds and on the push to `main`. A pull request that changes only the checklist builds nothing (§ 7, `0055-F2` B-012), so a checklist edit that breaks a claim is caught on `main`.
- **§ 9 status.** B-001 to B-006 and B-011 are `Covered`: each test passes, and no claim rests on a mechanism in code that would need a unit test of its own (the checklist is a document; the one derivation is test code). `Partial` is for a mechanism whose unit tier is missing, which none of these has.
- **Testability, reported to `implementer` and `spec-author`.** The design is testable as § 7 states it. Three things no test here reaches, each left to the owner rather than decided: (1) B-001 says "any change reaches `main`", but the ruleset's bypass list, recorded under C-3 and read by no test, lets the Admin role bypass it `always`, so an administrator's push can reach `main` without a pull request; whether B-001 or the bypass gives way is the `spec-author`'s and the owner's. (2) B-001 and B-011 assert the ruleset's rules, not that it is `active` or that it targets `main`; those values sit in the unclaimed C-3 section. (3) B-004 and B-005 read the repository's merge settings; the ruleset's own allowed merge methods, also C-3, could narrow them without a test failing. Whether the live settings match the checklist is out of scope (§ 5 #4).

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-28 (item 0090) -->

| Claim ID | Scenario                                        | Test                                                                                                                                   | Status  |
| -------- | ----------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| B-001    | Main accepts changes only through pull requests | `SettingsChecklistIntegrationTests.B001_Checklist_WhenRead_ShouldRequireAPullRequestBeforeAnyChangeReachesMain`                        | Covered |
| B-002    | The required checks are the checks CI produces  | `SettingsChecklistIntegrationTests.B002_ChecklistAndIntegrationWorkflow_WhenCompared_ShouldRequireExactlyTheChecksTheWorkflowProduces` | Covered |
| B-003    | No coverage status is required                  | `SettingsChecklistIntegrationTests.B003_Checklist_WhenRead_ShouldRequireNoCoverageStatus`                                              | Covered |
| B-004    | A squash keeps the commit messages              | `SettingsChecklistIntegrationTests.B004_Checklist_WhenRead_ShouldEnableSquashMergingWithTheCommitMessagesAsTheBody`                    | Covered |
| B-005    | Rebase is allowed and merge commits are not     | `SettingsChecklistIntegrationTests.B005_Checklist_WhenRead_ShouldEnableRebaseMergingAndDisableMergeCommits`                            | Covered |
| B-006    | A green pull request can merge itself           | `SettingsChecklistIntegrationTests.B006_Checklist_WhenRead_ShouldEnableAutoMerge`                                                      | Covered |
| B-007    | Every secret a workflow reads is listed         | Missing                                                                                                                                | Missing |
| B-008    | The service installations are listed            | Missing                                                                                                                                | Missing |
| B-009    | Outside reports can still be filed              | Missing                                                                                                                                | Missing |
| B-010    | The repository is public                        | Missing                                                                                                                                | Missing |
| B-011    | Required checks are the gate, not a review      | `SettingsChecklistIntegrationTests.B011_Checklist_WhenRead_ShouldRequireTheChecksToPassAndNoApprovingReview`                           | Covered |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-08, 2026-10-23 (`0055-F2` decision 0005), 2026-10-24 (`0055-F9`) -->

- 2026-10-24, the owner decided GitHub issues become a read-only mirror of the work items ([`0055-F9` decision 0001](../../mirror/.spec/decisions/0001-work-items-stay-the-tracker-and-github-mirrors-them.md)). A-2 and § 5 row 1 now point to `0055-F9`; `github_mode` stays `false` and no claim here changes. B-009 is unchanged: issues stay enabled, which the mirror also needs. B-007 is unchanged: the mirror reads no secret, only the workflow's own token, whose `issues: write` and `pull-requests: write` its workflow grants in `permissions:` (`0055-F9` C-5), so no repository setting is toggled for it.

- 2026-10-23, the owner moved CI's Linux leg to the owner's self-hosted macOS ARM64 pool ([`0055-F2` decision 0005](../../../.build/ContinuousIntegration/.spec/decisions/0005-the-unix-leg-runs-on-the-owners-macos-pool.md)). B-002 is unchanged: the checklist requires the per-operating-system checks the workflows produce, whatever their names. The `main` workflow's Unix check is renamed once, from `build (ubuntu-latest)` to the name GitHub derives from the matrix value `[self-hosted, macOS, ARM64]`; the `ci` workflow's `build` and the `main` workflow's `build (windows-latest)` are unchanged. Where branch protection requires `build (ubuntu-latest)`, the owner replaces it by hand (C-1, C-3), and items 0090 and 0264 name the check as observed.

- 2026-10-08, B-003 amended from requiring the patch coverage status to requiring no coverage status (`0055-F2` OQ-2, owner): a pull request `0055-F2` B-012 skips uploads no coverage, so a required patch status would never report on it. The patch status is still reported (`0055-F3` B-003, B-004).

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                      | Blocks       | Resolution                                                                                                       |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------ | ---------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Is the repository public or private? It decides whether Codecov needs a token for pull requests from forks, whether outside reports are possible at all, and what reading the published package requires.     | B-007, B-009 | Resolved 2026-10-08 by the repository owner: public. B-010 added.                                                |
| OQ-2 | Does branch protection require an approving review? With one maintainer, a required review blocks self-merging, and a dependency update merged without a person (`0055-F4` B-005) must be exempt.             | B-001, B-006 | Resolved 2026-10-08 by the repository owner: no approving review; the required checks are the gate. B-011 added. |
| OQ-3 | `0055-F2` B-012 skips the coverage upload on a pull request that changes only Markdown outside `.spec/`, so the patch status B-003 requires never reports on one. Asked and answered once, as `0055-F2` OQ-2. | B-003        | Resolved 2026-10-08 by the repository owner (`0055-F2` OQ-2): the patch status is not required. B-003 amended.   |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-28 (item 0090) -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| ------- | ------ | ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟢     | spec-reviewer | Round 2 (2026-10-08) - approved, reviewing `acf3cff`..`f32a31d`. B-007 and B-008 follow `0055-F4` OQ-6 and C-8 (the Renovate token secret, no app installation); `depends_on` names `0055-F4`, symmetric with its `blocks`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| Tasks   | 🟢     | spec-reviewer | Item cut (2026-10-08) - approved, reviewing `6113181`..`0657b68`. 0090 and 0091 carry B-001-B-011 once, each waiting on the items that produce the checks and secrets it names (0064, 0071; 0070, 0073).                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| 6-9     | 🟢     | spec-reviewer | Item 0090 (2026-10-28) - approved, reviewing the uncommitted diff on `0090/protection-and-merging` over `99a2d228`. `checklist.md` states B-001 to B-006 and B-011 and records the rest under C-3; every value matches the live ruleset `main` and repository settings read 2026-10-28; `ci.yml`'s one job `build` equals the required check; § 9 cites one Integration test per claim. Non-blocking: the Admin bypass `always` and the thread-resolution and unattributed-change parameters, all under C-3, sit beside B-001 and B-011 - the spec-author's and the owner's call; a checklist-only pull request reports `build` with its gates skipped (`0055-F2` B-012), so B-002 is checked on `main`; B-001, B-004 to B-006 and B-011 tests share one shape (test-writer); frontmatter `updated` is unchanged. |

### Review history

Earlier review rows are preserved verbatim; later approvals and the current record supersede them.

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| ------- | ------ | ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Draft; not reviewed                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| 1-5     | 🟡     | spec-reviewer | Round 1 (2026-10-08) - reviewed in draft, reviewing commit `04fdc28`; no blocking findings of its own. B-001 to B-011 state the owner's settings and OQ-1 and OQ-2 faithfully; B-002 and B-003 agree with `0055-F2` B-003 and B-007 and `0055-F3` B-003 and B-005; B-011 is what `0055-F4` B-005 needs. B-007 and B-008 wait on `0055-F4` OQ-6: a self-hosted Renovate has no app installation and reads a token secret. Non-blocking (spec-author): B-006 and B-008 serve `0055-F4`, and neither Feature lists the edge; B-005 carries two assertions; with B-003 required, a pull request whose upload failed (`0055-F3` B-007) waits for a re-run, as `0055-F3` OQ-2 intends - one line in the checklist would say so. Left `draft`: the spec-author has not submitted it, and epic `0055` says its eight-Feature split awaits the owner's confirmation. |

## Tasks

Cut 2026-10-08 into [`../.issue/`](../.issue/): `0089` (the Feature), with `0090` to `0091`.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
