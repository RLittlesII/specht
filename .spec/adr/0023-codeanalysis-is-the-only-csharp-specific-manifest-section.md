---
title: "ADR-0023: codeAnalysis is the manifest's only C#-specific section"
description: "The manifest is technology-neutral at its top level. codeAnalysis holds only what a Roslyn-loaded component needs; a setting the checker reads, or that describes the specification tree, is a top-level key the checker owns, and the analyzer reads it there."
type: adr
---

# ADR-0023: `codeAnalysis` is the manifest's only C#-specific section

## Status

Proposed, 2026-10-26, by `spec-author`, from the owner's direction given the
same day in session: `codeAnalysis` is not a dumping ground; it is the one
thing in the tool that is C#-specific, and the tool is to serve every
technology it can, for the most value. The owner accepts or amends this record
in review.

## Context

- The checker reads Markdown, YAML and Gherkin and no source code; every
  `SPEC###` rule runs on a tree with no .NET project
  (brief § 2, `docs/what-it-checks.md`). Only the `Specht.CodeAnalysis`
  analyzer and `specht evidence` are bound to C# (epic `0201`).
- The manifest's `codeAnalysis` section was added for the analyzer
  (`0201-F3` decision 0005, OQ-1) and is accepted by the check at every shipped
  pin and read by no rule (`docs/configuration.md`).
- `0201-F7` placed the native Gherkin claim settings under it:
  `codeAnalysis.records.claimSource`, `claimFileSets`, `plannedTags`,
  `withdrawnTags`, `supersededTags`, `featureLifecycleKey` and
  `scenarioScopes`. None of them is about C#: each says where a Feature's
  claims live in `.feature` files, which tags carry lifecycle and planning,
  and how a file names its Feature. `0201-F7` § 5 row 7 hands the checker-side
  reading of the same claims to epic `0001`, and `0001-F16` specifies it.
- A consumer with no .NET code - a Flutter application whose agreement is its
  `.feature` files - configures the checker alone. A setting it needs that sits
  under `codeAnalysis` tells that consumer the feature is not for it.
- A key two tools read from two places is two interpretations that happen to
  agree; `0201-F7` C-1 and `0001-F5` decision 0013 hold a claim tag to one.

## Decision drivers

1. One manifest serves a consumer whatever its technology.
2. A reader of the manifest can tell what the checker reads from what only the
   analyzer reads by the key it is under.
3. One key for one setting, read by every tool that needs it.

## Considered options

1. Keep placing a setting where the first tool that needed it put it. The
   analyzer's keys stay under `codeAnalysis`; the checker reads them there too.
2. The top level of the manifest is technology-neutral and the checker owns it;
   `codeAnalysis` holds only what a Roslyn-loaded component needs; the analyzer
   reads a shared setting from the top-level key.
3. One section per tool (`checker`, `codeAnalysis`), each tool reading only its
   own, with a shared setting copied into both.

## Decision

Choose option 2.

- The manifest's top-level keys are technology-neutral and the checker owns
  them: `layouts`, `exclusions`, `sections`, `tables`, `roles`, `markers`,
  `identifiers`, `claim`, `tests`, `matrix`, `rules` and the schema keys today,
  and every key a text-reading rule reads from now on.
- `codeAnalysis` holds only what the Roslyn-loaded analyzer needs and nothing
  the checker reads: project selection, the mappings into a consumer's build
  inputs, required links, reasoned exemptions and the evidence policy. No
  `SPEC###` rule reads a key under it.
- A setting that describes the specification tree - where claims live, which
  tags declare, plan, withdraw or supersede one, how a file names its Feature,
  which frontmatter key carries a lifecycle - is a top-level key, whichever
  tool asked for it first. The analyzer reads it from there.
- A new setting goes under `codeAnalysis` only when it has no meaning without
  a C# compilation. "The analyzer needs it" is not that test.

## Consequences

- `0001-F16` selects native claim input with a top-level key and reads nothing
  under `codeAnalysis` (`0001-F16` OQ-1, C-7).
- `0201-F7` takes a specification delta that moves `records.claimSource`,
  `claimFileSets`, `plannedTags`, `withdrawnTags`, `supersededTags`,
  `featureLifecycleKey` and `scenarioScopes` to the top-level key `0001-F16`
  names, and the analyzer reads them there; its other `records` keys, which map
  consumer frontmatter columns for the analyzer's record reader, are judged by
  the same test when that delta is written. Until the delta lands the analyzer
  reads its keys where they are, and the characterization test of `0201-F7`
  ADR-0001 item 3 holds the two readers to one answer.
- `docs/configuration.md` gains the rule in one sentence; `AGENTS.md` carries
  it as an invariant so no agent places a checker setting under
  `codeAnalysis` again.
- The cost is one move for a section that shipped only in the analyzer's
  private package, and a top-level key that a consumer without the analyzer
  sees members of it will never set. Both are accepted for a manifest a Flutter
  repository reads without finding C# in it.
