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
updated: "2026-11-12"
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

| Concern                                                                                                                        | Classification | Owner                                                                                   |
| ------------------------------------------------------------------------------------------------------------------------------ | -------------- | --------------------------------------------------------------------------------------- |
| Which files are Feature specifications, in what order, and that an epic file is not one (B-001, B-026)                         | Business       | The check's discovery, `ISpecDiscovery`, through `SpechtModelReader.Read` (C-1)         |
| A specification's path, qualified id, statuses and dependency lists as its frontmatter gives them (B-003, B-004)               | Business       | `FeatureSpec` and `SpechtModel.FeaturesWithIdentity`, projected by `FactsDocument.From` |
| Root, manifest and pin acceptance, and the exceptions that carry their failure (B-021)                                         | Technical      | `SpechtModelReader`, `SpechtManifestStore`, `SpechtManifestParser`                      |
| The document's members and their fixed shape under every pin (B-002, B-014, B-017; C-3, C-5)                                   | Both           | `FactsDocument` and `FeatureFacts`, held to `docs/schema/facts.schema.json`             |
| Byte-identical serialization (B-022)                                                                                           | Technical      | `FactsDocument.ToJson`                                                                  |
| Parsing the options, writing stdout and stderr, folding exceptions into exit codes (B-021, B-023; C-4)                         | Both           | `FactsCommand`                                                                          |
| A claim's id and line as the check reads them, every row kept, two sharing an id included (B-005, B-013; C-1)                  | Business       | `SpechtClaims` through `SpechtModel.Claims`, projected by `FactsDocument.From`          |
| A claim's text, and its status as its row writes it (B-005, B-012; C-5)                                                        | Business       | `FactsDocument.From`, reading the claims table's cells by header                        |
| Finding the constraints section under the heading `sections` lists, and its cells under the engine's headers (B-006; C-1, C-7) | Both           | `FactsDocument.From` through `SpechtDocument.Section`                                   |
| A claim's and a constraint's members, with no coverage member (B-025; C-3)                                                     | Both           | `ClaimFacts` and `ConstraintFacts`, held to `docs/schema/facts.schema.json`             |

## 7. Technical Design

<!-- last written by: implementer, 2026-11-13 -->

The design for item `0364`, as built, and for item `0365`, to build (§ "Claims and constraints"). Each built type is
declared in the file it links; the compiler is the authority on its members and signatures.

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

Two records in the engine folder `src/specht/Facts/`, namespace `Specht.Facts`. The folder sits beside `Report/` in
ADR-0010's list and reads only `Model/` below it.

- [`FactsDocument`](../../../../specht/Facts/FactsDocument.cs): the document. Its `From` projects a model, as
  [`SpechtReportDocument.From`](../../../../specht/Report/SpechtReportDocument.cs) makes the report: a function of the
  model alone, with no clock, machine or environment in it. Its `ToJson` serializes it.
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

A Feature's document for this repository's own specification:

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

Item `0365`'s design, to build. `FeatureFacts` gains two members after `children`, so every earlier member keeps its
name, type and place (C-3):

| Member        | Value                                                                                                              | Claim               |
| ------------- | ------------------------------------------------------------------------------------------------------------------ | ------------------- |
| `claims`      | One `ClaimFacts` per row of `SpechtModel.Claims[feature].Rows`, in row order; `[]` with no claims section or table | B-005, B-012, B-013 |
| `constraints` | One `ConstraintFacts` per body row of the constraints section, in row order; `[]` with no such section or table    | B-006               |

Two records in `src/specht/Facts/`, carrying values and no logic:

- `ClaimFacts(string Id, string? Text, string? Status, int Line)`, written as `id`, `text`, `status` and `line`.
- `ConstraintFacts(string Id, string? Text, string? RulesOut, int Line)`, written as `id`, `text`, `rulesOut` and `line`.

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
the entry `4. Constraints`, which every shipped manifest lists, read through `feature.Document.Section` when
`model.Schemas.Structure.Sections` lists it, as `SectionContractRule` finds a section by the titles `sections` lists
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

### Where later items attach

Each later item adds a member, never renames or retypes one (C-3), and adds it to the schema in the same change:

- `0365`: `claims` and `constraints` on `FeatureFacts`, from `SpechtModel.Claims` and the section the manifest's
  `sections` and `roles` name; § "Claims and constraints".
- `0366` and `0367`: `outOfScope`, `openQuestions`, `deltas` and `signOff` on `FeatureFacts`, each section found through
  `SpechtDocument.Section` under the manifest's heading (C-7), its cells read through `Cell`. Item `0367` also finds the
  claims and constraints under a heading the manifest renames (B-016), changing how a section is found and no member.
- `0368`: `records` on `FeatureFacts`. Records are not in the listing today; that item extends the listing through
  `ISpecDiscovery`, not a read inside `Features/Facts/`.
- `0369`: the path selector narrows `FactsDocument.Specifications` in the engine; the command maps an empty selection to
  `ExitCodes.NotFound`.
- `0370`: the text lines, made by the engine from `FactsDocument` as `SpechtReportDocument.SummaryLines` makes the
  check's summary; the command only writes them.

### The published schema

[`docs/schema/facts.schema.json`](../../../../../docs/schema/facts.schema.json), OQ-1's default, draft 2020-12 as the
report schema is. A root object requiring `specifications`; each item requiring the seven members above, with `id`,
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
`Path.GetFullPath`, projects the read's model through `FactsDocument.From`, writes `ToJson` to stdout and returns
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

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-11-12 -->

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

| Claim | Mechanism                                                                                                                            | Unit test                                                                                                                    |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------- |
| B-001 | `FactsDocument.From` walks `SpechtModel.Features` in its order; the order is the discovery's, which its own tests pin                | `FeaturesInTheModelsOrder_WhenTheDocumentIsMade_ShouldListEverySpecificationInThatOrder`                                     |
| B-002 | `ToJson` writes one document; the published schema describes it                                                                      | None at the unit tier: the mechanism is the published file, read from disk, as the report schema's test is (`0001-F3` B-008) |
| B-003 | `path` from `RelativePath`, `id` from `Identity` for a Feature in `FeaturesWithIdentity`, the statuses through the manifest's keys   | `AFeatureWithAnIdentity_WhenTheDocumentIsMade_ShouldCarryItsPathQualifiedIdAndBothStatuses`                                  |
| B-004 | `FeatureSpec.Strings` for each list, not `SpechtDependencyGraph`, so an edge the check cannot resolve is kept, in its written order  | `DependencyListsTheCheckCannotResolve_WhenTheDocumentIsMade_ShouldCarryEachAsTheFrontmatterWritesIt`                         |
| B-014 | `From` reads the model and runs no rule; the schema's `additionalProperties: false` refuses a violation, count or verdict member     | None at the unit tier: the schema theory reads the published file, so it is Integration                                      |
| B-017 | Every member is written whatever its value, so a value a pin changes, such as an epic-less Feature's identity from `0.3.0`, keeps it | `TheSameFeaturesUnderTwoPins_WhenEachDocumentIsMade_ShouldCarryTheSameMembers`                                               |
| B-021 | The command folds the read's typed exceptions into `2` and `3`                                                                       | `ABrokenRoot_WhenExecuted_ShouldExitWithTheChecksCode`                                                                       |
| B-022 | `From` is a function of the model alone and `ToJson` serializes in declaration and model order                                       | `TheSameFeaturesReadIntoTwoModels_WhenEachDocumentIsSerialized_ShouldGiveTheSameText`                                        |
| B-023 | The command calls only the read and the console; nothing it calls opens a file for writing                                           | `ATreeWithAFeature_WhenExecuted_ShouldCreateModifyOrDeleteNoFile`, with a sentinel outside the root                          |
| B-026 | `From` never reads `SpechtModel.Epics`                                                                                               | `AnEpicFileBesideAFeature_WhenTheDocumentIsMade_ShouldGiveOnlyTheFeature`                                                    |

The integration tier adds what a unit cannot see: the registration and `--json` binding, stdout as one document valid
against the schema with a `null` member in it (B-002), a tree in both layouts against the check's own read (B-001), the
check's violation absent from the document (B-014), two pins across `0.1.0`, `0.2.0` and `0.3.0` (B-017), the check's
exit code for each broken root beside the command's (B-021), two runs and two roots giving the same bytes (B-022), and
the folder holding the root unchanged (B-023).

**Red, item `0364`, 2026-11-12, `net11.0`.** Before the production types exist the unit files do not compile (`CS0234`:
no `Specht.Facts`, no `Specht.Tool.Features.Facts`); with those two files emptied for the run and restored after it, the
19 `FactsCommandIntegrationTests` cases were discovered and all failed on the launched tool's exit `255`, an unknown
command, every precondition on the check's own read passing first. `TheFactsCommandFeature` was discovered with 36
scenarios: 13 failed on the same exit `255` and 23 were skipped on unbound steps.

**For `implementer`.** The unit tests need the compile surface § 7 names: `Specht.Facts.FactsDocument` with a static
`From(SpechtModel)` and `ToJson()`, and `Specht.Tool.Features.Facts.FactsCommand(IAnsiConsole, SpechtModelReader,
TextWriter?)` with `ExecuteAsync(CommandContext, Settings, CancellationToken)`, as `RulesCommand` has, and a nested
`Settings` with init-settable `Root` and `Json`. B-021 names only the exit codes, so no test holds the stderr line § 7
describes to the check's; a claim would be needed for that. No benchmark is needed: the check's path is unchanged.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-11-12 -->

| Claim ID | Scenario                                                               | Test                                                                                                                                                                                                                                                                                                                                 | Status  |
| -------- | ---------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------- |
| B-001    | The facts cover every specification the check discovers                | `FactsSteps.ThenBothInDiscoveryOrder`; `FactsDocumentUnitTests.FeaturesInTheModelsOrder_WhenTheDocumentIsMade_ShouldListEverySpecificationInThatOrder`; `FactsCommandIntegrationTests.FeaturesInBothLayouts_WhenFactsAreGivenAsJson_ShouldListEverySpecificationInTheChecksDiscoveryOrder`                                           | Covered |
| B-002    | The JSON facts validate against the published schema                   | `FactsSteps.ThenOneDocument`; `FactsSteps.ThenValidatesAgainstSchema`; `FactsCommandIntegrationTests.FeaturesWithAndWithoutAnIdentity_WhenFactsAreGivenAsJson_ShouldWriteOneDocumentValidAgainstThePublishedFactsSchema`                                                                                                             | Covered |
| B-003    | A specification is given with its path, identity and statuses          | `FactsSteps.ThenIdentityAndStatuses`; `FactsDocumentUnitTests.AFeatureWithAnIdentity_WhenTheDocumentIsMade_ShouldCarryItsPathQualifiedIdAndBothStatuses`; `FactsCommandIntegrationTests.AFeature_WhenFactsAreGivenAsJson_ShouldCarryItsRootRelativePathQualifiedIdAndBothStatuses`                                                   | Covered |
| B-004    | A specification is given with its dependencies                         | `FactsSteps.ThenFirstDependsOnSecond`; `FactsSteps.ThenSecondBlocksFirst`; `FactsDocumentUnitTests.DependencyListsTheCheckCannotResolve_WhenTheDocumentIsMade_ShouldCarryEachAsTheFrontmatterWritesIt`; `FactsCommandIntegrationTests.FeaturesThatListEachOther_WhenFactsAreGivenAsJson_ShouldCarryEachListAsTheFrontmatterWritesIt` | Covered |
| B-005    | A claim is given with its text, status and line                        | Missing                                                                                                                                                                                                                                                                                                                              | Missing |
| B-006    | A constraint is given with what it rules out                           | Missing                                                                                                                                                                                                                                                                                                                              | Missing |
| B-007    | An out-of-scope row is given with its reason                           | Missing                                                                                                                                                                                                                                                                                                                              | Missing |
| B-008    | An open question is given with whether it is resolved                  | Missing                                                                                                                                                                                                                                                                                                                              | Missing |
| B-009    | A lessons and deltas entry is given with its text and line             | Missing                                                                                                                                                                                                                                                                                                                              | Missing |
| B-010    | A sign-off row is given with its reviewer and note                     | Missing                                                                                                                                                                                                                                                                                                                              | Missing |
| B-011    | The records beside a specification are given by number and title       | Missing                                                                                                                                                                                                                                                                                                                              | Missing |
| B-012    | A withdrawn claim is still given                                       | Missing                                                                                                                                                                                                                                                                                                                              | Missing |
| B-013    | The facts succeed on a tree the check fails                            | Missing                                                                                                                                                                                                                                                                                                                              | Missing |
| B-014    | The facts carry no verdict                                             | `FactsSteps.ThenNoVerdict`; `FactsCommandIntegrationTests.ATreeTheCheckReportsAViolationFor_WhenFactsAreGivenAsJson_ShouldCarryNoViolationCountOrVerdict`; `FactsCommandIntegrationTests.AFactsDocumentGivenAVerdictMember_WhenValidatedAgainstThePublishedFactsSchema_ShouldBeRejected`                                             | Covered |
| B-015    | A missing section or an unreadable table leaves only its own facts out | Missing                                                                                                                                                                                                                                                                                                                              | Missing |
| B-016    | A section the manifest renames is read under its new name              | Missing                                                                                                                                                                                                                                                                                                                              | Missing |
| B-017    | The facts have one shape under every pin                               | `FactsSteps.ThenSameMembers`; `FactsDocumentUnitTests.TheSameFeaturesUnderTwoPins_WhenEachDocumentIsMade_ShouldCarryTheSameMembers`; `FactsCommandIntegrationTests.TheSameSpecificationsUnderTwoShippedPins_WhenFactsAreGivenAsJson_ShouldCarryTheSameMembers`                                                                       | Covered |
| B-018    | A path selects one specification                                       | Missing                                                                                                                                                                                                                                                                                                                              | Missing |
| B-019    | A directory selects every specification under it                       | Missing                                                                                                                                                                                                                                                                                                                              | Missing |
| B-020    | A path that selects nothing fails                                      | Missing                                                                                                                                                                                                                                                                                                                              | Missing |
| B-021    | The facts fail on a broken root as the check does                      | `FactsSteps.ThenExitCode`; `FactsCommandUnitTests.ABrokenRoot_WhenExecuted_ShouldExitWithTheChecksCode`; `FactsCommandIntegrationTests.ABrokenRoot_WhenFactsAreGiven_ShouldExitWithTheCodeTheCheckGives`                                                                                                                             | Covered |
| B-022    | The same tree gives the same facts                                     | `FactsSteps.ThenByteIdentical`; `FactsDocumentUnitTests.TheSameFeaturesReadIntoTwoModels_WhenEachDocumentIsSerialized_ShouldGiveTheSameText`; `FactsCommandIntegrationTests.TheSameTreeUnderTwoRoots_WhenFactsAreGivenAsJsonTwice_ShouldGiveByteIdenticalOutput`                                                                     | Covered |
| B-023    | The facts write nothing                                                | `FactsSteps.ThenNothingWritten`; `FactsCommandUnitTests.ATreeWithAFeature_WhenExecuted_ShouldCreateModifyOrDeleteNoFile`; `FactsCommandIntegrationTests.ATreeBesideOtherFiles_WhenFactsAreGivenAsJson_ShouldCreateModifyOrDeleteNoFileUnderTheRootOrBesideIt`                                                                        | Covered |
| B-024    | The text form gives one line per fact                                  | Missing                                                                                                                                                                                                                                                                                                                              | Missing |
| B-025    | The facts carry no claim coverage                                      | Missing                                                                                                                                                                                                                                                                                                                              | Missing |
| B-026    | An epic file is not given                                              | `FactsSteps.ThenFeaturesNotEpic`; `FactsDocumentUnitTests.AnEpicFileBesideAFeature_WhenTheDocumentIsMade_ShouldGiveOnlyTheFeature`; `FactsCommandIntegrationTests.ATreeWithAnEpicFileTheCheckDiscovers_WhenFactsAreGivenAsJson_ShouldGiveTheFeaturesAndNotTheEpic`                                                                   | Covered |

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

<!-- last written by: spec-reviewer, 2026-10-30 (round 3; rounds 1 and 2 moved to Review history) -->

| Section   | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| --------- | ------ | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5, 9-11 | 🟢     | spec-reviewer | Round 3 (2026-10-30) - approved, reviewing the uncommitted working tree on `cc248c54`. C-7, C-1 and B-016 now find every section by the heading the manifest's `sections` lists, as `SectionStructureRule` does, with the four sections' table headers at the engine's reading (decision 0002 as amended); the B-016 Outline exercises a renamed claims, constraints and sign-off heading. § 9 has one row per claim, every tag resolves, the `@boundary` scenarios match § 5 rows 1, 2 and 10, and dependency edges with `0001-F2`, `0001-F5` and `0001-F6` are symmetric. OQ-1 to OQ-6 stay open on recorded defaults, which do not block agreement (as `0001-F9` was approved with OQ-1 to OQ-5 open). §§ 6-8 are pending after agreement and are reviewed when written; § 9 `Missing` rows block items, not this agreement. |

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
