---
title: "Specht.CodeAnalysis project home"
description: "Adopted CodeAnalysis project home for epic 0201"
type: reference
---

# Specht.CodeAnalysis

This is the adopted home of the `Specht.CodeAnalysis` deployment project. It
will package `Specht.Analysis` from Analyzers and `Specht.Codefixes` from CodeFixes.
The [consumer adoption agreement](.spec/README.md) owns installation and policy.

The planned project file is CodeAnalysis.csproj. No production deployment project
or empty package is produced by the current specification revision.

[ADR-0016](../../.spec/adr/0016-analyzer-codefix-deployment-projects.md) records
the project identities, dependency direction and deployment boundary. Reqnroll
remains optional; the binding model is framework-independent.
