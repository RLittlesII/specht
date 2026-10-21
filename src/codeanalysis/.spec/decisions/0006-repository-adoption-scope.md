---
title: "Decision 0006: Repository analysis adoption scope"
description: "Require authored bindings throughout the engine and CLI through the packaged analyzer"
type: decision
---

# Decision 0006: Repository analysis adoption scope

**Date:** 2026-10-20
**Decided by:** Repository owner

## The call

Extend the local packaged-analyzer verification to `specht` and `specht.tool`.
Declare both projects and their target frameworks in the repository's analysis
policy. Project selection covers each entire compilation, and the analyzer
validates every explicit binding in their source. Require authored
implementation bindings on every eligible type, constructor and method in
both projects. Generated code remains outside required authoring (B-013).

Keep the bootstrap two-stage: pack `Specht.CodeAnalysis` with no package version
property, then restore and build the selected projects from that local package
with `SpechtCodeAnalysisPackageVersion` set to its exact version. A version is
required for the private PackageReferences because the first compilation must
produce this checkout's analyzer before any project can consume it. The
standard build runs this sequence as a
required `CodeAnalysis` target between solution compilation and the test tiers;
CI runs the same target. The first solution compilation remains able to produce
the analysis package without a project-to-package cycle. Restore the ordinary
solution assets after the gate so later tests and packing use their normal
project graph.

## Why

This exercises the same package and MSBuild input delivery consumers receive
while making every eligible type, constructor and method in the two selected
projects' source an explicit implementation relationship. The owner selected
the full source scope, including shared infrastructure, through the existing
enforcement policy (B-001, B-012 and B-014).

## Rejected

- Infer required authoring merely from project selection: the manifest must
  still declare the enforcement scope and declaration kinds explicitly.
- Reference the analyzer project or its output path directly: that would bypass
  the deployment package and fail to exercise consumer installation (B-019).

The accepted cost is a two-stage build and a maintained project and Feature
selection in the repository manifest.
