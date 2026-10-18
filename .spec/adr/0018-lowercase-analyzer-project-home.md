---
title: "ADR-0018: Use a lowercase analyzer project home"
description: "Correct the analyzer project path while retaining the adopted analyzer, code-fix and deployment boundaries"
type: adr
---

# ADR-0018: Use a lowercase analyzer project home

## Status

Superseded for the deployment project home by
[ADR-0020](0020-lowercase-codeanalysis-deployment-home.md), 2026-10-18.
Partially superseded by [ADR-0019](0019-lowercase-code-fix-project-home.md),
2026-10-18, for the code-fix directory and project filename. The analyzer home
and the component identities and responsibilities remain unchanged.
Supersedes [ADR-0016](0016-analyzer-codefix-deployment-projects.md).

## Context

ADR-0016 selected separate analyzer, code-fix and deployment projects and recorded
their exact paths. In review of the first binding implementation, the owner
selected `src/analyzers/analyzers.csproj`. The accepted ADR retains its original
decision; this record replaces it with the corrected analyzer path and preserves
the component boundaries needed by 0201-F1 C-3, 0201-F2 C-3 and 0201-F6 B-002.

## Considered options

1. Keep the uppercase analyzer path from ADR-0016.
2. Lowercase the analyzer directory and project filename, retaining component
   identities and the other project paths.
3. Lowercase all three project homes and their project filenames.

## Decision

Choose option 2. The project homes, identities and responsibilities are:

| Folder and project file              | Identity            | Responsibility                                             |
| ------------------------------------ | ------------------- | ---------------------------------------------------------- |
| src/analyzers/analyzers.csproj       | Specht.Analysis     | Read-only analyzer diagnostics and binding interpretation. |
| src/CodeFixes/CodeFixes.csproj       | Specht.Codefixes    | Invoked code fixes using the same interpretation.          |
| src/CodeAnalysis/CodeAnalysis.csproj | Specht.CodeAnalysis | Deployment of the analyzer and code-fix assemblies.        |

CodeFixes references analyzers. CodeAnalysis references both components for
packaging. The analyzer component does not reference CodeFixes or its Workspaces
dependencies. The components are non-packable; the deployment project owns this
capability's package. Its own assembly is not a consumer runtime dependency.
Package contents must include the component assemblies under the supported
analyzer layout; packaging tests must reject missing or empty components.

Keep Features as vertical agreements with stable IDs, rather than one Feature
per assembly. F2 spans diagnostics and fixes and lives beside CodeFixes. The
other agreements live in their relevant project slices or beside deployment.
Move analyzer source, agreements, work items and their live references together.
The assembly identity and namespace remain `Specht.Analysis`. The existing
dotnet tool remains in src/tool; this does not repurpose the checker.

ADR-0017 continues to govern compiler-loaded targets and compatibility. This
correction changes no claim, scenario, lifecycle policy, dependency direction or
public API, and introduces no additional production project or package. It does
not settle the remaining installation/version policy, public package ID or input
transport, or waive ADR-0014's engine/host migration evidence gate.

## Consequences

The analyzer home matches the owner's requested spelling, including on
case-sensitive filesystems. Existing references must move with the directory and
project filename; the casing change requires no consumer assembly or namespace
migration. Code-fix and deployment paths retain their adopted spelling.

Keeping the original analyzer path would disregard the review correction.
Renaming the other homes would enlarge the change beyond the requested path.
Separate components retain the cost of explicit references and package-content
checks, while keeping code-fix/editor dependencies outside compiler-only loading.
