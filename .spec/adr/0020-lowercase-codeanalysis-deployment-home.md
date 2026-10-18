---
title: "ADR-0020: Lowercase the CodeAnalysis deployment home"
description: "Align the deployment project directory and file name with the repository's lowercase project convention"
type: adr
---

# ADR-0020: Lowercase the CodeAnalysis deployment home

## Status

Accepted, 2026-10-18, by the repository owner in
[PR 237 review](https://github.com/RLittlesII/specht/pull/237#discussion_r4309293749).
Supersedes the deployment home and project filename selected by ADR-0018.
ADR-0018 remains active for the analyzer project home; ADR-0019 remains active
for the code-fix project home.

## Context

ADR-0018 selected `src/CodeAnalysis/CodeAnalysis.csproj` as the deployment
project home. Review of PR 237 requested lowercase directory and project names,
matching the repository's project folder and project filename convention.

## Decision

Use `src/codeanalysis/codeanalysis.csproj` for the deployment project. Keep its
identity `Specht.CodeAnalysis`, package ID, assembly identity and responsibility
unchanged. The project continues to package the analyzer and code-fix assemblies.
The analyzer and code-fix project homes remain as decided by ADR-0018 and
ADR-0019.

## Consequences

The solution entry and every live source, test, specification and documentation
reference to the deployment path use lowercase. The `.spec/` and `.issue/`
records move with their project home. The path change alters no package contents,
consumer configuration, analyzer behavior or public API.
