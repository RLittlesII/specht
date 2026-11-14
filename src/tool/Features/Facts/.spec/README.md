---
title: "Specification: The facts command"
description: "specht facts gives what the specification tree holds - each Feature's identity, statuses, dependencies, claims, constraints, out-of-scope rows, open questions, deltas, sign-off rows and the records beside it - for the whole tree or a path, as text or one JSON document with a published schema, read through the pinned manifest, writing nothing and exiting 0 whatever the verdict"
type: feature
id: "F2"
epic: "0132"
spec_status: approved
status: ready
priority: med
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Specification queries"
author: "spec-author"
milestone: null
children: []
depends_on: ["0001/F2", "0001/F5", "0001/F6"]
blocks: ["F4"]
spikes: []
created: "2026-10-30"
updated: "2026-11-13"
github_issue: null
synced_at: null
---

# Specification: The facts command

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-30 -->

The check reports what is wrong with a specification tree, and nothing reports what the tree holds. An agent or a maintainer who needs a Feature's claims, constraints, open questions or dependencies reads and searches the Markdown, scrapes a command's output, or answers from memory, and two readers get two pictures. This Feature removes that failure state: `specht facts` gives the tree's facts as one structured answer, read through the same parse and the same pinned manifest the check uses, so that epic `0132`'s MCP host serves it and epic `0228`'s review surfaces build on it.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-30 (A-2, A-3 after review round 1) -->

| #   | Persona                                      | Need                                                       | Pain Point Today                                       |
| --- | -------------------------------------------- | ---------------------------------------------------------- | ------------------------------------------------------ |
| 1   | A coding agent in a consumer repository      | A Feature's claims, constraints and open questions as data | Reads and searches Markdown, or answers from memory    |
| 2   | The maintainer planning or reviewing work    | Every Feature's statuses and dependencies in one answer    | Opens each specification's frontmatter by hand         |
| 3   | Epic `0132` candidate C, reading through MCP | The command-line answer MCP serves unchanged               | N/A (internal dependency; no tree-facts answer exists) |
| 4   | Epic `0228` candidate B, the facts delta     | Two documents of one stable, published shape to compare    | N/A (internal dependency)                              |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                                                       |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | `specht facts` is a working name, recorded as one (the owner, 2026-10-30; decision 0001 call 3).                                                                                                                                                                                 |
| A-2 | The command has no rule id and judges nothing; its document has one shape under every shipped pin, and the sections and tables the pinned manifest names are read through it (the owner, 2026-10-30; decision 0001 call 5, narrowed by decision 0002 call 1; as `0001-F13` A-2). |
| A-3 | The root, the manifest, discovery and the exit codes `2` and `3` are the check's; this Feature depends on `0001-F2`, `0001-F5` and `0001-F6` (the owner, 2026-10-30).                                                                                                            |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-30 (review round 2) -->

| ID    | Claim                                                                                                                                                                                                                                                                    | Source                                                                                                                                        | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------- | ------ |
| B-001 | Given `specht facts [--root <dir>]`, this Feature gives the facts of every Feature specification the check discovers, in discovery order.                                                                                                                                | the owner, 2026-10-30; decision 0001 call 3; `0001-F6`                                                                                        | Active |
| B-002 | Given `specht facts --json`, this Feature writes one document to stdout, and it validates against the published facts schema (OQ-1).                                                                                                                                     | the owner, 2026-10-30; decision 0001 call 4                                                                                                   | Active |
| B-003 | Given `--json`, each specification carries its root-relative path, its qualified Feature id, its `spec_status` and its `status`.                                                                                                                                         | the owner, 2026-10-30; decision 0001 call 1; epic `0228` R1                                                                                   | Active |
| B-004 | Given `--json`, each specification carries its `depends_on`, `blocks` and `children` as its frontmatter lists them.                                                                                                                                                      | the owner, 2026-10-30; decision 0001 call 1; epic `0228` R1; `children` is spec-author's reading                                              | Active |
| B-005 | Given `--json`, each § 3 claim carries its id, its text, its status and its line.                                                                                                                                                                                        | the owner, 2026-10-30; decision 0001 call 1; epic `0228` R1                                                                                   | Active |
| B-006 | Given `--json`, each § 4 constraint carries its id, its text, what it rules out and its line.                                                                                                                                                                            | the owner, 2026-10-30; decision 0001 call 1; epic `0228` R1; C-7                                                                              | Active |
| B-007 | Given `--json`, each § 5 row carries its number, its item, its exclusion reason and its line.                                                                                                                                                                            | the owner, 2026-10-30; decision 0001 call 1; epic `0228` R1; C-7                                                                              | Active |
| B-008 | Given `--json`, each § 11 row carries its id, its question, its Blocks cell, its resolution, its line, and whether it is resolved as OQ-2 reads it.                                                                                                                      | the owner, 2026-10-30; decision 0001 call 1; epic `0228` R1; C-7; OQ-2                                                                        | Active |
| B-009 | Given `--json`, each § 10 entry carries its line and its text: a list item's text, or a table row's cells.                                                                                                                                                               | the owner, 2026-10-30; decision 0001 call 1; epic `0228` R1; C-7; the entry's shape is spec-author's reading                                  | Active |
| B-010 | Given `--json`, each § 12 row carries its section, its status, its reviewer, its note and its line.                                                                                                                                                                      | the owner, 2026-10-30; decision 0001 call 1; epic `0228` R1                                                                                   | Active |
| B-011 | Given decision, ADR or lesson records in the `decisions/`, `adr/` and `lessons/` folders beside a specification, this Feature gives each with its kind, its number from its file name, its frontmatter `title`, its status as OQ-3 reads it, and its root-relative path. | the owner, 2026-10-30; decision 0001 call 1; epic `0228` R1; the status, the owner, 2026-10-30 (decision 0002 call 2); how it is read is OQ-3 | Active |
| B-012 | Given a claim whose § 3 Status is the withdrawn or superseded marker, this Feature still gives the claim, with that status.                                                                                                                                              | AGENTS.md § "Stable IDs"; spec-author's reading                                                                                               | Active |
| B-013 | Given a tree the check reports violations for, this Feature gives every fact that parses, two claims sharing an id included, and exits `0`.                                                                                                                              | the owner, 2026-10-30; decision 0001 call 5; as `0001-F13` B-006                                                                              | Active |
| B-014 | Given `--json`, the document carries no violation, no count and no verdict.                                                                                                                                                                                              | the owner, 2026-10-30; decision 0001 call 5; C-5                                                                                              | Active |
| B-015 | Given a specification missing a section, or with a table the parse cannot read, this Feature gives no facts for that section and every other fact of that specification.                                                                                                 | the owner, 2026-10-30; decision 0001 call 5 ("give what parses")                                                                              | Active |
| B-016 | Given a manifest whose `sections` list a heading other than the default, whose `roles` name the claims or the sign-off section otherwise, or whose `tables` name the sign-off table's headers otherwise, this Feature reads those facts from what the manifest names.    | the owner, 2026-10-30; decision 0001 call 5; decision 0002 call 1 as amended 2026-10-30; `0001-F5`; C-7                                       | Active |
| B-017 | Given the same specifications under two shipped pins, this Feature's two `--json` documents carry the same members.                                                                                                                                                      | the owner, 2026-10-30; decision 0001 call 5; epic `0132` "two schema versions" criterion                                                      | Active |
| B-018 | Given `specht facts <path>` naming a discovered specification's root-relative path, this Feature gives that specification's facts only.                                                                                                                                  | the owner, 2026-10-30; decision 0001 call 3                                                                                                   | Active |
| B-019 | Given `specht facts <path>` naming a root-relative directory, this Feature gives the facts of every discovered specification under it, in discovery order.                                                                                                               | spec-author's reading; OQ-4                                                                                                                   | Active |
| B-020 | Given `specht facts <path>` selecting no discovered specification, this Feature writes nothing on stdout and exits `4`.                                                                                                                                                  | the owner, 2026-10-30; decision 0001 call 3; `0001-F2` decision 0003                                                                          | Active |
| B-021 | Given a missing root or manifest this Feature exits `2`, and given a manifest the check rejects it exits `3`, as the check does.                                                                                                                                         | `0001-F2` B-006, B-007; as `0001-F13` B-006                                                                                                   | Active |
| B-022 | Given the same tree and arguments, this Feature gives byte-identical output.                                                                                                                                                                                             | brief § 9; as `0001-F13` B-007                                                                                                                | Active |
| B-023 | Given `specht facts`, no file under the root or elsewhere is created, modified or deleted.                                                                                                                                                                               | brief § 9; AGENTS.md § Invariants                                                                                                             | Active |
| B-024 | Given `specht facts` without `--json`, this Feature prints one line per claim, constraint, § 5 row and open question, each beginning with its specification's root-relative path.                                                                                        | spec-author's reading; OQ-5                                                                                                                   | Active |
| B-025 | Given `--json`, a claim carries no § 9 row, no scenario and no test.                                                                                                                                                                                                     | the owner, 2026-10-30; decision 0001 call 2                                                                                                   | Active |
| B-026 | Given an epic file the check discovers, this Feature gives no facts for it.                                                                                                                                                                                              | spec-author's reading; OQ-6                                                                                                                   | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-30 (review round 2) -->

| ID  | Constraint                                                                                                                                                                                                                                                                                                                                                                                                                                   | Rules Out                                                                                                                                      |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | The facts are read by the discovery and the parse the check uses (`0001-F6`; AGENTS.md § Architecture); where the check reads a section, the facts find and read it the same way.                                                                                                                                                                                                                                                            | A second Markdown, YAML or table parser; a fact disagreeing with a violation about a claim's id, line or section.                              |
| C-2 | Offline and read-only; every path is relative to the root with `/` separators (brief § 9).                                                                                                                                                                                                                                                                                                                                                   | A network call; any write, a cache file included; an absolute path in a line or in the document.                                               |
| C-3 | Within a tool major version the document changes only by addition (the owner, 2026-10-30; decision 0001 call 4; as `0001-F3` C-4).                                                                                                                                                                                                                                                                                                           | Renaming or removing a member, or changing a member's type, inside a major version.                                                            |
| C-4 | A command parses, calls the engine and folds the result into an exit code (`0001-F2` C-4).                                                                                                                                                                                                                                                                                                                                                   | Reading Markdown, YAML or record files inside `Features/Facts/`.                                                                               |
| C-5 | A fact is given, never judged: the command has no rule id and its shape does not vary with the pin (the owner, 2026-10-30; decision 0001 call 5).                                                                                                                                                                                                                                                                                            | A severity, a violation or a verdict in the output; a new `SPEC###`; a member present under one pin and absent under another.                  |
| C-6 | Every fact MCP gives about the tree is one this Feature gives (epic `0132` § Constraints).                                                                                                                                                                                                                                                                                                                                                   | A tree fact, or a member of one, available through MCP and not here.                                                                           |
| C-7 | Only the `sections`, `roles` and `tables` keys the shipped manifest declares are read from the manifest: every section is found under the heading the manifest's `sections` lists, as the check finds it, and the table headers inside the constraints, out-of-scope, lessons and deltas, and open-questions sections are the engine's, since no shipped key names them (the owner, 2026-10-30; decision 0002 call 1 as amended 2026-10-30). | A manifest key for those four tables added by this Feature; waiting on a `0001-F5` delta; a heading or a header that differs from the check's. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-30 -->

| #   | Item                                                                | Exclusion Reason                                                                                        |
| --- | ------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| 1   | Claim coverage: § 9 rows, scenarios carrying a tag, test resolution | `0001-F13` (`specht trace`); the owner, 2026-10-30; decision 0001 call 2; B-025.                        |
| 2   | The verdict, a violation or a count                                 | The check (`0001-F2`, `0001-F3`); B-014.                                                                |
| 3   | Selecting a specification by id, such as `0001-F3`                  | Qualified-key resolution is epic `0228` candidate C and item `0307`; decision 0001 call 3.              |
| 4   | Serving the facts through MCP                                       | Epic `0132` candidates B and C; this Feature is the command-line answer they serve.                     |
| 5   | Any write, `--report <path>` included                               | Not asked for; C-2.                                                                                     |
| 6   | Work items and the work queue                                       | Epic `0132` candidate E, parked; the item schema differs between this repository and its consumers.     |
| 7   | Comparing two facts documents                                       | Epic `0228` candidate B.                                                                                |
| 8   | A record's body, or a specification's §§ 1, 2 and 6 to 8 as text    | Not asked for: epic `0228` R1 names the sections given; complete documents are spike `0252`'s question. |
| 9   | A persistent index or cache of the facts                            | `0132-F1`; C-2.                                                                                         |
| 10  | Epic files                                                          | Not decided (OQ-6); B-026 proceeds on the default.                                                      |

## 6. Concern Separation

<!-- last written by: implementer, 2026-11-13 -->

| Concern                                                                                                                                                       | Classification | Owner                                                                                                                                          |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| Which files are Feature specifications, in what order, and that an epic file is not one (B-001, B-026)                                                        | Business       | The check's discovery, `ISpecDiscovery`, through `SpechtModelReader.Read` (C-1)                                                                |
| A specification's path, qualified id, statuses and dependency lists as its frontmatter gives them (B-003, B-004)                                              | Business       | `FeatureSpec` and `SpechtModel.FeaturesWithIdentity`, projected by `FactsDocument.From`                                                        |
| Root, manifest and pin acceptance, and the exceptions that carry their failure (B-021)                                                                        | Technical      | `SpechtModelReader`, `SpechtManifestStore`, `SpechtManifestParser`                                                                             |
| The document's members and their fixed shape under every pin (B-002, B-014, B-017; C-3, C-5)                                                                  | Both           | `FactsDocument` and `FeatureFacts`, held to `docs/schema/facts.schema.json`                                                                    |
| Byte-identical serialization (B-022)                                                                                                                          | Technical      | `FactsDocument.ToJson`                                                                                                                         |
| Parsing the options, writing stdout and stderr, folding exceptions into exit codes (B-021, B-023; C-4)                                                        | Both           | `FactsCommand`                                                                                                                                 |
| A claim's id and line as the check reads them, every row kept, two sharing an id included (B-005, B-013; C-1)                                                 | Business       | `SpechtClaims` through `SpechtModel.Claims`, projected by `FactsDocument.From`                                                                 |
| A claim's text, and its status as its row writes it (B-005, B-012; C-5)                                                                                       | Business       | `FactsDocument.From`, reading the claims table's cells by header                                                                               |
| Finding the constraints section under the heading `sections` lists, and its cells under the engine's headers (B-006; C-1, C-7)                                | Both           | `FactsDocument.From` through `SpechtDocument.Section`                                                                                          |
| A claim's and a constraint's members, with no coverage member (B-025; C-3)                                                                                    | Both           | `ClaimFacts` and `ConstraintFacts`, held to `docs/schema/facts.schema.json`                                                                    |
| Finding the out-of-scope and open-questions sections under the headings `sections` lists, and their cells under the engine's headers (B-007, B-008; C-1, C-7) | Both           | `FactsDocument.From` through `SpechtDocument.Section`                                                                                          |
| Whether an open question is resolved, on OQ-2's default, its Resolution given as written beside it (B-008; C-5)                                               | Business       | `FactsDocument.From`                                                                                                                           |
| A missing section, or one with no table the parse reads, emptying only its own member (B-015; C-1)                                                            | Both           | `MarkdownTextReader` through `SpechtDocumentParser`, which gives such a section no table; `FactsDocument.From`, which reads each section alone |
| An out-of-scope row's and an open question's members (B-007, B-008; C-3)                                                                                      | Both           | `OutOfScopeFacts` and `OpenQuestionFacts`, held to `docs/schema/facts.schema.json`                                                             |
| Which files are the records beside a specification, in both layouts, listed without reading them (B-011, B-022; C-2)                                          | Business       | `ISpecDiscovery.FindRecords` in `SpechtDiscovery`                                                                                              |
| Reading the records for the facts alone, the check's read left as it is, so a record is never a specification and no verdict changes (B-011; C-4)             | Technical      | `SpechtModelReader.ReadRecords` through `SpechtTreeStore`                                                                                      |
| A record's frontmatter and the first line under its `## Status` heading, through the check's own frontmatter reader and Markdown parse (B-011; C-1)           | Technical      | `SpechtModelBuilder.Record` through `FrontmatterReader` and `SpechtDocumentParser.FirstLine`, giving a `RecordFile`                            |
| A record's kind, number, title and path, and its status on OQ-3's default, a missing or unreadable part emptying only its own member (B-011, B-013; C-5)      | Business       | `FactsDocument.From`                                                                                                                           |
| A record's members (B-011; C-3)                                                                                                                               | Both           | `RecordFacts`, held to `docs/schema/facts.schema.json`                                                                                         |
| Calling the records read beside the model read (C-4)                                                                                                          | Technical      | `FactsCommand`                                                                                                                                 |

## 7. Technical Design

<!-- last written by: implementer, 2026-11-13 -->

The design for items `0364`, `0365`, `0366` and `0368`, as built. Each built type is declared in the file it links; the compiler is the authority on its members and
signatures.

### Reading the tree

The command calls the check's own read,
[`SpechtModelReader.Read(root, null)`](../../../../specht/Model/SpechtModelReader.cs), the same call
[`SpechtRunner.Run`](../../../../specht/SpechtRunner.cs) makes, with the manifest's own schema source. That one call
checks the root, reads, parses and pins the manifest, compiles the selected frontmatter schemas, discovers through
[`SpechtDiscovery`](../../../../specht/Discovery/SpechtDiscovery.cs), loads through `SpechtTreeStore` and parses through
[`SpechtModelBuilder`](../../../../specht/Model/SpechtModelBuilder.cs), so every fact comes from the parse the check
evaluates and no second parser exists (C-1). No rule runs: the model is projected, never evaluated, so nothing in the
document can be a violation, a count or a verdict (B-014, C-5). [`MintCommand`](../../Mint/MintCommand.cs) already
injects `SpechtModelReader` directly; this follows it.

The read throws the check's typed exceptions, and the command folds them exactly as
[`CheckCommand`](../../Check/CheckCommand.cs) and [`RulesCommand`](../../Rules/RulesCommand.cs) do (B-021):
`SpechtRootNotFoundException` and `SpechtManifestNotFoundException` exit `ExitCodes.MissingInput` (`2`);
`SpechtManifestUnreadableException` and `SpechtManifestException`, an unshipped pin and a rejected disk schema
included, exit `ExitCodes.InvalidManifest` (`3`). Each writes one `specht: ` line to stderr and nothing to stdout.

### The document

Two records in the engine folder `src/specht/Facts/`, namespace `Specht.Facts`, and the two row records of § "Claims
and constraints". The folder sits beside `Report/` in ADR-0010's list and reads only `Model/` below it.

- [`FactsDocument`](../../../../specht/Facts/FactsDocument.cs): the document. Its `From` projects a model, as
  [`SpechtReportDocument.From`](../../../../specht/Report/SpechtReportDocument.cs) makes the report: a function of the
  model and the records it is handed alone (§ "Records beside a specification"), with no clock, machine or environment
  in it. Its `ToJson` serializes it.
- [`FeatureFacts`](../../../../specht/Facts/FeatureFacts.cs): one discovered Feature specification.

`From` walks `model.Features`, which `SpechtModelBuilder` fills in the listing's order, so the document lists every
Feature specification in discovery order (B-001). `model.Epics` is never read, so an epic file gives no facts (B-026,
OQ-6); a Feature specification in the legacy `epics/**/spec.md` layout is a Feature and is given. Each member is taken
from a [`FeatureSpec`](../../../../specht/Model/FeatureSpec.cs) as the check reads it:

| Member       | Value                                                                                                                                       | Claim |
| ------------ | ------------------------------------------------------------------------------------------------------------------------------------------- | ----- |
| `path`       | `FeatureSpec.RelativePath`: root-relative, `/`-separated, from `SpechtLocation`                                                             | B-003 |
| `id`         | `FeatureSpec.Identity` when the Feature is in `SpechtModel.FeaturesWithIdentity`, else `null`; the check's identity form, such as `0132-F2` | B-003 |
| `specStatus` | `FeatureSpec.SpecStatus`, read through the manifest's frontmatter key, or `null`                                                            | B-003 |
| `status`     | `FeatureSpec.Status`, or `null`                                                                                                             | B-003 |
| `dependsOn`  | `FeatureSpec.Strings` under the Feature's `Keys.DependsOn`: the list as written, unresolved, `[]` when absent                               | B-004 |
| `blocks`     | `FeatureSpec.Strings` under `Keys.Blocks`                                                                                                   | B-004 |
| `children`   | `FeatureSpec.Strings` under `Keys.Children`                                                                                                 | B-004 |

The dependency lists come from the frontmatter, not from `SpechtDependencyGraph`, which resolves edges to identities:
B-004 gives the lists as the frontmatter has them, and an edge the check cannot resolve is still given.

Every member is always present, with `null` or `[]` when the frontmatter has no value, so a reader never has to tell an
absent member from an empty one and no member's presence depends on a pin (B-017, C-5). Member names are the role, not
the consumer's frontmatter key: from schema `0.3.0` the manifest's `featureKeys` can rename `spec_status`, and the
member stays `specStatus` (`0001-F5` B-004). Names are camelCase, as in the published report document
(`docs/schema/report.schema.json`, `0001-F3`).

A Feature's document for this repository's own specification, its `claims`, `constraints`, `outOfScope`,
`openQuestions` and `records` left out (§ "Claims and constraints", § "Out-of-scope rows and open questions", § "Records
beside a specification"):

```json
{
  "specifications": [
    {
      "path": "src/tool/Features/Facts/.spec/README.md",
      "id": "0132-F2",
      "specStatus": "approved",
      "status": "ready",
      "dependsOn": ["0001/F2", "0001/F5", "0001/F6"],
      "blocks": ["F4"],
      "children": []
    }
  ]
}
```

`ToJson` uses `System.Text.Json` with options held in one `static readonly` field, as `SpechtReportDocument` does:
indented, `\n` line endings whatever the platform, camelCase names, and, unlike the report, nulls written. Members come
out in declaration order and lists in model order, so the same tree and arguments give the same bytes (B-022).

### Claims and constraints

Item `0365`'s design, as built. `FeatureFacts` has two members after `children`, added there so every earlier member
keeps its name, type and place (C-3):

| Member        | Value                                                                                                              | Claim               |
| ------------- | ------------------------------------------------------------------------------------------------------------------ | ------------------- |
| `claims`      | One `ClaimFacts` per row of `SpechtModel.Claims[feature].Rows`, in row order; `[]` with no claims section or table | B-005, B-012, B-013 |
| `constraints` | One `ConstraintFacts` per body row of the constraints section, in row order; `[]` with no such section or table    | B-006               |

Two records in `src/specht/Facts/`, carrying values and no logic:

- [`ClaimFacts`](../../../../specht/Facts/ClaimFacts.cs), written as `id`, `text`, `status` and `line`.
- [`ConstraintFacts`](../../../../specht/Facts/ConstraintFacts.cs), written as `id`, `text`, `rulesOut` and `line`.

**The claims.** `From` reads `model.Claims[feature]`, the [`SpechtClaims`](../../../../specht/Model/SpechtClaims.cs) the
check's claim rules read, found under the manifest's `roles.claims` heading (C-1). Each `SpechtClaim` gives `id`, its
trimmed first cell as written (`""` when the cell is empty), and `line`, its one-based row line, so no fact can disagree
with a violation about a claim's id or line. Every row is given: two rows sharing an id both (B-013), and a row whose id
the claim grammar refuses, since the row parses and judging its id is the check's (C-5). `Rows` runs parallel to
`Section.Rows`, so row _i_'s `text` and `status` are the cells of `Section.Rows[i]` under the headers `Claim` and
`Status`, which every shipped template's claims table carries (`ID | Claim | Source | Status`). The status is the cell
as written: a withdrawn claim carries `Withdrawn`, the text the manifest's `withdrawn` marker writes, and under a pin
with no markers whatever its row writes (B-012). Nothing maps it to a marker or tests it against one.

**The constraints.** No role names the constraints section, so `From` takes its heading from the manifest's `sections`:
the entry `4. Constraints`, which every shipped manifest lists and `FactsDocument` holds as one private constant, read
through `feature.Document.Section` when `model.Schemas.Structure.Sections` lists it, as `SectionContractRule` finds a section by the titles `sections` lists
(C-1, C-7). Under a manifest whose `sections` does not list it, `constraints` is `[]`; a heading the manifest renames is
item `0367`'s (B-016). A row's `id` is its first cell, as the check reads a constraint id for `mint`
(`SpechtDocumentParser.FirstCells`, `0001-F19`), and `line` its row line; `text` and `rulesOut` are the cells under the
engine's headers `Constraint` and `Rules Out` (C-7).

**The cells.** One private `Cell(SpechtSection section, int row, string header)` in `FactsDocument` serves both: the
index of `header` in `section.Headers`, compared ordinally as `TableHeaderRule` compares headers, and the row's cell at
that index. A header the table lacks, or a row shorter than that index, gives `null`; an empty cell gives `""`. A cell's
text is the parse's, from `MarkdownTextReader`: its literals and code-span contents, trimmed, with no Markdown marker -
the text the check reads, not the source bytes.

Neither record has a traceability row, scenario or test member, and `From` reads no `MatrixSections`, companion or test
file (B-025). `From` still runs no rule, so a tree the check fails gives every row and the command exits
`ExitCodes.Success` on the path item `0364` built (B-013). The command does not change (C-4).

```json
{
  "claims": [
    {
      "id": "B-001",
      "text": "Given a tree, this Feature gives its facts.",
      "status": "Active",
      "line": 61
    },
    {
      "id": "B-001",
      "text": "Given a flag, this Feature writes one document.",
      "status": "Withdrawn",
      "line": 62
    }
  ],
  "constraints": [
    {
      "id": "C-1",
      "text": "Offline and read-only.",
      "rulesOut": "A network call.",
      "line": 68
    }
  ]
}
```

The schema gains, on each specification item, `claims` and `constraints` in `properties` and in `required`. Each is an
array of objects with `additionalProperties: false`: a claim requires `id` (string), `text` and `status`
(`["string", "null"]`) and `line` (integer, minimum `1`); a constraint the same, with `rulesOut` in place of `status`.
No existing member, type or constraint of the schema changes (C-3). The `specht facts` paragraph in
[`docs/cli.md`](../../../../../docs/cli.md) names the claims and constraints among what is printed.

### Out-of-scope rows and open questions

Item `0366`'s design, as built. `FeatureFacts` has two members after `constraints`, added there so every earlier member
keeps its name, type and place (C-3):

| Member          | Value                                                                                                                | Claim        |
| --------------- | -------------------------------------------------------------------------------------------------------------------- | ------------ |
| `outOfScope`    | One `OutOfScopeFacts` per body row of the out-of-scope section, in row order; `[]` with no such section or table     | B-007, B-015 |
| `openQuestions` | One `OpenQuestionFacts` per body row of the open-questions section, in row order; `[]` with no such section or table | B-008, B-015 |

Two records in `src/specht/Facts/`, each in a file of its own name, carrying values and no logic:

- [`OutOfScopeFacts`](../../../../specht/Facts/OutOfScopeFacts.cs), written as `number`, `item`, `reason` and `line`.
- [`OpenQuestionFacts`](../../../../specht/Facts/OpenQuestionFacts.cs), written as `id`, `question`, `blocks`,
  `resolution`, `resolved` and `line`.

**The sections.** No role names either section, so `From` finds each as it finds the constraints: under the `sections`
entries `5. Out of Scope` and `11. Open Questions`, which every shipped manifest lists, `0.1.0` to `0.4.0`, held as
private constants beside `ConstraintsTitle` and read through `feature.Document.Section` only when
`model.Schemas.Structure.Sections` lists the title (C-1, C-7). With three sections found this way, the one
`constraintsListed` test became one private method, `Listed`, giving the section titled by a constant when `sections`
lists it, and `null` otherwise, serving all three; the constraints keep their reading. A heading the manifest renames is item
`0367`'s (B-016).

**The rows.** An out-of-scope row's `number` and an open question's `id` are the row's first cell, as a constraint's
`id` is and as the check reads an open-question id for `mint` (`HeldIds`, through `SpechtDocumentParser.FirstCells`):
the string the parse gives, `""` when the row has none, never parsed into a number; one private `FirstCell` gives it for
the constraints, out-of-scope rows and open questions alike. `line` is `section.RowLines[row]`.
The other cells come through `Cell` under the engine's headers (C-7), which every shipped template's tables carry,
`0.1.0` to `0.4.0`: `Item` and `Exclusion Reason` give `item` and `reason`; `Question`, `Blocks` and `Resolution` give
`question`, `blocks` and `resolution`. Each is the cell's text as the parse gives it, `null` under a header the table
lacks; `blocks` is the cell's text, not split into ids. Every body row is given, one whose first cell is no
open-question id included.

**Whether a question is resolved.** OQ-2's default, `0228-F1` B-009's reading, in one private `Resolved` in
`FactsDocument`: `resolved` is `false` when the `resolution` is `null` or `""`, or its first word is the open marker in
exact case, and `true` for any other resolution. The first word is the resolution's text up to its first character that is neither a letter nor a digit
(`char.IsLetterOrDigit`), compared ordinally. So `Open`, `Open. Proceeds on the default.`, `Open; …` and `Open 2026-11-…`
read unresolved, and `Resolved 2026-10-30 …`, `RESOLVED, …`, `Superseded …`, `open` and `Opened …` read resolved. The
text is the parse's, so a cell written `**Open**` reads as `Open`.

The open marker read is `Open`, one private constant in `FactsDocument`. The manifest's markers live in
[`SpechtStructure.Markers`](../../../../specht/Manifest/SpechtStructure.cs), which `SpechtManifestParser` fills from the
pinned shipped manifest's `markers` and overlays with the consumer's. No shipped manifest declares an `open` marker:
`0.1.0` declares `missing`, `draft`, `blocked` and `approved`, `0.2.0` adds `covered`, `partial`, `waived`, `withdrawn`
and `superseded`, and `0.3.0` adds `done`, which `0.3.1` and `0.4.0` keep. So under every shipped pin OQ-2's default reads
`Open`. `From` reads no entry of `Markers`: C-7 reads only `sections`, `roles` and `tables` from the manifest, and a
consumer's `markers.open` is a name the parser keeps without checking and no rule reads. The `resolution` is given as
written beside `resolved`, so a reader can apply a reading of its own (C-5).

**A missing section or an unreadable table.** Each row member is read from its own section alone and no read throws, so
a fault in one section empties that member and no other (B-015). A specification gives no facts for a section when:

- the section is absent, and `SpechtDocument.Section` gives `null`;
- the manifest's `sections` does not list its heading;
- the section has no table the parse reads. `SpechtDocumentParser` reads tables with Markdig's pipe-table extension,
  which recognises a table only as a header row followed by a delimiter row (`| --- |`); pipe-separated lines with no
  delimiter row, or a malformed one, parse as a paragraph. `MarkdownTextReader` gives a section the first table between
  its heading and the next `##` heading, so a section with none has empty `Headers`, `Rows` and `RowLines`, and its
  member is `[]`. This is B-015's table the parse cannot read: the section the check sees with no table (C-1). A section
  reading `None.` is another. Where a readable table follows an unreadable one inside the section, that table is the
  section's, for the check and the facts alike.

A table the parse reads whose headers are not the engine's is read: each row is given with its first cell and line, and
`null` under each engine header the table lacks, as the claims and constraints are. `From` still runs no rule, and the
command does not change (C-4).

```json
{
  "outOfScope": [
    {
      "number": "1",
      "item": "Any other input",
      "reason": "Owned by a sibling Feature",
      "line": 77
    }
  ],
  "openQuestions": [
    {
      "id": "OQ-1",
      "question": "Which input comes first?",
      "blocks": "B-001",
      "resolution": "Open. Proceeds on the default.",
      "resolved": false,
      "line": 112
    },
    {
      "id": "OQ-2",
      "question": "Which output comes last?",
      "blocks": "B-002",
      "resolution": "Resolved 2026-10-30 by the owner: the answer",
      "resolved": true,
      "line": 113
    }
  ]
}
```

The schema gains, on each specification item, `outOfScope` and `openQuestions` in `properties` and in `required`, so an
item requires eleven members. Each is an array of objects with `additionalProperties: false`: an out-of-scope row
requires `number` (string), `item` and `reason` (`["string", "null"]`) and `line` (integer, minimum `1`); an open
question requires `id` (string), `question`, `blocks` and `resolution` (`["string", "null"]`), `resolved` (boolean) and
`line` (integer, minimum `1`). No existing member, type or constraint of the schema changes (C-3). The `specht facts`
paragraph in [`docs/cli.md`](../../../../../docs/cli.md) names the out-of-scope rows and open questions among what is
printed.

### Records beside a specification

Item `0368`'s design, as built. `FeatureFacts` has one member, `records`, at its end, after `openQuestions`, since item
`0367` has not landed. Every earlier member keeps its name, type and place (C-3):

| Member    | Value                                                                                                                | Claim        |
| --------- | -------------------------------------------------------------------------------------------------------------------- | ------------ |
| `records` | One `RecordFacts` per record beside the specification, in ordinal order of root-relative path; `[]` when it has none | B-011, B-022 |

One record in `src/specht/Facts/`, [`RecordFacts`](../../../../specht/Facts/RecordFacts.cs), carrying values and no
logic, written as `kind`, `number`, `title`, `status` and `path`.

**Beside a specification.** A record is a file whose name ends `.md`, directly inside a folder named `adr`,
`decisions` or `lessons` in the folder that holds the specification file, `SpechtLocation.Directory`. In the co-located
layout that folder is the specification's `.spec/`, so this Feature's decisions are
`src/tool/Features/Facts/.spec/decisions/*.md`; in the legacy layout it is the Feature's own folder, so
`epics/0001/F1/adr/*.md` sits beside `epics/0001/F1/spec.md`. One rule serves both layouts, as items and companions are
found beside the specification file in both. Not a record: a file in a folder below one of the three
(`adr/archive/`), a file of another extension, a folder whose name differs in case, the repository-wide `.spec/adr/`
and `.spec/lessons/` at the root, which sit beside no specification (the root `.spec/` holds none and the default
exclusions skip it, `0001-F6` decision 0003), and the folders beside an epic file, which gives no facts (B-026).

**The listing.** [`ISpecDiscovery`](../../../../specht/Discovery/ISpecDiscovery.cs) has one finder for it, implemented
in [`SpechtDiscovery`](../../../../specht/Discovery/SpechtDiscovery.cs): `FindRecords(root, specification, folder)`, the
root-relative path of every file whose name ends `.md`, compared ordinally, directly in the folder named `folder` beside
one specification, in ordinal order of file name; `[]` when there is no such folder. It reuses what `FindCompanions`
does through the private `Beside`: it lists through the `IFileSystem` in both discovery modes, so a record git ignores is
found as a companion is. `Beside` now takes a root-relative directory where it took the location, so the item and
companion finders hand it `specification.Directory` and `FindRecords` hands it the record folder's path; two private
helpers beside it, `Under`, which joins the root to a root-relative directory, and `Joined`, which prefixes a name with
one, serve all three. `FindRecords` compares `folder` ordinally against the child folders the file system lists, so a
path carries the folder's name as it is written, and it enters no folder that is a link, as the walk enters none
(`0001-F6` B-011). It opens no file, and no exclusion applies, as none applies to a companion beside a discovered
specification. `0001-F6` § 7's table of finders names it.

**The read.** [`SpechtModelReader`](../../../../specht/Model/SpechtModelReader.cs) has
`ReadRecords(root, features)`, beside `Read`. For each Feature in order it calls `FindRecords` for `adr`, `decisions`
and `lessons`, in that ordinal order, so each Feature's records come out in ordinal order of root-relative path (B-022);
it loads every listed path, once, in one `SpechtTreeStore.Load` and hands each text to `SpechtModelBuilder.Record`. It
returns one list per Feature, keyed by `FeatureSpec` as `SpechtModel.Claims` is, as an
`IReadOnlyDictionary<FeatureSpec, IReadOnlyList<RecordFile>>`; a Feature with no record has an empty list. The folder-to-kind table - `adr` to `adr`,
`decisions` to `decision`, `lessons` to `lesson` - is one private static in the reader, the one place that names the
folders; the kinds are the three record types of the manifest's `ids` key and the `type` each shipped record template
writes. No manifest key is read: the folders are the engine's (C-7), so the records are the same under every pin
(B-017).

`Read` does not change. The check lists, loads and parses no record, `SpechtListing` and `SpechtModel` gain no member,
and no rule sees a record, so no verdict, count or check timing changes, and a record is never discovered as a
specification. Only the facts call `ReadRecords`.

**The parse.** [`SpechtModelBuilder.Record(path, kind, text)`](../../../../specht/Model/SpechtModelBuilder.cs) gives a
[`RecordFile`](../../../../specht/Model/RecordFile.cs), a record in `src/specht/Model/` beside
[`EpicFile`](../../../../specht/Model/EpicFile.cs), carrying the path, the kind, the frontmatter through the existing
`FrontmatterReader.Parse`, and `StatusLine`. `StatusLine` comes from an entry point on the existing parser,
[`SpechtDocumentParser.FirstLine(text, title)`](../../../../specht/Model/SpechtDocumentParser.cs), beside `FirstCells`,
over the same pipe-table pipeline and delegating to `MarkdownTextReader.FirstLine` as `FirstCells` does, so no second
parser exists (C-1); the builder holds the title `Status` as one private constant. The inline flattening a cell and a
heading already used is one private `Visible` in `MarkdownTextReader`, which `Flatten` and the status line share, so a
line is flattened exactly as a cell is. `FirstLine` finds the first level-2 heading whose
text is `Status`, compared ordinally as `SpechtDocument.Section` compares a title; takes the first paragraph, in
document order, before the next level-2 heading, one inside a list or a quote included; and gives that paragraph's
inlines up to its first line break, flattened as a cell is: literals and code-span contents, trimmed. It is `null` with
no such heading or no paragraph under it. The builder reads the line for every record, whatever its kind, so OQ-3's
reading lives whole in `FactsDocument`. `SpechtDocumentParser.Parse` and `SpechtSection` do not change.

**The members.** `FactsDocument.From` takes the records beside the model, `From(model, records)`; a Feature the map
lacks gives `[]`. It stays a function of what it is given (B-022). Each `RecordFile` gives:

| Member   | Value                                                                                                                                                                                                                                                                              |
| -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `kind`   | `RecordFile.Kind`: `decision`, `adr` or `lesson`, from the folder                                                                                                                                                                                                                  |
| `number` | The file name's first word: `0001` for `0001-the-title.md`, and `k3m9xq2a` for a record named by a Nano ID minted under schema `0.4.0` (`0001-F19` B-020). A string, never parsed into a number; `null` when the name begins with a character that is neither a letter nor a digit |
| `title`  | `Frontmatter.Text("title")`: the frontmatter's `title` as written; `null` when the frontmatter is missing or unreadable, has no `title`, or its `title` is not a string                                                                                                            |
| `status` | OQ-3's default: for an `adr`, the first word of `StatusLine`, `null` when `StatusLine` is `null` or begins with a character that is neither a letter nor a digit; `null` for a `decision` and a `lesson`                                                                           |
| `path`   | `RecordFile.RelativePath`: root-relative, `/`-separated (C-2)                                                                                                                                                                                                                      |

A first word is the reading `Resolved` makes of a Resolution (§ "Out-of-scope rows and open questions"): the text up to
its first character that is neither a letter nor a digit (`char.IsLetterOrDigit`), `null` when that is empty. One
private `FirstWord` serves the resolution, the number and the status; `Resolved` reads the same as before it, since a
Resolution with no leading word is not `Open`. The status keeps its case: `accepted - 2026-10-08, …` gives `accepted`,
`Accepted, 2026-10-12, …` gives `Accepted`, and `Rejected — withdrawn …` gives `Rejected`. Nothing maps it to a
vocabulary or judges it (C-5). OQ-3's reading is one private `RecordStatus` in `FactsDocument`; an answer to OQ-3 that
differs from the default changes that method and no member (C-3).

**What parses is given.** A record is given whatever its file holds, as every claim row is (B-013, C-5). A missing or
unreadable frontmatter, which `FrontmatterReader` gives as no node and never throws on, empties `title` alone; an ADR
with no `## Status` heading, or none with a paragraph under it, empties `status` alone; a file name with no leading word
empties `number` alone. No record is dropped for its content, its name, or a frontmatter `type` that differs from its
folder. A file the store cannot open fails the read as a specification the store cannot open fails the check's.

**The command.** [`FactsCommand`](../FactsCommand.cs) has one call more: it reads the model, then
`ReadRecords(root, model.Features)` on the same reader, and projects both through `From`. It still reads no Markdown,
YAML or record file itself (C-4), and nothing it calls writes or touches the network (B-023, C-2).

```json
{
  "records": [
    {
      "kind": "adr",
      "number": "0001",
      "title": "ADR-0001: Read each record as written",
      "status": "Accepted",
      "path": "src/area/.spec/adr/0001-read-each-record-as-written.md"
    },
    {
      "kind": "decision",
      "number": "0001",
      "title": "Decision 0001: the records are given",
      "status": null,
      "path": "src/area/.spec/decisions/0001-the-records-are-given.md"
    },
    {
      "kind": "lesson",
      "number": "0001",
      "title": null,
      "status": null,
      "path": "src/area/.spec/lessons/0001-a-lesson-with-no-frontmatter.md"
    }
  ]
}
```

The schema has, on each specification item, `records` in `properties` and in `required`: an array of objects with
`additionalProperties: false`, each requiring `kind` (string, one of `decision`, `adr` and `lesson`), `number`, `title`
and `status` (`["string", "null"]`), and `path`, under the report schema's `file` pattern as a specification's `path`
is. No existing member, type or constraint of the schema changes (C-3). The `specht facts` paragraph in
[`docs/cli.md`](../../../../../docs/cli.md) names the records among what is printed.

### Where later items attach

Each later item adds a member, never renames or retypes one (C-3), and adds it to the schema in the same change:

- `0365`, built: `claims` and `constraints` on `FeatureFacts`, from `SpechtModel.Claims` and the section the manifest's
  `sections` and `roles` name; § "Claims and constraints".
- `0366`, built: `outOfScope` and `openQuestions` on `FeatureFacts`, after `constraints`, from the sections the
  manifest's `sections` lists, an open question's `resolved` on OQ-2's default; § "Out-of-scope rows and open
  questions".
- `0367`: `deltas` and `signOff` at the end of `FeatureFacts`, after `records`, each section found through the private method § "Out-of-scope rows and open questions" names under the
  manifest's heading (C-7), its cells read through `Cell`. Item `0367` also finds the claims, constraints, out-of-scope
  and open-questions sections under a heading the manifest renames (B-016), changing how a section is found and no
  member.
- `0368`, built: `records` at the end of `FeatureFacts`, listed by the finder `ISpecDiscovery.FindRecords` and
  read by `SpechtModelReader.ReadRecords` beside the check's `Read`, which does not change; never a read inside
  `Features/Facts/` (C-4); § "Records beside a specification".
- `0369`: the path selector narrows `FactsDocument.Specifications` in the engine; the command maps an empty selection to
  `ExitCodes.NotFound`.
- `0370`: the text lines, made by the engine from `FactsDocument` as `SpechtReportDocument.SummaryLines` makes the
  check's summary; the command only writes them.

### The published schema

[`docs/schema/facts.schema.json`](../../../../../docs/schema/facts.schema.json), OQ-1's default, draft 2020-12 as the
report schema is. A root object requiring `specifications`; each item requiring the twelve members of § "The
document", § "Claims and constraints", § "Out-of-scope rows and open questions" and § "Records beside a
specification", with `id`,
`specStatus` and `status` typed `["string", "null"]` and the three lists arrays of strings. `path` takes the report schema's `file` pattern: non-empty,
not starting with `/`, no drive letter, no `\`. `additionalProperties` is `false` at every level, as in the report
schema, so a violation, a count or a verdict member fails validation (B-014), and each later item that adds a member
widens the schema in the same pull request.

A test holds the schema true by validating the command's output against the file with JsonSchema.Net, as
`SpechtReportDocument.Integration.Tests` validates the report. `test/tests/tests.csproj` and
`test/acceptance/acceptance.csproj` already copy `docs/schema/*.json` into their output, so no project file changes.

### The command

[`FactsCommand`](../FactsCommand.cs), `specht facts [--root <DIR>] --json`, with its nested `Settings`. It injects the
console, `SpechtModelReader` and an optional error writer, as `RulesCommand` does. It resolves the root with
`Path.GetFullPath`, projects the read's model and the records `ReadRecords` gives beside it through `FactsDocument.From`,
writes `ToJson` to stdout and returns
`ExitCodes.Success`. It reads no Markdown, YAML or record file (C-4). It has no `--report` and calls nothing
that writes, so a run creates, modifies and deletes nothing (B-023, C-2); the read makes no network call.

Without `--json` the command writes the usage line to stderr, nothing to stdout, and exits `ExitCodes.MissingInput`
(`2`) before reading the tree, as `FrontmatterCommand` does for its own usage. The text form is item `0370`'s (B-024);
until it lands, `--json` is required, and `docs/cli.md` says so. Item `0370` removes the refusal.

[`FactsRegistration`](../FactsRegistration.cs) adds the command to the configurator, as `ManifestRegistration` does
(ADR-0007), and [`Program.cs`](../../../Program.cs) adds `.AddFacts()` to its chain. No service registration is needed:
`AddSpechtEngine` already registers `SpechtModelReader`, and `FactsDocument` is a record built by `From`.

[`docs/cli.md`](../../../../../docs/cli.md) has the `specht facts` section, held to the help by the `0002-F1` test, and
[`docs/exit-codes.md`](../../../../../docs/exit-codes.md) row `2` names the facts usage. The command adds no rule id (C-5).

### Rejected

- **A `FactsReader` class wrapping the read and the projection.** One call to a type that already exists and one pure
  function; a second type would only forward.
- **A `Facts` method on `SpechtRunner`.** `SpechtRunner` is the check's runner, and ADR-0010 moves it into a folder; the
  facts read evaluates nothing.
- **Frontmatter key names as members (`spec_status`, `depends_on`).** The key is the consumer's from schema `0.3.0`, so
  the member would change with the manifest or the pin (B-017, C-5).
- **Omitting a member whose value is absent.** A reader could not tell absent from empty, and the shape would vary with
  the tree.
- **Writing the JSON document without `--json` until item `0370`.** The flag would change nothing, and the default
  output would change when the text form lands.
- **A per-specification text line now.** No claim states it, and OQ-5's line shape is for claims, constraints, § 5 rows
  and open questions.
- **A claim's status mapped to a marker name (`withdrawn`, `superseded`).** A reading of the row, not the row (C-5); a
  status no marker names would need a fallback, and a pin with no markers has nothing to map to.
- **Only the claims whose id the grammar accepts, or the first of two sharing an id.** B-013 gives every fact that
  parses; dropping a row puts the check's verdict in the facts.
- **`SpechtClaim.IsValid` or `FirstDeclarationLine` as a member.** Each is a verdict on the row (B-014, C-5).
- **A claim's text and status by column position, cells 1 and 3.** A claims table with no `Source` column,
  `ID | Claim | Status`, would give no status; the header says which cell is which.
- **The claims table's headers from the manifest's `tables`.** The shipped manifest's `tables` names only `matrix` and
  `signOff` (C-7), and a consumer's `tables.claims`, which the check accepts, lists headers without saying which is the
  text or the status.
- **The constraints heading by its position, or by its leading number, in `sections`.** Either reads a heading the
  manifest renames, which is item `0367`'s (B-016) and which the specification does not settle; the listed entry reads
  what item `0365` claims and leaves that open, with no member to change when it is settled (C-3).
- **A constraints member on `SpechtModel` beside `Claims`.** No rule reads the constraints section; the facts read the
  parsed section the model already holds, and an engine member waits for a rule that needs it.
- **A cell's Markdown source, backticks and emphasis kept.** The parse keeps no source span per cell, so it would take a
  second read of the file: a second parser (C-1).
- **One row record shared by claims and constraints.** Their third members differ; they stay two until items `0366` and
  `0367` show whether the rows share a shape.
- **The open marker from the manifest, `Markers.GetValueOrDefault("open", "Open")`, as `ClaimRowRule` reads
  `withdrawn`.** No shipped manifest declares `open`, so under a shipped manifest it reads `Open` either way; it differs
  only for a consumer's `markers.open`, a name no schema version defines and C-7 does not let the facts read. A schema
  version that declares `markers.open` (`0228-F1` OQ-4) needs C-7 amended before the facts read it.
- **The first word split at white space alone.** `Open.` would not be `Open`, and `0228-F1` B-009 reads
  `Open. Proposed default: …` as open.
- **Resolved only when the Resolution begins `Resolved`.** Not OQ-2's reading: `RESOLVED, …` and `Superseded …` would
  read open.
- **`resolved` as `null`, or a state string, when the table has no Resolution cell.** A third state no claim states; B-009
  reads an empty Resolution as open, and an absent cell is read as empty.
- **`number` as an integer.** Parsing the cell is a reading of it, and a cell such as `1a` or an empty one would need a
  fallback; the first cell as the parse gives it is how every other row's id is given.
- **`number` and `id` from the cells under `#` and `ID`.** The first cell is how the check reads an open-question id
  (`HeldIds`) and how a constraint's id is given, and every shipped template puts `#` and `ID` first.
- **`blocks` as a list of ids.** Splitting the cell is the Blocks rule's reading (`0228-F1`), and prose in the cell would
  need a grammar.
- **A table whose headers differ from the engine's as unreadable.** The parse reads it (C-1); dropping its rows would put
  a verdict on the table into the facts (C-5).
- **One row record for every section, its cells keyed by header.** The schema would follow each table's headers and
  vary with the tree (C-5), and B-007 and B-008 name the members. The four row shapes share only `line`, so each keeps a
  record of its own.
- **The records in `SpechtListing` and `SpechtModel`, listed and read by `Read`.** Every check would list, load and
  parse the records for no rule - 280 files beside this repository's 57 specifications - and the model would carry a
  member no rule reads, as the constraints member above was rejected. § "Where later items attach" first said the
  listing; `ReadRecords` beside `Read` keeps the check's read, and its verdicts, as they are.
- **`FindTestFiles` with the three folders' globs, as `HeldIds` lists record names.** It lists the whole tree to find
  files beside specifications already found; its git listing leaves out a file git ignores, which `Beside` finds beside
  the same specification; and a glob built from a folder's name reads a `*` or `[` in it as a wildcard.
- **A paragraph-text member on `SpechtSection`, filled by `Parse`.** Every check's parse would carry it for no rule;
  `FirstLine`, beside `FirstCells`, reads only the records the facts ask for.
- **A records reader in `src/specht/Facts/`, or `From` given the root.** ADR-0010 keeps I/O in the reader that
  sequences the stores, `Facts/` reads only `Model/`, and `From` stays a function of what it is given (B-022).
- **`From(model)` kept beside `From(model, records)`.** Its only callers would be tests, and it would give `[]` for a
  tree that has records.
- **A flag on `Read` that reads the records too.** `Read` returns the model, which has no place for them; the flag would
  be a configuration point only the facts set.
- **The whole first line under `## Status` as the status.** OQ-3's other option; its default is the first word.
- **A status lower-cased, or mapped to the template's `proposed`, `accepted`, `superseded`, `deprecated` and
  `rejected`.** A reading of the record, not the record (C-5): this repository's ADRs write `Accepted` and `accepted`,
  and one opens its Status section with prose, whose first word is given as written.
- **A status for a decision or a lesson, such as `accepted`.** OQ-3's other option for them; its default gives none.
- **The status from the manifest's `codeAnalysis.records.adrProfiles` `lifecycle`.** C-7 reads only `sections`, `roles`
  and `tables`; `codeAnalysis` is the C#-specific section (ADR-0023), and its globs name ADRs across the tree, not the
  records beside a specification.
- **`number` as an integer, or the file name's leading digits.** From schema `0.4.0` a record may be named by a Nano ID
  or a base32 Snowflake id (`0001-F19` B-020, B-030); a Nano ID that begins with a digit would be cut to it, and `0001`
  would lose its zeros.
- **`number` from the frontmatter `title`, such as `ADR-0031: …`, or from the `#` heading.** B-011 reads it from the
  file name, and a title is free text.
- **`kind` from the frontmatter `type`.** B-011 names the folders, and a record whose frontmatter is unreadable would
  have no kind.
- **Every file in the three folders, or the files in folders below them.** A record is a Markdown file written from a
  `.md` template into the folder itself; an image or a `.gitkeep` beside the records is not one.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-11-13 -->

Item `0364`'s claims are proved on three tiers. `FactsDocumentUnitTests` projects models built in memory with the
shared `SpechtModelFixture`, `FeatureSpecFixture` and `EpicFileFixture` and reads the members from `ToJson()`, so no test
names a C# property § 7 does not. `FactsCommandUnitTests` constructs `FactsCommand` over a `MockFileSystem`, as
`RulesCommandUnitTests` does. `FactsCommandIntegrationTests` launches the built tool over `SpechtTree`s on disk, and
`FactsSteps` binds the `@B-001` to `@B-004`, `@B-014`, `@B-017`, `@B-021` to `@B-023` and `@B-026` scenarios the same way,
each naming no form run with `--json` (item `0364` summary). The schema is read from `docs/schema/facts.schema.json` as
both test projects copy it, so its absence fails the test that reads it. Theory rows come from `FactsPinPairs`,
`FactsBrokenRoots` and `FactsVerdictMembers`. Every tree is synthetic and pinned in the test; nothing reaches the
network or reads a clock. The other scenarios of `facts.feature` keep an unbound step and are skipped
(`missingOrPendingStepsOutcome: Ignore`) until their items bind them.

Item `0365`'s claims, B-005, B-006, B-012, B-013 and B-025, are proved the same way on the same three tiers. The unit
tests build a claims or constraints section in memory as a `SpechtSection` and read `claims` and `constraints` from
`ToJson()` by the names § 7 gives, so no test names `ClaimFacts` or `ConstraintFacts`. `FactsSteps` binds the `@B-005`,
`@B-006`, `@B-012`, `@B-013` and `@B-025` scenarios; the Background's two specifications now carry two claims and two
constraints, each claim with a § 9 row and a scenario carrying its tag. Theory rows come from `FactsClaimTables`,
`FactsRetiredClaimStatuses` and `FactsCoverageMembers`. A line is the one-based line of the row in the file the test
wrote, found by the row's leading cells, not the engine's own reading of it.

Item `0366`'s claims, B-007, B-008 and B-015, are proved the same way on the same three tiers. The unit tests build an
out-of-scope or open-questions section in memory as a `SpechtSection` and read `outOfScope` and `openQuestions` from
`ToJson()` by the names § 7 gives, so no test names `OutOfScopeFacts` or `OpenQuestionFacts`. `FactsSteps` binds the
`@B-007`, `@B-008` and `@B-015` scenarios; the Background's two specifications now carry two out-of-scope rows and one
open question, and the B-008 step rewrites the first one's Resolution. Theory rows come from
`FactsOpenQuestionResolutions` and `FactsOutOfScopeFaults`, each read by the unit and the integration tier. An
unreadable table is a header row with no delimiter row, and each B-015 test first holds the engine's own read to the
fault - no out-of-scope section, or one with no rows - so a fixture the parse reads as a table cannot pass as one it
cannot.

Item `0368`'s claim, B-011, is proved on the same three tiers, and its mechanism, which § 7 puts in the engine below
`FactsDocument`, on the unit tier where each part lives. `FactsDocumentUnitTests` hands `From` a model and a map of
`RecordFile`s built with `RecordFileFixture`, in `test/tests/Facts/` since only the facts use it, and reads `records`
from `ToJson()` by the names § 7 gives, so no test names `RecordFacts`. `SpechtDiscoveryRecordsUnitTests` calls
`FindRecords` over a `MockFileSystem` with no git, and `SpechtModelReaderRecordsUnitTests` calls `ReadRecords` after
`Read` over one, both in either layout. `SpechtDocumentParserUnitTests` reads `FirstLine` from text, and
`SpechtModelBuilderUnitTests` builds one `Record` whose frontmatter YAML cannot read. `FactsSteps` binds the `@B-011`
scenario, writing a decision, an ADR and a lesson beside the first specification. Theory rows come from
`FactsRecordNumbers`, `FactsAdrStatusLines`, `StatusSectionTexts` and `RecordLayouts`. The existing `FactsDocument`
tests call `From(model, NoRecords)`, an empty map, as § 7 gives `From` no one-argument form.

| Claim | Mechanism                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Unit test                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| B-001 | `FactsDocument.From` walks `SpechtModel.Features` in its order; the order is the discovery's, which its own tests pin                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       | `FeaturesInTheModelsOrder_WhenTheDocumentIsMade_ShouldListEverySpecificationInThatOrder`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| B-002 | `ToJson` writes one document; the published schema describes it                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             | None at the unit tier: the mechanism is the published file, read from disk, as the report schema's test is (`0001-F3` B-008)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| B-003 | `path` from `RelativePath`, `id` from `Identity` for a Feature in `FeaturesWithIdentity`, the statuses through the manifest's keys                                                                                                                                                                                                                                                                                                                                                                                                                                                                          | `AFeatureWithAnIdentity_WhenTheDocumentIsMade_ShouldCarryItsPathQualifiedIdAndBothStatuses`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| B-004 | `FeatureSpec.Strings` for each list, not `SpechtDependencyGraph`, so an edge the check cannot resolve is kept, in its written order                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | `DependencyListsTheCheckCannotResolve_WhenTheDocumentIsMade_ShouldCarryEachAsTheFrontmatterWritesIt`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| B-005 | One entry per `SpechtClaims.Rows` row in row order, `id` and `line` from the row, `text` and `status` the cells under the `Claim` and `Status` headers, wherever the table puts them                                                                                                                                                                                                                                                                                                                                                                                                                        | `ClaimRowsUnderTheirTablesHeaders_WhenTheDocumentIsMade_ShouldCarryEachClaimsIdTextStatusAndLine`, with and without a `Source` column (`FactsClaimTables`)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| B-006 | The section the manifest's `sections` lists as `4. Constraints`; each body row's first cell and line, and the cells under `Constraint` and `Rules Out`                                                                                                                                                                                                                                                                                                                                                                                                                                                      | `ConstraintRows_WhenTheDocumentIsMade_ShouldCarryEachConstraintsIdTextWhatItRulesOutAndLine`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| B-007 | The section the manifest's `sections` lists as `5. Out of Scope`; each body row's first cell as its number and its line, and the cells under `Item` and `Exclusion Reason`                                                                                                                                                                                                                                                                                                                                                                                                                                  | `OutOfScopeRows_WhenTheDocumentIsMade_ShouldCarryEachRowsNumberItemReasonAndLine`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| B-008 | The section listed as `11. Open Questions`; each body row's first cell and line, the cells under `Question`, `Blocks` and `Resolution`, and `resolved` read on OQ-2's default: `false` for an empty Resolution or one whose first word, up to its first character neither a letter nor a digit, is `Open` in exact case                                                                                                                                                                                                                                                                                     | `AnOpenQuestionsResolution_WhenTheDocumentIsMade_ShouldCarryItsIdQuestionBlocksResolutionLineAndWhetherItIsResolved` (`FactsOpenQuestionResolutions`): the Outline's three, an empty cell, `Open` before `;` and before a space unresolved; `RESOLVED`, `Superseded`, `open` and `Opened` resolved                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| B-011 | `FindRecords` lists the `.md` files directly in one folder beside the specification file, in ordinal order of file name, in either layout; `ReadRecords` calls it for `adr`, `decisions` and `lessons` in that order, gives each its kind and one list per Feature; `FirstLine` gives the first paragraph's first line under the first `## Status`; `Record` keeps a record whose frontmatter cannot be read; `From` gives `kind` and `path` as handed, `title` from the frontmatter, `number` the file name's first word and `status` an ADR's `StatusLine`'s first word, `[]` for a Feature the map lacks | `FactsDocumentUnitTests.RecordsBesideOneFeatureAndNoneBesideAnother_WhenTheDocumentIsMade_ShouldCarryEachRecordsKindNumberTitleStatusAndPathAndNoRecordForTheOther`; `FactsDocumentUnitTests.ARecordsFileName_WhenTheDocumentIsMade_ShouldCarryItsFirstWordAsTheNumber` (`FactsRecordNumbers`); `FactsDocumentUnitTests.AnAdrsStatusLine_WhenTheDocumentIsMade_ShouldCarryItsFirstWordAsWrittenAsTheStatus` (`FactsAdrStatusLines`); `SpechtDiscoveryRecordsUnitTests.FilesInAndBelowTheFoldersBesideASpecification_WhenItsRecordsAreFound_ShouldListOnlyTheMarkdownFilesDirectlyInTheNamedFolderInOrdinalOrder` (`RecordLayouts`); `SpechtModelReaderRecordsUnitTests.FeaturesInBothLayoutsWithRecordsInEachFolderAndOneWithNone_WhenTheirRecordsAreRead_ShouldGiveEachFeaturesRecordsByKindInOrdinalOrderOfPath`; `SpechtDocumentParserUnitTests.ARecordsText_WhenTheFirstLineUnderItsStatusHeadingIsRead_ShouldGiveTheFirstParagraphsFirstLineFlattenedOrNone` (`StatusSectionTexts`); `SpechtModelBuilderUnitTests.ARecordWhoseFrontmatterCannotBeRead_WhenBuilt_ShouldCarryItsPathKindNoTitleAndTheFirstLineUnderItsStatusHeading` |
| B-012 | The status is the `Status` cell as written; nothing maps it to a marker or drops a row for it                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | `AClaimWhoseStatusIsTheWithdrawnOrSupersededMarker_WhenTheDocumentIsMade_ShouldStillGiveItWithThatStatus` (`FactsRetiredClaimStatuses`)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| B-013 | Every row of `SpechtClaims.Rows` is given, a second row sharing an id and a row whose id the grammar refuses included; `From` runs no rule                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | `TwoClaimsSharingAnIdBesideOneTheGrammarRefuses_WhenTheDocumentIsMade_ShouldGiveEveryRowInItsOrder`; the exit `0` is the command's unchanged path, pinned at the integration tier                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| B-014 | `From` reads the model and runs no rule; the schema's `additionalProperties: false` refuses a violation, count or verdict member                                                                                                                                                                                                                                                                                                                                                                                                                                                                            | None at the unit tier: the schema theory reads the published file, so it is Integration                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| B-015 | Each row member is read from its own section alone, and a section that is absent or has no table gives `[]`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | `AnOutOfScopeSectionMissingOrWithNoTable_WhenTheDocumentIsMade_ShouldGiveNoOutOfScopeRowsAndEveryOtherRow` (`FactsOutOfScopeFaults`); the parse giving a header row with no delimiter row no table is Markdig's, held at the integration tier where the Markdown is parsed                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| B-017 | Every member is written whatever its value, so a value a pin changes, such as an epic-less Feature's identity from `0.3.0`, keeps it                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | `TheSameFeaturesUnderTwoPins_WhenEachDocumentIsMade_ShouldCarryTheSameMembers`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| B-021 | The command folds the read's typed exceptions into `2` and `3`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | `ABrokenRoot_WhenExecuted_ShouldExitWithTheChecksCode`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| B-022 | `From` is a function of the model alone and `ToJson` serializes in declaration and model order                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | `TheSameFeaturesReadIntoTwoModels_WhenEachDocumentIsSerialized_ShouldGiveTheSameText`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| B-023 | The command calls only the read and the console; nothing it calls opens a file for writing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | `ATreeWithAFeature_WhenExecuted_ShouldCreateModifyOrDeleteNoFile`, with a sentinel outside the root                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| B-025 | `From` reads no `MatrixSections`, companion `.feature` or test file; the schema's `additionalProperties: false` on a claim refuses a coverage member                                                                                                                                                                                                                                                                                                                                                                                                                                                        | `AClaimWithATraceabilityRowATaggedScenarioAndATestFile_WhenTheDocumentIsMade_ShouldCarryNoneOfThem`; the schema half reads the published file, so it is Integration, as B-014's is                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| B-026 | `From` never reads `SpechtModel.Epics`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | `AnEpicFileBesideAFeature_WhenTheDocumentIsMade_ShouldGiveOnlyTheFeature`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               |

The integration tier adds what a unit cannot see: the registration and `--json` binding, stdout as one document valid
against the schema with a `null` member in it (B-002), a tree in both layouts against the check's own read (B-001), the
check's violation absent from the document (B-014), two pins across `0.1.0`, `0.2.0` and `0.3.0` (B-017), the check's
exit code for each broken root beside the command's (B-021), two runs and two roots giving the same bytes (B-022), and
the folder holding the root unchanged (B-023). For item `0365` it adds each claim's and constraint's line in a file on disk
(B-005, B-006), a withdrawn and a superseded row (B-012), the check's exit `1` and its violation on the second of two
claims sharing an id beside the command's `0` and both rows (B-013), and a claim with a `Covered` § 9 row and a tagged
scenario given with neither, and the schema refusing a traceability, scenario or test member on a claim (B-025). For item
`0366` it adds each out-of-scope row's and open question's line in a file on disk with the document valid against the
schema (B-007, B-008), each resolution of `FactsOpenQuestionResolutions` beside a resolved question, an empty cell read
as `""` (B-008), and a specification with no out-of-scope section, or one whose table has no delimiter row, still giving
its claims, constraints and open questions (B-015). For item `0368` it adds records beside a co-located and a
legacy specification, from files on disk, in ordinal order of path with the document valid against the schema; an ADR
with no `## Status` section, one whose frontmatter cannot be read, a decision whose name has no leading word and a
lesson with no frontmatter, each given with that member `null`; and records written beside a specification leaving the
check's own read and its `--json` report byte-identical, while the facts give them under `records` (B-011).

**Red, item `0364`, 2026-11-12, `net11.0`.** Before the production types exist the unit files do not compile (`CS0234`:
no `Specht.Facts`, no `Specht.Tool.Features.Facts`); with those two files emptied for the run and restored after it, the
19 `FactsCommandIntegrationTests` cases were discovered and all failed on the launched tool's exit `255`, an unknown
command, every precondition on the check's own read passing first. `TheFactsCommandFeature` was discovered with 36
scenarios: 13 failed on the same exit `255` and 23 were skipped on unbound steps.

**Red, item `0365`, 2026-11-13, `net11.0`.** The tests compile against the surface item `0364` built: they read the new
members from `ToJson()` and name no new C# type. The 7 new `FactsDocumentUnitTests` cases failed on `claims` or
`constraints` absent from the document (`found <null>`), the 7 earlier cases passing. The 9 new
`FactsCommandIntegrationTests` cases failed the same way, every precondition passing first: the check exits `1` with a
violation on the second of two claims sharing an id, and `0` on the tree whose claim is `Covered`; the 19 earlier cases
passed. `TheFactsCommandFeature` ran 36 scenarios: the 5 newly bound failed on the absent members, 13 passed and 18 were
skipped on unbound steps.

**Red, item `0366`, 2026-11-13, `net11.0`.** The tests compile against the surface items `0364` and `0365` built: they
read the new members from `ToJson()` and name no new C# type. The 13 new `FactsDocumentUnitTests` cases failed on
`outOfScope` or `openQuestions` absent from the document (`found <null>`). The 13 new `FactsCommandIntegrationTests`
cases failed the same way, every assertion before it passing: the engine's own read giving no out-of-scope section, or
one with no rows, and the document valid against the schema as published. The 47 earlier cases in `Specht.Tests.Facts`
passed. `TheFactsCommandFeature` ran 36 scenarios: the 6 newly bound failed on the absent members, 18 passed and 12 were
skipped on unbound steps.

**Red, item `0368`, 2026-11-13, `net11.0`.** `test/tests` does not compile without the surface § 7 names: the build
stops on `CS0246`, no `RecordFile`, in `RecordFileFixture` and `FactsDocumentUnitTests`, and the workspace `Format` loads
reports the rest, `CS1061` for `FindRecords`, `ReadRecords`, `Record`, `FirstLine` and the fixture's setters. So
`Format` fails on the six files that name that surface, on those compile errors alone, until it is built. With the rest
of that surface stood in for by a throwaway file in `test/tests` - `RecordFile` as a positional record, and C# extension members
for `FindRecords`, `ReadRecords`, `Record` and `FirstLine` throwing `NotImplementedException` and a static `From(model,
records)` forwarding to `From(model)` - run once and deleted, never committed, the tests compiled and 30 new unit cases
failed: the 14 `FactsDocumentUnitTests` cases on `records` absent from the document (`found <null>`), and the 16 others
on the stand-in's `NotImplementedException`; the 36 earlier cases in those classes passed. The 3 new
`FactsCommandIntegrationTests` cases, which name no new type, failed on `records` absent, every assertion before it
passing: the document valid against the schema as published, and the check's report and its exit code unchanged by the
records; the 41 earlier cases passed. `TheFactsCommandFeature` ran 36 scenarios: the newly bound one failed on the
absent member, 24 passed and 11 were skipped on unbound steps.

**For `implementer`, item `0368`.** The tests need the compile surface § 7 names, with these shapes, which § 7 implies and
does not spell: `Specht.Model.RecordFile` with `RelativePath`, `Kind` and `StatusLine` as strings (`StatusLine`
nullable) and `Frontmatter` as a `Frontmatter`, a shape `[AutoFixture(typeof(RecordFile))]` can build, as it builds
`EpicFile`; `SpechtDiscovery.FindRecords(string, SpechtLocation, string)` returning the paths; an instance
`SpechtModelReader.ReadRecords(string, IReadOnlyList<FeatureSpec>)` whose result indexes by `FeatureSpec` to records, a
Feature with none present with an empty list; an instance `SpechtModelBuilder.Record(string, string, string)`; an
instance `SpechtDocumentParser.FirstLine(string, string)` returning `string?`; and `FactsDocument.From(SpechtModel, …)`
accepting a `Dictionary<FeatureSpec, IReadOnlyList<RecordFile>>`, as `IReadOnlyDictionary` does. `FindRecords` gives
`[]` for a folder that does not exist. The schema must widen so the document with records validates (B-002's test and
the new integration tests). No test holds a link to a folder, a folder named in another case, the root `.spec/adr/`,
the records beside an epic file, a title that is not a string, or the schema refusing another member on a record: no
claim states them beyond what § 7 reads. Nothing in the design is hard to test.

**For `implementer`, item `0366`.** The tests need no new compile surface; they need `outOfScope` and `openQuestions`
written on every specification, with the members § 7 names, and the schema widened so a document carrying rows of both
still validates (B-002's test and the new integration tests). No test holds the `null` § 7 gives under a header the
table lacks, the `[]` under a manifest whose `sections` does not list either heading, a cell's Markdown markers such as
`**Open**`, a row whose first cell is no open-question id, or the schema refusing another member on either row: no claim
states them, and the renamed heading is item `0367`'s (B-016). Nothing in the design is hard to test.

**For `implementer`, item `0365`.** The tests need no new compile surface; they need `claims` and `constraints` written
on every specification, with the members § 7 names, and the schema widened so the document still validates (B-002's
test) while a claim with any other member does not (B-025). No test holds the `null` § 7 gives for a header the table
lacks, the `[]` under a manifest whose `sections` does not list `4. Constraints`, or a cell's Markdown markers: no claim
states them, and the renamed heading is item `0367`'s (B-016).

**For `implementer`, item `0364`.** The unit tests need the compile surface § 7 names: `Specht.Facts.FactsDocument` with a static
`From(SpechtModel)` and `ToJson()`, and `Specht.Tool.Features.Facts.FactsCommand(IAnsiConsole, SpechtModelReader,
TextWriter?)` with `ExecuteAsync(CommandContext, Settings, CancellationToken)`, as `RulesCommand` has, and a nested
`Settings` with init-settable `Root` and `Json`. B-021 names only the exit codes, so no test holds the stderr line § 7
describes to the check's; a claim would be needed for that. No benchmark is needed: the check's path is unchanged.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-11-13 -->

| Claim ID | Scenario                                                               | Test                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | Status  |
| -------- | ---------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------- |
| B-001    | The facts cover every specification the check discovers                | `FactsSteps.ThenBothInDiscoveryOrder`; `FactsDocumentUnitTests.FeaturesInTheModelsOrder_WhenTheDocumentIsMade_ShouldListEverySpecificationInThatOrder`; `FactsCommandIntegrationTests.FeaturesInBothLayouts_WhenFactsAreGivenAsJson_ShouldListEverySpecificationInTheChecksDiscoveryOrder`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | Covered |
| B-002    | The JSON facts validate against the published schema                   | `FactsSteps.ThenOneDocument`; `FactsSteps.ThenValidatesAgainstSchema`; `FactsCommandIntegrationTests.FeaturesWithAndWithoutAnIdentity_WhenFactsAreGivenAsJson_ShouldWriteOneDocumentValidAgainstThePublishedFactsSchema`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Covered |
| B-003    | A specification is given with its path, identity and statuses          | `FactsSteps.ThenIdentityAndStatuses`; `FactsDocumentUnitTests.AFeatureWithAnIdentity_WhenTheDocumentIsMade_ShouldCarryItsPathQualifiedIdAndBothStatuses`; `FactsCommandIntegrationTests.AFeature_WhenFactsAreGivenAsJson_ShouldCarryItsRootRelativePathQualifiedIdAndBothStatuses`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | Covered |
| B-004    | A specification is given with its dependencies                         | `FactsSteps.ThenFirstDependsOnSecond`; `FactsSteps.ThenSecondBlocksFirst`; `FactsDocumentUnitTests.DependencyListsTheCheckCannotResolve_WhenTheDocumentIsMade_ShouldCarryEachAsTheFrontmatterWritesIt`; `FactsCommandIntegrationTests.FeaturesThatListEachOther_WhenFactsAreGivenAsJson_ShouldCarryEachListAsTheFrontmatterWritesIt`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | Covered |
| B-005    | A claim is given with its text, status and line                        | `FactsSteps.ThenEachClaim`; `FactsDocumentUnitTests.ClaimRowsUnderTheirTablesHeaders_WhenTheDocumentIsMade_ShouldCarryEachClaimsIdTextStatusAndLine`; `FactsCommandIntegrationTests.AFeatureWithClaims_WhenFactsAreGivenAsJson_ShouldCarryEachClaimsIdTextStatusAndTheLineItIsOn`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Covered |
| B-006    | A constraint is given with what it rules out                           | `FactsSteps.ThenEachConstraint`; `FactsDocumentUnitTests.ConstraintRows_WhenTheDocumentIsMade_ShouldCarryEachConstraintsIdTextWhatItRulesOutAndLine`; `FactsCommandIntegrationTests.AFeatureWithConstraints_WhenFactsAreGivenAsJson_ShouldCarryEachConstraintsIdTextWhatItRulesOutAndTheLineItIsOn`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | Covered |
| B-007    | An out-of-scope row is given with its reason                           | `FactsSteps.ThenEachOutOfScopeRow`; `FactsDocumentUnitTests.OutOfScopeRows_WhenTheDocumentIsMade_ShouldCarryEachRowsNumberItemReasonAndLine`; `FactsCommandIntegrationTests.AFeatureWithOutOfScopeRows_WhenFactsAreGivenAsJson_ShouldCarryEachRowsNumberItemReasonAndTheLineItIsOn`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | Covered |
| B-008    | An open question is given with whether it is resolved                  | `FactsSteps.ThenOpenQuestion`; `FactsSteps.ThenGivenAs`; `FactsDocumentUnitTests.AnOpenQuestionsResolution_WhenTheDocumentIsMade_ShouldCarryItsIdQuestionBlocksResolutionLineAndWhetherItIsResolved`; `FactsCommandIntegrationTests.AnOpenQuestionsResolution_WhenFactsAreGivenAsJson_ShouldCarryItsIdQuestionBlocksResolutionLineAndWhetherItIsResolved`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          | Covered |
| B-009    | A lessons and deltas entry is given with its text and line             | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            | Missing |
| B-010    | A sign-off row is given with its reviewer and note                     | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            | Missing |
| B-011    | The records beside a specification are given by number and title       | `FactsSteps.ThenThreeRecords`; `FactsSteps.ThenEachRecord`; `FactsDocumentUnitTests.RecordsBesideOneFeatureAndNoneBesideAnother_WhenTheDocumentIsMade_ShouldCarryEachRecordsKindNumberTitleStatusAndPathAndNoRecordForTheOther`; `FactsDocumentUnitTests.ARecordsFileName_WhenTheDocumentIsMade_ShouldCarryItsFirstWordAsTheNumber`; `FactsDocumentUnitTests.AnAdrsStatusLine_WhenTheDocumentIsMade_ShouldCarryItsFirstWordAsWrittenAsTheStatus`; `SpechtDiscoveryRecordsUnitTests.FilesInAndBelowTheFoldersBesideASpecification_WhenItsRecordsAreFound_ShouldListOnlyTheMarkdownFilesDirectlyInTheNamedFolderInOrdinalOrder`; `SpechtModelReaderRecordsUnitTests.FeaturesInBothLayoutsWithRecordsInEachFolderAndOneWithNone_WhenTheirRecordsAreRead_ShouldGiveEachFeaturesRecordsByKindInOrdinalOrderOfPath`; `SpechtDocumentParserUnitTests.ARecordsText_WhenTheFirstLineUnderItsStatusHeadingIsRead_ShouldGiveTheFirstParagraphsFirstLineFlattenedOrNone`; `SpechtModelBuilderUnitTests.ARecordWhoseFrontmatterCannotBeRead_WhenBuilt_ShouldCarryItsPathKindNoTitleAndTheFirstLineUnderItsStatusHeading`; `FactsCommandIntegrationTests.RecordsBesideSpecificationsInBothLayouts_WhenFactsAreGivenAsJson_ShouldCarryEachRecordsKindNumberTitleStatusAndPathInOrdinalOrderOfPath`; `FactsCommandIntegrationTests.RecordsWithNoOrAnUnreadableFrontmatterNoStatusSectionOrNoLeadingWord_WhenFactsAreGivenAsJson_ShouldGiveEachWithOnlyThatMemberNull`; `FactsCommandIntegrationTests.RecordsBesideASpecification_WhenTheTreeIsCheckedAndItsFactsAreGiven_ShouldDiscoverNoRecordAsASpecificationAndChangeNoVerdict` | Covered |
| B-012    | A withdrawn claim is still given                                       | `FactsSteps.ThenWithdrawnClaimGiven`; `FactsDocumentUnitTests.AClaimWhoseStatusIsTheWithdrawnOrSupersededMarker_WhenTheDocumentIsMade_ShouldStillGiveItWithThatStatus`; `FactsCommandIntegrationTests.AClaimWhoseStatusIsTheWithdrawnOrSupersededMarker_WhenFactsAreGivenAsJson_ShouldStillGiveItWithThatStatus`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Covered |
| B-013    | The facts succeed on a tree the check fails                            | `FactsSteps.ThenSucceeds`; `FactsSteps.ThenBothClaimsGiven`; `FactsDocumentUnitTests.TwoClaimsSharingAnIdBesideOneTheGrammarRefuses_WhenTheDocumentIsMade_ShouldGiveEveryRowInItsOrder`; `FactsCommandIntegrationTests.TwoClaimsSharingAnIdTheCheckReports_WhenFactsAreGivenAsJson_ShouldExitZeroAndGiveBoth`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | Covered |
| B-014    | The facts carry no verdict                                             | `FactsSteps.ThenNoVerdict`; `FactsCommandIntegrationTests.ATreeTheCheckReportsAViolationFor_WhenFactsAreGivenAsJson_ShouldCarryNoViolationCountOrVerdict`; `FactsCommandIntegrationTests.AFactsDocumentGivenAVerdictMember_WhenValidatedAgainstThePublishedFactsSchema_ShouldBeRejected`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Covered |
| B-015    | A missing section or an unreadable table leaves only its own facts out | `FactsSteps.ThenNoOutOfScopeRows`; `FactsSteps.ThenStillCarriesOtherRows`; `FactsDocumentUnitTests.AnOutOfScopeSectionMissingOrWithNoTable_WhenTheDocumentIsMade_ShouldGiveNoOutOfScopeRowsAndEveryOtherRow`; `FactsCommandIntegrationTests.AnOutOfScopeSectionMissingOrWithATableTheParseCannotRead_WhenFactsAreGivenAsJson_ShouldGiveNoOutOfScopeRowsAndEveryOtherRow`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Covered |
| B-016    | A section the manifest renames is read under its new name              | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            | Missing |
| B-017    | The facts have one shape under every pin                               | `FactsSteps.ThenSameMembers`; `FactsDocumentUnitTests.TheSameFeaturesUnderTwoPins_WhenEachDocumentIsMade_ShouldCarryTheSameMembers`; `FactsCommandIntegrationTests.TheSameSpecificationsUnderTwoShippedPins_WhenFactsAreGivenAsJson_ShouldCarryTheSameMembers`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     | Covered |
| B-018    | A path selects one specification                                       | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            | Missing |
| B-019    | A directory selects every specification under it                       | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            | Missing |
| B-020    | A path that selects nothing fails                                      | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            | Missing |
| B-021    | The facts fail on a broken root as the check does                      | `FactsSteps.ThenExitCode`; `FactsCommandUnitTests.ABrokenRoot_WhenExecuted_ShouldExitWithTheChecksCode`; `FactsCommandIntegrationTests.ABrokenRoot_WhenFactsAreGiven_ShouldExitWithTheCodeTheCheckGives`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Covered |
| B-022    | The same tree gives the same facts                                     | `FactsSteps.ThenByteIdentical`; `FactsDocumentUnitTests.TheSameFeaturesReadIntoTwoModels_WhenEachDocumentIsSerialized_ShouldGiveTheSameText`; `FactsCommandIntegrationTests.TheSameTreeUnderTwoRoots_WhenFactsAreGivenAsJsonTwice_ShouldGiveByteIdenticalOutput`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Covered |
| B-023    | The facts write nothing                                                | `FactsSteps.ThenNothingWritten`; `FactsCommandUnitTests.ATreeWithAFeature_WhenExecuted_ShouldCreateModifyOrDeleteNoFile`; `FactsCommandIntegrationTests.ATreeBesideOtherFiles_WhenFactsAreGivenAsJson_ShouldCreateModifyOrDeleteNoFileUnderTheRootOrBesideIt`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | Covered |
| B-024    | The text form gives one line per fact                                  | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            | Missing |
| B-025    | The facts carry no claim coverage                                      | `FactsSteps.ThenNoCoverage`; `FactsDocumentUnitTests.AClaimWithATraceabilityRowATaggedScenarioAndATestFile_WhenTheDocumentIsMade_ShouldCarryNoneOfThem`; `FactsCommandIntegrationTests.AClaimWithATraceabilityRowAndATaggedScenario_WhenFactsAreGivenAsJson_ShouldCarryNoRowScenarioOrTest`; `FactsCommandIntegrationTests.AClaimGivenACoverageMember_WhenValidatedAgainstThePublishedFactsSchema_ShouldBeRejected`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | Covered |
| B-026    | An epic file is not given                                              | `FactsSteps.ThenFeaturesNotEpic`; `FactsDocumentUnitTests.AnEpicFileBesideAFeature_WhenTheDocumentIsMade_ShouldGiveOnlyTheFeature`; `FactsCommandIntegrationTests.ATreeWithAnEpicFileTheCheckDiscovers_WhenFactsAreGivenAsJson_ShouldGiveTheFeaturesAndNotTheEpic`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | Covered |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-30 (review round 2) -->

- 2026-10-30, after `spec-reviewer` round 1 on `cc248c54` and the owner's answers ([decision 0002](decisions/0002-only-the-sections-the-manifest-names-follow-it-and-a-records-status-is-claimed.md)). B-016 narrowed to the `roles` and `tables` the shipped manifest declares; C-7 added for the four sections read under fixed headings; C-1 amended to read a section as the check does; B-006 to B-009 cite C-7. B-011 claims a record's status; OQ-3 asks only how it is read. `depends_on` lists `0001-F2`, `0001-F5` and `0001-F6`, each of which lists this Feature in `blocks`; A-3 drops its condition. Scenarios: B-004 states the reverse edge as given; B-009 and B-015 exercise the table-row form and the unreadable table; B-011 and B-016 follow the claims.
- 2026-10-30, after `spec-reviewer` round 2. Corrected premise: the engine has no fixed headings; the check finds every section under the heading the manifest's `sections` key lists (decision 0002, amended). C-7 and B-016 now read `sections` as the check does; only the table headers inside §§ 4, 5, 10 and 11 stay at the engine's reading. C-1 says the facts find a section as the check does. B-016's Outline gains a renamed constraints heading. OQ-1's default cites the schema paths `0001-F9` B-021 and `0001-F10` B-020 now name.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-30 (review round 2) -->

| ID   | Question                                                                                                                                                                                                                                                                                                        | Blocks | Resolution                                                                                                                                                                                                                                                                         |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | What is the published facts schema's path and name?                                                                                                                                                                                                                                                             | B-002  | Open. Proceeds on `docs/schema/facts.schema.json`, as `0001-F13` B-002, `0001-F9` B-021 and `0001-F10` B-020 name theirs under `docs/schema/` (the owner, 2026-10-30, for those two). Owner not asked for this one.                                                                |
| OQ-2 | How is an open question read as resolved? `0228-F1` B-009 reads a Resolution as open when it is empty or its first word is the open marker; schema `0.2.1` and earlier declare no open marker.                                                                                                                  | B-008  | Open. Proceeds on `0228-F1` B-009's reading, with `Open` in exact case wherever the pinned manifest declares no marker. Owner not asked.                                                                                                                                           |
| OQ-3 | How is a record's status read? An ADR's is in its `## Status` body section, not its frontmatter; a decision and a lesson have no status key. Options for the ADR: the first word of that section, or the whole first line. Options for a decision and a lesson: no status, or a fixed value such as `accepted`. | B-011  | Open. Amended 2026-10-30 (decision 0002 call 2): B-011 claims the status; this question asks only how it is read. Proceeds on the first word of an ADR's `## Status` section, and no status for a decision or a lesson. Owner not asked. Was: B-011 gave no status until answered. |
| OQ-4 | Does the path selector take a directory, selecting every specification under it, or only a specification's own path?                                                                                                                                                                                            | B-019  | Open. Proceeds on a directory selecting by whole path segments. Owner not asked.                                                                                                                                                                                                   |
| OQ-5 | What is the text form's exact line shape: which values, in what order, and separated how?                                                                                                                                                                                                                       | B-024  | Open. Proceeds on one tab-separated line per fact: path, kind, id, line, text, as `0001-F10` decision 0002 lists the rules. Owner not asked.                                                                                                                                       |
| OQ-6 | Does the document carry epic files? An epic has sections of its own, not the twelve a Feature specification has.                                                                                                                                                                                                | B-026  | Open. Proceeds on no epic facts. Owner not asked.                                                                                                                                                                                                                                  |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-11-13 (item `0368`: 6-9 approved); spec-reviewer, 2026-11-13 (item `0366`: 6-9 approved); spec-reviewer, 2026-10-30 (round 3; rounds 1 and 2 moved to Review history) -->

| Section           | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| ----------------- | ------ | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5, 9-11         | 🟢     | spec-reviewer | Round 3 (2026-10-30) - approved, reviewing the uncommitted working tree on `cc248c54`. C-7, C-1 and B-016 now find every section by the heading the manifest's `sections` lists, as `SectionStructureRule` does, with the four sections' table headers at the engine's reading (decision 0002 as amended); the B-016 Outline exercises a renamed claims, constraints and sign-off heading. § 9 has one row per claim, every tag resolves, the `@boundary` scenarios match § 5 rows 1, 2 and 10, and dependency edges with `0001-F2`, `0001-F5` and `0001-F6` are symmetric. OQ-1 to OQ-6 stay open on recorded defaults, which do not block agreement (as `0001-F9` was approved with OQ-1 to OQ-5 open). §§ 6-8 are pending after agreement and are reviewed when written; § 9 `Missing` rows block items, not this agreement.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| 6-9 (item `0366`) | 🟢     | spec-reviewer | Item `0366`, round 1: Approved, reviewing `c5c787f6`...`79e8f0cc`. B-007, B-008 and B-015 are built in `FactsDocument.From` and proved on the unit, integration and acceptance tiers § 8 names; the 26 new `Specht.Tests.Facts` cases and the 6 newly bound `TheFactsCommandFeature` scenarios pass, and § 8 records each red. `resolved` follows OQ-2's default, its ten theory rows pinning exact case and the word boundary; the open marker is not read from `markers` (C-7). The schema gains `outOfScope` and `openQuestions` by addition only (C-3); `FactsCommand` is unchanged (C-4); § 7 matches the code as built, and each § 9 row names real tests. Nothing on the item's `out_of_scope` list is delivered. Advisory, not blocking, for `test-writer` in item `0367`: no test pins `Listed`'s `[]` when the manifest's `sections` omits `5. Out of Scope` or `11. Open Questions` (C-7), as none did for `4. Constraints` in item `0365`; B-016's tests, which change how a section is found, are where it is held.                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| 6-9 (item `0368`) | 🟢     | spec-reviewer | Item `0368`, round 1: Approved, reviewing `a88ef361`...`0460b325`. B-011 is built in `ISpecDiscovery.FindRecords`, `SpechtModelReader.ReadRecords`, `SpechtModelBuilder.Record`, `SpechtDocumentParser.FirstLine` and `FactsDocument.From`, and proved on the unit, integration and acceptance tiers § 8 names; the `Facts`, `Model` and `Discovery` unit and integration cases and the 25 bound `TheFactsCommandFeature` scenarios pass, and § 8 records the red. `status` follows OQ-3's default. `number` as the file name's first word stays within B-011, which reads it from the file name: § 7 rejects the leading digits for a Nano ID name, so a `README.md` in a record folder gives `README`, given and not judged (C-5). `Read` is unchanged, and an integration case holds the check's `--json` report byte-identical with records present, none discovered as a specification (C-1); `0001-F6` § 7 names `FindRecords`, and no `0001-F1` or `0001-F6` claim changes. The schema gains `records` by addition only (C-3); `FactsCommand` makes one more engine call and reads no file (C-4). Nothing on the item's `out_of_scope` list is delivered. Advisory, not blocking, for `test-writer`: no test pins `FindRecords` entering no record folder that is a link, or none whose name differs from `adr`, `decisions` or `lessons` only in case, two § 7 boundaries that dropping `LinkTarget is null` or an ordinal-ignore-case comparison would leave green. |

### Review history

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| ------- | ------ | ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Round 1 - draft. First review, 2026-10-30, of `9b153e83`...`cc248c54`: findings on B-004, B-009, B-011, B-015, B-016 and A-3, all closed by decision 0002 and the round-2 fixes.                                                                                                                                                                                                                                                                                                                                                                                |
| 1-5     | 🟡     | spec-reviewer | Round 2 - draft. Second review, 2026-10-30, of the uncommitted fixes on `cc248c54`. Round-1 findings on B-004, B-009, B-011, B-015, B-016 and A-3 are closed, and the dependency edges are symmetric. One finding remains: C-7 reads §§ 4, 5, 10 and 11 under "the engine's fixed headings", but the engine has none; the check takes every heading from the manifest's `sections` (`SectionStructureRule`), so a consumer renaming one makes C-7 contradict its own Rules Out and C-1. OQ-1 to OQ-6 open with recorded defaults, which do not block agreement. |

## Tasks

- `0364`: the command and its published facts schema, the document contract every later item adds to by addition (B-001 to B-004, B-014, B-017, B-021 to B-023, B-026).
- `0365`: claims and constraints (B-005, B-006, B-012, B-013, B-025), waiting on `0364`.
- `0366`: § 5 rows, open questions, and a missing section or unreadable table (B-007, B-008, B-015), waiting on `0364` and `0365`.
- `0367`: § 10 entries, § 12 rows, and the sections, roles and tables the manifest names (B-009, B-010, B-016), waiting on `0364` and `0365`.
- `0368`: the decision, ADR and lesson records beside a specification (B-011), waiting on `0364`.
- `0369`: the path selector (B-018 to B-020), waiting on `0364`.
- `0370`: the text form (B-024), waiting on `0365` and `0366`.

OQ-1 to OQ-6 stay open; the items proceed on their recorded defaults.

## Scoring

| Field | Value | Basis                                    |
| ----- | ----- | ---------------------------------------- |
| value | 0     | Not scored while the agreement is draft. |
| risk  | 0     | Not scored while the agreement is draft. |
