---
title: "Specht.Codefixes project home"
description: "Invoked XML binding repairs for epic 0201"
type: reference
---

# Specht.Codefixes

[codefixes.csproj](codefixes.csproj) builds the non-packable netstandard2.0 editor
component. Its [documentation provider](Documentation/DocumentationCodeFixProvider.cs)
offers explicit existing-target choices, unambiguous default materialization and
valid duplicate cleanup. Fix All admits the two deterministic repairs only.

The [CSharp binding agreement](.spec/README.md) covers the shared analyzer verdict
and invoked repairs. This project references `Specht.Analysis`; Workspaces stays
outside the read-only analyzer assembly. [ADR-0018](../../.spec/adr/0018-lowercase-analyzer-project-home.md)
records the project identities and dependency direction. Deployment packaging
remains separate work.
