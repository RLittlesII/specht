---
title: "Decision 0010: the MCP package is packed and pushed with the tool"
description: "Release 0.5.0 shipped without specht.mcp: Pack and the release package list named two packages. The release packs and pushes three, specht.tool, Specht.CodeAnalysis and specht.mcp, at the one repository release version"
type: decision
---

# Decision 0010: the MCP package is packed and pushed with the tool

**Date:** 2026-11-05
**Decided by:** the owner's earlier ruling, ADR-0031 and `0132-F3` C-11
(the MCP is versioned and tagged with the tool on the one `v<version>` tag);
recorded by spec-author for item 0363

## The question

Release 0.5.0 carried no `specht.mcp`. `Pack` in `.build/SpechtBuild.cs`
packs `src/tool` and `src/codeanalysis` only, and `ReleasePackages()` in
`.build/SpechtBuild.Release.cs` names those two, so `PushReleasePackages` and
the generated release workflow never publish the MCP. B-003 and `0055-F1`
B-009 each said two packages, so the build matched its specification and the
specification contradicted C-11.

## The call

1. **`Pack` writes three packages**, `specht.tool`, `Specht.CodeAnalysis` and
   `specht.mcp`, each at the version the build computes (`0055-F1` B-009).
2. **The release pushes the same three to nuget.org** at the repository
   release version (B-003; `0132-F3` B-035).
3. **B-007 stays withdrawn** (decision 0009): the Release carries no package,
   so the third package is attached to nothing.

## Why

C-11 gives the MCP no version line of its own. A package that is versioned and
tagged with the tool and left out of the push is a version nobody can install.

## Rejected

**A release of the MCP of its own**, on its own tag. Cost of rejecting: one
tag publishes every package, so a change to the MCP alone still moves the
others' version. ADR-0015 decision 2, which ADR-0031 leaves standing, chose
the one tag.

## Affects

- `0055-F1`: B-009 amended; its scenario amended; § 10 entry.
- `0055-F6`: B-003 amended; its scenario amended; § 5 row 16 added; § 10 entry.
- `0132-F3`: B-035 added, with its scenario and § 9 row; § 10 entry.
- `0201-F6` B-020 says the analyzer package is published alongside
  `specht.tool` and is true as it stands.
- Item `0363` delivers it. `0055-F6` § 6 to § 8 and `0055-F1` § 7
  (`implementer`), and § 8 and § 9 of both (`test-writer`), follow in that item.

## Reversal

None.
