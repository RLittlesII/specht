---
title: "Decision 0001: the push runs in a second job of the same workflow"
description: "The owner chose a second job, publish, in the generated publish.yml: it needs build, runs only for a pushed v* tag and alone holds package write; no Publish target is built, and three readings of C-4, 0055-F1 C-1 and B-008 are accepted"
type: decision
---

# Decision 0001: the push runs in a second job of the same workflow

**Date:** 2026-10-09
**Decided by:** the repository owner, 2026-10-09, asked while item `0083` was being designed; recorded by spec-author

## The call

- **The names** (OQ-6): the workflow file is `publish.yml`, the tag-check
  target is `VerifyTag`, and the job that runs the gates is `build`, on
  `ubuntu-latest`.
- **The push runs in a second job, `publish`, in the same generated file**
  (OQ-8, option A). The job needs `build`. It runs only for a push of a `v*`
  tag. It is the only job whose token carries package write; the
  workflow-level token is read-only. The name `publish` is ratified with it.
- **The package is handed from `build` to `publish` as a run artifact.**
- **No `Publish` target is built.** No target pushes a package.
- **Every line of `publish.yml` comes from the NUKE generator.**

Three readings are accepted with it:

1. **C-4.** "The file `Pack` wrote in the same run" allows a hand-off of that
   file as a run artifact between two jobs of one workflow run.
2. **`0055-F1` C-1.** "Every gate is a NUKE target" covers gates. The push is
   not a gate, so a `dotnet nuget push` line that no target runs is allowed in
   a workflow.
3. **B-008.** A dry run runs every step of `build`, the upload included, and
   the `publish` job is skipped.

## Why

The owner, 2026-10-09: "Can the actual package publish to the source be done
on a trigger after the build succeeds? I get your concern of any machine can
publish I'd like to keep nuke generting yaml as a convention."

The owner was then shown three shapes, each emitted by the generator or
described with its cost in § 7 ("What the generator can express", "Where the
push runs"), and chose option A. Of the three, A is the one in which the
publish follows a successful build inside one run, every line stays generated,
and no token that can write a package exists in a dry run or in the job that
runs the repository's code.

## Rejected

**Option B: a second generated workflow started by `workflow_run`.** C-4 rules
out a package "fetched from another run", so B needs C-4 amended, and it
confines the write token no more tightly than A does. Cost of rejecting: none
that § 7 found.

**One job, the push its last step.** B-008 stays literal and nothing leaves the
job, but package write is present in every dry run and while every gate runs.
Cost of rejecting: the package crosses GitHub's artifact store and nothing
compares the copy with the file `Pack` wrote; a dry run exercises neither the
`publish` job nor the hand-off, so both first run on the first tag; a dry run
leaves the package behind as a run artifact for its retention.

**The generator's built-in `PublishNugetPackagesJobAttribute`.** § 7 records
what it wrote: it pushes to NuGet.org (C-3), pushes a glob fetched from another
run (C-4), needs a stored secret (C-5), and accepts no enhancement. Cost of
rejecting: the `publish` job is assembled step by step in the build project
instead of declared by one attribute.

**A `Publish` target.** It would put a publish one command away on a
developer's machine, which C-1 rules out. Cost of rejecting: one `dotnet` line
in a workflow that no target runs, which is reading 2.

**The condition on the push step instead of on the job**, which would keep
B-008's first wording. A job's permissions cannot follow the event, so every
dry run would hold package write. Cost of rejecting: reading 3.

## Affects

- B-008: reworded per reading 3 (`Amended`); its scenario in `release.feature`
  follows.
- C-4: amended to carry reading 1.
- C-7: added. Package write belongs to the `publish` job only.
- § 5 #7, #8 and #10: added.
- OQ-6 and OQ-8: resolved.
- `0055-F1` C-1: its words are unchanged; reading 2 is noted on its row.
- B-001, B-002, B-005, B-009, C-1, C-3 and C-5: unchanged.
- Item `0083`: builds this shape.

## Reversal

None.
