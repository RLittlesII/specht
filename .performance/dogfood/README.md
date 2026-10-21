---
title: "Dogfood Specht.CodeAnalysis"
description: "Build the local analysis package and consume it from the repository's analyzer project"
type: guide
---

# Dogfood Specht.CodeAnalysis

Restore the solution once, then run `bash .performance/dogfood/run.sh` from this repository. It builds and packs
the checked-out `Specht.CodeAnalysis` project, restores the analyzer, engine and
CLI projects from that local package, and compiles all three with the package's
analyzer and build assets enabled. The package cache
and output live under `.artifacts/analysis-dogfood/`. The ordinary solution
build leaves the conditional package references disabled, so the package can be
built before its source projects consume it.

Run `python3 .performance/dogfood/verify.py` after that build to compile a
disposable consumer copy. It confirms a clean baseline in all three projects,
`SPCT0004` and `SPCT0007` for changed and missing engine and CLI bindings,
`SPCT0004` after an analyzer source binding or selected Feature claim change,
and `SPCT0001` after a manifest selection change. The copy and its cache stay under
`.artifacts/analysis-dogfood/`; the repository inputs are never changed by this
verification.

The three projects and `0201-F1`, `0001-F1` and `0001-F2` Feature inputs are declared in
`.specht/manifest.json` and `Directory.Build.props`. XML documentation parsing
is enabled for these projects, with unrelated missing-comment warnings
suppressed. The analyzer project's explicit claim and ADR bindings in
`src/analyzers/Bindings/BindingResolver.cs` resolve against the current Feature
and accepted ADR. The package supplies those actual records and the consumer
frontmatter schemas as AdditionalFiles. The engine's `FeatureFileRule` and the
CLI's `ExitCodes` carry required implementation bindings in the selected
authoring scopes. Explicit bindings elsewhere in all three projects are also validated.
A successful compile establishes that the supplied inputs are usable. The run does not establish a performance
finding; OQ-13 records the measurement protocol.
