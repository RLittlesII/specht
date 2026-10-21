---
title: "Decision 0006: Repository analysis adoption scope"
description: "Analyze the engine and CLI through the packaged analyzer with selected required authoring scopes"
type: decision
---

# Decision 0006: Repository analysis adoption scope

**Date:** 2026-10-20
**Decided by:** Repository owner

## The call

Extend the local packaged-analyzer verification to `specht` and `specht.tool`.
Declare both projects and their target frameworks in the repository's analysis
policy. The analyzer validates every explicit binding in their source. Require
authored implementation bindings on selected core classes: `FeatureFileRule`
in the engine and `ExitCodes` in the CLI. The engine runner and check command
also carry explicit member bindings outside those mandatory authoring scopes.

Keep the bootstrap two-stage: pack `Specht.CodeAnalysis` with conditional
references off, then restore and build the selected projects from that local
package with the references on. The standard build runs this sequence as a
required `CodeAnalysis` target between solution compilation and the test tiers;
CI runs the same target. The first solution compilation remains able to produce
the analysis package without a project-to-package cycle. Restore the ordinary
solution assets after the gate so later tests and packing use their normal
project graph.

## Why

This exercises the same package and MSBuild input delivery consumers receive
while selecting the declarations whose authoring relationship is required.
The selected scopes make missing bindings observable without imposing a blanket
requirement on shared infrastructure (B-001, B-012 and B-014).

## Rejected

- Require an authored binding on every source declaration: shared infrastructure
  would gain obligations the selected-scope policy does not establish.
- Reference the analyzer project or its output path directly: that would bypass
  the deployment package and fail to exercise consumer installation (B-019).

The accepted cost is a two-stage build and a maintained project and Feature
selection in the repository manifest.
