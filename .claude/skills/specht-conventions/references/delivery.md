---
title: Delivery in specht
description: The GitHub-issue tracker, the status labels and what mirrors from them, branch and pull-request conventions, and why a claim citation belongs in the commit message
type: reference
---

# Delivery

Extends [`deliver-change`](../../deliver-change/SKILL.md).

## The tracker is GitHub issues

There are no local work-item files. A Feature's specification lives in the
repository; its delivery state lives on the issue.

```
specification  →  grooming  →  GitHub issues  →  scenarios  →  tests  →  implementation  →  PR
```

The specification exists first and stands alone — issues are cut from its § 3
claims after agreement. A bug, spike or chore starts at the issue instead.

Issue templates: `.github/ISSUE_TEMPLATE/{feature,task,bug}.yml` (scaffold,
README § 8 step 1).

## Each step of README § 8 is a pull request

The order of work is fixed and each step is one pull request whose description
names the README § 2 requirement it serves. Step 2 — copying the engine — is
tagged when it merges: that commit is the behaviour baseline every later step
is measured against (Should-7), and a diff of `src/Specht` against `hooked` at
that tag must show only the namespace rename.

## Status labels

`status:in-progress` is the only signal an item is taken. **Apply it before the
branch, before the worktree, and before the first edit**, and never pick up an
issue that already carries it.

| Label                              | Means                                                     |
| ---------------------------------- | --------------------------------------------------------- |
| `status:needs-decomposition`       | Epic or Feature not yet split into single-responsibility children |
| `status:ready-for-architecture`    | child issue exists, single-responsibility, awaiting design |
| `status:ready-for-implementation`  | design and scaffold in place, safe to implement            |
| `status:ready`                     | dependencies resolved, pick-up-able                        |
| `status:in-progress`               | taken                                                      |
| `status:in-review`                 | PR open, awaiting a reviewer                                |
| `status:blocked`                   | a blocker must be resolved first                            |

The design sections are owned by [`implementer`](../../../.agents/implementer.md);
a label description that names any other role is historical text, and the role
contract is authoritative.

There is **no `priority:*` label.** `priority` is authored in the specification's
frontmatter, permanently, and nothing mirrors it.

## What mirrors, and what never does

| Field          | Source                               | Rule                                        |
| -------------- | ------------------------------------ | ------------------------------------------- |
| `status`       | the issue's `status:*` label          | mirrored once `github_issue` is set; never hand-edited after |
| `synced_at`    | the mirror run                        | set whenever `status` is mirrored            |
| `priority`     | the specification's frontmatter        | authored here, permanently                   |
| `rank`, `blocks` | `value`, `risk`, the dependency graph | derived — recompute, never hand-edit       |

A set `github_issue` with no `synced_at` is a `specht` error (`SPEC005`).

## One at a time

One issue `in-progress` at a time, per Feature. A task that cannot be finished
without a sibling finishing first is a decomposition problem, not a small task —
take it back to grooming rather than stacking.

Branch from `main`. Branch names are prefixed by kind — `grooming/`, `spike/`,
`process/`, `feat/`, `fix/`. Stack only one level deep, and retarget the dependent
pull request to `main` as soon as its base merges. `deleteBranchOnMerge` is on.

## Cite the claim in the commit message

The repository allows **squash** and **rebase** merges and no merge commits.
Squash is configured `COMMIT_OR_PR_TITLE` + **`COMMIT_MESSAGES`** — the squashed
body is built from the commit messages, not the pull-request body.

So a citation that lives only in the pull-request body survives **neither** merge
path. Put it in the commit message:

```
fix: relativize the report path before writing it

Satisfies 0001-F1 B-012. Constraints preserved: 0001-F1 C-3.
```

The pull-request body is still where the reviewer reads what changed upstream —
it is just not where the citation can live alone.

## Exemptions

A behaviour change touching no scenario claims an exemption by citing the
constraints it preserves, in the fixed format:

```
No .feature change needed: refactor — extracted the discovery walk;
the same file set is returned in the same order and the report is unchanged.
Constraints preserved: <epic>-F<n> C-<n>, <epic>-F<n> C-<n>
```

Reaching for it to avoid writing a scenario is forbidden outright. Nothing
automates the check; review enforces it.

## Before opening the pull request

```sh
./build.sh Format      # verifies rather than fixes; CI runs SpecCheck, then this, before anything compiles
./build.sh SpecCheck   # if the change touched a specification, a schema or a template
./build.sh             # Compile + Test
```

Then open the pull request and **watch the checks**. A failing check is yours.

## Responding to review

One round is: commit, push, resolve. Each thread the commit addresses gets exactly
one reply, in the fixed format, and is then resolved:

```
addressed: <short sha>
```

No prose, no explanation, no thanks - the diff at that sha is the explanation,
and the reviewer reopens the thread if it is not. A thread you are not going to
act on is answered with why, and left open for the reviewer to close.

Threads are resolved through the GraphQL API; `gh pr comment` cannot reach them:

```sh
gh api graphql -f query='query { repository(owner:"<owner>", name:"<repo>") { pullRequest(number:<n>) {
  reviewThreads(first:50) { nodes { id isResolved path } } } } }'
gh api graphql -f query='mutation { addPullRequestReviewThreadReply(input:{pullRequestReviewThreadId:"<id>", body:"addressed: <sha>"}) { comment { id } } }'
gh api graphql -f query='mutation { resolveReviewThread(input:{threadId:"<id>"}) { thread { isResolved } } }'
```

## Never add

- A local work-item file as a tracker.
- A branch or an edit before `status:in-progress` is applied.
- A second issue in progress on the same Feature.
- A claim citation that exists only in the pull-request body.
- A `priority:*` label.
- A hand edit to `status` or `synced_at` after `github_issue` is set.
- A merge commit.
- A pull request that spans two steps of README § 8, or one whose description
  names no § 2 requirement.
- A review thread resolved before its commit is pushed, or with a reply that is
  not `addressed: <sha>`.
