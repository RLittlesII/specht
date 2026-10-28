---
title: "Repository settings checklist"
description: "The GitHub settings that make the checks binding on main - the main ruleset and the pull-request merge settings - applied by hand in one sitting"
type: reference
---

# Repository settings checklist

Apply these by hand, top to bottom: first Settings › Rules › Rulesets › `main`, then Settings › General › Pull Requests. Change a setting here first, then on GitHub (`0055-F8` C-3).

`Setting` is the label on the page. `Field` and `Value` are the REST names and values: a repository field bare, a ruleset field as `ruleset.<field>`, a rule as `ruleset.<rule type>`, where `true` means the ruleset holds the rule, and a rule parameter as `ruleset.<rule type>.<parameter>`.

## Require a pull request before merging (B-001)

| Setting                               | Field                  | Value  |
| ------------------------------------- | ---------------------- | ------ |
| Require a pull request before merging | `ruleset.pull_request` | `true` |

## Required approvals (B-011)

No approving review. The required checks are the gate.

| Setting            | Field                                                  | Value |
| ------------------ | ------------------------------------------------------ | ----- |
| Required approvals | `ruleset.pull_request.required_approving_review_count` | `0`   |

## Require status checks to pass (B-002)

| Setting                       | Field                            | Value  |
| ----------------------------- | -------------------------------- | ------ |
| Require status checks to pass | `ruleset.required_status_checks` | `true` |

Add each check below with its source. These are exactly the checks the `ci` workflow produces on a pull request.

| Check   | Source         | Integration id |
| ------- | -------------- | -------------- |
| `build` | GitHub Actions | `15368`        |

## Coverage statuses are not required (B-003)

The coverage service reports `codecov/patch` and `codecov/project` on pull requests. Do not add either to the required checks.

## Allow squash merging (B-004)

Under Allow squash merging, pick the default commit message format whose body is the pull request's commit messages: the commit's title and message for one commit, the pull request title and the list of commits for several.

| Setting                                    | Field                         | Value                |
| ------------------------------------------ | ----------------------------- | -------------------- |
| Allow squash merging                       | `allow_squash_merge`          | `true`               |
| Allow squash merging, default commit title | `squash_merge_commit_title`   | `COMMIT_OR_PR_TITLE` |
| Allow squash merging, default commit body  | `squash_merge_commit_message` | `COMMIT_MESSAGES`    |

## Allow rebase merging; Allow merge commits (B-005)

| Setting              | Field                | Value   |
| -------------------- | -------------------- | ------- |
| Allow rebase merging | `allow_rebase_merge` | `true`  |
| Allow merge commits  | `allow_merge_commit` | `false` |

## Allow auto-merge (B-006)

| Setting          | Field              | Value  |
| ---------------- | ------------------ | ------ |
| Allow auto-merge | `allow_auto_merge` | `true` |

## Recorded without a claim (C-3)

The rest of the live settings, so none lives only on GitHub. No claim states these; changing one is the specification's call first.

| Setting                                                          | Field                                                                  | Value             |
| ---------------------------------------------------------------- | ---------------------------------------------------------------------- | ----------------- |
| Ruleset name                                                     | `ruleset.name`                                                         | `main`            |
| Target                                                           | `ruleset.target`                                                       | `branch`          |
| Enforcement status                                               | `ruleset.enforcement`                                                  | `active`          |
| Target branches: include default branch                          | `ruleset.conditions.ref_name.include`                                  | `~DEFAULT_BRANCH` |
| Bypass list: role                                                | `ruleset.bypass_actors.actor_type`                                     | `RepositoryRole`  |
| Bypass list: role id, Admin                                      | `ruleset.bypass_actors.actor_id`                                       | `5`               |
| Bypass list: mode                                                | `ruleset.bypass_actors.bypass_mode`                                    | `always`          |
| Restrict deletions                                               | `ruleset.deletion`                                                     | `true`            |
| Block force pushes                                               | `ruleset.non_fast_forward`                                             | `true`            |
| Require linear history                                           | `ruleset.required_linear_history`                                      | `true`            |
| Require signed commits                                           | `ruleset.required_signatures`                                          | `true`            |
| Dismiss stale pull request approvals when new commits are pushed | `ruleset.pull_request.dismiss_stale_reviews_on_push`                   | `false`           |
| Required reviewers                                               | `ruleset.pull_request.required_reviewers`                              | none              |
| Require review from Code Owners                                  | `ruleset.pull_request.require_code_owner_review`                       | `false`           |
| Require approval of the most recent reviewable push              | `ruleset.pull_request.require_last_push_approval`                      | `false`           |
| Require conversation resolution before merging                   | `ruleset.pull_request.required_review_thread_resolution`               | `true`            |
| Extra approval for unattributed changes                          | `ruleset.pull_request.require_extra_approval_for_unattributed_changes` | `true`            |
| Allowed merge methods                                            | `ruleset.pull_request.allowed_merge_methods`                           | `squash, rebase`  |
| Require branches to be up to date before merging                 | `ruleset.required_status_checks.strict_required_status_checks_policy`  | `true`            |
| Do not require status checks on creation                         | `ruleset.required_status_checks.do_not_enforce_on_create`              | `false`           |
| Automatically delete head branches                               | `delete_branch_on_merge`                                               | `true`            |
