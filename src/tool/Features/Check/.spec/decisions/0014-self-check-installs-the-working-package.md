---
title: "Decision 0014: Self-check installs the working package"
description: "Build and install this checkout through an isolated local manifest so developing a schema does not require a published checker that already supports it"
type: decision
---

# Decision 0014: Self-check installs the working package

**Date:** 2026-10-17
**Decided by:** the repository owner, selecting the local-package option for bug 0237.

## The call

This repository's `Specht` target builds and packs the working checkout's tool,
installs that exact package through an isolated local manifest under
`.artifacts/self-check/`, and invokes the installed command against the requested
root. The hook uses the staged snapshot it already supplies. Preparing the package
is part of the gate: build, pack or install failure stops the check and fails it.

Each preparation replaces the local installation and uses isolated tool and
package caches. Source edits can retain the same computed package version; an
earlier installation must never stand in for the new bytes.

The committed `.config/dotnet-tools.json` retains its published consumer pin.
Consumer installation and native restore remain as specified in `0055-F7`.
This reverses the published-pin requirement for this repository's self-check
in A-2, B-014, decision 0009 and the corresponding `0055-F1` assumption and
constraint. It changes no consumer installation contract and introduces no
engine project reference, global install, version bump or release.

## Why

Checker 0.1.131 rejects the `tests` and `owners` settings required by the working
schema 0.2.0. Requiring a supporting published checker before developing that
checker is circular. Installing a package built from this checkout keeps the
package boundary in the gate while validating the code and schema being changed.

## Cost accepted

Self-check and specification commits now build, pack and locally install the
checker. They need compilable checker source and take longer. Formatting still
precedes the full solution compile; preparation inside `Specht` compiles the tool
earlier. No package is published, and the release `Pack` output remains separate.

## Rejected

**Run the built CLI directly and test installation separately.** Simpler and
faster, but the self-check would no longer prove installed-package invocation.
The owner chose to retain that proof in the gate.

**Require a newer published pin first.** It cannot validate development of that
same capability. Cost of rejecting: the isolated preparation step above.

**Strip the new manifest settings or fall back to the old checker.** Either
checks a different contract or can accept stale behavior. Cost of rejecting:
preparation failures fail the gate; there is no fallback.

## Affects

- `0001-F2` A-2, B-010, B-014, B-046, B-047, C-6, OQ-9 and scenarios.
- `0055-F1` A-2, B-010, C-3 and the `Specht` target's implementation.
- `0055-F2` section 5 row 18: bootstrap packaging is not a release `Pack` step.
- brief section 7, contributor instructions and build documentation.
- Decisions 0001 and 0009: append this limited reversal; preserve their history.

## Reversal

None.
