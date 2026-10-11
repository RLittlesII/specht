---
title: Delivery in specht
description: The local .issue/ work-item tracker, the status vocabulary, which fields are authored and where rank is computed, branch and pull-request conventions, and why a claim citation belongs in the commit message
type: reference
---

# Delivery

Extends [`deliver-change`](../../deliver-change/SKILL.md).

## The tracker is local `.issue/` items

There are **no GitHub issues, labels or milestones** in this workflow. A
`<id>-<slug>.yml` work item stands in for an issue: the item owns delivery state,
the Feature's `.spec/README.md` owns content, and neither duplicates the other.
The schema, the status vocabulary and the rank definition live in
[`.spec/templates/item.yml`](../../../../.spec/templates/item.yml) — not restated here.

```
specification  →  grooming  →  .issue/ items  →  scenarios  →  tests  →  implementation  →  PR
```

The specification exists first and stands alone — a `type: feature` item is cut
from its § 3 claims after agreement, and cites the claim ids it delivers, never
their text. A bug, spike or chore starts at the item instead, with `spec: null`.

A `type: refactor` item starts at the item too. It changes structure, never
behaviour, so it carries `claims: []`, names in `spec:` the Feature whose code it
restructures (or null when it spans several), and its acceptance criteria cite the
constraints it preserves in the [exemption](#exemptions) form. A change that needs
a scenario amended is not a refactor.

### Where an item goes

| Item                                                    | Path                              |
| ------------------------------------------------------- | --------------------------------- |
| A Feature item, or a task, test or bug delivering one   | `<home>/.issue/`, beside `.spec/` |
| A refactor of one Feature's code                        | `<home>/.issue/`, beside `.spec/` |
| A bug, spike, chore or refactor belonging to no Feature | `.issue/` at the repository root  |

The same blast-radius split `.spec/adr/` and `.spec/lessons/` use. Find any item
with `**/.issue/<id>-*.yml`.

### Claiming an id

Ids are four digits, repository-wide, and share one number space with the epics
under `epics/`. `.issue/.sequence` holds the **last id claimed**: take the next
number and write it back in the same commit that adds the item. **Claim after
rebasing** — an id is taken the moment someone else merges it. `main` is not
the only claimant: an open pull request holds ids too, and
[Reserve an id before a branch claims it](#reserve-an-id-before-a-branch-claims-it)
says how to count them. Ids are never reused, and a closed item stays as
permanent history.

## Each step of brief § 8 is a pull request

The order of work is fixed and each step is one pull request whose description
names the brief § 2 requirement it serves. Step 2 — copying the engine — is
tagged when it merges: that commit is the behaviour baseline every later step
is measured against (Should-7), and a diff of `src/specht` against `hooked` at
that tag must show only the namespace rename.

## Status

`status: in-progress` is the only signal an item is taken. **Set it before the
branch, before the worktree, and before the first edit**, and never pick up an
item that already carries it. Bump `updated:` on every edit.

| Status                     | Means                                                             |
| -------------------------- | ----------------------------------------------------------------- |
| `needs-decomposition`      | Epic or Feature not yet split into single-responsibility children |
| `ready-for-architecture`   | child item exists, single-responsibility, awaiting design         |
| `ready-for-implementation` | design and scaffold in place, safe to implement                   |
| `ready`                    | dependencies resolved, pick-up-able                               |
| `in-progress`              | taken                                                             |
| `in-review`                | pull request open, awaiting a reviewer                            |
| `blocked`                  | a blocker must be resolved first                                  |
| `done`                     | delivered; `closed:` carries the date                             |

**`done` and `closed:` are written by the pull request that delivers the item,
before it is opened** — after the merge nobody is present to flip them. A pull
request closes every item its work finishes, not only the one it was cut for.

The design sections are owned by [`implementer`](../../../agents/implementer.md);
any text naming another role for them is historical, and the role contract is
authoritative.

## What is authored, and what is derived

| Field                        | Where             | Rule                                                               |
| ---------------------------- | ----------------- | ------------------------------------------------------------------ |
| `status`, `closed`           | the item          | authored; the item is the delivery record                          |
| `value`, `risk`              | the item          | authored; `risk` is never inherited from a parent                  |
| `priority`, `rank`, `blocks` | the item          | never stored; the queue script computes rank and priority each run |
| `claims`                     | the item          | ids only; the claim text lives in the spec's § 3                   |
| `priority`                   | the specification | authored in its frontmatter, permanently                           |
| `github_issue`, `synced_at`  | the specification | `null` — there is no issue to link and nothing to mirror from      |

An item's rank moves when any item in its Feature's closure changes `status`,
`risk` or `depends_on`, in any Feature. Nothing is recomputed by hand: the next
run of the script in [Choosing the next item](#choosing-the-next-item) reads
the change.

## One at a time

One item `in-progress` at a time, per Feature. A task that cannot be finished
without a sibling finishing first is a decomposition problem, not a small task —
take it back to grooming rather than stacking.

Branch from `main` as `<id>/<short-description>`, from the item id — or
`spec/<feature-slug>` when authoring a specification, which has no item to take
an id from.

## Reserve an id before a branch claims it

An id is permanent once it is on `main`. Before that it is a claim only its own
branch can see, and two branches cut from the same `main` take the same "next"
number. Git does not report it: two records sharing a number under different
file names merge cleanly
([lesson 0005](../../../../.spec/lessons/0005-an-id-is-not-reserved-until-it-merges.md)).

This covers every number a branch takes: a work item, a claim, constraint or
open question in an existing specification, and a `decisions/`, `adr/` or
`lessons/` file.

**Check before claiming.** Read what `main` holds _and_ what every open pull
request adds in that number space:

```sh
gh pr list --state open --json number,title,files
```

For an item, the next id is one above the highest id on `main` or in any open
pull request - not one above `.issue/.sequence` on `main`.

**Reserve by landing first.** `main` is protected, so a reservation is a pull
request, not a push: one that bumps `.issue/.sequence` and names the range and
what it is for in its commit message. It changes one line, so it merges before
the work starts. A session that cannot wait takes the range above every open
pull request's highest id, and says so in its pull-request description.

**One writer per specification.** Two branches that both amend the same
Feature's § 1-5 are sequenced, never run in parallel: the Feature's claim,
constraint, question and `decisions/` numbers have no sequence file, and one
writer is what reserves them. Work on different Features runs in parallel once
its item ids are reserved. The root `.spec/adr/` and `.spec/lessons/` numbers
are shared by every branch and are checked as above.

**When a clash is found anyway:**

1. The pull request that merges first keeps its ids. The unmerged one renumbers
   its own new ids before it merges, never after.
2. Renumber on the branch **before** merging `main`, while every such id in the
   tree is unambiguously the branch's own.
3. Follow every citation: specifications, `@B-` tags, items, `README.md` and
   decision records.
4. Leave a reviewer's earlier § 12 rows verbatim, for the reviewer to annotate.
5. Record the renumber in that specification's § 10.
6. State the new numbers in the merge commit's message and in the pull-request
   text - the squash is built from commit messages, and the earlier commits
   carry the old numbers.

**A stacked pull request whose base was squash-merged** is brought up to date by
merging `main`. When `main`'s tree equals what the branch already merged, that
merge changes no file.

## Choosing the next item

Extends [`next`](../../next/SKILL.md). One command reads every `.issue/` item
and prints the answer:

```sh
python3 .claude/skills/specht-conventions/scripts/next.py   # --top N, --width N, --root DIR
```

It prints a choice, not one item: first a "Pick one per lane" table, one row
per lane with its head, the items queued behind it and the write set it holds,
then the startable table, the blocked list, the "Blocked by status" list and
the taken list. Both tables carry a Toward column after Priority: the Feature
the rank comes from and, in brackets, the cost left in its closure, or `-` for
an item no Feature needs. A blocked line reads `- 0062 (medium 83) …`.

It needs nothing but Python 3's standard library and reads the tree it lives
in, so a worktree answers for its own branch. It walks hidden folders - most
items live under `.build/`, `.github/` and `.config/` - and strips a trailing
`# comment` from every field before comparing it.

| It treats              | As                                                                                        |
| ---------------------- | ----------------------------------------------------------------------------------------- |
| startable              | `ready`, `ready-for-architecture`, `ready-for-implementation`, every `depends_on` `done`  |
| taken                  | `in-progress`, `in-review`                                                                |
| a container            | any item another open item names as `parent` - scored as a Feature, never offered as next |
| a missing `depends_on` | a blocker, printed with a `?`                                                             |
| blocked by status      | `blocked`, with no open `depends_on`                                                      |

`rank` and `priority` are computed on every run from `value`, `risk`,
`depends_on`, `parent` and `status`, and stored nowhere; the definition stays
in [`.spec/templates/item.yml`](../../../../.spec/templates/item.yml).

**The product goal** is `.issue/.goal`, one item id per line: every open item
on those items' dependency path is `high`. It names Release (`0082`) and
Consuming the package (`0086`), the first NuGet release. Moving the finish line
is an edit to that file.

**Lanes.** Two startable items share a lane when they name the same `spec:`, or
when their homes fall under the same entry of `SHARED_WRITE_SETS` at the top of
the script: `.build/ContinuousIntegration`, `.build/Releasing` and `.github`
share the generated workflows (`.build/SpechtBuild.GitHubActions.cs` and the committed
`.github/workflows/`, which every such branch regenerates), and the rest of
`.build` shares `.build/SpechtBuild.cs`. A new generated workflow or a new hand-edited
build file adds an entry there in the pull request that creates it.

## Cite the item and the claim in the commit message

The repository allows **squash** and **rebase** merges and no merge commits.
Squash is configured `COMMIT_OR_PR_TITLE` + **`COMMIT_MESSAGES`** — the squashed
body is built from the commit messages, not the pull-request body.

So a citation that lives only in the pull-request body survives **neither** merge
path. The commit body opens with `Delivers <id>` — or `Specifies <feature path>`
when authoring a specification — and cites the claims it satisfies. There is no
issue for `Closes` to close.

The subject is `<type>(<id>): <summary>` — the delivered item's `type`
(`feature`, `task`, `test`, `bug`, `spike`, `refactor`) and its id, never a conventional-commit
type or a component scope (`feat(ci):`). The pull-request title takes the same
form, because a squash of more than one commit takes its subject from the title.

```
bug(0004): relativize the report path before writing it

Delivers 0004. Satisfies 0001-F1 B-012. Constraints preserved: 0001-F1 C-3.
```

A refactor satisfies no claim; its body carries the exemption instead:

```
refactor(0123): extract the discovery walk

Delivers 0123.
No .feature change needed: refactor — extracted the discovery walk;
the same file set is returned in the same order and the report is unchanged.
Constraints preserved: 0001-F1 C-9.
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

Set the item `in-review` — and `done` with its `closed:` date, if this pull
request finishes it — then open the pull request and **watch the checks**. A
failing check is yours.

## Responding to review

One round is: commit, push, resolve. Each thread the commit addresses gets exactly
one reply, in the fixed format, and is then resolved:

```
addressed: <short sha>
```

No prose, no explanation, no thanks - the diff at that sha is the explanation,
and the reviewer reopens the thread if it is not. A thread you are not going to
act on is answered with why, and left open for the reviewer to close.

## Never add

- A GitHub issue, label or milestone as a tracker.
- An item id not claimed from `.issue/.sequence`, or a reused one.
- An id claimed by reading `main` alone while a pull request is open.
- A second branch amending a Feature's § 1-5 while another is open on them.
- A renumber after the merge, or one made after merging `main` into the branch.
- A branch or an edit before the item is `in-progress`.
- A second item in progress on the same Feature.
- A `type: feature` item without `spec:` and the `claims:` it delivers.
- Claim text copied into an item — cite the id.
- A `priority`, `rank` or `blocks` field stored on an item.
- A deleted item — a closed one stays as history.
- A claim citation that exists only in the pull-request body.
- A merge commit.
- A pull request that spans two steps of brief § 8, or one whose description
  names no § 2 requirement.
- A review thread resolved before its commit is pushed, or with a reply that is
  not `addressed: <sha>`.
