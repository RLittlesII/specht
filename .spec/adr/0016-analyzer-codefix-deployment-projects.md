---
title: "ADR-0016: Separate analyzer, code-fix and deployment projects"
description: "Adopt sibling Analyzers, CodeFixes and CodeAnalysis project homes with the owner-selected identities"
type: adr
---

# ADR-0016: Separate analyzer, code-fix and deployment projects

## Status

Superseded, 2026-10-17, by
[ADR-0018](0018-lowercase-analyzer-project-home.md).
Originally accepted, 2026-10-17, by the repository owner in
[PR 225 review 5560492857](https://github.com/RLittlesII/specht/pull/225#pullrequestreview-5560492857).
This selects the project boundaries for epic 0201; production remains deferred.

## Context

The first decomposition put six Feature agreements under src/analysis, treating
agreement slices as uncommitted project homes. The owner rejected that foldering
and selected the separation demonstrated by
[Airframe](https://github.com/RocketSurgeonsGuild/Airframe/tree/main/src/CodeAnalysis).
Its analyzer and code-fix projects are separate components; its CodeAnalysis
project references and packages them. The correction affects 0201-F1 through F6,
so this record is repository-wide.

0201-F2 C-3 separates read-only analysis from invoked edits. 0201-F6 B-002
requires consistent editor/build verdicts. Their implementation must retain a
single binding interpretation without making compiler analysis load code-fix
Workspaces dependencies. Framework-independent identity and optional Reqnroll
remain the adopted model (0201-F1 C-3; 0201-F3 C-1).

## Considered options

1. Six area folders under src/analysis, with project composition deferred.
2. One assembly containing analyzers, code fixes and deployment behavior.
3. Sibling Analyzers, CodeFixes and deployment CodeAnalysis projects.

## Decision

Adopt option 3 with the owner's exact identities:

| Folder and planned project file      | Identity            | Responsibility                                             |
| ------------------------------------ | ------------------- | ---------------------------------------------------------- |
| src/Analyzers/Analyzers.csproj       | Specht.Analysis     | Read-only analyzer diagnostics and binding interpretation. |
| src/CodeFixes/CodeFixes.csproj       | Specht.Codefixes    | Invoked code fixes using the same interpretation.          |
| src/CodeAnalysis/CodeAnalysis.csproj | Specht.CodeAnalysis | Deployment of the analyzer and code-fix assemblies.        |

CodeFixes references Analyzers. CodeAnalysis references both components for
packaging. Analyzers does not reference CodeFixes or its Workspaces dependencies.
The two components are non-packable; the deployment project owns this
capability's package. Its own assembly is not a consumer runtime dependency.
Package contents must include the component assemblies under the supported
analyzer layout; a future packaging test must reject missing/empty components.

Keep Features as vertical agreements with stable IDs, not one Feature per
assembly. F2 spans diagnostics and fixes and lives beside CodeFixes. The other
agreements live in their relevant project slices or beside deployment. The
existing dotnet tool remains in src/tool; this does not repurpose the checker.

The current correction moves agreements and literal acceptance links only.
Do not add empty production projects or move the isolated experiment into the
solution. Supported targets, dependency versions, installation/version policy,
public package ID and input transport remain design work before implementation.
This decision does not waive ADR-0014's engine/host migration evidence gate.

## Consequences

The compiler analyzer and editor code-fix dependencies can evolve separately,
while one deployment project owns their coherent distribution. The folder
structure exposes those boundaries before implementation begins.

The cost is maintaining component references and package-content checks. One
Feature can span projects, so its agreement names each responsibility rather
than duplicating claims. Rejected option 1 hides deployment ownership; rejected
option 2 makes analyzer loading depend on code-fix/editor dependencies.
