---
title: "Specht.Analysis project home"
description: "Adopted Analyzers project home for epic 0201"
type: reference
---

# Specht.Analysis

This is the adopted home of the `Specht.Analysis` analyzer project. It owns
read-only diagnostics and binding interpretation. Agreement slices live under
Bindings, TestEvidence, Scenarios and Reconciliation. The CSharp diagnostics and
repairs agreement is shared with [CodeFixes](../CodeFixes/.spec/README.md).

The planned project file is Analyzers.csproj. No production project is scaffolded
by the current specification revision.

[ADR-0016](../../.spec/adr/0016-analyzer-codefix-deployment-projects.md) records
the project identities, dependency direction and deployment boundary. Reqnroll
remains optional; the binding model is framework-independent.
