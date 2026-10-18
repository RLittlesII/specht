---
title: "Specht.Analysis project home"
description: "Framework-independent binding identity component and adopted analyzer home for epic 0201"
type: reference
---

# Specht.Analysis

This is the adopted home of the `Specht.Analysis` analyzer project. It owns
read-only diagnostics and binding interpretation. Agreement slices live under
Bindings, TestEvidence, Scenarios and Reconciliation. The CSharp diagnostics and
repairs agreement is shared with [CodeFixes](../codefixes/.spec/README.md).

`analyzers.csproj` builds the non-packable `Specht.Analysis` component for
`netstandard2.0` under
[ADR-0017](../../.spec/adr/0017-compiler-loaded-components-target-netstandard.md).
The [binding identity implementation](Bindings/.spec/README.md) resolves supplied
claim and ADR declarations independently of test frameworks and shares the
engine's YAML/Markdown text readers. Compiler diagnostics, invoked fixes, discovery
adapters and deployment remain downstream work.

[ADR-0018](../../.spec/adr/0018-lowercase-analyzer-project-home.md) records
the project identities, dependency direction and deployment boundary. Reqnroll
remains optional; the binding model is framework-independent.
