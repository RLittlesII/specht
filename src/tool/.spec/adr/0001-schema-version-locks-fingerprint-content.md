---
title: "ADR-0001: schema version locks fingerprint content"
description: "A schema version is locked to a SHA-256 digest of its shipping files; Git records provenance, while separate verification enforces the frozen content."
type: adr
---

# ADR-0001: schema version locks fingerprint content

## Status

Accepted — 2026-10-13, repository owner. Records the technical rationale for
[decision 0009](../decisions/0009-a-versions-folder-is-its-number-one-number-covers-the-set-and-a-version-lock-freezes-it.md);
it does not supersede that decision.

## Context

A frozen schema version must keep the same shipping files as the tool evolves
(`0001-F7` B-013, B-089, C-17). Its schemas, manifest, templates and rule pages
share one version number. Unrelated application changes must not alter that
version's content identity.

The hexadecimal value in `src/tool/schema/versions.lock.json` is a SHA-256
digest of the selected files' paths and exact bytes. It is not a Git commit
hash. Git retains the history of when the lock was recorded.

## Considered options

1. **Record one content digest per schema version.** Chosen. Verification can
   compare the current shipping files with a compact baseline without reading
   Git history.
2. **Record a Git commit as the baseline.** Viable, but verification would
   still need to retrieve that commit's relevant files and compare them. A
   commit identifies a repository revision; it does not directly fingerprint
   just the schema version's files.
3. **Rely on version labels and review alone.** Rejected in decision 0009:
   naming a version does not detect changes to its contents.

## Decision

Keep a committed JSON mapping from each locked `major.minor.patch` to one
SHA-256 content digest, with no file list. JSON stores the baseline; the digest
identifies its contents. The deterministic input ordering and encoding are
defined in [the technical design](../README.md#7-technical-design) and implemented
by [`SchemaVersionLock`](../../../../.build/SchemaVersionLock.cs).

Record a version once. Repeating the lock command leaves existing entries and
the lock's bytes unchanged (`0001-F7` B-092, B-093). A change to a frozen
version requires a new version, rather than regenerating the old entry.

Recording and enforcement are separate responsibilities. Verification
recomputes each locked version's digest and rejects a mismatch (`0001-F7`
B-089), and checks which shipped versions remain unlocked (B-090, B-091).
Release gating refuses publication while a shipped version is unlocked
(`0055-F6` B-013).

At this record's adoption, item `0188` supplies the writer. Verification and
release gating remain pending in
[`0044`](../../.issue/0044-shipped-versions-frozen.yml) and
[`0190`](../../../../.build/Releasing/.issue/0190-refuse-a-tag-over-an-unlocked-schema-version.yml).
A recorded digest alone does not make subsequent builds reject changed files.

## Consequences

- Unrelated commits leave a frozen schema version's digest unchanged.
- Verification can run offline without a historical Git checkout.
- Renames, additions, deletions and byte changes, including line endings,
  change the digest. This matches the byte-identity requirement.
- One aggregate digest detects a mismatch; the Git diff identifies the changed
  files. Existing lock entries must remain immutable: editing both the files
  and their recorded digest defeats the baseline, as decision 0009 records.
- The digest encoding is part of the verification contract. Changing it would
  change existing digests even if the shipping files stayed the same.

This follows an established content-verification pattern: NuGet also records
package content hashes in its lock file and verifies them against restored
packages. See [NuGet's SHA validation](https://devblogs.microsoft.com/dotnet/enable-repeatable-package-restores-using-a-lock-file/#sha-validation).
