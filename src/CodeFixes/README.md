---
title: "Specht.Codefixes project home"
description: "Adopted CodeFixes project home for epic 0201"
type: reference
---

# Specht.Codefixes

This is the adopted home of the `Specht.Codefixes` code-fix project. It owns
invoked XML authoring repairs and will reference `Specht.Analysis` in Analyzers.
The [CSharp binding agreement](.spec/README.md) specifies both the analyzer verdict
and its safe repairs, with each responsibility in its own assembly.

The planned project file is CodeFixes.csproj. No production project is scaffolded
by the current specification revision.

[ADR-0016](../../.spec/adr/0016-analyzer-codefix-deployment-projects.md) records
the project identities, dependency direction and deployment boundary. Reqnroll
remains optional; the binding model is framework-independent.
