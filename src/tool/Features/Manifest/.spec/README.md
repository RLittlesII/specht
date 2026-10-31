---
title: "Specification: The manifest command"
description: "specht manifest prints the effective manifest of a root, the manifest on disk with every key it leaves out filled from the pinned version's default as the engine reads it, or with --defaults the default manifest the tool ships, as text or as one JSON document, and writes nothing"
type: feature
id: "F9"
epic: "0001"
spec_status: approved
status: ready-for-architecture
priority: med
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Specification governance"
author: "spec-author"
milestone: null
children: []
depends_on: ["F2", "F4", "F5", "F7"]
blocks: []
spikes: []
created: "2026-10-11"
updated: "2026-10-30"
github_issue: null
synced_at: null
---

# Specification: The manifest command

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-11 -->

A manifest states only what its author wrote; every key it leaves out is read from a default that lives inside the tool, so nobody can see the configuration a check actually ran under without reading the engine. This Feature removes that failure state: `specht manifest` prints the effective manifest of a root, and with `--defaults` the default manifest the tool ships, as text or as one JSON document.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-11, 2026-10-24 -->

| #   | Persona                                 | Need                                                                    | Pain Point Today                                                    |
| --- | --------------------------------------- | ----------------------------------------------------------------------- | ------------------------------------------------------------------- |
| 1   | The maintainer configuring a repository | See every value the check reads, the ones the manifest leaves out too   | A left-out key's value is in the engine's embedded default only     |
| 2   | The agent editing a manifest            | The default of every key, without a repository at hand                  | Runs `specht init` in an empty directory to read the files it wrote |
| 3   | A script or an MCP host (epic `0132`)   | The same answer as one JSON document on stdout                          | No command prints schema knowledge                                  |
| 4   | `0001-F5`, `0001-F7`                    | A reader of the loaded manifest that adds no second reading of the file | N/A (internal dependency)                                           |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                                                                       |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| A-1 | The command line is `specht manifest [--root <dir>] [--defaults] [--json]`, beside `specht`, `init` and `upgrade`; `--root` defaults to the current directory as it does for the check (owner, 2026-10-11; decision 0001).                                                                       |
| A-2 | "The effective manifest" is the manifest on disk as the engine reads it for a check: each key it declares at its own value, and each key it leaves out that the engine reads from the default manifest at the value the pinned version's default gives it (`0001-F5` B-019, A-4; decision 0001). |
| A-3 | "The default manifest" under `--defaults` is the `manifest.json` the tool embeds for the newest version it ships, the copy `init` writes and `0001-F4` B-003 holds byte-identical; it is not the output of any command. Whether another version can be named is OQ-3.                            |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-30 (B-021 names its schema path) -->

| ID    | Claim                                                                                                                                                                                                                                                                                                                                                                                                                                              | Source                                                                                                                                            | Status |
| ----- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- | ------ |
| B-001 | Given a root whose manifest the check accepts, this Feature prints the effective manifest on stdout.                                                                                                                                                                                                                                                                                                                                               | owner, 2026-10-11; decision 0001                                                                                                                  | Active |
| B-002 | Given a root whose manifest the check accepts, this Feature exits with code `0`.                                                                                                                                                                                                                                                                                                                                                                   | `0001-F2` C-2; C-5                                                                                                                                | Active |
| B-003 | Given a manifest that declares a key, the effective manifest carries that key at the manifest's value.                                                                                                                                                                                                                                                                                                                                             | decision 0001; `0001-F7` C-4                                                                                                                      | Active |
| B-004 | Given a manifest that leaves out a key the engine reads from the default manifest (`0001-F5` B-019), the effective manifest carries that key at the value the pinned version's default gives it.                                                                                                                                                                                                                                                   | owner, 2026-10-11; decision 0001; `0001-F5` B-019                                                                                                 | Active |
| B-005 | Given a manifest that sets no level for a rule id of the pinned version's vocabulary, the effective manifest's rule settings carry that id at the rule's own severity.                                                                                                                                                                                                                                                                             | decision 0001; `0001-F5` B-044, decision 0009                                                                                                     | Active |
| B-006 | Given a manifest with no `schemaVersion` key, the effective manifest carries `schemaVersion` `0.1.0`.                                                                                                                                                                                                                                                                                                                                              | derived from decision 0001 and `0001-F7` A-1, B-002; not stated by the owner                                                                      | Active |
| B-007 | Given `--json` and the effective manifest it prints written as the manifest of a copy of the root, the check reports for the copy what it reports for the root.                                                                                                                                                                                                                                                                                    | derived by spec-author from the owner's "what the engine reads" (decision 0001); not stated by the owner; OQ-2                                    | Active |
| B-008 | Given `--defaults`, this Feature prints the default manifest of the newest version the tool ships on stdout.                                                                                                                                                                                                                                                                                                                                       | owner, 2026-10-11; decision 0001; A-3; OQ-3                                                                                                       | Active |
| B-009 | Given `--defaults`, stdout is the same whatever the root holds: no manifest, a manifest the check rejects, or a `--root` that is not a directory.                                                                                                                                                                                                                                                                                                  | owner, 2026-10-11 ("needs no root or manifest"); decision 0001                                                                                    | Active |
| B-010 | Given `--defaults`, this Feature exits with code `0` whatever the root holds.                                                                                                                                                                                                                                                                                                                                                                      | owner, 2026-10-11; decision 0001; C-5                                                                                                             | Active |
| B-011 | Given `--json`, with or without `--defaults`, stdout parses as exactly one JSON document.                                                                                                                                                                                                                                                                                                                                                          | owner, 2026-10-11; decision 0001                                                                                                                  | Active |
| B-012 | Given `--json` and `--defaults`, the document's keys and values equal those of the `manifest.json` the tool embeds for the newest version it ships.                                                                                                                                                                                                                                                                                                | derived from A-3 and `0001-F4` B-003; not stated by the owner; decision 0002                                                                      | Active |
| B-013 | Given `--root` names a path that is not a directory and no `--defaults`, this Feature writes a message naming that path as it was given on stderr, writes nothing on stdout, and exits with code `2`.                                                                                                                                                                                                                                              | `0001-F2` B-005; C-5                                                                                                                              | Active |
| B-014 | Given a root with no manifest at the manifest path and no `--defaults`, this Feature writes a message naming the manifest path on stderr, writes nothing on stdout, and exits with code `2`.                                                                                                                                                                                                                                                       | `0001-F2` B-006; C-5; C-2                                                                                                                         | Active |
| B-015 | Given a root whose manifest the check rejects - one that does not parse (`0001-F2` B-007), one `0001-F5` rejects (`0001-F5` B-022), one whose `schemaVersion` the check rejects (`0001-F7` B-003, B-039), or one recording an upstream schema source while explicitly selecting the embedded source (`0001-F7` B-036) - and no `--defaults`, this Feature names on stderr what the check names, writes nothing on stdout, and exits with code `3`. | owner, 2026-10-08 (every command answers a rejected manifest with `3`); `0001-F2` decision 0003; C-5                                              | Active |
| B-016 | Given any run, stdout carries only the manifest printed, and every message about the tool itself goes to stderr.                                                                                                                                                                                                                                                                                                                                   | `0001-F2` B-009, C-3; C-6                                                                                                                         | Active |
| B-017 | Given any run, no file is created, modified or deleted, under the root or anywhere else.                                                                                                                                                                                                                                                                                                                                                           | brief § 9; C-1                                                                                                                                    | Active |
| B-018 | Given any output on stdout or stderr, every path the tool derives is relative to the root with `/` separators, and the only path that may be absolute is one typed on the command line, echoed as given.                                                                                                                                                                                                                                           | brief § 9; `0001-F2` B-013                                                                                                                        | Active |
| B-019 | Given the same root, manifest and options, two runs print the same stdout and exit with the same code.                                                                                                                                                                                                                                                                                                                                             | brief § 9; C-3                                                                                                                                    | Active |
| B-020 | Given no `--json`, stdout is the document `--json` prints for the same options, indented, with its keys in the order the default manifest of the version being printed holds them and its rule settings in ascending ordinal order of rule id.                                                                                                                                                                                                     | derived by spec-author (a form a test can fail, decision 0002); not stated by the owner; OQ-1                                                     | Active |
| B-021 | Given `--json`, with or without `--defaults`, the document validates against `docs/schema/manifest.schema.json`.                                                                                                                                                                                                                                                                                                                                   | the owner, 2026-10-30; `0132-F2` decision 0001 call 4; OQ-4; the path, the owner, 2026-10-30, as `0001-F13` B-002 names its own; as `0001-F3` C-1 | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-30 (OQ-4: B-021, C-7) -->

| ID  | Constraint                                                                                                                                                                                                                                 | Rules Out                                                                                                                                                                    |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | The command is read-only: it writes nothing, anywhere.                                                                                                                                                                                     | An `--output <path>`; writing the filled manifest back into the root; a `--fix`; creating a manifest where none exists.                                                      |
| C-2 | The effective manifest is the one the check's loader produced: the same reader and the same defaults give both.                                                                                                                            | A second manifest reader or a second table of defaults in the command; an embedded default used when the root has no manifest; a value printed that the check does not read. |
| C-3 | Deterministic and offline: the output is a function of the manifest, the options and the tool version.                                                                                                                                     | A network call, a fetch of a recorded upstream schema source included (`0001-F7` C-8); a clock, a locale or a machine name in the output.                                    |
| C-4 | A command parses, calls the engine and folds the result into an exit code; nothing else lives in the command (`0001-F2` C-4).                                                                                                              | Filling a default or ordering keys inside `Features/Manifest/`.                                                                                                              |
| C-5 | Exit codes are `0001-F2` C-2's constants, with the meanings the check gives them.                                                                                                                                                          | A sixth exit code; a code that means one thing to this command and another to the check.                                                                                     |
| C-6 | stdout carries the product only, as plain text (`0001-F2` C-3).                                                                                                                                                                            | A banner, a heading, colour markup or a note about a filled key on stdout.                                                                                                   |
| C-7 | The document's shape is written once, as a JSON Schema published in this repository, and within a tool major version the document changes only by addition (the owner, 2026-10-30; `0132-F2` decision 0001 call 4; as `0001-F3` C-1, C-4). | An anonymous object as the only definition; a member the schema does not name; renaming or removing a member, or changing its type, inside a major version.                  |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-11, 2026-10-11 (row 8, `0001-F2` decision 0010) -->

| #   | Item                                                                  | Exclusion Reason                                                                                                                                                                                                                                                                                    |
| --- | --------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Editing the manifest: a rule's level, or any other key                | `0001-F11` sets one rule's level; `schemaVersion` is the pin command's (`0001-F7` B-043); every other key is a hand edit. C-1.                                                                                                                                                                      |
| 2   | Listing the rules with their family and default severity              | `0001-F10`. This command prints the rule settings as the manifest holds them and nothing about a rule.                                                                                                                                                                                              |
| 3   | Printing the frontmatter schemas or the templates                     | `init` writes them as files (`0001-F4`); whether they need a printing command is epic `0132`'s open question and the owner's.                                                                                                                                                                       |
| 4   | Printing the report schema                                            | `0001-F3` A-1: it is a published file, the tool's contract.                                                                                                                                                                                                                                         |
| 5   | A diff of the manifest against the default                            | Not asked for; a caller compares the `--json` outputs of the two forms.                                                                                                                                                                                                                             |
| 6   | Checking the tree, or any rule                                        | `0001-F2`. The command stops at the loaded manifest.                                                                                                                                                                                                                                                |
| 7   | The default manifest of a version other than the newest               | Not decided (OQ-3).                                                                                                                                                                                                                                                                                 |
| 8   | A `--version` option that prints the schema version the manifest pins | Considered by the owner and not chosen, 2026-10-11 (`0001-F2` decision 0010): the pinned version is marked in `specht schema list` (`0001-F7` B-084), and the effective manifest carries `schemaVersion` (B-006). A boolean `--version` would also take the name OQ-3 may want for a version value. |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-27 -->

| Concern                                                                  | Classification | Home                                                                            |
| ------------------------------------------------------------------------ | -------------- | ------------------------------------------------------------------------------- |
| Declared values and exact-pin omission defaults (B-003, B-004, B-006)    | Business       | The existing manifest parser, alongside its current value selection             |
| Omission that disables an optional capability (B-007, OQ-2)              | Business       | The parser's existing declaration checks; the effective document preserves them |
| Effective levels from the pinned vocabulary and rule pages (B-005)       | Business       | The existing `RuleEntriesReader.Read(SpecStructure)`                            |
| Root observation, one manifest read and input failures                   | Technical      | The existing `SpecManifestStore` and `SpecSchemasLoader`                        |
| A reusable effective JSON document with no file writes (B-007, C-1, C-2) | Both           | A concrete engine reader under `Specht.Manifest`                                |
| Console output, formatting and command exit codes                        | Technical      | The manifest command folds failures and writes reader-produced text             |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-27 -->

### Existing implementation to extend

- [`SpecSchemasLoader.LoadManifest`](../../../../specht/Model/SpecSchemasLoader.cs)
  owns root and manifest observation and the wrapping of malformed input.
  [`SpecManifestStore.ReadManifest`](../../../../specht/Manifest/SpecManifestStore.cs)
  is the file read; it already handles the encodings the check accepts.
- [`SpecManifestParser.Parse`](../../../../specht/Manifest/SpecManifestParser.cs)
  selects the exact pin's embedded `SchemaVersion.Manifest` and resolves each
  value. `Filled`, `Discovery`, table-role filling and claim-tag selection
  already define omission granularity. `PinnedVersion` rejects unshipped pins
  and rule ids outside their vocabulary.
- [`RuleEntriesReader.Read(SpecStructure)`](../../../../specht/Rules/RuleEntriesReader.cs)
  is the pure, already-loaded path that supplies each rule's effective level.
  Its explicit-level precedence includes `off`; an omitted level comes from
  the rule page, even when the embedded manifest declares another level.
- [`SpecStructure`](../../../../specht/Manifest/SpecStructure.cs) is the rules'
  typed view, not a serializable manifest. It contains declaration flags and
  derived upstream addresses, and omits accepted `$` metadata and
  `codeAnalysis`. Serializing that record would lose declared JSON and could
  enable an epic grammar that was never declared.

### Engine path

The parser and loader retain their current entry points and expose a document
path through the same parsing and failure handling.

| Implementation                                                                            | Responsibility                                                                                                                      |
| ----------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| [`SpecManifestParser.ParseDocument`](../../../../specht/Manifest/SpecManifestParser.cs)   | Capture selected defaults during the existing parse, returning the validated structure with an owned JSON document                  |
| [`SpecManifestDocument`](../../../../specht/Manifest/SpecManifestDocument.cs)             | Carry the invocation-local structure and document without behavior or loaded state                                                  |
| [`SpecSchemasLoader.LoadManifestDocument`](../../../../specht/Model/SpecSchemasLoader.cs) | Share the existing root checks, one store read and malformed-input wrapping through its two-caller `ReadManifest` helper            |
| [`EffectiveManifestReader.Read`](../../../../specht/Manifest/EffectiveManifestReader.cs)  | Call the document loader once and expand effective rule levels through the already-loaded structure overload of `RuleEntriesReader` |

The paired parser result is an invocation-local data value; it carries no
store, root path, cache or behavior. Keep the existing `Parse` and
`LoadManifest` APIs on their current validation path. Share their parsing and
failure handling with the new entry points instead of maintaining a second
implementation. The document is materialized only when requested; ordinary
checks need no JSON clone or effective-rules expansion.

The document starts as an invocation-owned clone of the parsed declaration. Capture
only when the document path is requested. Preserve accepted metadata,
`codeAnalysis`, optional settings and nested extension values. Capture
the values selected by the existing parser at the point they are selected;
do not reconstruct a second defaulting policy in the reader, serialize the
typed record, or apply a generic recursive merge over the embedded manifest.
JSON nodes returned to the caller are owned by that invocation and never alias
an embedded default or another root's document.

The reader passes the already-loaded structure to `RuleEntriesReader`; it does
not use that reader's root overload. This both validates the pin and vocabulary
before returning a document and avoids reading the root twice. Replace only the
effective document's `rules` projection, with lower-case `error`, `warning` and
`off` entries in ordinal id order. Explicit consumer levels retain their value.
Do not mutate the structure's explicit settings or evaluate a rule.

The concrete reader is registered in
[`AddSpechtEngine`](../../../../specht/Composition/SpechtEngineServiceCollectionExtensions.cs).
It uses the existing composition, stores and catalog. No new interface,
mediator, model-reader migration, host factory, package or schema version is
needed; ADR-0014's execution gate remains in force.

### Omission and preservation boundaries

| Input or existing parser decision                                                                                      | Effective document                                                                                                             |
| ---------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| A declared scalar, list or nested extension value                                                                      | Preserve the declared value; supplement only omitted members the parser actually fills                                         |
| A whole discovery list or `sections` is omitted                                                                        | Carry the whole value selected from the exact pin's default; never concatenate arrays                                          |
| A role, marker, frontmatter schema name or non-epic grammar is omitted                                                 | Carry the parser's per-member filled value                                                                                     |
| `tables` is declared under an older pin                                                                                | Preserve the parser's whole-table selection; do not add absent roles                                                           |
| A table role is omitted under schema `0.2.0` or later                                                                  | Carry the parser's per-role default; a declared header list remains whole                                                      |
| The epic grammar is absent from the declaration                                                                        | Keep it absent, even though the typed identifiers dictionary holds a fallback; reparsing must keep `DeclaresEpicGrammar` false |
| `schemaVersion` is absent                                                                                              | Carry the parsed version as `0.1.0`, rather than the newest version                                                            |
| A rule level is absent                                                                                                 | Carry that pinned rule page's severity through the shared rule-entry reader, not the embedded manifest's rule level            |
| `claim` members are omitted                                                                                            | Materialize only the tag defaults the selected parser branch reads; never introduce this key for an older pin                  |
| `schemaSource`, `spechtSchema`, `tests`, `matrix`, `owners`, `requiredSpecifications` or `consumerTemplates` is absent | Preserve absence; do not invent optional configuration from a default or from a typed sentinel                                 |
| `$` metadata or `codeAnalysis` is declared                                                                             | Preserve its JSON without interpreting, filling or relocating its members                                                      |

An explicit JSON `null` is not a missing key. Preserve an accepted declared
value unless an existing claim requires its expansion, as B-005 does for rule
settings. Do not tighten the check's accepted shapes or introduce a new null
policy during projection. B-003 preserves explicit values; B-004 supplies
omissions; B-007 requires the result to retain the check's interpretation.

[Decision 0003](decisions/0003-design-the-effective-document-on-the-existing-omission-boundary.md)
records proceeding on the existing OQ-2 boundary without owner ratification.
No work-item declaration, native-claim setting or analyzer default is added by
this Feature. An output that reparses differently is a design failure, not an
exception to B-007.

### Boundary of this design

The reader stops before discovery, schema-file loading, upstream verification
or fetch, and rule evaluation. A recorded upstream source remains JSON data;
its derived file addresses are not serialized. Manifest-only inspection uses
the same manifest validation as the check without making an optional local
schema file or a network request a prerequisite.

The reader's JSON projection remains reusable. Its text entry point formats that
projection in the engine, using the exact pin's embedded manifest to order the
root keys. Declared keys absent from that template follow in ordinal order;
rule entries always follow ordinal rule id order. Nested values retain their
JSON content, including accepted nulls and extension data. Formatting clones
nodes so it cannot mutate a caller's document or the embedded defaults.

The command uses the existing production composition, a nested settings type and
slice registration. It calls the reader and writes its returned text through the
injected console's plain writer. Typed root and manifest failures map to the
check's existing exit constants and stderr messages. It neither runs the check
nor loads optional schema files. The root option defaults to the working
directory. No host factory or architecture migration is introduced.

The default path selects the newest entry from the injected version catalogue
and formats its embedded manifest directly. It bypasses both root normalization
and the manifest loader, retaining the embedded keys and values rather than
expanding rule severities or omission defaults. This follows A-3 and decision
0002 while OQ-3 remains open.

Both selections share the same engine formatter. The `--json` option selects
compact serialization; the text form selects indentation. They preserve the
same document values. Neither selection writes a report or consumer file.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-27 -->

Every companion scenario runs the production command in a child process.
The round-trip scenario writes its actual `--json` output into a synthetic copy
and compares complete reports with nonempty violations. No pending command
binding is counted as coverage. Unit tests exercise the reader and direct
command over `MockFileSystem`; integration tests cross the process boundary.
The fixtures and network addresses are synthetic and no test contacts a server.

| Claim | Mechanism and proof                                                                                                                                                                    |
| ----- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| B-001 | Reader projection and plain output: `ManifestCommandUnitTests` and `ManifestCommandIntegrationTests`.                                                                                  |
| B-002 | Success fold: `ManifestCommandUnitTests`.                                                                                                                                              |
| B-003 | Declaration preservation: `EffectiveManifestReaderUnitTests`.                                                                                                                          |
| B-004 | Exact-pin omission selection: `EffectiveManifestReaderUnitTests`.                                                                                                                      |
| B-005 | Shared pinned-page severity precedence: `EffectiveManifestReaderUnitTests`.                                                                                                            |
| B-006 | Original-version fallback: `EffectiveManifestReaderUnitTests`.                                                                                                                         |
| B-007 | Declaration-sensitive round-trip: `EffectiveManifestReaderUnitTests`, `EffectiveManifestReaderIntegrationTests` and the command scenario.                                              |
| B-008 | Newest embedded selection: `ManifestDefaultsUnitTests` and `ManifestDefaultsIntegrationTests`.                                                                                         |
| B-009 | Default branch before root normalization/loading: `ManifestDefaultsUnitTests` uses an invalid path and an empty file system.                                                           |
| B-010 | Default success fold: `ManifestDefaultsUnitTests`.                                                                                                                                     |
| B-011 | JSON selection and one-document serialization: `ManifestJsonUnitTests` and `ManifestJsonIntegrationTests`.                                                                             |
| B-012 | Embedded values without effective expansion: `ManifestDefaultsUnitTests` uses a synthetic newest default with explicit rule levels.                                                    |
| B-013 | Root-not-found fold: `ManifestCommandUnitTests` and check-diagnostic equivalence in `ManifestCommandIntegrationTests`.                                                                 |
| B-014 | Manifest-not-found fold: the same typed failure theory.                                                                                                                                |
| B-015 | Malformed, unknown-member, malformed-pin, unshipped-pin and conflicting-source folds: the same theory.                                                                                 |
| B-016 | Product-only writer and separate error writer: `ManifestCommandUnitTests`.                                                                                                             |
| B-017 | Read-only engine/command seams: all fake files, including an outside sentinel, are unchanged in `ManifestCommandUnitTests`; process snapshots cover success, defaults and repeat runs. |
| B-018 | Root-relative typed diagnostics: `ManifestCommandUnitTests` and process comparison with the check.                                                                                     |
| B-019 | Repeatability and preserved inputs: `ManifestTextUnitTests` and `ManifestCommandIntegrationTests`.                                                                                     |
| B-020 | Engine-owned default-key and ordinal-rule ordering: `ManifestTextUnitTests`, synthetic default rules in `ManifestDefaultsUnitTests` and both command forms in the scenario.            |

`ManifestCommand.ExecuteAsync` only selects the reader entry point, writes its
text and folds typed failures. Its decisions are exercised directly without
Spectre's parser; the integration tier verifies registration, options, stdout,
stderr and exit codes through the shipped composition. `Format` only orders
root keys and rule entries and serializes owned clones; its nested values
remain the projection's responsibility. No new host factory or shared-source
link is added.

Focused verification selects `net11.0`; required repository gates retain their
configured target matrix. No benchmark is needed: the normal check does not
materialize or format the projection, no architecture migration is introduced,
and no performance benefit is claimed.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-30 (OQ-4: B-021, C-7) -->

| Claim ID | Scenario                                                 | Test                                                                                                                                                                                                                                                                                                                                                                                                                                                | Status  |
| -------- | -------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| B-001    | The effective manifest is printed                        | `EffectiveManifestSteps.ThenEffectiveDocument`; `ManifestCommandUnitTests.AValidRoot_WhenExecuted_ShouldPrintOnlyTheEffectiveManifestAndLeaveAllFilesUnchanged`; `ManifestCommandIntegrationTests.AValidRoot_WhenPrinting_ShouldEmitOnlyTheEffectiveDocumentInDefaultOrder`                                                                                                                                                                         | Covered |
| B-002    | Printing the effective manifest succeeds                 | `EffectiveManifestSteps.ThenExitCode`; `ManifestCommandUnitTests.AValidRoot_WhenExecuted_ShouldPrintOnlyTheEffectiveManifestAndLeaveAllFilesUnchanged`                                                                                                                                                                                                                                                                                              | Covered |
| B-003    | A declared value is printed as declared                  | `EffectiveManifestSteps.ThenDeclaredName`; `EffectiveManifestReaderUnitTests.DeclaredJson_WhenProjected_ShouldKeepItsValuesAndNestedExtensions`; `EffectiveManifestReaderUnitTests.AcceptedNull_WhenProjected_ShouldRemainDeclared`; `EffectiveManifestReaderUnitTests.ARecordedSourceWithoutLocalSchemas_WhenInspected_ShouldPreserveItsJsonWithoutLoadingTheSchemas`                                                                              | Covered |
| B-004    | A value left out is printed at its default               | `EffectiveManifestSteps.ThenDefaultName`; `EffectiveManifestReaderUnitTests.OmittedValues_WhenProjected_ShouldCarryTheExactPinsSelectedDefaults`; `EffectiveManifestReaderUnitTests.PartialMapsAndTables_WhenProjected_ShouldFollowThePinsMergeBoundary`; `EffectiveManifestReaderUnitTests.AlternatingRootsAndMutatedResults_WhenProjected_ShouldKeepDefaultsAndOtherInvocationsIsolated`                                                          | Covered |
| B-005    | A rule with no level is printed at its own severity      | `EffectiveManifestSteps.ThenRuleSeverity`; `EffectiveManifestReaderUnitTests.ExplicitAndMissingLevels_WhenProjected_ShouldUseTheSharedPinnedPagePrecedence`                                                                                                                                                                                                                                                                                         | Covered |
| B-006    | A manifest without a version is printed as version 0.1.0 | `EffectiveManifestSteps.ThenVersion`; `EffectiveManifestReaderUnitTests.NoPinOrOptionalCapabilities_WhenProjected_ShouldRetainTheOriginalVersionAndDeclarationSemantics`                                                                                                                                                                                                                                                                            | Covered |
| B-007    | The effective manifest gives the check's verdict         | `EffectiveManifestSteps.ThenEquivalentReports`; `EffectiveManifestReaderUnitTests.OptionalDeclarationsAndNestedNulls_WhenReparsed_ShouldKeepTheChecksInterpretation`; `EffectiveManifestReaderUnitTests.NoPinOrOptionalCapabilities_WhenProjected_ShouldRetainTheOriginalVersionAndDeclarationSemantics`; `EffectiveManifestReaderIntegrationTests.AViolatingRootWithoutEpicGrammar_WhenCopiedWithItsEffectiveManifest_ShouldKeepTheCompleteReport` | Covered |
| B-008    | The default manifest is printed                          | `EffectiveManifestSteps.ThenDefaultDocument`; `ManifestDefaultsUnitTests.NoReadableRoot_WhenPrintingDefaults_ShouldUseTheNewestEmbeddedValuesWithoutProjection`; `ManifestDefaultsIntegrationTests.AnyRoot_WhenPrintingDefaults_ShouldIgnoreTheRootAndPrintTheNewestEmbeddedDocument`                                                                                                                                                               | Covered |
| B-009    | The default manifest does not depend on the root         | `EffectiveManifestSteps.ThenIndependentOfRoot`; `ManifestDefaultsUnitTests.NoReadableRoot_WhenPrintingDefaults_ShouldUseTheNewestEmbeddedValuesWithoutProjection`; `ManifestDefaultsIntegrationTests.AnyRoot_WhenPrintingDefaults_ShouldIgnoreTheRootAndPrintTheNewestEmbeddedDocument`                                                                                                                                                             | Covered |
| B-010    | Printing the default manifest succeeds without a root    | `EffectiveManifestSteps.ThenExitCode`; `ManifestDefaultsUnitTests.NoReadableRoot_WhenPrintingDefaults_ShouldUseTheNewestEmbeddedValuesWithoutProjection`                                                                                                                                                                                                                                                                                            | Covered |
| B-011    | The JSON form is one document                            | `EffectiveManifestSteps.ThenOneDocument`; `ManifestJsonUnitTests.EitherSelection_WhenExecutedAsJson_ShouldKeepTheSelectedDocumentsValues`; `ManifestJsonIntegrationTests.EitherSelection_WhenPrintingJson_ShouldEmitOneDocumentEquivalentToTheTextForm`                                                                                                                                                                                             | Covered |
| B-012    | The default manifest is the embedded one                 | `EffectiveManifestSteps.ThenEmbeddedValues`; `ManifestDefaultsUnitTests.NoReadableRoot_WhenPrintingDefaults_ShouldUseTheNewestEmbeddedValuesWithoutProjection`; `ManifestDefaultsIntegrationTests.AnyRoot_WhenPrintingDefaults_ShouldIgnoreTheRootAndPrintTheNewestEmbeddedDocument`                                                                                                                                                                | Covered |
| B-013    | A missing root is a missing-input failure                | `EffectiveManifestSteps.ThenNamesRoot`; `ManifestCommandUnitTests.InvalidInput_WhenExecuted_ShouldFoldTheTypedFailureAndWriteOnlyToStderr`; `ManifestCommandIntegrationTests.InvalidInput_WhenPrinting_ShouldMatchTheChecksFailure`                                                                                                                                                                                                                 | Covered |
| B-014    | A root without a manifest is a missing-input failure     | `EffectiveManifestSteps.ThenNamesManifest`; `ManifestCommandUnitTests.InvalidInput_WhenExecuted_ShouldFoldTheTypedFailureAndWriteOnlyToStderr`; `ManifestCommandIntegrationTests.InvalidInput_WhenPrinting_ShouldMatchTheChecksFailure`                                                                                                                                                                                                             | Covered |
| B-015    | A rejected manifest is an invalid-manifest failure       | `EffectiveManifestSteps.ThenMatchesCheck`; `ManifestCommandUnitTests.InvalidInput_WhenExecuted_ShouldFoldTheTypedFailureAndWriteOnlyToStderr`; `ManifestCommandIntegrationTests.InvalidInput_WhenPrinting_ShouldMatchTheChecksFailure`                                                                                                                                                                                                              | Covered |
| B-016    | Messages about the tool go to standard error             | `EffectiveManifestSteps.ThenProductOnly`; `ManifestCommandUnitTests.AValidRoot_WhenExecuted_ShouldPrintOnlyTheEffectiveManifestAndLeaveAllFilesUnchanged`                                                                                                                                                                                                                                                                                           | Covered |
| B-017    | A run writes nothing                                     | `EffectiveManifestSteps.ThenUnchanged`; `ManifestCommandUnitTests.AValidRoot_WhenExecuted_ShouldPrintOnlyTheEffectiveManifestAndLeaveAllFilesUnchanged`; `ManifestCommandIntegrationTests.RepeatedRuns_WhenPrinting_ShouldPreserveFilesAndOutput`                                                                                                                                                                                                   | Covered |
| B-018    | No derived path in the output is absolute                | `EffectiveManifestSteps.ThenRelativePaths`; `ManifestCommandUnitTests.InvalidInput_WhenExecuted_ShouldFoldTheTypedFailureAndWriteOnlyToStderr`; `ManifestCommandIntegrationTests.InvalidInput_WhenPrinting_ShouldMatchTheChecksFailure`                                                                                                                                                                                                             | Covered |
| B-019    | The same input gives the same output                     | `EffectiveManifestSteps.ThenDeterministic`; `ManifestTextUnitTests.ReorderedDeclarations_WhenFormatted_ShouldFollowTheExactDefaultAndKeepExtensionValues`; `ManifestCommandIntegrationTests.RepeatedRuns_WhenPrinting_ShouldPreserveFilesAndOutput`                                                                                                                                                                                                 | Covered |
| B-020    | The text form is the JSON document, indented, in order   | `EffectiveManifestSteps.ThenSameDocument`; `EffectiveManifestSteps.ThenKeyOrder`; `EffectiveManifestSteps.ThenRuleOrder`; `ManifestTextUnitTests.ReorderedDeclarations_WhenFormatted_ShouldFollowTheExactDefaultAndKeepExtensionValues`                                                                                                                                                                                                             | Covered |
| B-021    | The JSON form validates against its published schema     | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                             | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-30 -->

- 2026-10-11, § 5 row 8 added: the owner considered a `--version` option on this command for the pinned schema version and did not choose it (`0001-F2` decision 0010). No claim, constraint or scenario changed.
- 2026-10-24, groomed from `blocked` to `ready-for-architecture` ([decision 0002](decisions/0002-the-default-is-the-embedded-manifest-and-the-text-form-is-the-json-document.md)). The block named `0001-F2`, `0001-F4`, `0001-F5` and `0001-F7`: `0001-F2` and `0001-F5` are built; `0001-F7`'s list and pin commands are built (items `0121` to `0125`) and only `upgrade` (item `0047`) is outstanding, which no claim here depends on. `0001-F4` stays a reference for B-003 only.
- 2026-10-24, A-3 and B-012 re-anchored: they named what `specht init --epics` writes, a command specified and not built (`0001-F4` B-018 Missing, OQ-18); they now name the default manifest the tool embeds for the newest shipped version. B-012 keeps its id. B-020 added: the text form, derived on OQ-1's default.
- 2026-10-30, OQ-4 resolved by the owner (`0132-F2` decision 0001 call 4): the JSON document gets a published schema and changes only by addition within a tool major version. B-021 and C-7 added, with a scenario and a `Missing` § 9 row. `spec_status` returns from `approved` to `draft`: the agreement is reopened for review of the new claim and constraint.
- 2026-10-30, after `spec-reviewer` round 2: B-021 names `docs/schema/manifest.schema.json` (the owner, 2026-10-30), as `0001-F13` B-002 names its schema. No other claim changed.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-30 (OQ-4 resolved) -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                                                                                                                            | Blocks                              | Resolution                                                                                                                                                                                                                                                     |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | What does the output look like without `--json`, and in what order are the keys printed, in either form? The manifest is itself JSON, so the text form may be that JSON, a key-per-line listing, or something else; the order may be the on-disk manifest's, the default manifest's, or alphabetical. Owner not asked.                                                                                                                                                              | B-001, B-008, B-020 (the form only) | Open. Proceeds on the JSON document, indented, in the default manifest's key order (B-020). Owner not asked.                                                                                                                                                   |
| OQ-2 | The owner's decision reads "every omitted key filled from the pinned version's default" and "what the engine reads". They part on a key whose absence is itself a setting: the epic grammar (`0001-F5` C-10, decision 0004), which the default declares and the engine does not fill; what declares work-item files, which is never filled from a default (`0101-F8` C-12). Does the effective manifest leave such a key out, as the engine reads it, or print the default's value? | B-004, B-007                        | Open. B-004 claims only the keys `0001-F5` B-019 fills, and B-007 is written on "what the engine reads". Owner not asked.                                                                                                                                      |
| OQ-3 | Does `--defaults` take a version, so the default manifest of an older shipped version can be printed, and beside a manifest does it follow that manifest's pin?                                                                                                                                                                                                                                                                                                                     | B-008, B-012; § 5 row 7             | Open. Proceeds on the default: the newest version the tool ships, and no root read (B-009). Owner not asked.                                                                                                                                                   |
| OQ-4 | Does the JSON document of this command get a published schema, as the report has (`0001-F3` C-1), and is it bound by an additive-change rule as `0001-F3` C-4 binds the report? Epic `0132` would read it.                                                                                                                                                                                                                                                                          | -                                   | Resolved 2026-10-30 by the owner (`0132-F2` decision 0001 call 4): yes, a published JSON Schema bound to change only by addition within a tool major version, as for every `--json` document MCP serves. Rejected: no schema. B-021 and C-7 say so. Was: open. |
| OQ-5 | Does the effective manifest say which values came from the manifest and which from the default?                                                                                                                                                                                                                                                                                                                                                                                     | -                                   | Open. No claim is written; C-6 rules out a note on stdout until it is answered. Owner not asked.                                                                                                                                                               |

## 12. Sign-off

<!-- last written by: spec-author, 2026-10-11; spec-reviewer, 2026-10-30 -->

| Section                | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| ---------------------- | ------ | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5                    | 🟢     | spec-reviewer | Delivery self-review, 2026-10-27: B-001 to B-020, constraints and exclusions agree with decisions 0001 to 0003. OQ-1 to OQ-5 retain their recorded scope and provenance; proceeding on the existing defaults does not ratify them. No unresolved findings within this agreement; no independent sign-off is claimed.                                                                                                                                                                                       |
| 1-5, B-021 and C-7     | 🟢     | spec-reviewer | Re-review, 2026-10-30, of the agreement reopened by OQ-4, round 2: B-021 now names `docs/schema/manifest.schema.json`, as `0001-F13` B-002 names its own. B-021, C-7, the `@B-021` scenario and its § 9 row agree with `0132-F2` decision 0001 call 4; no findings. With every row 🟢, `spec_status` returns to `approved`; B-021's `Missing` row blocks an item, not the agreement.                                                                                                                       |
| 5 (row 8)              | 🟢     | spec-reviewer | Row 8 only (2026-10-11), reviewing `origin/main` (`5c22c84`)...`28d0ba6`: it states choice 3 of `0001-F2` decision 0010 and cites `0001-F7` B-084, B-006 and OQ-3, which exist. Everything else in § 1-5 still awaits its first review, so `spec_status` stays `draft`.                                                                                                                                                                                                                                    |
| 6-8, engine design     | 🟢     | spec-reviewer | Architecture proposal prepared 2026-10-26 against B-003 to B-007; review pending. No implementation or claim coverage is approved by this row. Decision 0003 preserves OQ-2's open status. Superseded by the complete delivery self-review below.                                                                                                                                                                                                                                                          |
| 6-9, engine projection | 🟢     | spec-reviewer | Delivery self-review, 2026-10-26: B-003 to B-007 are covered by 23 unit cases, one disk round-trip integration case and five engine scenarios. The complete build passes, including working-package self-check, Format, CodeAnalysis and all configured test tiers. No unresolved findings in the engine scope. This supersedes the pending engine-design review only; broader agreement and command behavior remain outside this approval. No independent sign-off is claimed.                            |
| 6-9, complete command  | 🟢     | spec-reviewer | Delivery self-review, 2026-10-27: all 20 claims have scenario and mechanism proofs. Focused net11.0 verification passes 300 manifest unit/integration cases and all 27 manifest scenarios with zero skips. The complete build passes the working-package self-check, Format, CodeAnalysis and all configured test tiers. The command uses the existing reader and slice registration; no write, network path or architecture rollout is added. No unresolved findings; no independent sign-off is claimed. |

## Tasks

- `0283`: the engine produces the effective manifest (B-003 to B-007).
- `0284`: the `specht manifest` command, text form, exit codes and failure paths (B-001, B-002, B-013 to B-020).
- `0285`: `--defaults` (B-008, B-009, B-010, B-012).
- `0286`: `--json`, with and without `--defaults` (B-011).

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
