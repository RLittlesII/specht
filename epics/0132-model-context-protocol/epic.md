---
title: "Epic 0132: specht through the Model Context Protocol"
description: "A second door onto specht for agents and editors: pinned rule knowledge, structured tree facts and check verdicts matching the CLI, a persistent keyed index of Markdown, and project-scoped document sequences independent of git"
id: "0132"
type: epic
status: blocked
priority: med
milestone: null
children: ["0132-F1"]
created: "2026-10-09"
updated: "2026-10-21"
github_issue: null
---

# Epic 0132: `specht` through the Model Context Protocol

## Summary

The Model Context Protocol (MCP) is the protocol through which a coding agent
or an editor's assistant calls a tool and receives a structured answer. This
epic makes `specht` reachable through it, so that an agent working in a
repository that pins `specht` can ask the tool what a rule requires, what the
pinned schema version demands and what the specification tree holds, and can
ask for the check's verdict, with no terminal session and no output to scrape.

MCP is a second door onto answers the command line already gives. It adds
nothing the command line cannot say: `--json` and `--explain` stay the
agent-facing contract of record (`specht-conventions`, questions reference,
"Already settled"). One kind of answer does not exist on either door today -
nothing returns the tree's facts, because the engine emits violations and
counts - so this epic also adds that answer to the command line, and MCP then
serves the same one.

The owner added a request on 2026-10-17 for unique document sequences per
project and document type, allocated through MCP without coordinating through
git. The requirement is recorded under "Document sequence allocation";
project identity and reservation storage remain open.

The owner added a persistent specification index on 2026-10-19. Markdown remains
authoritative, while a local database keeps keyed facts and relationships for
repeated MCP lookups. Every indexed answer must reflect the current tree, even
after direct edits and branch switches (`0132-F1` decision 0001). Feature
`0132-F1` is a draft; its key scope and performance evidence remain open.

On 2026-10-20 the owner clarified that the proposed store concerns all records
linked to a specification: items, Features, epics, ADRs, lessons and decisions,
as well as their keys and sequence values. Whether agents answer better from
indexed files or complete documents served by MCP is unresolved. The owner also
raised optional online mode; if adopted, it would use one project sequence
shared across machines. Spike `0252` will test the access paths and storage
options before those choices amend the existing agreement.

The need and its scope were decided by the repository owner on 2026-10-09.
This file is the record of that decision. Nothing here is designed: no MCP
tool is named, no transport is chosen, no project is laid out and no package
is decided. Candidates A to F remain an unconfirmed cut; the owner confirmed
the distinct indexed-storage capability in Feature `0132-F1`.

## Business Value

**The problem.** An agent working in a repository that pins `specht` cannot
ask the tool what a rule requires, what the pinned schema version demands or
what the specification tree holds. It reads and searches Markdown, runs the
command and scrapes its output, or answers from memory.

**Why now.** More repositories are about to pin `specht`. Each would otherwise
hand-copy the skills that explain the rules and the shape of the tree, which
is the drift `specht` exists to remove (AGENTS.md § Project), one layer up.

**The solved state.** A consumer carries no copied rule knowledge, the tree is
answerable as structured data, and one client configuration entry gives every
agent and editor in the repository `specht`.

Who it is for. Where two of them want different things, the primary persona's
need wins.

| Persona                                 | Standing     | Goal                                                                         |
| --------------------------------------- | ------------ | ---------------------------------------------------------------------------- |
| A coding agent in a consumer repository | Primary      | Author a specification, repair a violation, trace a claim to code, plan work |
| A role agent in this repository         | Secondary    | The same, on this repository's own tree                                      |
| A person in an editor or a chat         | Secondary    | Ask an assistant about the tree with no terminal session                     |
| Automation with no .NET SDK             | Out of scope | The .NET SDK is required; there is no second distribution (Out of this epic) |

What it costs them today:

| Pain                    | Cost                                                                              |
| ----------------------- | --------------------------------------------------------------------------------- |
| Copied skills drift     | A copy falls behind the pinned schema version, and agents follow stale rules      |
| Context spent on lookup | Every working session rediscovers structure the tool already knows                |
| Wrong work ships        | A guessed rule or a missed dependency surfaces at the build or in review          |
| Adoption stalls         | A new repository ports rule knowledge by hand before its agents are useful        |
| Sequence files conflict | Parallel authors claim the same next id or conflict when merging a shared counter |

## Success criteria

Each criterion carries the tier the owner gave it. A Must is the epic; a Could
is delivered only if it is cheap once the Musts exist. The work queue is the
first thing dropped, and the existing writes are dropped with it. Requested
marks the new sequence capability, whose tier has not been decided.

| Criterion                                                                                                                                                                                     | Tier      | Notes                                                                                                                  |
| --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------- | ---------------------------------------------------------------------------------------------------------------------- |
| Rule and schema knowledge for the pinned schema version is answerable: what each `SPEC###` requires, the sections, the table headers, the id grammars, the frontmatter keys and the templates | Must      | Facts only; no guidance on how to author                                                                               |
| The tree's facts are answerable as structured data: its Features, claims, constraints, open questions, dependencies and claim coverage                                                        | Must      | From the command line and through MCP, as one answer, with a test proving they agree. The command line gains a command |
| The check's verdict is answerable on request                                                                                                                                                  | Must      | On request only; not feedback while a document is being typed                                                          |
| Every answer is the answer for the schema version the repository pins                                                                                                                         | Must      | Proven by a test across two schema versions                                                                            |
| A verdict or a fact given through MCP is identical to the `specht` command's on the same tree                                                                                                 | Must      | Proven by a test                                                                                                       |
| One client configuration entry gives a fresh repository every fact: an agent with no `specht`-specific skill authors a Feature specification that passes the check                            | Must      | The claim covers facts about `specht` and its schema. Method skills - how to write a good specification - are outside  |
| The existing writes are reachable through MCP: `init`, `upgrade`, the pin command and `format`                                                                                                | Could     | Each bound by the same write rule as its command (brief § 9). Dropped alongside the work queue                         |
| The work queue is answerable: items, their status, their derived priority and rank, and which are startable                                                                                   | Could     | Only if cheap once the tree's facts exist. The first thing dropped                                                     |
| A caller can obtain a unique sequence value for a project and a supported document type without git coordination                                                                              | Requested | Added by the owner, 2026-10-17; tier and authority boundaries await agreement under "Document sequence allocation"     |
| Repeated MCP key lookups can follow stored specification relationships without reparsing unchanged Markdown, while each answer remains current with the repository tree                       | Requested | Added by the owner, 2026-10-19; `0132-F1` is draft, with key scope and measurement open                                |
| A caller can find the items, Features, epics, ADRs, lessons and decisions related to a specification                                                                                          | Requested | Clarified by the owner, 2026-10-20; spike `0252` will evaluate how their content is served                             |

## Related records

The store request is broader than candidate F's sequence allocator. It asks
MCP to connect items, Features, epics, ADRs, lessons and decisions to their
specifications. These records have different scopes and relationships, so a
single parent link or number counter does not describe the whole model.

Feature `0132-F1` currently specifies a local index derived from authoritative
Markdown. Its decision 0001 and constraints C-1, C-3 and C-8 govern the draft.
Spike `0252` compares direct filesystem access, MCP lookup that opens source
files, and MCP retrieval of complete stored documents. A recommendation for
database-owned documents or a hosted mode must name the agreements and
repository invariants to amend; this epic does not change them implicitly.

## Document sequence allocation

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

## Features

**Candidates A to F await the owner's confirmation.** They have no Feature id,
specification or place in `children`: a candidate takes its Feature id when
its specification is written. Candidate G is confirmed as a separate draft
Feature and is listed in `children`.

The candidates are cut by capability dimension, where the invariant changes:

| Candidate     | Name                           | Invariant it would carry                                                                                                      | Tier      |
| ------------- | ------------------------------ | ----------------------------------------------------------------------------------------------------------------------------- | --------- |
| A             | Tree facts on the command line | The tree's facts are a read-only function of the tree and the pinned schema version, given as structured data                 | Must      |
| B             | The MCP host                   | A local process the client starts, the same version as the tool, reached by one client configuration entry, with no port      | Must      |
| C             | Reading through MCP            | Every answer MCP gives is the command line's answer on the same tree, and nothing is written                                  | Must      |
| D             | Writing through MCP            | A write through MCP is one an existing command already makes, bound by that command's write rule                              | Could     |
| E             | The work queue                 | The work items' delivery state is answerable, read-only                                                                       | Could     |
| F             | Document sequence allocation   | Concurrent allocations within an agreed project and document-type scope never return the same value, without git coordination | Requested |
| G (`0132-F1`) | Persistent specification index | A local, rebuildable database holds keyed facts and relationships; current-tree answers avoid reparsing unchanged Markdown    | Requested |

Where the candidate cut was tested, and what the owner is asked to confirm:

- **Candidate A is cut from candidate C.** Tree facts must exist on the
  command line whether or not MCP is ever built, and candidate A survives
  candidates B to E all being dropped. Its invariant names no protocol.
- **Candidate B is cut from candidate C.** The host's claims - it starts
  locally, opens no port, makes no outbound call and cannot be a different
  version from the tool - can each fail with no answer served. Candidate C's
  invariant is parity, which can fail with the host correct. The counter-case
  is that a host serving nothing delivers nothing a user can see, so the owner
  may prefer them as one Feature.
- **Candidate C joins three kinds of answer:** rule and schema knowledge, the
  tree's facts, and the verdict. They are kept together because all three
  carry one invariant - the same answer as the command line, for the pinned
  version, with nothing written. They would be cut apart if an open question
  below gives one of them a rule the others do not share.
- **Candidate D is cut from candidate C** because it is the only candidate
  through which MCP writes into a consumer's tree, and because it waits on
  different work (see "What this epic waits on").
- **Candidate E is cut from candidate A.** The tree's facts come from
  specifications, which every consumer has in one model. The work queue comes
  from work items, whose schema differs between this repository and its
  consumers (an open question below).
- **The AGENTS.md "&" heuristic** found no conjunction in any candidate name.
- **Candidate F is cut from candidates D and E.** It reserves identifiers,
  rather than exposing an existing tree write or reading delivery state;
  its machine-local reservations need their own state contract.
- **Candidate G is cut from A and C.** A defines the public facts, and C serves
  the same answer through MCP. G owns durable indexing, key traversal and
  freshness underneath those answers. It keeps Markdown authoritative and
  adds no new public answer; its draft agreement is `0132-F1`.
- **Two things no candidate owns yet.** The sixth Must criterion - a fresh
  repository's agent authoring a passing Feature specification - is a
  property of candidates A, B and C together. And if the owner decides that
  schema knowledge or a check of one specification needs a command-line form
  (open questions below), that command has no candidate here and no Feature
  in epic `0001`.

## What this epic waits on

For its existing capabilities, this epic exposes what other Features build.
It starts `blocked` because the Must criteria cannot be met until those capabilities
exist. As of 2026-10-15 only listing and pinning, and the rule pages, are
delivered.

| Capability                                               | Specified in                          | Work items                                                                           | Blocks                             |
| -------------------------------------------------------- | ------------------------------------- | ------------------------------------------------------------------------------------ | ---------------------------------- |
| A rule's full text                                       | `0001-F3` B-019, B-020; `0001-F3` C-6 | `0040` (`--explain`) ready for architecture; `0129` (a rule page per `SPEC###`) done | The Must criteria                  |
| What a rule expected, per finding                        | `0001-F3` C-2                         | `0037`, `0038`, `0039`, ready for architecture                                       | The Must criteria                  |
| The rule list, with each rule's family and level         | `0001-F10` (draft; `blocked`)         | None cut                                                                             | The Must criteria                  |
| The manifest: sections, headers, grammars, keys, the pin | `0001-F9` (draft; `blocked`)          | None cut                                                                             | The Must criteria                  |
| Listing and pinning schema versions                      | `0001-F7` B-041 to B-051              | Resolved 2026-10-15: `0121` to `0123` and `0125` done; `0124` (pin policy) `blocked` | Nothing; was only the Could writes |
| `upgrade`                                                | `0001-F7`                             | `0045` to `0048`, `0093`, `0094`, `0131`, ready for architecture                     | Only the Could writes              |
| `format`                                                 | `0101-F5`                             | None cut; the Feature is itself `blocked`                                            | Only the Could writes              |

Resolved 2026-10-15: item `0129` is done on `main`. When this section was
written it was open as pull request #79.

The tree-facts command is not in this table: it is this epic's own
(candidate A). The new sequence capability (candidate F) also belongs to
this epic; its unresolved authority boundaries are separate from these
dependencies on existing capabilities.

Feature `0132-F1` can define its index independently, but its public lookups
wait on candidate A's facts and candidate C's MCP read surface. Its draft
agreement records the key-scope and performance questions.

## The command line MCP serves

The owner, 2026-10-15: "I'd like the MCP to have an understanding of what
Spec tool commands it needs to have available to be valuable." MCP adds
nothing the command line cannot say (§ Constraints), so each answer a
criterion names needs a command first. States are read from the items and
specifications on 2026-10-15.

| Criterion | Answer                                                                        | Command                                        | Specified in                                             | State                                                                                                                                                                                       |
| --------- | ----------------------------------------------------------------------------- | ---------------------------------------------- | -------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Must      | The check's verdict                                                           | `specht --json`                                | `0001-F3` B-001                                          | Built (`0035` done)                                                                                                                                                                         |
| Must      | What each violation expected                                                  | `expected` in the `--json` report              | `0001-F3` C-2                                            | Specified; `0037` to `0039` ready for architecture                                                                                                                                          |
| Must      | What each `SPEC###` requires                                                  | `specht --explain SPEC###`                     | `0001-F3` B-019, B-020                                   | Specified; `0040` ready for architecture. The rule pages are built (`0129` done)                                                                                                            |
| Must      | The rules, with family and level                                              | `specht rules --json`                          | `0001-F10`                                               | Draft; `blocked`; no item                                                                                                                                                                   |
| Must      | Sections, table headers, id grammars, frontmatter key names                   | `specht manifest --json`                       | `0001-F9`                                                | Draft; `blocked`; no item                                                                                                                                                                   |
| Must      | The frontmatter schemas and the templates                                     | None                                           | Nowhere; `0001-F9` § 5 row 3 leaves it to this epic      | Gap: `init` writes them as files, and no command prints them                                                                                                                                |
| Must      | The pinned schema version                                                     | `specht schema list`; `specht manifest --json` | `0001-F7` B-084; `0001-F9` B-006                         | `schema list` built (`0121`, `0125` done) with no `--json` (`docs/cli.md`); `manifest` as above                                                                                             |
| Must      | The tree's facts: Features, claims, constraints, open questions, dependencies | None                                           | Nowhere: candidate A, shared with epic `0228` (its OQ-2) | Gap                                                                                                                                                                                         |
| Must      | Claim coverage                                                                | `specht trace --json`                          | `0001-F13`                                               | Approved; `blocked`; no item. Overlaps candidate A; which answers coverage is not decided here                                                                                              |
| Open      | A check of one specification                                                  | None                                           | Nowhere                                                  | Gap; the open question below stands. Candidates: epic `0228` candidate E (`--only`, the verdict stays the tree's); `specht frontmatter` (`0001-F8`, draft, `blocked`, `--json` is its OQ-2) |
| Could     | Write the configuration                                                       | `specht init`                                  | `0001-F4`                                                | Built (`0185`, `0186` done)                                                                                                                                                                 |
| Could     | Move the pin                                                                  | `specht schema pin`                            | `0001-F7` B-041 to B-051                                 | Built (`0122`, `0123`, `0125` done); `0124` (pin policy) `blocked`                                                                                                                          |
| Could     | Bring files to the pin                                                        | `specht upgrade`                               | `0001-F7`                                                | Specified, not built; its items ready for architecture                                                                                                                                      |
| Could     | Reorder rows and keys                                                         | `specht format`                                | `0101-F5`                                                | Draft; `blocked`; no item                                                                                                                                                                   |
| Could     | The work queue                                                                | None                                           | Nowhere                                                  | Gap                                                                                                                                                                                         |

The two remaining Must criteria - parity with the command line, and one client
configuration entry - are properties of the host (candidates B and C), not
answers, and need no command.

**The minimum set.** MCP is valuable once the Must rows answer: `specht
--json` with `expected`, `--explain`, `specht rules --json`, `specht manifest
--json`, which also carries the pin, the tree's facts, claim coverage, and a
printer for the frontmatter schemas and templates. Of these, only `specht
--json` is built.

**Findings for the owner.** Four answers have no specification. None is
decided here, and none is a Feature or an item:

1. The tree-facts command: candidate A; which epic owns it is epic `0228`
   OQ-2.
2. A printer for the frontmatter schemas and the templates: the open question
   below on schema knowledge, which `0001-F9` § 5 row 3 defers to this epic.
3. `specht schema list --json`: the list of shipped versions has no document
   form. Whether that list is a Must answer at all is the open question
   below on the list of shipped versions.
4. A check of one specification: the open question below.

## Constraints

Every constraint is hard. None is this epic's invention: each is an existing
rule of the repository or the owner's word, and each is listed so that a
Feature specification cut from this epic carries it forward.

The sequence and related-record requests do not silently waive these
constraints. Their authorities must fit them, or their owning records must be
amended by an explicit decision before implementation.

| Constraint                                                                                                                                   | Source                                                       | What it rules out                                                                                       |
| -------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------- |
| MCP is served by a local process the client starts. It opens no network port and makes no outbound call; only `init` and `upgrade` may fetch | AGENTS.md § Invariants; `0001-F7` decision 0004              | A hosted or remote service; a listening port; a fetch from any answer that is not `init` or `upgrade`   |
| No answer carries an absolute path                                                                                                           | brief § 9                                                    | A path that is not relative to the root the tool was given                                              |
| No MCP answer writes into the consumer's tree; the index writes only derived state outside that tree                                         | brief § 9; owner, 2026-10-19; `0132-F1` C-1, C-2             | Scaffolding or editing a specification; a cache file in the repository                                  |
| `--json` and `--explain` stay the agent-facing contract of record; MCP adds nothing the command line cannot say                              | Owner, 2026-10-09; `specht-conventions`, questions reference | An answer available only through MCP; a field MCP has and the command line lacks                        |
| The tool's answers and MCP's answers can never come from different versions                                                                  | Owner, 2026-10-09                                            | A host that can be installed or upgraded apart from the tool it answers for. How is the design's choice |
| The epic waits on what it exposes and builds no command-line capability another epic owns                                                    | Owner, 2026-10-09                                            | Building rule text, `expected`, the list and pin commands or `format` here to unblock sooner            |
| The rule vocabulary is fixed per schema version                                                                                              | AGENTS.md § Invariants; `0001-F7` C-11                       | A rule, or a rule's text, that MCP knows and the pinned schema version does not                         |

## Open questions

The existing questions below remain open. Each travels to the Feature specification it binds,
where it takes an `OQ-n` id and is resolved in place.

| Question                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       | Who answers                                                               |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------- |
| Is the candidate cut in "Features" the cut?                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    | The owner                                                                 |
| Is MCP delivered in the `specht.tool` package or in a package of its own?                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | The design, under the same-version constraint; it touches epic `0055`     |
| Schema knowledge - the sections, the id grammars, the frontmatter keys, the templates - has no command that prints it today. Under the rule that MCP adds nothing, does it need one, or do the files `init` writes count as the command line's door? Noted 2026-10-11 by spec-author, not an answer: `0001-F9` (`specht manifest`) and `0001-F10` (`specht rules`) are now specified, as drafts, and print the manifest - sections, grammars and key names - and the rule list; they are candidate Features for this question. The frontmatter schemas and the templates still have no printing command (`0001-F9` § 5 row 3). | The owner                                                                 |
| A check of one specification has no command-line form. Is one added to the command line, or does MCP give the verdict for the whole tree only?                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | The owner                                                                 |
| What exactly are the tree's facts, in what shape, and do they get a published schema as the report has (`0001-F3` C-1)?                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | The `spec-author` in candidate A's specification, then the owner          |
| How does a caller confirm a write made through MCP?                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            | The design                                                                |
| Which MCP clients must the epic be proven against?                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             | The owner                                                                 |
| The work queue rests on this repository's item schema, and consumers use the `000X-NN` task id instead. Which does the work queue answer for?                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | The owner, if the Could criterion is taken up                             |
| Does a person in an editor need anything an answer shaped for an agent lacks?                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | The owner                                                                 |
| Listing the shipped schema versions (`0001-F7` B-041) writes nothing, yet it is grouped with the pin command as blocking only the Could writes. Is the list of shipped versions part of the schema knowledge the first Must criterion names?                                                                                                                                                                                                                                                                                                                                                                                   | The owner. Raised while writing this file; it was not asked on 2026-10-09 |

### Questions for document sequence allocation

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

### Questions for related records

| Question                                                                                                                        | Who answers                         |
| ------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- |
| Does an MCP index over authoritative files improve agent answers enough, or does serving complete database documents help more? | Spike `0252`, then the owner        |
| Which record types, links and lifecycle fields must a first query cover, and how are missing or stale links reported?           | The Feature specification and owner |
| If full documents move into a database, which copy owns edits, Git history and conflicts between worktrees or machines?         | The owner, then the design          |

## Placement

Feature `0132-F1` lives beside the proposed engine Index area at
`src/specht/Index/.spec/README.md`. The remaining candidate placements follow
the project layout and are not chosen here. Candidate A is a command and would
sit in its command folder under
`src/tool/Features/`, as every command's specification does.
The host lives in this repository and releases on the tool's tag
(ADR-0015).

This epic file lives at `epics/0132-model-context-protocol/epic.md`, where
schema version 1's epic glob `epics/**/epic.md` discovers it (`0001-F6`
decision 0001).

## Status

`blocked`: the owner decided the epic waits on what it exposes. It unblocks
when the rows of "What this epic waits on" that block the Must criteria are
delivered: the rule text, `expected`, the rule list and the manifest. The
next step does not wait for that: the owner confirms or changes candidates
A to F, while `0132-F1` resolves its key scope and measures the index proposal
before any storage implementation is chosen.

## Out of this epic

| Item                                                                    | Where it lives instead                                                                |
| ----------------------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| A caller with no .NET SDK                                               | Nowhere: the .NET SDK is required, and there is no second distribution                |
| Remote or hosted access, or a network port                              | Never: the offline invariant binds MCP as it binds the check (AGENTS.md § Invariants) |
| A new authoring write - MCP scaffolding or editing a specification      | Never: the list of writes stands unchanged (brief § 9)                                |
| Authoring guidance, method skills, role contracts and the delivery flow | The consumer's own skills: `specht` serves facts and does not distribute skills       |
| Anything MCP can say that the command line cannot                       | Never: MCP is additive, and the command line stays the contract of record             |
| Feedback while a document is being typed                                | Not built: running the check on request is enough for the authoring loop              |
| Rule text, `--explain` and `expected`                                   | `0001-F3`; this epic only serves them                                                 |
| The list and pin commands, `init` and `upgrade`                         | `0001-F7`, `0001-F4`; this epic only reaches them, and only under the Could criterion |
| `format`                                                                | `0101-F5`; reached only under the Could criterion                                     |

The sequence request reserves values only; document scaffolding and editing
remain outside this epic. Cross-machine coordination is outside the current
offline scope; online adoption remains open. The existing write exclusion must
explicitly account for reservation state before candidate F is implemented.
