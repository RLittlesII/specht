---
title: "ADR-0021: Gherkin claim semantics live in a netstandard2.0 library"
description: "One small library, Specht.Claims, reads claim tags and case scopes for the engine, the analyzer and the tool, and parses Gherkin with the Gherkin package"
type: adr
---

# ADR-0021: Gherkin claim semantics live in a netstandard2.0 library

## Status

Proposed — 2026-10-23, `implementer`, from item 0258. Needs the repository
owner's acceptance before item 0259 starts: it adds a project and a package.
It does not widen ADR-0017's target exception beyond a second
`netstandard2.0` project, and adopts none of what ADR-0017 and ADR-0014 leave
unadopted: no mediator, no CQRS pipeline, no engine layering, no model-reader
migration.

## Context

0201-F7 reads claims from tagged Gherkin and holds that claim tags have exactly
one interpretation (F7 C-1, 0201-F4 C-1). Three callers need that
interpretation, and they sit on three sides of a target boundary:

- The engine's `FeatureTagParser` (`net10.0;net11.0`) reads claim tags today. It
  also reads `SpecStructure`, a manifest type that stays in the engine.
- The analyzer (`netstandard2.0`, compiler-loaded) must read them from Gherkin
  elements, and cannot reference the engine (0201-F6 § 7, ADR-0017).
- `specht evidence` (`net10.0;net11.0`) must read the same claims, planned
  marks and per-project case scopes, and cannot load the analyzer assembly
  (0201-F5 § 7: "does not reference `Specht.Analysis`").

`src/analyzers/analyzers.csproj` already compiles four engine files through
`<Compile Include>` links (0201-F1 § 7). That is the only reuse mechanism the
analyzer has used. The owner has since directed that projects stop using MSBuild
links to get around a reference they cannot take, and that a type two projects
need goes in a library both reference. No Gherkin package is referenced today,
although `Gherkin` 35.0.0 (`netstandard2.0`, one dependency, `Cucumber.Messages`
29.0.1, itself without dependencies) is already restored by Reqnroll in the test
and acceptance projects.

## Considered options

1. Link `FeatureTagParser.cs` into the analyzer. It cannot compile there without
   `SpecStructure` and the manifest types behind it, so the link grows until it
   is the engine; and it is the mechanism the owner has directed against.
2. Reimplement the interpretation in the analyzer and compare the two with a
   test. Two implementations is a second interpretation that happens to agree.
3. Reference the analyzer assembly from the engine and the tool. The tool is
   specified not to load it, and the engine would take Roslyn-typed code.
4. Retarget the engine to `netstandard2.0`. ADR-0017 rules a whole-engine
   retarget out as a prerequisite.
5. A new `netstandard2.0` library, `Specht.Claims`, referenced by the engine,
   the analyzer and, when it needs them, the tool.

For parsing: a hand-written line reader against the `Gherkin` package. A
hand-written reader is a second Gherkin grammar, to be kept right for keywords,
languages, doc strings, tables and every parse error that B-014 calls invalid. The
package is the reference parser, gives tags and elements their line numbers, and
adds two small `netstandard2.0` assemblies to the analyzer's closure.

## Decision

Choose option 5 and the `Gherkin` package.

`src/claims/claims.csproj` is a `netstandard2.0` project, assembly and root
namespace `Specht.Claims`, with no TFM list (the analyzer's override) and no
reference except the `Gherkin` package. `src/analyzers` and `src/specht` take
`ProjectReference`s to it; `src/tool` and the collector take one when they use
it. It sits in `specht.slnx` under `/src/` beside the analyzer. `Gherkin` is
declared once in `Directory.Packages.props` at `35.0.0`, the version Reqnroll
already resolves, so the test projects see no downgrade.

The library holds the tag-reading semantics and the Gherkin claim reader that
0201-F7 § 7 specifies, and the case-scope expression that 0201-F6 § 7
specifies. It has no I/O and no state beyond its constructor arguments.
`FeatureTagParser` keeps its method and its legacy scanner and delegates each
token to the library, so the engine and the analyzer read one implementation.
The engine's baseline reports do not change.

## Consequences

- C-1 holds structurally: there is one place a tag becomes a claim.
- The analyzer's `*.dll` pack glob carries `Specht.Claims.dll`, `Gherkin.dll` and
  `Cucumber.Messages.dll` into `analyzers/dotnet/cs/` without a packaging change;
  0259 adds a closure test that names them, as ADR-0017 asks of the closure.
- The engine and the tool package carry the two Gherkin assemblies. That cost is
  accepted for one library; splitting the Gherkin reader from the tag semantics
  is the reversal if it matters.
- The four existing source links stay. Replacing them is a separate decision.
- `Specht.Claims` is not added to the package-fed enforcement gate
  (0201-F6 decision 0006); a later decision may.

## Reversal

None.
