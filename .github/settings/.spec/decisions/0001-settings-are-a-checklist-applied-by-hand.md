---
title: "Decision 0001: settings are a checklist applied by hand"
description: "The GitHub repository settings are a checked-in checklist applied by hand once after the first push, not a script or a settings app"
type: decision
---

# Decision 0001: settings are a checklist applied by hand

**Date:** 2026-10-08
**Decided by:** the repository owner (2026-10-08); recorded by spec-author

## The call

Branch protection, required checks, merge methods, auto-merge, secrets and app
installations are written in a checked-in checklist. The maintainer applies it
by hand, once, after the first push. Nothing applies or verifies the settings
automatically.

## Why

The settings are applied once and change rarely. A checklist is reviewable in
a pull request like any other file, needs no credential with administration
rights, and keeps the repository's code offline (AGENTS.md § Invariants).

## Rejected

**A script that calls the GitHub API.** Cost of rejecting: the settings are
applied by hand, and drift between GitHub and the checklist is caught by
nobody but the maintainer (C-3). Taking it would cost an administration token
and code that runs once.

**A settings-as-code app reading a file from the repository.** Cost of
rejecting: the same drift risk. Taking it would cost a third-party app with
administration rights over the repository.

## Affects

- `0055-F8` § 4 C-1, C-3; § 5 row 4.

## Reversal

None.
