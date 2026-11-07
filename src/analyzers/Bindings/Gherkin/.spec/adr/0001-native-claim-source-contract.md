---
title: "ADR-0001: Native claim source contract"
description: "The manifest keys, the Gherkin claim reader, the element rules and the diagnostic identities by which tagged Gherkin becomes the claim catalogue"
type: adr
---

# ADR-0001: Native claim source contract

## Status

Proposed — 2026-10-23, `implementer`, from item 0258, reworked the same day after
[review 5635389907](https://github.com/RLittlesII/specht/pull/260#pullrequestreview-5635389907).
It is the design that [decision 0002](../decisions/0002-a-claim-tag-is-the-existing-claim-tag-form.md),
[decision 0003](../decisions/0003-the-analyzer-exports-the-native-claim-catalogue.md)
and OQ-1 to OQ-7 leave open. It depends on no repo-wide ADR beyond ADR-0017 and
ADR-0014. Section 7 states the keys and rules in full.

Revised 2026-11-06 by `spec-author`, after
[decision 0004](../decisions/0004-the-gherkin-claim-reader-lives-in-the-engine.md)
superseded decision 0003. Item 0411 revised items 2, 3 and 10 and the
consequences resting on them: the parser lives in the engine
(`src/specht/Model/`), the analyzer compiles it through a source link, and the
catalogue is not exported. Context and "Why the parser choice" keep the
reasoning as first written.

## Context

The owner's calls fix what native input does, that a claim tag is the existing
`claim.tag` form and `identifiers.claim` grammar, that the analyzer alone reads
claim files, and that it exports the catalogue in the test-evidence artifact.
They leave open how the analyzer parses Gherkin, where the keys sit, which
elements declare, what a case is, how failures and findings are named, how C-1
is held without a shared library, and where the catalogue sits in the artifact.
Item 0259 builds against one answer.

## Decision

1. **Where.** The record keys sit under `codeAnalysis.records`. `claimSource` is
   repository-wide and has the single value `gherkin`; claim file sets are a list
   of `{ glob, feature, epic?, record? }`; the planned and lifecycle tags are
   separate keys beside them. The claim tag encoding is read through one
   explicit mapping, `"/claim": "/claim"`, as `/identifiers` is read; no key
   restates it.
2. **The parser is a narrow hand-written reader in the engine.** It sits in
   `src/specht/Model/` beside the engine's text readers, and the analyzer
   compiles it through a `<Compile Include … Link="Text/…">` link; no project,
   assembly or package is added (decision 0004; ADR-0017). It reads the
   English keywords `Feature`, `Rule`, `Background`, `Scenario`, `Example`,
   `Scenario Outline`, `Scenario Template`, `Examples` and `Scenarios`, tag
   lines, table rows, doc strings and comments, line by line, and nothing else.
   No Gherkin package is referenced. A claim file with a `# language:` header
   other than `en` is an input failure (B-029).
3. **One tag rule, one source file, one test.** The form-and-grammar rule of
   decision 0002 is one engine file, compiled into the engine and the
   analyzer. The engine's `FeatureTagParser` still carries its own token rule,
   so a characterization case list runs against it and the reader and holds
   them to one answer (C-1).
4. **Tag lists are exact tags.** Entries are `@`-led strings compared ordinally
   with Gherkin tag names. Only claim tags honour `claim.tagIgnoreCase`.
5. **What declares.** A Feature, a Rule, a Scenario or a Scenario Outline
   declares the claims whose tags it carries. A claim tag on an `Examples` block
   is a malformed claim tag (decision 0002, B-028).
6. **What a case is.** One Scenario is one case; an Outline has one case per
   Examples row. Planned state is read from the tags in effect for the case:
   Feature, enclosing Rule, the Scenario or Outline and, for a row, its own
   Examples block. A claim's lifecycle tag is read from the declaring element only.
7. **One set per Feature, one Feature per file.** Two claim file sets naming one
   Feature identity, or one file matched by two sets, is an input failure.
8. **A Feature record supplies lifecycle only.** With no record the Feature is
   `current`, a lifecycle value the resolver treats as eligible.
9. **Failures reuse SPCT0001**, one stated message per cause. **Findings take new
   ids**: SPCT0010 duplicate claim, SPCT0011 claim tag where no claim can be
   declared, SPCT0012 scenario without a claim, each at warning (0201-F6
   decision 0001).
10. **The catalogue is not exported.** Item 0411 withdrew the `catalog`
    member with B-030 (decision 0004): no artifact carries it, and 0201-F3
    decision 0010 retires format version 2. `specht evidence` reads claim
    files through the engine's reader (item 0409).
11. **Incomplete runs are not SPCT diagnostics.** A run happens after the build
    that reports SPCT diagnostics; the incomplete-run diagnosis is a typed finding
    in the evidence artifact and report (0201-F5 § 7).

## Why the parser choice

The `Gherkin` package was the only reason a shared library came up: the engine,
the analyzer and the tool would each have needed it, so it would have had to
live somewhere all three reference. With the analyzer the only reader (decision
0003), the package's one remaining cost is in the compiler-loaded closure:
`Gherkin.dll` and `Cucumber.Messages.dll` beside `Specht.Analysis.dll`, each a
dependency ADR-0017 requires a closure test for. The owner questioned it in the
same review. What the catalogue needs from a file is small: element keywords,
their lines, their tags, the rows of an `Examples` table, and enough doc-string
and comment handling not to misread their content.

## Consequences

- A claim file in another Gherkin language is an input failure, not read
  (B-029). Supporting one is a keyword table per language and a new claim.
- The reader is not a Gherkin validator. B-014's "not valid Gherkin" means the
  causes § 7 lists; a file the reference parser rejects for another reason,
  such as a step before any scenario, may still read. The test fixtures and the
  acceptance tree are what hold the reader to the language it accepts.
- C-1 is held by one source file between the engine's reader and the analyzer,
  and by a test between that file and `FeatureTagParser`. A change to
  `FeatureTagParser`'s token rule that the reader does not follow fails the
  characterization test; a rule change nobody adds a case for is not caught.
- The analyzer's closure gains no assembly.
- A version-2 artifact a released analyzer wrote reads as
  `unsupportedContract`; the consumer rebuilds (0201-F3 decision 0010).

## Rejected

- The `Gherkin` package: two assemblies in the compiler-loaded closure for a
  reading the analyzer can do in one class.
- A keyword table for every Gherkin language: no consumer has asked for one.
- Linking `FeatureTagParser.cs` into the analyzer: it needs `SpecStructure` and
  the manifest types behind it, and decision 0004 covers only the Gherkin claim
  reader beside the four text readers.
- A shared library for the tag rule: withdrawn by the owner (ADR-0021).
- A candidate prefix or a `/claimTag` mapping target: decision 0002; a second
  name for a setting the manifest already has.
- `catalog` as an optional member of version 1: version-1 objects are closed, so
  an existing reader would call a new artifact invalid instead of unsupported.
- A `claimSource` per file set: OQ-1 resolved to a repository-wide selection.
- A regular expression or a bare name for tag lists: a tag is a tag.
- Deriving a case's planned state from the declaring element's tags only: an
  `@ignore` on a Rule would not mark the cases that generated tests skip.
- A new SPCT id for each input failure: SPCT0001 already means "analysis
  unavailable" and carries the cause as its message.

## Reversal

None.
