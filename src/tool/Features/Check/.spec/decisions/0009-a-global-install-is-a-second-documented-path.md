---
title: "Decision 0009: A global install is a second documented path"
description: "Two installs are documented: the committed local tool manifest, which a build, a hook and CI use, and a global install, which puts specht on PATH in a workstation shell; no build depends on the global one. Reverses in part the global-install alternative rejected in decision 0001"
type: decision
---

# Decision 0009: A global install is a second documented path

**Date:** 2026-10-11
**Decided by:** the repository owner; recorded by spec-author

## The call

Two installs are documented.

- **The local tool manifest stays the one a build, a hook and CI use**, here
  and in a consumer. It makes the tool's version a fact of the commit.
- **A global install is documented beside it, for a workstation shell:**
  `dotnet tool install -g specht.tool`. It puts `specht` on PATH.

No build, hook or CI run depends on a global install (C-6). Nothing in the
tool changes: the package already installs with `-g`.

## Why

- **A manifest install puts no command on PATH.** It runs as `dotnet specht`
  or `dotnet tool run specht`, and the owner wants bare `specht` in a shell.
- **Decision 0001's objection still holds, and is why no build depends on a
  global install:** it pins nothing beside the repository. The objection was
  to a global install as the documented path; as a second path for a shell it
  takes nothing from the pin a build uses.
- **brief § 6 allows a default to be reversed** when the reversal is recorded
  as a decision. This is that record.

## Cost accepted

A global `specht` may be a different version from the one a repository's tool
manifest pins, so a shell run and that repository's build can disagree.

The schema stays per repository either way: the tool reads the manifest from
the root it is given, embeds every schema version it ships, and exits `3` on a
pin it does not ship (`docs/configuration.md`).

## Rejected

**A documented shell alias or function wrapping `dotnet specht`.** Keeps the
repository's pin in the shell. Rejected: the owner chose a real command on
PATH. Cost of rejecting: the version difference above.

**Correcting the documents to `dotnet specht` only.** One install, one pin.
Rejected: no bare command. Cost of rejecting: two installs to document, each
with its own invocation.

## Affects

- `0001-F2` C-6, amended.
- `0055-F7` C-2, amended; B-008, added.
- `README.md` and `docs/configuration.md`, which named the manifest as the
  only install.
- Decision 0001: its rejected alternative "A global tool install" is reversed
  in part.
- The owner's decision of 2026-10-09 recorded in item `0111`, "keep the local
  tool manifest as the documented install", with "Documenting a global install
  as a supported path" out of scope: reversed. The manifest stays a documented
  install and is no longer the only one. The item is closed and is not edited.
- Unchanged: `0001-F2` B-014 and `0055-F1` C-3, which hold this repository's
  build to the manifest.

## Reversal

None.
