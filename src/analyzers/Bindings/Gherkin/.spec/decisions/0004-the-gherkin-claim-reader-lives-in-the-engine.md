---
title: "Decision 0004: The Gherkin claim reader lives in the engine"
description: "The Gherkin claim reader sits in the engine beside the YAML and Markdown text readers, the analyzer compiles it through a source link, and specht evidence reads claim files through it; supersedes decision 0003"
type: decision
---

# Decision 0004: The Gherkin claim reader lives in the engine

**Date:** 2026-11-06
**Decided by:** Repository owner, in chat; recorded by spec-author

## The call

The owner's words, verbatim:

- On the analyzer exporting the claim catalogue in the evidence artifact
  (decision 0003): "This sounds like uncessary coupling. It does not sounds like
  what I would ask you to do."
- Asked where the Gherkin claim reader should live so the tool can use it: "In
  the specht engine".
- Then: "same place as the yaml ... and markdown readers ... right?!"

The recorder reads those words as follows. The placement, the link and the
reading path below are that reading, not the owner's wording.

1. The Gherkin claim reader moves into `src/specht/Model/`, beside
   `FrontmatterTextReader`, `FrontmatterText`, `MarkdownTextReader` and
   `MarkdownTextSection`.
2. The analyzer compiles it through a `<Compile Include … Link="Text/…">` link,
   as `src/analyzers/analyzers.csproj` already compiles those four files. No
   assembly and no package is added.
3. `specht evidence` reads claim files through the engine's reader. The
   test-evidence artifact need not carry a claim catalogue.
4. This supersedes [decision 0003](0003-the-analyzer-exports-the-native-claim-catalogue.md).
5. Item 0411 moves the reader and writes the § 3 change: B-030 withdrawn or
   amended, and every section that rests on decision 0003. This decision
   changes no claim.

## Why

The engine already holds the text readers the analyzer shares, and the analyzer
already compiles them by source link. ADR-0017 (Accepted) says "Reuse the
existing parsing" and adopts no new shared-core assembly. `src/tool` references
`src/specht`, so the tool reaches the reader without a new reference, and one
reader gives one reading of a claim file.

A tension, stated plainly. On
[PR #114](https://github.com/RLittlesII/specht/pull/114), 2026-10-11, the owner
wrote: "We should also stop using MSBuild as a way around circular
references." The owner's 2026-11-06 answer accepts the link pattern for the
engine's text readers. This record follows the later answer and does not widen
it: it covers the Gherkin claim reader beside the four linked text readers, and
no other source link.

## Rejected

- The analyzer exports the catalogue in the test-evidence artifact and the tool
  reads it there (decision 0003): the owner calls it "uncessary coupling".
- A shared assembly both reference (ADR-0021, Rejected): the owner's objection
  of 2026-10-23 on PR #260: "Why do we need a new assembly." and "Part of
  `specht` and `specht.tool` was to keep EVERYTHING for the tool in `specht`."
- A second, separate Gherkin reader in the tool: two readings of one claim file,
  where the owner placed one reader in the engine.

## Affects

Decision 0003 (superseded); B-030; § 5 row 11; § 6 "Catalogue export"; § 7
"Export in the test-evidence artifact"; ADR-0001 items 2, 3 and 10; repo-wide
ADR-0021 § Withdrawal; 0201-F5 decision 0007, § 5 rows 5 and 6, OQ-3; 0201-F3
§ 7 `catalog`; 0201-F6 § 7; items 0302, 0409 and 0411.

## Reversal

None.
