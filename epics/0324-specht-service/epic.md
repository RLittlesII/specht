---
title: "Epic 0324: a specht service"
description: "An opt-in authority behind MCP: unique document sequences per project and document type allocated without git coordination, and a database-driven mode in which every managed document keeps one lifetime key; moved from epic 0132 on 2026-10-30"
id: "0324"
type: epic
status: needs-decomposition
priority: med
milestone: null
children: []
created: "2026-10-30"
updated: "2026-10-30"
github_issue: null
---

# Epic 0324: a `specht` service

## Summary

This epic holds two requests the owner made of epic `0132` and, on
2026-10-30, decided to move out of it: document sequence allocation
(`0132` candidate F) and the database-authority service mode with its
lifetime keys. Both ask for an authority with durable state of its own,
behind MCP, beside the Markdown tree that is authoritative today.

The owner added a request on 2026-10-17 for unique document sequences per
project and document type, allocated through MCP without coordinating through
git. The requirement is recorded under "Document sequence allocation";
project identity and reservation storage remain open.

The owner held a service requirements session on 2026-10-30, recorded under
"Service mode": an opt-in mode in which a database behind MCP is the source of
truth for every document `specht` manages, and every record keeps one key for
its whole lifecycle.

This file is the record of the move. Every requirement in it is the owner's,
moved unchanged from epic `0132` at commit `9f37d6f3`; nothing here is
designed, no Feature is confirmed and no Feature specification exists.

**Why the two sit together, and the AGENTS.md "&" heuristic.** The title
joins no two nouns, but the epic holds two capability dimensions: allocating
sequence values, and a database that owns documents and their keys. They are
kept together because the owner moved them together ("Candidate F moves with
the service mode", owner, 2026-10-30), and because each needs an authority and
durable state outside the tree that the permitted-write list does not yet
cover. The bundling is checked, not settled: a confirmed cut may still put
them in separate Features.

## Business Value

**The problem.** Parallel authors claim the same next id, or conflict when
merging a shared counter (AGENTS.md § Stable IDs: "An id is not reserved
until it merges"). Today's only remedy is to check `main` and every open pull
request by hand.

**The solved state.** A caller obtains a unique sequence value for a project
and a supported document type without git coordination; where a consumer opts
into the service mode, every record keeps the key it entered with.

Who it is for, as the moved text names them. The owner has ranked no persona.

| Persona                                          | Goal                                                                   |
| ------------------------------------------------ | ---------------------------------------------------------------------- |
| Parallel authors in one project's worktrees      | Distinct sequence values with no git operation or tracked counter edit |
| An individual developer working entirely locally | Originate and groom their own requirements in the service mode         |
| A team sharing a database, local or remote       | Reach a team-accessible database, local or remote                      |

What it costs them today:

| Pain                    | Cost                                                                              |
| ----------------------- | --------------------------------------------------------------------------------- |
| Sequence files conflict | Parallel authors claim the same next id or conflict when merging a shared counter |

## Success criteria

Requested marks a capability whose tier has not been decided. The service
mode has no criterion yet; its confirmed identity rule is recorded under
"Service mode".

| Criterion                                                                                                        | Tier      | Notes                                                                                                                                                                           |
| ---------------------------------------------------------------------------------------------------------------- | --------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A caller can obtain a unique sequence value for a project and a supported document type without git coordination | Requested | Added by the owner, 2026-10-17; tier and authority boundaries await agreement under "Document sequence allocation". Moved from epic `0132` with candidate F, 2026-10-30 (owner) |

## Document sequence allocation

Moved from epic `0132` on 2026-10-30, by the owner's decision; candidate F
moved with the service mode.

**Feature request, 2026-10-17.** A caller supplies a project and a document
type supported by `specht`, and an MCP operation allocates a unique sequence
value for that project and type on one machine. Concurrent callers across
that project's worktrees and processes on the machine must receive distinct
values within the same sequence scope (owner, 2026-10-17). Allocation must not
depend on a git repository, branch, commit, merge, pull request or tracked
counter file.

The operation reserves a value; merely reading the highest id and returning
the next one cannot meet the concurrency requirement. It does not create,
edit or renumber a document. The set of supported document types and their
sequence scopes must be agreed before a Feature specification is cut; the
request is not limited to this repository's work-item sequence.

This is a new capability, separate from the optional existing writes and
work queue. It needs an allocation authority and durable reservation state.
The authority is machine-local and offline; its storage is not chosen here.
The permitted-write list does not yet cover reservation state; its owning
records must explicitly permit those state changes before implementation.
CLI parity still applies: the same allocation capability needs a command-line
entry point, whose name is open.

Examples to carry into that Feature's scenarios once the scope is agreed:

- Two concurrent requests from different worktrees or processes for the same
  project and document type on one machine receive different values without a
  git operation or a tracked counter edit.
- Restarting the allocation host does not make a reserved value available
  for allocation again.
- Requesting a sequence value leaves the project's documents unchanged.

The open questions below block implementation, not recording this request.
The current offline allocation scope is one machine; the online proposal below
would extend coordination across machines if adopted.

**Conditional online proposal, 2026-10-20.** If an online mode is adopted, the
owner wants one project sequence across machines. That requires a shared
allocation authority and an explicit change to the current offline boundary.
Independent local counters cannot provide that guarantee. Spike `0252` will
examine mode switching and an unavailable online authority before the owner
decides whether to adopt the mode.

## Service mode

Moved from epic `0132` § "Related records", "Service requirements session,
2026-10-30", on 2026-10-30, by the owner's decision. It covers the
database-authority service mode, lifetime keys, revisions and snapshots,
GitHub projection and a team or remote store.

The owner requests an opt-in, database-driven mode covering every document
managed by specht: Markdown, YAML, specifications and supporting records. In
that mode the database behind MCP is the source of truth, projecting repository
files for check-in and GitHub issues. Existing file-based use remains supported;
database authority is not imposed on every consumer. This request does not
require converting every document into structured YAML.

The service must accommodate an individual developer working entirely locally,
including originating and grooming their own requirements. A team-accessible
database may be local or remote. Business requirements intake is a possible
additional entry point into the same system, not a mandatory actor or starting
stage. The broader direction is a potential service for specification-driven
development, with GitHub serving as one integration. Exact service capabilities
and their delivery scope remain to be agreed.

**Identity rule, confirmed by the owner:** every record receives a unique key
when it enters the system and retains that key throughout its entire lifecycle.
This applies regardless of entry point or whether one developer or a team uses
the system. Grooming and projection preserve that identity. Key syntax,
namespace and allocation remain to be specified; this rule does not authorize
renumbering existing stable identifiers.

The owner raised Snowflake as a candidate for these lifetime keys. Existing
spike `0256` already compares Snowflake with alternative allocation strategies;
carry the solo-local and optional shared-service requirements into that
evaluation. A candidate must address concurrent generators, restarts and clock
rollback within its claimed uniqueness scope. The algorithm is not selected by
this discussion. The original implementation comes from Twitter; its source is
the `snowflake-2010` tag in `twitter-archive/snowflake`.

The requirements session focuses first on the specification's shape, its
traceability matrix and keys throughout the model. Database selection and
implementation follow the agreement.

**Document retrieval and traceability, clarified by the owner:** retain complete
documents as well as their keyed relationships. The scenario-coverage relation
connects an acceptance criterion key, its specification key and the covering
scenario key. Retrieving a specification returns its complete document for
rendering; retrieving the feature file containing a scenario returns that whole
file. Keyed lookup must not restrict reading to extracted rows or snippets.
The representation must retain the relationship from a scenario to its
containing feature document. This establishes structural scenario coverage;
test bindings and execution evidence remain separate coverage concerns.

Both relational links and complete-document retrieval are required capabilities.
Whether one database engine or multiple stores provide them remains a design
choice. Rendering behavior, document revision consistency and synchronization
between content and relationships still need an agreement. No database product
is chosen by this requirement.

**Revision model proposed for discussion, not yet agreed:** separate each
record's permanent key from immutable content revisions. A saved project
snapshot selects exact document revisions and the traceability relationships
applicable to them. Keyed elements resolve within their containing document's
selected revision, so a scenario lookup retrieves the complete feature file
from that same snapshot. Relationships must resolve to elements present in the
selected document revisions. A historical matrix and links opened from it
retain the snapshot context rather than resolving independently to latest.

An approved baseline could name such a snapshot; a current-work view could
resolve a selected workspace's head to one snapshot before generating its
matrix. Database transactions alone do not preserve a reader's context across
later requests; responses and follow-up links would carry the saved snapshot
identity. Repository projections would record their snapshot identity, with
Git commit associations recorded when available. GitHub projection freshness
would be recorded separately because external publication is not atomic with
database updates.

Under this proposal, editing retains identity and creates a new revision;
previous snapshots remain readable. A retained coverage relationship does not
transfer approval or passing evidence to changed inputs automatically.
Revalidation policy, snapshot scope, concurrent-edit handling, revision
identifier generation and retention still require agreement. Retaining this
history would also amend `0132-F1` section 5 row 5, which currently excludes a
revision archive from the derived index.

Database authority revises `0132-F1` decision 0001 for consumers opting into
that mode. Before implementation, record the mode boundary in that decision
and update the owning claims, scenarios and constraints, including current-tree
freshness. The permitted-write boundary and `0055-F9`'s local-item authority
also need explicit reconciliation with database-owned projections. File-based
offline checking remains supported; a remote service is not a prerequisite for
the solo local workflow.

Qualified-key resolution remains epic `0228` candidate C and item `0307`;
`0132-F1` and spike `0252` own the existing indexing and storage investigation.
The session must reconcile those agreements rather than introduce a second
resolver or infer a database product from the requested authority model.

## Features

**A candidate cut, awaiting the owner's confirmation.** Nothing below is
agreed. No Feature here has an id, a specification or a place in `children`:
a candidate takes its Feature id when its specification is written, and joins
`children` then, so that every child resolves to a file.

| Candidate | Name                         | Invariant it would carry                                                                                                      | Tier      |
| --------- | ---------------------------- | ----------------------------------------------------------------------------------------------------------------------------- | --------- |
| F         | Document sequence allocation | Concurrent allocations within an agreed project and document-type scope never return the same value, without git coordination | Requested |

The candidate keeps the letter it had in epic `0132`, so that every citation
of `0132` candidate F still names it.

- **Candidate F was cut from epic `0132`'s candidates D and E.** It reserves
  identifiers, rather than exposing an existing tree write or reading delivery
  state; its machine-local reservations need their own state contract.
- **The service mode has no candidate.** The owner moved it here; no
  candidate has been proposed for it.
- **The AGENTS.md "&" heuristic** found no conjunction in the candidate name.

## What this epic waits on

None of these stops a Feature specification being written. Each feeds an
open question below.

| Waits on                                                                | Recorded in                                               | Blocks                                                                                        |
| ----------------------------------------------------------------------- | --------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| Spike `0252`: MCP access to linked records, and local or online storage | `.issue/0252-evaluate-mcp-record-storage.yml` (`ready`)   | The online-mode questions, and whether serving complete database documents helps more         |
| Spike `0256`: Snowflake and other collision-free ids for records        | `.issue/0256-evaluate-snowflake-record-ids.yml` (`ready`) | The choice of lifetime-key allocation; the solo-local and shared-service requirements join it |

## Constraints

Every constraint is hard. None is this epic's invention: each is an existing
rule of the repository or the owner's word, and each is listed so that a
Feature specification cut from this epic carries it forward. The requests
here do not silently waive these constraints; their authorities must fit
them, or their owning records must be amended by an explicit decision before
implementation.

| Constraint                                                                                                  | Source                                                       | What it rules out                                                                                                 |
| ----------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------- |
| The check never touches the network; only `init` and `upgrade` may fetch                                    | AGENTS.md § Invariants; brief § 9                            | An online authority or a remote store before an explicit decision amends the offline boundary                     |
| Nothing is written outside the permitted list of writes                                                     | AGENTS.md § Invariants; brief § 9                            | Reservation state or database-owned projections written before their owning records permit them                   |
| MCP adds nothing the command line cannot say                                                                | Owner, 2026-10-09; `specht-conventions`, questions reference | An allocation reachable only through MCP; the same capability needs a command-line entry point                    |
| Allocation does not depend on git                                                                           | Owner, 2026-10-17                                            | A git repository, branch, commit, merge, pull request or tracked counter file in the allocation path              |
| Ids are never reused and never renumbered                                                                   | AGENTS.md § Stable IDs; the identity rule, owner, 2026-10-30 | A reserved value made available again; a key changed by grooming or projection; renumbering an existing stable id |
| Markdown stays authoritative unless a consumer opts into the service mode; file-based use remains supported | `0132-F1` decision 0001; owner, 2026-10-30                   | Database authority imposed on every consumer; a remote service required for the solo local workflow               |
| No output carries an absolute path                                                                          | AGENTS.md § Invariants; brief § 9                            | A path in an allocation or service answer that is not relative to the root the tool was given                     |

## Open questions

Each question travels to the Feature specification it binds, where it takes
an `OQ-n` id and is resolved in place. The questions moved from epic `0132`
on 2026-10-30 with their answers unchanged.

### Questions for document sequence allocation

Each question travels with candidate F.

| Question                                                                                                                                                                                                                                                                                                  | Who answers                                                               |
| --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| Must allocations stay unique across machines and clones of one project, or only across worktrees and processes on one machine? Resolved 2026-10-17 for the current offline scope: one machine. Conditional online scope clarified 2026-10-20: if adopted, one project sequence is shared across machines. | Resolved by the owner for each mode; online adoption remains open         |
| Should online mode be adopted, and which offline, permitted-write and CLI parity rules would need amendment?                                                                                                                                                                                              | The owner after spike `0252`                                              |
| If online allocation is adopted, how does it handle an unreachable authority or a project changing modes without reissuing a value?                                                                                                                                                                       | The owner, then the design                                                |
| What stable identity denotes a project across paths and worktrees without depending on git, and how are separate clones on the same machine associated with it?                                                                                                                                           | The owner, then the design                                                |
| Which document types are supported, and which share a sequence or require a parent scope? This repository shares epic and work-item numbers; Features and their claims use other scopes.                                                                                                                  | The spec-author, then the owner                                           |
| Where does the machine-local, offline authority persist reservations, and what narrow permission for reservation-state writes is added to the owning records?                                                                                                                                             | The owner, then the design                                                |
| How are existing document numbers adopted without reissuing them, and what are the rules for abandoned reservations, gaps and retries after an uncertain response?                                                                                                                                        | The owner, then the design                                                |
| What MCP operation and equivalent CLI command expose allocation, and what priority does this capability have relative to the existing Musts?                                                                                                                                                              | The owner for priority; the Feature specification for the public contract |

### Questions for the service mode

| Question                                                                                                                | Who answers                                                                                       |
| ----------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| If full documents move into a database, which copy owns edits, Git history and conflicts between worktrees or machines? | The owner, then the design. 2026-10-30: moved here from epic `0132` with the service mode (owner) |

The following questions remain for the service requirements session; they
are not agreed behavior:

- How does the database represent document content and keyed elements while
  preserving the readable specification and its versioned schema contract?
- How are keys scoped and allocated, and which elements within a document are
  independently keyed? The owner identifies the acceptance criterion,
  specification and scenario keys for scenario coverage; how do test and
  evidence links extend that view, and is the matrix generated from those links?
- How do edits to repository files or GitHub issues enter the authority, if
  allowed, and what happens when a projection differs from its source?
- Which authoritative revision belongs to a branch, worktree or commit, and
  how do review, concurrent edits and projection failures affect publication?
- How are existing files imported and the authoritative database recovered?
- How does a consumer opt into database authority or return to file-based use,
  preserving keys and relationships? What capabilities require shared access?

## Placement

No Feature specification exists, so none is placed. A specification sits
beside what it specifies, and the folder each takes is the design's choice.

This epic file lives at `epics/0324-specht-service/epic.md`, where the epic
glob `epics/**/epic.md` discovers it (`0001-F6` decision 0001).

## Status

`needs-decomposition` (2026-10-30): the requests moved here from epic `0132`
on 2026-10-30, candidate F is an unconfirmed candidate with no Feature id, the
service mode has no candidate, and no child exists. The epic is not
`blocked`: nothing in "What this epic waits on" stops a Feature specification
being written. The next step is the owner's: confirm or change the candidate
cut, including whether the service mode takes a candidate of its own.

## Out of this epic

| Item                                                                   | Where it lives instead                                                                   |
| ---------------------------------------------------------------------- | ---------------------------------------------------------------------------------------- |
| Creating, editing or renumbering a document when a value is reserved   | Nowhere: allocation reserves values only, and scaffolding and editing are not part of it |
| The MCP host and reading through MCP                                   | Epic `0132`, candidates B and C                                                          |
| The persistent specification index derived from authoritative Markdown | `0132-F1`                                                                                |
| Qualified-key resolution                                               | Epic `0228` candidate C and item `0307`                                                  |
| The database product                                                   | Not chosen by these requirements; selection follows the agreement                        |

The sequence request reserves values only; document scaffolding and editing
remain outside this epic. Cross-machine coordination is outside the current
offline scope; online adoption remains open. The existing write exclusion must
explicitly account for reservation state before candidate F is implemented.
