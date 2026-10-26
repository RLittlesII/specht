---
title: "Specification: The rules command"
description: "specht rules lists every rule id of the pinned schema version with its family, its default severity and the level it has under the root's manifest, as text or as one JSON document, read from the version's vocabulary and its rule pages, and writes nothing"
type: feature
id: "F10"
epic: "0001"
spec_status: draft
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
depends_on: ["F2", "F3", "F5", "F7"]
blocks: []
spikes: []
created: "2026-10-11"
updated: "2026-10-26"
github_issue: null
synced_at: null
---

# Specification: The rules command

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-11 -->

A rule can be lowered, raised or switched off from the manifest, but nothing lists the rules a pinned version has: a maintainer learns a rule id from the violation that names it, and its level only by reading the manifest beside the rule's page. This Feature removes that failure state: `specht rules` lists every rule id of the pinned version with its family, its default severity and the level it has under this manifest, as text or as one JSON document.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-26 -->

| #   | Persona                                         | Need                                                                                  | Pain Point Today                                                             |
| --- | ----------------------------------------------- | ------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------- |
| 1   | The maintainer adopting the tool on an old tree | Every rule of the pinned version and what each is set to, in one listing              | Rule ids are met one violation at a time; brief § 4 is in another repository |
| 2   | The agent about to change a rule's level        | The ids the manifest may name and the level each has now                              | Reads the manifest, then `--explain` for each id it already knows            |
| 3   | A script or an MCP host (epic `0132`)           | The same listing as one JSON document on stdout                                       | No command prints the rule vocabulary                                        |
| 4   | `0001-F3`, `0001-F5`                            | A listing that reads the vocabulary and the rule settings and keeps no copy of either | N/A (internal dependency)                                                    |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                                                                                                                                                                                         |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| A-1 | The command line is `specht rules [--root <dir>] [--json]`, beside `specht`, `init` and `upgrade`; `--root` defaults to the current directory as it does for the check (owner, 2026-10-11). `rules` is also a branch whose default command is this one, so `0001-F11`'s set command can hang under it as `specht schema pin` hangs under `schema` (`0001-F11` A-1); that places a name and adds no behaviour here. |
| A-2 | A rule's family and default severity are the ones its rule page gives (`0001-F3` B-032, A-5): one of the eight families, and `error`, `warning` or `off`; schema `0.2.0`'s `SPEC034` and `SPEC035` pages give `Off`.                                                                                                                                                                                               |
| A-3 | A rule's effective level is `error`, `warning` or `off` (`0001-F5` decision 0008): the level the manifest sets for its id, or, where the manifest sets none, the level of the rule's own severity (`0001-F5` B-044).                                                                                                                                                                                               |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-11, 2026-10-24 -->

| ID    | Claim                                                                                                                                                                                                                                                                                                                                                                                                                         | Source                                                                                                           | Status |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- | ------ |
| B-001 | Given a root whose manifest the check accepts, this Feature prints on stdout one entry for each rule id in the pinned version's vocabulary and no other entry.                                                                                                                                                                                                                                                                | owner, 2026-10-11; decision 0001; `0001-F3` B-031                                                                | Active |
| B-002 | Given an entry, it carries its rule's family.                                                                                                                                                                                                                                                                                                                                                                                 | owner, 2026-10-11; A-2                                                                                           | Active |
| B-003 | Given an entry, it carries its rule's default severity.                                                                                                                                                                                                                                                                                                                                                                       | owner, 2026-10-11; A-2                                                                                           | Active |
| B-004 | Given a manifest that sets a level for a rule id, that rule's entry carries that level as its effective level.                                                                                                                                                                                                                                                                                                                | owner, 2026-10-11; A-3; `0001-F5` B-010, B-011, B-046                                                            | Active |
| B-005 | Given a manifest that sets no level for a rule id, that rule's entry carries the level of the rule's own severity as its effective level.                                                                                                                                                                                                                                                                                     | A-3; `0001-F5` B-044                                                                                             | Active |
| B-006 | Given a manifest that sets a rule id to `off`, the listing still carries that rule's entry.                                                                                                                                                                                                                                                                                                                                   | derived from B-001 and `0001-F5` B-011; not stated by the owner                                                  | Active |
| B-007 | Given an entry, the family and the default severity it carries are the ones `--explain` prints for that rule id under the same root.                                                                                                                                                                                                                                                                                          | decision 0001; `0001-F3` B-019, B-032, C-6                                                                       | Active |
| B-008 | Given the listing, its entries are in ascending ordinal order of rule id.                                                                                                                                                                                                                                                                                                                                                     | derived by spec-author (the order the vocabulary is written in, brief § 4); not stated by the owner; OQ-5        | Active |
| B-009 | Given `--json`, stdout parses as exactly one JSON document.                                                                                                                                                                                                                                                                                                                                                                   | owner, 2026-10-11                                                                                                | Active |
| B-010 | Given `--json`, the document carries the entries the text form prints, each with the same rule id, family, default severity and effective level, in the same order.                                                                                                                                                                                                                                                           | owner, 2026-10-11; C-2                                                                                           | Active |
| B-011 | Given a root whose manifest the check accepts, this Feature exits with code `0`, whatever levels the manifest sets.                                                                                                                                                                                                                                                                                                           | `0001-F2` C-2; C-5                                                                                               | Active |
| B-012 | Given `--root` names a path that is not a directory, this Feature writes a message naming that path as it was given on stderr, writes nothing on stdout, and exits with code `2`.                                                                                                                                                                                                                                             | `0001-F2` B-005; C-5                                                                                             | Active |
| B-013 | Given a root with no manifest at the manifest path, this Feature writes a message naming the manifest path on stderr, writes nothing on stdout, and exits with code `2`.                                                                                                                                                                                                                                                      | `0001-F2` B-006; `0001-F3` B-029, OQ-2; OQ-1                                                                     | Active |
| B-014 | Given a root whose manifest the check rejects - one that does not parse (`0001-F2` B-007), one `0001-F5` rejects (`0001-F5` B-022), one whose `schemaVersion` the check rejects (`0001-F7` B-003, B-039), or one recording an upstream schema source while explicitly selecting the embedded source (`0001-F7` B-036) - this Feature names on stderr what the check names, writes nothing on stdout, and exits with code `3`. | owner, 2026-10-08 (every command answers a rejected manifest with `3`); `0001-F3` B-030; `0001-F2` decision 0003 | Active |
| B-015 | Given any run, stdout carries only the listing, and every message about the tool itself goes to stderr.                                                                                                                                                                                                                                                                                                                       | `0001-F2` B-009, C-3; C-6                                                                                        | Active |
| B-016 | Given any run, no file is created, modified or deleted, under the root or anywhere else.                                                                                                                                                                                                                                                                                                                                      | brief § 9; C-1                                                                                                   | Active |
| B-017 | Given any output on stdout or stderr, every path the tool derives is relative to the root with `/` separators, and the only path that may be absolute is one typed on the command line, echoed as given.                                                                                                                                                                                                                      | brief § 9; `0001-F2` B-013                                                                                       | Active |
| B-018 | Given the same root, manifest and options, two runs print the same stdout and exit with the same code.                                                                                                                                                                                                                                                                                                                        | brief § 9; C-3                                                                                                   | Active |
| B-019 | Given the text form, stdout carries one line for each entry, holding the rule id, the family, the default severity and the effective level in that order, separated by single tab characters, with the default severity and the effective level in lower case.                                                                                                                                                                | derived by spec-author (a form a test can fail, decision 0002); not stated by the owner; OQ-2                    | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-11 -->

| ID  | Constraint                                                                                                                                                     | Rules Out                                                                                                                                                                                       |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | The command is read-only: it writes nothing, anywhere.                                                                                                         | An `--output <path>`; writing a rules index file under the root or under `docs/`; setting a level, which is `0001-F11`'s.                                                                       |
| C-2 | The listing is read from the pinned version's vocabulary and its rule pages, and from the manifest the check's loader produced (decision 0001; `0001-F3` C-6). | A second catalogue of rule ids, families or severities in the command or in a file; a hand-kept or generated index (`0001-F3` § 5 row 10); a rule listed that the pinned version does not hold. |
| C-3 | Deterministic and offline: the output is a function of the manifest, the pinned version and the tool version.                                                  | A network call; a clock, a locale or a machine name in the output; an order that follows a culture.                                                                                             |
| C-4 | A command parses, calls the engine and folds the result into an exit code; nothing else lives in the command (`0001-F2` C-4).                                  | Reading a rule page or grading a level inside `Features/Rules/`.                                                                                                                                |
| C-5 | Exit codes are `0001-F2` C-2's constants, with the meanings the check gives them.                                                                              | A sixth exit code; exit `1` because a rule is set to `off` or `warning`.                                                                                                                        |
| C-6 | stdout carries the product only, as plain text (`0001-F2` C-3).                                                                                                | A banner, a count line, colour markup or a rendered table border on stdout.                                                                                                                     |
| C-7 | The command evaluates no rule and reads no specification.                                                                                                      | Violation counts per rule; a listing that changes with the tree's content.                                                                                                                      |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-11 -->

| #   | Item                                                                          | Exclusion Reason                                                                                               |
| --- | ----------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| 1   | Setting or removing a rule's level                                            | `0001-F11`. C-1.                                                                                               |
| 2   | A rule's cause, examples and fix                                              | `--explain` prints the rule page (`0001-F3` B-019).                                                            |
| 3   | The rest of the manifest                                                      | `0001-F9`.                                                                                                     |
| 4   | How many violations each rule reports on this tree                            | The check and its report (`0001-F2`, `0001-F3`); C-7.                                                          |
| 5   | The rules of a version other than the pinned one, or of every shipped version | Not asked for; the shipped versions are the list command's (`0001-F7` B-041). OQ-1 holds the no-manifest case. |
| 6   | Filtering by family, by level or by default severity                          | Not asked for; a caller filters the `--json` document.                                                         |
| 7   | A link or address for a rule's page                                           | No page has a published address (`0001-F3` § 5 row 9).                                                         |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-26 -->

| Concern                                                                                             | Classification | Owner                                                                      |
| --------------------------------------------------------------------------------------------------- | -------------- | -------------------------------------------------------------------------- |
| The exact pinned vocabulary and its ordinal order (B-001, B-008)                                    | Business       | `RuleEntriesReader`, using `SpecManifestParser.PinnedVersion`              |
| The family and default severity on the authoritative page (B-002, B-003, B-007)                     | Business       | `RuleEntriesReader`, using the existing Markdown document parser           |
| An explicit manifest level, fallback to the page default and retaining off entries (B-004 to B-006) | Business       | `RuleEntriesReader` over the loader-produced `SpecStructure.Rules`         |
| Root/manifest observation and validated pin selection                                               | Technical      | Existing `SpecSchemasLoader`, `SpecManifestStore` and `SpecManifestParser` |
| Rendering the entries, command failures and exit codes                                              | Both           | The later command slice; outside this engine change                        |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-26 -->

### Engine catalogue

[`RuleEntriesReader`](../../../../specht/Rules/RuleEntriesReader.cs) is a concrete instance registered by the existing [`AddSpechtEngine`](../../../../specht/Composition/SpechtEngineServiceCollectionExtensions.cs) composition. Its root entry point loads only the manifest through [`SpecSchemasLoader.LoadManifest`](../../../../specht/Model/SpecSchemasLoader.cs), then calls its in-memory overload with that `SpecStructure`. It does not load frontmatter schemas, discover specifications or evaluate rules.

The in-memory overload resolves the exact pin through [`SpecManifestParser.PinnedVersion`](../../../../specht/Manifest/SpecManifestParser.cs), orders that version's `RuleIds` with `StringComparer.Ordinal`, and reads the corresponding `SchemaVersion.RulePages` text. For each page it reuses [`SpecDocument.Parse`](../../../../specht/Model/SpecDocument.cs), taking the `Family` and `Default severity` rows of the `Metadata` section's first pipe table. It does not parse Markdown by splitting pipes. Page metadata is trusted shipped content, whose shape and page-per-vocabulary invariant are already checked by `SchemaVersionRulePagesUnitTests`.

[`RuleEntry`](../../../../specht/Rules/RuleEntry.cs) carries the vocabulary id, family, default severity and effective level. Both level values use the existing `SpecRuleLevel`, including `Off`; the family retains the page's spelling. An explicit value in `SpecStructure.Rules` wins; an omitted entry uses the page's default severity. No entry is filtered for its effective level. The returned entries are produced on demand, with no maintained metadata index or cache. Manifest inspection can consume this same result rather than compute effective levels again.

ADR-0014 retains direct calls and the existing stores, parser and composition. This change introduces no interface, dependency package, mediator, new static parser, vocabulary, rule-page edit or command registration. Command stdout, text/JSON rendering, error mapping and exit codes remain separate work.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-26 -->

The engine catalogue scenarios run through `RuleEntriesSteps` over a synthetic `SpecTree` and the existing manifest loader. B-007 compares the entry to the output of the built `--explain` command. The command and JSON scenarios retain unbound steps until their owning items supply them; the existing Reqnroll pending-step policy reports them as skipped.

| Claim | Mechanism                                                                          | Unit proof                                                                                                                                        |
| ----- | ---------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| B-001 | Exact-pin vocabulary projection; extra pages are excluded                          | `RuleEntriesReaderUnitTests.AnExactPin_WhenRead_ShouldReturnOnlyThatVersionsVocabulary`; stdout remains the command item's proof                  |
| B-002 | Family read from the pinned page's Metadata table                                  | `RuleEntriesReaderUnitTests.APageMetadataValue_WhenRead_ShouldSupplyTheFamilyAndDefaultSeverity`                                                  |
| B-003 | Page default mapped to Error, Warning or Off                                       | `RuleEntriesReaderUnitTests.APageMetadataValue_WhenRead_ShouldSupplyTheFamilyAndDefaultSeverity`                                                  |
| B-004 | Explicit manifest level wins over the page default                                 | `RuleEntriesReaderUnitTests.AnExplicitLevel_WhenRead_ShouldOverrideThePageDefaultWithoutRemovingTheEntry`                                         |
| B-005 | An omitted level uses the page default, independently of shipped manifest defaults | `RuleEntriesReaderUnitTests.NoExplicitLevel_WhenRead_ShouldUseTheRulesOwnSeverity`                                                                |
| B-006 | Off is an effective value, not an entry filter                                     | `RuleEntriesReaderUnitTests.AnExplicitLevel_WhenRead_ShouldOverrideThePageDefaultWithoutRemovingTheEntry`                                         |
| B-007 | The selected version's page supplies both catalogue metadata and explanation       | `RuleEntriesReaderUnitTests.APageMetadataValue_WhenRead_ShouldSupplyTheFamilyAndDefaultSeverity`; the acceptance comparison exercises `--explain` |
| B-008 | Explicit ordinal ordering of an unordered vocabulary                               | `RuleEntriesReaderUnitTests.AnUnorderedVocabulary_WhenRead_ShouldReturnEntriesInOrdinalIdOrder`                                                   |

The pure `Read(SpecStructure)` overload admits synthetic `SchemaVersionFixture` vocabularies and pages; the root overload exercises the existing loader without specification discovery. No extra test seam is required. B-009 to B-019 remain outside the engine catalogue scope.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-26 -->

| Claim ID | Scenario                                                       | Test                                                                                                                                                                                    | Status  |
| -------- | -------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| B-001    | Every rule of the pinned version is listed once                | `RuleEntriesSteps.ThenListingHasPinnedVocabulary`; `RuleEntriesReaderUnitTests.AnExactPin_WhenRead_ShouldReturnOnlyThatVersionsVocabulary`; stdout proof belongs to the command item    | Partial |
| B-002    | A rule is listed with its family                               | `RuleEntriesSteps.ThenSectionRuleFamily`; `RuleEntriesReaderUnitTests.APageMetadataValue_WhenRead_ShouldSupplyTheFamilyAndDefaultSeverity`                                              | Covered |
| B-003    | A rule is listed with its default severity                     | `RuleEntriesSteps.ThenSectionRuleDefault`; `RuleEntriesSteps.ThenRetiredClaimDefault`; `RuleEntriesReaderUnitTests.APageMetadataValue_WhenRead_ShouldSupplyTheFamilyAndDefaultSeverity` | Covered |
| B-004    | A rule the manifest sets is listed at that level               | `RuleEntriesSteps.ThenSectionRuleEffectiveLevel`; `RuleEntriesReaderUnitTests.AnExplicitLevel_WhenRead_ShouldOverrideThePageDefaultWithoutRemovingTheEntry`                             | Covered |
| B-005    | A rule the manifest does not set is listed at its own severity | `RuleEntriesSteps.ThenSectionRuleEffectiveLevel`; `RuleEntriesReaderUnitTests.NoExplicitLevel_WhenRead_ShouldUseTheRulesOwnSeverity`                                                    | Covered |
| B-006    | A rule switched off is still listed                            | `RuleEntriesSteps.ThenListingRetainsSectionRule`; `RuleEntriesReaderUnitTests.AnExplicitLevel_WhenRead_ShouldOverrideThePageDefaultWithoutRemovingTheEntry`                             | Covered |
| B-007    | The listing agrees with the rule's page                        | `RuleEntriesSteps.ThenListingAgreesWithExplanation`; `RuleEntriesReaderUnitTests.APageMetadataValue_WhenRead_ShouldSupplyTheFamilyAndDefaultSeverity`                                   | Covered |
| B-008    | Rules are listed in order of rule id                           | `RuleEntriesSteps.ThenListingIsOrdinallyOrdered`; `RuleEntriesReaderUnitTests.AnUnorderedVocabulary_WhenRead_ShouldReturnEntriesInOrdinalIdOrder`                                       | Covered |
| B-009    | The JSON form is one document                                  | Missing                                                                                                                                                                                 | Missing |
| B-010    | The JSON form carries what the text form prints                | Missing                                                                                                                                                                                 | Missing |
| B-011    | Listing succeeds whatever the levels are                       | Missing                                                                                                                                                                                 | Missing |
| B-012    | A missing root is a missing-input failure                      | Missing                                                                                                                                                                                 | Missing |
| B-013    | A root without a manifest is a missing-input failure           | Missing                                                                                                                                                                                 | Missing |
| B-014    | A rejected manifest is an invalid-manifest failure             | Missing                                                                                                                                                                                 | Missing |
| B-015    | Messages about the tool go to standard error                   | Missing                                                                                                                                                                                 | Missing |
| B-016    | A run writes nothing                                           | Missing                                                                                                                                                                                 | Missing |
| B-017    | No derived path in the output is absolute                      | Missing                                                                                                                                                                                 | Missing |
| B-018    | The same input gives the same output                           | Missing                                                                                                                                                                                 | Missing |
| B-019    | The text form is one tab-separated line per rule               | Missing                                                                                                                                                                                 | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-26 -->

- 2026-10-24, groomed from `blocked` to `ready-for-architecture` ([decision 0002](decisions/0002-the-text-form-is-one-tab-separated-line-per-rule-and-the-blocked-status-is-lifted.md)). The block named `0001-F2`, `0001-F3`, `0001-F5` and `0001-F7`: the first three are built, and `0001-F7`'s list and pin commands are built (items `0121` to `0125`); only `upgrade` (item `0047`) is outstanding and no claim here depends on it. `depends_on` stays as references.
- 2026-10-24, B-019 added: the text form, derived by spec-author on OQ-2's default, so a test can fail. A-1 notes that `rules` is also the branch `0001-F11` hangs under. No other claim changed.
- 2026-10-26, corrected A-2 to include `off`, already given by schema `0.2.0`'s authoritative `SPEC034` and `SPEC035` pages. Added a B-003 scenario for a page whose default is off; B-003 and B-007 continue to require the page's value. No claim or rule-page content changed.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-11, 2026-10-24 -->

| ID   | Question                                                                                                                                                                                                                                                        | Blocks                | Resolution                                                                                              |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------- | ------------------------------------------------------------------------------------------------------- |
| OQ-1 | With no manifest, does `specht rules` fail as the check and `--explain` do (`0001-F3` OQ-2, B-029), or list the newest shipped version's rules with each at its own severity? The effective level needs a manifest; the id, family and default severity do not. | B-013; § 5 row 5      | Open. Proceeds on the default: it fails with `2`, as `--explain` does. Owner not asked.                 |
| OQ-2 | What does an entry look like without `--json`: one line per rule with its four values, an aligned table, or something else?                                                                                                                                     | B-019 (the form only) | Open. Proceeds on one tab-separated line per rule (B-019). Owner not asked.                             |
| OQ-3 | Does an entry also carry the rule's title, which its page holds (`0001-F3` B-032)? The owner named the family, the default severity and the effective level.                                                                                                    | B-001, B-010          | Open. No claim is written for a title. Owner not asked.                                                 |
| OQ-4 | Does the JSON document of this command get a published schema, as the report has (`0001-F3` C-1), with its member names fixed and bound to change by addition only? Epic `0132` would read it.                                                                  | B-010                 | Open. B-010 fixes what the document carries and no member name. Owner not asked.                        |
| OQ-5 | Item `0169` renumbers the rule ids to four digits whose thousands digit names the group. Does the listing then order and group by that digit, and is "family" (`0001-F3` A-5) still the word, or the group ADR-0010 names?                                      | B-002, B-008          | Open. B-008's ordinal order holds under either numbering; B-002 follows the rule page. Owner not asked. |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-26 -->

| Section               | Status | Reviewer      | Note                                                                                                                                                                                                 |
| --------------------- | ------ | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5                   | 🟡     | spec-reviewer | Awaiting first review of the broader command agreement.                                                                                                                                              |
| 6-9, engine catalogue | 🟢     | spec-reviewer | B-001 to B-008 engine scope reviewed with no findings; focused tests on both frameworks and repository gates pass. B-001 remains Partial for stdout; command and JSON work is outside this approval. |

## Tasks

- `0280`: the engine reads the rule entries (B-001 to B-008).
- `0281`: the `specht rules` command, text form, exit codes and failure paths (B-011 to B-019).
- `0282`: `--json` (B-009, B-010).

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
