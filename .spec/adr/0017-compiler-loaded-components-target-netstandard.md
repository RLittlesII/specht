---
title: "ADR-0017: Compiler-loaded components target netstandard2.0"
description: "Permit the analyzer/code-fix target exception while retaining the existing CLI, collector and test targets"
type: adr
---

# ADR-0017: Compiler-loaded components target netstandard2.0

## Status

Accepted, 2026-10-17, by the repository owner during epic 0201 agreement.
Limits the all-project target rule in 0001-F2 decision 0011 only for the
compiler-loaded analyzer and code-fix components.

## Context

The owner supplied the [analyzer skill](https://github.com/littlestechnology/.agents/blob/main/skills/csharp/analyzers/SKILL.md),
which requires compiler-compatible `netstandard2.0` components. The existing
all-project rule selects `net10.0;net11.0`, apart from the NUKE host. Applying it
unchanged to compiler-loaded assemblies would unnecessarily restrict loading.
The owner explicitly approved the narrow analyzer/code-fix exception.

## Considered options

1. Keep the existing targets for compiler-loaded components as well.
2. Use `netstandard2.0` for analyzer/code-fix components, preserving the existing
   CLI, collector and test targets.

## Decision

Choose option 2. `Specht.Analysis` and `Specht.Codefixes` target `netstandard2.0`;
clear the inherited multi-target setting in those project files. Their entire
compiler-loaded dependency closure must be compatible with that host contract.
Retain `net10.0;net11.0` for the CLI, collector/test executables and other existing
projects; the NUKE exception remains unchanged.

Reuse the existing parsing and manifest semantics rather than introduce a second
checker implementation. Compatibility work must preserve existing public behavior,
baseline reports and direct engine/host composition. This record does not adopt
CQRS, a mediator, a new shared-core assembly or ADR-0010's model-reader migration;
ADR-0014's evidence gate remains in force. This is a normal mixed-target solution;
there is no requirement to retarget the engine. Characterize the text-reading
semantics the analyzer actually reuses, and test its actual compiler-host
dependency closure before packaging is considered complete.

## Consequences

Compiler-loaded components use a portable target; packaging/loading tests must
verify the dependency closure and editor/build behavior, including absence of
code-fix Workspaces dependencies from compiler-only loading. Existing runtime
and test targets are retained. The cost is a narrow target override and explicit
dependency compatibility checks for the analyzer. A whole-engine retargeting
experiment does not test analyzer loading and is not a prerequisite.
