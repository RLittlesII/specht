---
title: "Dogfood Specht.CodeAnalysis"
description: "Build the local analysis package and consume it from the repository's analyzer project"
type: guide
---

# Dogfood Specht.CodeAnalysis

Restore the solution once, then run `bash .performance/dogfood/run.sh` from this repository. It builds and packs
the checked-out `Specht.CodeAnalysis` project, restores
`src/analyzers/analyzers.csproj` from that local package, and compiles the
project with the package's analyzer and build assets enabled. The package cache
and output live under `.artifacts/analysis-dogfood/`. The ordinary solution
build leaves the conditional package reference disabled, so the package can be
built before its source project consumes it.

Run `python3 .performance/dogfood/verify.py` after that build to compile a
disposable consumer copy. It confirms a clean baseline, `SPCT0004` after a
source binding change, `SPCT0004` after a selected Feature claim change, and
`SPCT0001` after a manifest selection change. The copy and its cache stay under
`.artifacts/analysis-dogfood/`; the repository inputs are never changed by this
verification.

The selected project and `0201-F1` scope are declared in
`.specht/manifest.json` and `Directory.Build.props`. XML documentation parsing
is enabled for this selected project, with unrelated missing-comment warnings
suppressed. Its explicit claim and ADR bindings in
`src/analyzers/Bindings/BindingResolver.cs` resolve against the current Feature
and accepted ADR. The package supplies those actual records and the consumer
frontmatter schemas as AdditionalFiles. A successful compile establishes that
the supplied inputs are usable. The run does not establish a performance
finding; OQ-13 records the measurement protocol.
