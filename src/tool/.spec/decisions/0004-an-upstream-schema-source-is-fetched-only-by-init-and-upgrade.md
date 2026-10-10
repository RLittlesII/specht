---
title: "Decision 0004: an upstream schema source is fetched only by init and upgrade"
description: "The manifest may record an upstream schema source - a URL or a package - with its version and content hash; init and upgrade fetch it into the root's schema folder, and the check reads only that local copy and stays offline"
type: decision
---

# Decision 0004: an upstream schema source is fetched only by init and upgrade

**Date:** 2026-10-08
**Decided by:** the repository owner (2026-10-08), answering OQ-7; recorded by spec-author

## The call

A manifest may record an **upstream schema source**: a URL or a package, with
its version and a content hash. `init` and `upgrade` are the only commands that
may touch the network, and only to fetch a recorded source into
`<root>/.spec/schema/`. They write the content only when it matches the
recorded hash. The check never touches the network: it reads the local copy.

The rule vocabulary stays the pinned embedded version's. C-6's `$id` scheme
binds specht's own embedded set; a consumer's upstream schema carries its own
`$id`. The field names are OQ-1's.

This amends the "offline" invariant on purpose. The check is offline. `init`
and `upgrade` may fetch, and only a source the manifest records (brief § 9;
AGENTS.md § Invariants).

## Why

- A consumer can share one schema set across its own repositories without
  hand-copying it and without forking the tool.
- The check stays deterministic: the same tree gives the same verdict, because
  what it reads is committed under the root. The network is touched only by a
  command the maintainer runs on purpose, and its result is a diff they review.
- A version and a content hash make the fetch reproducible and the content
  verifiable.

## Addendum - 2026-10-08

Added by the repository owner after spec-reviewer round 5, answering how a
recorded source relates to the source selection and to `schemaVersion`:

- Recording an upstream source **selects the on-disk source**: the fetched
  copy is the schema (B-032).
- The upstream version and hash are **independent of `schemaVersion`**.
  `schemaVersion` still pins the rule vocabulary and the templates (C-10).
- `upgrade` moves `schemaVersion` and rewrites the templates. It skips the
  frontmatter schemas, since disk is selected (B-029). It fetches the recorded
  upstream version only when the local copy's hash differs from the recorded
  one (B-026, B-033). Moving the upstream version is a manifest edit by the
  consumer (B-034).
- A source that cannot be reached when a fetch is needed is refused like a
  hash mismatch: the command names the source, writes nothing and exits `3`
  (B-035; `0001-F4` B-016).
- A manifest that records an upstream source **and** explicitly selects the
  embedded source contradicts itself and is rejected. The check names the
  contradiction on stderr, writes nothing on stdout and exits `3`, as for any
  rejected manifest (B-036; `0001-F2` decision 0003). `init`, `upgrade` and
  `--explain` reject it the same way (`0001-F3` B-030). Recording a source
  selects disk only when no source is selected explicitly (B-032).

## Rejected

**A path outside the root.** It breaks root-relative paths in every output
(brief § 9), and a CI checkout does not have the other path. Cost of
rejecting: a consumer wanting a shared local folder fetches it through a
recorded source instead.

**A fork of the tool carrying the consumer's schema.** It is the drift the
repository exists to remove (brief § 2). Cost of rejecting: none.

**The check fetching the source itself.** It breaks determinism and the
offline check. Cost of rejecting: a consumer runs `upgrade` to pick up a new
upstream version.

Kept alongside the decision: the copy kept by hand in the repository (the
on-disk source, B-009). Both a URL and a NuGet schema package were taken.

## Affects

- `0001-F7` § 3 B-024 to B-028; § 4 C-6 (amended), C-8, C-9; § 5 row 6;
  § 11 OQ-1 (scope extended), OQ-7 (resolved).
- `0001-F4` B-014 to B-016: `init` fetches a recorded source (C-8).
- By the addendum: `0001-F7` § 3 B-001, B-005, B-006, B-010, B-011, B-024,
  B-026, B-029, B-032 (amended), B-033 to B-036; § 4 C-10; `0001-F3` B-005,
  B-030.
- brief § 9; AGENTS.md § Invariants ("Deterministic and offline").

## Reversal

None.
