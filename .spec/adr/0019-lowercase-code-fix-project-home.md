---
title: "ADR-0019: Lowercase the code-fix project home"
description: "Use lowercase directory and project file names for the code-fix component while preserving its assembly and namespace identities"
type: adr
---

# ADR-0019: Lowercase the code-fix project home

## Status

Accepted, 2026-10-18, by the repository owner in
[PR 235 review](https://github.com/RLittlesII/specht/pull/235#pullrequestreview-5565361394).
Partially supersedes [ADR-0018](0018-lowercase-analyzer-project-home.md).

## Context

ADR-0018 corrected the analyzer project home to lowercase and retained the
adopted `src/CodeFixes/CodeFixes.csproj` path. Review of PR 235 requested the
same lowercase directory and project filename convention for the code-fix
component.

## Decision

Use `src/codefixes/codefixes.csproj` for the code-fix project. Keep its assembly
and namespace identities as `Specht.Codefixes`, its responsibility, dependency
direction and non-packable status unchanged. Update all project references and
specification links to the lowercase path.

## Consequences

The project directory and filename follow the repository's lowercase naming
convention on case-sensitive filesystems. This is a rename only; it changes no
analysis or repair behavior and preserves the component boundaries in ADR-0018.
