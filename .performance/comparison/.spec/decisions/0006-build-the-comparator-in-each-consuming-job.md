---
title: Build the comparator in each consuming job
description: Replace the comparator binary handoff with direct source builds in release and comparison
type: decision
---

# Build the comparator in each consuming job

Accepted 2026-11-03 by the owner for `0109-F5` B-001 and B-018.

The owner requested building the comparator from source in both jobs and removing
its artifact. Each job publishes `.performance/comparison/comparison.csproj` from
its tagged checkout for `net10.0`, in Release configuration, into
`.artifacts/comparator`. Direct `dotnet publish` restores the project's nuget.org
packages without restoring local tools or needing a private-feed credential.

The release job builds after successful publication. The comparison job builds
before collection. Each build requires a successful checkout, because comparison
treats checkout failure as advisory and a self-hosted runner can hold stale source.
Each build is advisory and comparator invocations require that job's build to succeed. A skipped invocation reaches the existing Incomplete
summary with its rerun instruction. The build step receives no comparison token.

This supersedes § 7's build-job `PublishComparator` target and binary artifact
handoff. Both jobs compile their own comparator from the same tagged source,
adding compilation and restore work to each job. Keeping one published binary
would avoid that duplicate work but retain the artifact transport the owner
asked to remove.

The request and result artifacts continue to carry the rendezvous data. Package
publication, collection permissions and measurement policy keep their existing
contracts. Generated-workflow tests prove declarations and ordering. They do not
prove live runner execution.
