---
title: "Specification: Persistent specification index"
description: "Keep a rebuildable, keyed database view of repository specifications so MCP can follow their relationships without reparsing unchanged Markdown."
type: feature
id: "F1"
epic: "0132"
spec_status: draft
status: blocked
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
depends_on: []
blocks: []
spikes: []
created: "2026-10-19"
updated: "2026-10-30"
github_issue: null
synced_at: null
---

# Specification: Persistent specification index

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-19 -->

An MCP client repeatedly needs facts and relationships already present in a repository's Markdown specification tree. Today each new query must discover and parse those files again. This Feature gives the client a durable, keyed view of the tree that can answer repeated lookups without reparsing unchanged Markdown, while the repository's files remain authoritative.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-30 -->

| #   | Persona                                     | Need                                                                                                          | Pain Point Today                                             |
| --- | ------------------------------------------- | ------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------ |
| 1   | An agent using MCP in a consumer repository | Find a specification fact by its stable key and follow its relationships to other facts and source locations. | It must search and parse Markdown to find each related fact. |
| 2   | A maintainer editing specifications         | Have an indexed answer reflect the repository's authoritative Markdown.                                       | A persistent copy could silently describe an earlier tree.   |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                          |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The database is a derived index. Markdown in the repository remains authoritative (decision 0001). The database engine and physical schema are design choices, not part of this agreement.                                                          |
| A-2 | The tree-facts answer in epic `0132` candidate A and the citation resolver in epic `0228` candidate C own their public document shapes. This Feature supplies indexed facts to those answers; it does not define a second facts or citation format. |

**Requirements revision requested, 2026-10-30.** Epic `0132` records the owner's
request for an opt-in database authority over all managed documents, with
repository and GitHub projections, solo local use and optional shared access.
Every entering record receives a unique key retained throughout its lifecycle.
The owner also requires complete specification and feature documents for
retrieval and rendering, alongside keyed scenario-coverage relationships.
Before implementation, reconcile this Feature's file-index scope, decision
0001, B-001 to B-006 and C-1, C-2, C-3, C-8 with the optional authority mode.
The revised agreement remains open; existing file-based use must stay supported.

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-19 -->

| ID    | Claim                                                                                                                                                                                                           | Source                                       | Status |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------- | ------ |
| B-001 | Given a repository's Markdown specification tree, this Feature can build a durable index of each discovered specification file's source text, identity, facts, relationships and root-relative source location. | Owner, 2026-10-19; epic `0132` tree facts    | Active |
| B-002 | Given a keyed fact in the index, this Feature can follow its recorded relationships to other keyed facts and return their root-relative source locations without parsing Markdown for that lookup.              | Owner, 2026-10-19; epic `0228` R4            | Active |
| B-003 | Given an unchanged, validated index, this Feature serves repeated keyed lookups without reparsing specification Markdown.                                                                                       | Owner, 2026-10-19; decision 0001             | Active |
| B-004 | Given an absent, stale or incompatible index, this Feature brings it current from the authoritative tree before serving an indexed answer.                                                                      | Decision 0001; epic `0132` parity constraint | Active |
| B-005 | Given a rebuilt index after a source file is removed or a key changes, this Feature no longer resolves facts or relationships that belonged only to the prior tree.                                             | Owner's source-of-truth decision, 2026-10-19 | Active |
| B-006 | Given the same repository tree and schema pin, this Feature returns the same facts and source locations through the indexed MCP path as the command-line answer.                                                | Epic `0132` same-tree parity constraint      | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-19 -->

| ID  | Constraint                                                                                                             | Rules Out                                                                                         |
| --- | ---------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| C-1 | Markdown and the repository's pinned schema version remain authoritative; the index can be discarded and rebuilt.      | Editing a specification through the database or making database content a second source of truth. |
| C-2 | Index state lives outside the consumer's tree and produces no tracked file.                                            | A cache under `.spec/`, `.specht/` or another consumer path.                                      |
| C-3 | Index construction and lookup are local and offline.                                                                   | A hosted database or network call from an MCP answer.                                             |
| C-4 | Every returned path is root-relative and uses `/` separators.                                                          | Leaking an absolute cache path or checkout path into a fact or answer.                            |
| C-5 | Existing stable specification and citation identifiers are query keys; database-generated row identities are internal. | Asking a caller to know a table or document identifier to resolve a claim.                        |
| C-6 | Indexing does not change the checker's verdict or the CLI and MCP answer contracts.                                    | A database-specific answer, rule interpretation or schema vocabulary.                             |
| C-7 | An index optimization does not waive ADR-0014's benchmark gate for the wider engine and host migration.                | Adopting ADR-0010's target architecture as a side effect of this Feature.                         |
| C-8 | Each answer reflects the current repository tree, including direct file edits and branch switches.                     | Serving an earlier indexed snapshot until a caller requests refresh.                              |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-19 -->

| #   | Item                                                                                             | Exclusion Reason                                                              |
| --- | ------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------- |
| 1   | Editing, creating or deleting repository documents through the index.                            | C-1; existing commands own the permitted writes.                              |
| 2   | Defining the public tree-facts document or citation-resolution command.                          | Epic `0132` candidate A and epic `0228` candidate C own those answers.        |
| 3   | Document-number reservation state.                                                               | Epic `0132` candidate F owns allocation and its separate authority.           |
| 4   | Cross-machine index sharing or a network service.                                                | The MCP host is local and offline.                                            |
| 5   | Retaining specification history after files change.                                              | The index represents one current tree, not a revision archive.                |
| 6   | Choosing a document or relational database before the query and freshness contracts are settled. | Either physical model may satisfy the claims; § 7 belongs to the implementer. |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-19 -->

Pending design. The current-tree rule is settled in decision 0001; its change-detection mechanism remains to be designed and measured.

## 7. Technical Design

<!-- last written by: implementer, 2026-10-19 -->

Pending design. A benchmark must measure the current read-and-parse path against index validation, construction and repeated lookups under equivalent repository workloads before a database is selected.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-19 -->

Pending tests. The scenarios below state the expected behavior; no test is claimed yet.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-19 -->

| Claim ID | Scenario                                | Test    | Status  |
| -------- | --------------------------------------- | ------- | ------- |
| B-001    | Build a durable index from Markdown     | Missing | Missing |
| B-002    | Follow keys to related source locations | Missing | Missing |
| B-003    | Reuse an unchanged index                | Missing | Missing |
| B-004    | Bring a stale index current             | Missing | Missing |
| B-005    | Remove stale keys after rebuild         | Missing | Missing |
| B-006    | Match the command-line answer           | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-19 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-19 -->

| ID   | Question                                                                                                                                                               | Blocks                              | Resolution                                                                                 |
| ---- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- | ------------------------------------------------------------------------------------------ |
| OQ-1 | After an out-of-band Markdown edit or branch switch, must each MCP answer match the current tree, or may it name and serve an indexed snapshot until explicit refresh? | B-003, B-004, B-006; implementation | Resolved 2026-10-19 by the owner: every answer reflects the current tree (decision 0001).  |
| OQ-2 | Which existing identifiers and relationships must the first indexed query cover beyond Feature and claim identity?                                                     | B-001, B-002; implementation        | Open; align with epic `0132` candidate A and epic `0228` candidate C before cutting items. |
| OQ-3 | What evidence shows the indexed path reduces repeated-query cost enough to justify persistent state?                                                                   | B-003; implementation               | Open; benchmark the current path and representative warm queries.                          |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-19 -->

| Section | Status | Reviewer      | Note                              |
| ------- | ------ | ------------- | --------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Draft; OQ-2 and OQ-3 remain open. |

## Tasks

None. Cut items from claims after the agreement is approved.

## Scoring

| Field | Value | Basis                                    |
| ----- | ----- | ---------------------------------------- |
| value | 0     | Not scored while the agreement is draft. |
| risk  | 0     | Not scored while the agreement is draft. |
