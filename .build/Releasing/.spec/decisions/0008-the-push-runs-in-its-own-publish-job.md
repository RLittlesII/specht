---
title: "Decision 0008: the push runs in its own publish job"
description: "The owner chose a separate publish job over accepting the advisory comparison's same-host exposure for the nuget.org OIDC credential: id-token write moves to a publish job that pushes, never waits on the comparison and runs on a GitHub-hosted runner, and the release job keeps contents write alone"
type: decision
---

# Decision 0008: the push runs in its own publish job

**Date:** 2026-11-01
**Decided by:** the repository owner, 2026-11-01, in chat; recorded by spec-author

## The question

[Decision 0007](0007-publish-prereleases-to-nuget-org-through-trusted-publishing.md)
gave the `release` job `id-token: write`. `0109-F5` then added its advisory
comparison steps to the same job, after "Create GitHub Release", `attach`'s
wait of up to 100 minutes among them (`0109-F5` decision 0005, ADR-0001). With
`id-token: write`, the runner sets `ACTIONS_ID_TOKEN_REQUEST_URL` and
`ACTIONS_ID_TOKEN_REQUEST_TOKEN` in the environment of every step of the job,
so during that wait a measured revision on the same host and operating-system
account could request an OIDC token and exchange it for a nuget.org push key.
`0109-F5` A-5 and its decision 0005 addendum accept a same-host exposure, but
for the workflow token, and name no OIDC grant.

The owner was asked: "Does A-5's accepted exposure also cover the nuget.org
OIDC credential?" The owner answered, verbatim:

> Yes, accept it

Asked then "Which answer stands for the NuGet credential?", the owner
answered, verbatim:

> Separate publish job

## The call

The second answer supersedes the first. "Yes, accept it" is not in force.

1. **"Log in to NuGet" and "Push release packages" run in their own job,
   `publish`**, in the same generated `release.yml`. It needs `build`, carries
   the same tag-push condition as `release`, and is the only job granted
   `id-token: write`. It is granted contents read to check out and no other
   scope: no contents write, no actions read and no package write. Its steps
   are a checkout that persists no credential, the download of this run's
   package artifact, the SDK, the login and the push.
2. **The `release` job needs `publish`**, so a GitHub Release is still created
   only after both packages are pushed. It holds contents write and actions
   read, and `id-token: none`. It keeps "Create GitHub Release" and
   `0109-F5`'s comparison steps.
3. **The `comparison` job is unchanged**: it needs `build` and is read-only.
4. **A rerun of all jobs** reruns the push, which skips a version already on
   the feed (`--skip-duplicate`), and keeps a Release already published at the
   run's commit (`0109-F5` A-6; § 7, item 0214 amendment).
5. **The `publish` job runs on GitHub-hosted `ubuntu-latest`**, not on the
   self-hosted pool of the other three jobs (owner, 2026-11-01; "Where the
   job runs" below).
6. **The `publish` job pushes only what `build` packed.** Its packages come
   from this run's `nupkg` artifact alone, which `build` uploaded (C-4). The
   artifact is handed between jobs by the workflow run, not by the runner's
   disk, so the transport does not depend on the runner kind: `publish` reads
   nothing a pool runner left, and packs nothing itself.

C-5, C-7, C-9 and B-008 are amended; § 5 row 13 is added.

## Where the job runs

**Decided by:** the repository owner, 2026-11-01, in chat.

Call 5 first read "The `publish` job runs on the self-hosted pool of the other
three jobs, from a clean checkout", derived by spec-author from `0109-F5`
C-12, which rules out a further runner provider in the release run, and from
`0055-F2` C-11. No owner ruling stood behind it. This section reverses that
derived placement.

The owner was asked: "Where should the `publish` job run?" The reason raised
with the question:

- A runner of the self-hosted pool persists between jobs (`0055-F2` A-5), and
  `0109-F5`'s measured revisions run on that pool (`0109-F5` C-12).
- `comparison` and `publish` both need only `build`, so they can run at the
  same time on the pool.
- An ephemeral GitHub-hosted runner closes that exposure: the publishing
  credential - `id-token: write`, and the nuget.org key the login mints from
  it - never sits on a host where measured code runs or has run.

The owner answered, verbatim:

> GitHub-hosted runner

**The label is `ubuntu-latest`**, derived by spec-author; the owner named the
provider, not the image.

- It is this Feature's runner before item 0264: every job of the release
  workflow ran on `ubuntu-latest` until `0055-F2` decision 0005 moved `build`
  and `release` to the pool on 2026-10-23 (C-9; § 10, 2026-10-23).
- The push has run there. Job 115442456381 of tag run 38439090996
  downloaded the run's artifact and pushed `specht.tool.0.1.90.nupkg`, to the
  GitHub Packages feed of its day, on `ubuntu-latest` (§ 8, "First-release reconciliation").
- The repository already uses it as its one hosted Linux image: the Renovate
  workflow runs there (`0055-F2` C-12; `.github/.spec` decision 0001).
- The job's steps need no particular operating system: the checkout, the
  artifact download, the SDK from `global.json`, the login action and
  `build.cmd`, which hands a Unix shell to `build.sh`.

`windows-latest`, the other hosted image this repository uses, was not chosen:
no record ties the push to Windows, and the Windows leg exists to prove the
path separator (`0055-F2` B-003), which the push does not exercise.

A hosted runner starts every job on a fresh image (`0055-F2` A-5), so
`0055-F2` C-11's clean checkout is not required of `publish`. The job's
checkout persists no credential (call 1).

## The job's name

`publish`, derived by spec-author; the owner said "Separate publish job" and
named no job id.

- The job does what decision 0001's `publish` job did on 2026-10-09 and
  nothing more: it pushes the package and holds the one publishing grant.
  Decision 0003 renamed that job `release` when the Release step joined it;
  the Release step now leaves it again, so the name returns to its first
  meaning.
- The Feature's own words for the act are "publish": C-1, B-002, B-005 and
  § 5 row 7.
- **What the name costs.** A record dated 2026-10-09 or 2026-10-10 that names
  the job `publish`, decision 0001 and B-008's amendment note among them,
  means the job named `release` from 2026-10-11 to this decision. Decision
  0003's rename table stays a record of its day.
- **The nuget.org trusted-publishing policy.** This Feature's records say only
  that `NUGET_USER` names the nuget.org profile whose trusted-publishing policy
  accepts this repository's workflow (§ 7, item 0327 amendment; decision
  0007). They record no condition on a job's name. The job stays in
  `release.yml`, the workflow they name.

Rejected names: `push`, which is also the event name in the job's own `if`;
`nuget`, which is the login step's id and names a provider, not the act;
`publish-packages`, which says no more than `publish` and is a name no record
has used.

## Why

During `attach`'s wait the `release` job runs no publication step, yet its
grant let any process on the same host and operating-system account obtain a
nuget.org push key, and a nuget.org version cannot be deleted (decision 0007).
In its own job the grant exists only while the login and the push run. A
measured revision of the same run starts only after `release` uploads the
receipt (`0109-F5` § 7, "Acyclic execution and credential boundary"), which is
after `publish` has finished, and one release runs at a time (`0109-F5` A-5).
The job ordering alone does not keep the credential off a measured host: pool
runners persist between jobs, revisions measured by earlier runs ran on them,
and `comparison` can hold a pool runner while `publish` runs. On a
GitHub-hosted runner the credential never shares a host with measured code.
So the same-host exposure `0109-F5` A-5 accepts is the workflow token only.

## Rejected

**Accept the exposure** - the owner's first answer, "Yes, accept it",
superseded by the second. Cost of rejecting: a fourth job in the release
workflow, the checkout, download and SDK steps written a second time, and the
Release created by a job other than the one that pushed.

## Consequences

- **The two write grants are separated.** Decision 0003 rejected "a third job
  holding `contents: write` alone" at the cost that "the two grants are not
  separated from each other". That rejection is superseded: the `release` job
  now holds contents write without the publishing grant, and the `publish` job
  holds the publishing grant without contents write.
- **A failed push skips the `release` job**, so no Release is created and no
  comparison is requested, as when the push was a step before it. A rerun of
  failed jobs alone reruns `publish`, then `release`.
- **A run whose `release` job fails after `publish` passed** reaches the
  Release step again through a rerun of failed jobs, without a second push.
  OQ-14 stays open.
- **A hosted runner in the release run.** The `publish` job takes a
  GitHub-hosted runner for its five steps and no runner of the pool, so it
  never waits for a free pool runner. `0109-F5` OQ-4's single-runner delay now
  falls on the `release` job alone. `0109-F5` C-12's "further runner provider"
  excepts `publish`; its note says so.
- **The transport is unchanged.** `publish` downloads this run's `nupkg`
  artifact as the `release` job did (call 6; C-4), so the move off the pool
  changes no package and no step.
- **The `publish` job is not the job `0109-F5` C-10 rules out.** It holds no
  contents write; C-10's note says so.

## Affects

- `0055-F6`: C-5, C-7 and C-9 amended; B-008 and its scenario amended; § 5
  row 13 added; § 10 entry. Decision 0003's rejection of a third job holding contents
  write alone is superseded, and decision 0007's call 3 for the job that holds
  `id-token: write`; each Reversal points here.
- `0109-F5`: A-5 amended to the workflow token only, and to the hosted
  runner; C-10 and C-12 clarified with dated notes; decision 0005's addendum
  gains an entry quoting both answers; § 10 entry. OQ-4 clarified with a dated
  note: only the Release creation waits. ADR-0001 gains an amendment:
  `release` needs `publish`, and the write authority is split between them.
- `0055-F2`: C-12 and § 5 #28 clarified with dated notes: `publish` is not a
  job that left `ubuntu-latest` and runs no gate. C-11 is unchanged; it
  already exempts a GitHub-hosted job.
- Item `0333` delivers it. § 6 and § 7 (`implementer`), § 8 and § 9
  (`test-writer`) follow in that item.

## Reversal - 2026-11-01 (decision 0009)

[Decision 0009](0009-the-github-release-carries-no-package.md) removes the
`release` job's package download: the Release attaches no package. Of the cost
recorded under Rejected, the download is no longer written a second time; the
checkout and SDK steps are. "The transport is unchanged" holds for `publish`
alone, which still downloads this run's `nupkg` artifact (call 6). The calls
stand.
