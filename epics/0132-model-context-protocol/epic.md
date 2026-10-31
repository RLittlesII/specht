---
title: "Epic 0132: specht through the Model Context Protocol"
description: "A second door onto specht for agents and editors: pinned rule knowledge, structured tree facts and check verdicts matching the CLI, and a persistent keyed index of Markdown; document sequences and the service mode moved to epic 0324 on 2026-10-30"
id: "0132"
type: epic
status: needs-decomposition
priority: med
milestone: null
children: ["0132-F1", "0132-F2"]
created: "2026-10-09"
updated: "2026-10-30"
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
project identity and reservation storage remain open. Moved to epic `0324`,
2026-10-30, with the service mode and lifetime keys.

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
options before those choices amend the existing agreement. The online
sequence proposal moved to epic `0324`, 2026-10-30.

The need and its scope were decided by the repository owner on 2026-10-09.
This file is the record of that decision. Nothing here is designed: no MCP
tool is named, no transport is chosen, no project is laid out and no package
is decided. Candidates A to F remain an unconfirmed cut; the owner confirmed
the distinct indexed-storage capability in Feature `0132-F1`. Resolved
2026-10-30: the owner confirmed the cut as amended in "Features"; candidate A
is `0132-F2`, specified as a draft. Candidate F, the service mode and lifetime
keys moved to epic `0324`, 2026-10-30.

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

| Pain                    | Cost                                                                         |
| ----------------------- | ---------------------------------------------------------------------------- |
| Copied skills drift     | A copy falls behind the pinned schema version, and agents follow stale rules |
| Context spent on lookup | Every working session rediscovers structure the tool already knows           |
| Wrong work ships        | A guessed rule or a missed dependency surfaces at the build or in review     |
| Adoption stalls         | A new repository ports rule knowledge by hand before its agents are useful   |
| Sequence files conflict | Moved to epic `0324`, 2026-10-30                                             |

## Success criteria

Each criterion carries the tier the owner gave it. A Must is the epic; a Could
is delivered only if it is cheap once the Musts exist. The work queue is the
first thing dropped, and the existing writes are dropped with it. Requested
marks the new sequence capability, whose tier has not been decided. Moved to
epic `0324`, 2026-10-30: the sequence capability's row. Requested still marks
the rows added after 2026-10-09, whose tiers have not been decided.

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
| A caller can obtain a unique sequence value for a project and a supported document type without git coordination                                                                              | Requested | Moved to epic `0324`, 2026-10-30                                                                                       |
| Repeated MCP key lookups can follow stored specification relationships without reparsing unchanged Markdown, while each answer remains current with the repository tree                       | Requested | Added by the owner, 2026-10-19; `0132-F1` is draft, with key scope and measurement open                                |
| A caller can find the items, Features, epics, ADRs, lessons and decisions related to a specification                                                                                          | Requested | Clarified by the owner, 2026-10-20; spike `0252` will evaluate how their content is served                             |

## Related records

The store request is broader than candidate F's sequence allocator (moved to
epic `0324`, 2026-10-30). It asks
MCP to connect items, Features, epics, ADRs, lessons and decisions to their
specifications. These records have different scopes and relationships, so a
single parent link or number counter does not describe the whole model.

Feature `0132-F1` currently specifies a local index derived from authoritative
Markdown. Its decision 0001 and constraints C-1, C-3 and C-8 govern the draft.
Spike `0252` compares direct filesystem access, MCP lookup that opens source
files, and MCP retrieval of complete stored documents. A recommendation for
database-owned documents or a hosted mode must name the agreements and
repository invariants to amend; this epic does not change them implicitly.

### Service requirements session, 2026-10-30

Moved to epic `0324`, 2026-10-30: the database-authority service mode,
lifetime keys, revisions and snapshots, GitHub projection and a team or remote
store.

## Document sequence allocation

Moved to epic `0324`, 2026-10-30, with candidate F.

## Features

**Candidates A to F await the owner's confirmation.** They have no Feature id,
specification or place in `children`: a candidate takes its Feature id when
its specification is written. Candidate G is confirmed as a separate draft
Feature and is listed in `children`.

**Resolved 2026-10-30 by the owner: the cut is confirmed as amended here.**
The owner reserved Feature ids on 2026-10-30: `0132-F3` for candidate B,
`0132-F4` for candidate C and `0001-F18` for the new epic `0001` Feature.
Each joins its epic's `children` when its specification is written.

| Candidate     | Decision, 2026-10-30                                                                                                                                                                                                                                |
| ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A (`0132-F2`) | Confirmed and owned here; epic `0228` depends on it (its OQ-2). Specified as a draft at `src/tool/Features/Facts/.spec/`, working name `specht facts`                                                                                               |
| B             | Confirmed, not yet specified. A Feature of its own, shipped with C; its items are cut so that no B item is done while serving nothing. Packaging is its open question. Feature id `0132-F3` reserved, 2026-10-30                                    |
| C             | Confirmed, not yet specified. The verdict for the whole tree only; no single-specification filter. Its open questions: which clients, how the sixth Must is proven, and what a person in an editor needs. Feature id `0132-F4` reserved, 2026-10-30 |
| D             | Could, deferred                                                                                                                                                                                                                                     |
| E             | Parked, unconfirmed                                                                                                                                                                                                                                 |
| F             | Moved to epic `0324` with the service mode, 2026-10-30. Was: moves to a new epic with the service mode; destination epic not yet created                                                                                                            |
| G (`0132-F1`) | Unchanged: a draft; its decision 0001 and C-1 stand                                                                                                                                                                                                 |
| New, `0001`   | Decided and pending: a Feature in epic `0001` printing the frontmatter schemas and the templates, with `specht schema list --json`. Not yet specified. Feature id `0001-F18` reserved, 2026-10-30                                                   |

Claim coverage is `specht trace` (`0001-F13`), which candidate A leaves out;
cutting `0001-F13`'s items is on the Must path (owner, 2026-10-30). No item
is cut by this record. The table below is the cut as first proposed.

The candidates are cut by capability dimension, where the invariant changes:

| Candidate     | Name                           | Invariant it would carry                                                                                                   | Tier      |
| ------------- | ------------------------------ | -------------------------------------------------------------------------------------------------------------------------- | --------- |
| A             | Tree facts on the command line | The tree's facts are a read-only function of the tree and the pinned schema version, given as structured data              | Must      |
| B             | The MCP host                   | A local process the client starts, the same version as the tool, reached by one client configuration entry, with no port   | Must      |
| C             | Reading through MCP            | Every answer MCP gives is the command line's answer on the same tree, and nothing is written                               | Must      |
| D             | Writing through MCP            | A write through MCP is one an existing command already makes, bound by that command's write rule                           | Could     |
| E             | The work queue                 | The work items' delivery state is answerable, read-only                                                                    | Could     |
| F             | Document sequence allocation   | Moved to epic `0324`, 2026-10-30                                                                                           | Requested |
| G (`0132-F1`) | Persistent specification index | A local, rebuildable database holds keyed facts and relationships; current-tree answers avoid reparsing unchanged Markdown | Requested |

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
  its machine-local reservations need their own state contract. Moved to
  epic `0324`, 2026-10-30.
- **Candidate G is cut from A and C.** A defines the public facts, and C serves
  the same answer through MCP. G owns durable indexing, key traversal and
  freshness underneath those answers. It keeps Markdown authoritative and
  adds no new public answer; its draft agreement is `0132-F1`.
- **Two things no candidate owns yet.** The sixth Must criterion - a fresh
  repository's agent authoring a passing Feature specification - is a
  property of candidates A, B and C together. And if the owner decides that
  schema knowledge or a check of one specification needs a command-line form
  (open questions below), that command has no candidate here and no Feature
  in epic `0001`. Resolved 2026-10-30 by the owner: schema knowledge gets the
  new epic `0001` Feature, and a check of one specification gets none.

## What this epic waits on

For its existing capabilities, this epic exposes what other Features build.
It started `blocked` because the Must criteria cannot be met until those capabilities
exist. As of 2026-10-15 only listing and pinning, and the rule pages, were
delivered. Resolved 2026-10-30: every row that blocked the Must criteria is
delivered; only the pin policy and `format`, which block nothing a Must needs,
remain open.

| Capability                                               | Specified in                          | Work items                                                                                                              | Blocks                             |
| -------------------------------------------------------- | ------------------------------------- | ----------------------------------------------------------------------------------------------------------------------- | ---------------------------------- |
| A rule's full text                                       | `0001-F3` B-019, B-020; `0001-F3` C-6 | Resolved 2026-10-30: `0040` (`--explain`) done. Was: ready for architecture; `0129` (a rule page per `SPEC###`) done    | Nothing; was the Must criteria     |
| What a rule expected, per finding                        | `0001-F3` C-2                         | Resolved 2026-10-30: `0037`, `0038`, `0039` done. Was: ready for architecture                                           | Nothing; was the Must criteria     |
| The rule list, with each rule's family and level         | `0001-F10`                            | Resolved 2026-10-30: `0280` to `0282` done (`specht rules`, `--json`). Was: draft, `blocked`, none cut                  | Nothing; was the Must criteria     |
| The manifest: sections, headers, grammars, keys, the pin | `0001-F9`                             | Resolved 2026-10-30: `0283` to `0286` done (`specht manifest`, `--defaults`, `--json`). Was: draft, `blocked`, none cut | Nothing; was the Must criteria     |
| Listing and pinning schema versions                      | `0001-F7` B-041 to B-051              | Resolved 2026-10-15: `0121` to `0123` and `0125` done; `0124` (pin policy) `blocked`, still `blocked` on 2026-10-30     | Nothing; was only the Could writes |
| `upgrade`                                                | `0001-F7`                             | Resolved 2026-10-30: `0045` to `0048`, `0093`, `0094` and `0131` done. Was: ready for architecture                      | Nothing; was only the Could writes |
| `format`                                                 | `0101-F5`                             | None cut; the Feature is itself `blocked`, still on 2026-10-30                                                          | Only the Could writes              |

Resolved 2026-10-15: item `0129` is done on `main`. When this section was
written it was open as pull request #79.

The tree-facts command is not in this table: it is this epic's own
(candidate A). The new sequence capability (candidate F) also belongs to
this epic; its unresolved authority boundaries are separate from these
dependencies on existing capabilities. Moved to epic `0324`, 2026-10-30:
candidate F no longer belongs to this epic.

What is still unbuilt for the Musts is not in this table (2026-10-30): the
tree-facts command, now the draft `0132-F2`; a printer for the frontmatter
schemas and the templates, decided as a new epic `0001` Feature not yet
specified, its id `0001-F18` reserved; and the published schemas for the rule listing and the manifest
(`0001-F10` B-020, `0001-F9` B-021). A check of one specification is decided:
no single-specification form; scoping is epic `0228` candidate E. Claim
coverage is decided as `specht trace` (`0001-F13`, approved, `blocked`, no
item cut). See "The command line MCP serves".

Feature `0132-F1` can define its index independently, but its public lookups
wait on candidate A's facts and candidate C's MCP read surface. Its draft
agreement records the key-scope and performance questions.

## The command line MCP serves

The owner, 2026-10-15: "I'd like the MCP to have an understanding of what
Spec tool commands it needs to have available to be valuable." MCP adds
nothing the command line cannot say (§ Constraints), so each answer a
criterion names needs a command first. States are read from the items and
specifications on 2026-10-15. Resolved 2026-10-30: the states were read again;
a changed cell says so and keeps what it replaced.

| Criterion | Answer                                                                        | Command                                        | Specified in                                        | State                                                                                                                                                                                                                                                                                                                                          |
| --------- | ----------------------------------------------------------------------------- | ---------------------------------------------- | --------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Must      | The check's verdict                                                           | `specht --json`                                | `0001-F3` B-001                                     | Built (`0035` done)                                                                                                                                                                                                                                                                                                                            |
| Must      | What each violation expected                                                  | `expected` in the `--json` report              | `0001-F3` C-2                                       | Resolved 2026-10-30: built (`0037` to `0039` done). Was: specified, ready for architecture                                                                                                                                                                                                                                                     |
| Must      | What each `SPEC###` requires                                                  | `specht --explain SPEC###`                     | `0001-F3` B-019, B-020                              | Resolved 2026-10-30: built (`0040` done). The rule pages are built (`0129` done). Was: specified, ready for architecture                                                                                                                                                                                                                       |
| Must      | The rules, with family and level                                              | `specht rules --json`                          | `0001-F10`                                          | Resolved 2026-10-30: built (`0280` to `0282` done). Was: draft, `blocked`, no item. Its OQ-4 resolved 2026-10-30 by the owner: a published schema, claimed as `0001-F10` B-020 and C-8. Was: its OQ-4, a published schema for the document, is open                                                                                            |
| Must      | Sections, table headers, id grammars, frontmatter key names                   | `specht manifest --json`                       | `0001-F9`                                           | Resolved 2026-10-30: built (`0283` to `0286` done). Was: draft, `blocked`, no item. Its OQ-4 resolved 2026-10-30 by the owner: a published schema, claimed as `0001-F9` B-021 and C-7, and `0001-F9` reopened to `draft` for review. Its OQ-1 to OQ-3 and OQ-5 are open. Was: its OQ-1 to OQ-5, a published schema among them (OQ-4), are open |
| Must      | The frontmatter schemas and the templates                                     | None                                           | Nowhere; `0001-F9` § 5 row 3 leaves it to this epic | Resolved 2026-10-30 by the owner: a new epic `0001` Feature prints them, decided and pending; not yet specified. Was: gap: `init` writes them as files, and no command prints them                                                                                                                                                             |
| Must      | The pinned schema version                                                     | `specht schema list`; `specht manifest --json` | `0001-F7` B-084; `0001-F9` B-006                    | `schema list` built (`0121`, `0125` done) with no `--json` (`docs/cli.md`), still on 2026-10-30. Resolved 2026-10-30: `manifest --json` built and carries `schemaVersion`. Resolved 2026-10-30 by the owner: `specht schema list --json` joins the new epic `0001` Feature, decided and pending                                                |
| Must      | The tree's facts: Features, claims, constraints, open questions, dependencies | `specht facts --json` (working name)           | `0132-F2`, draft                                    | Resolved 2026-10-30: specified as the draft `0132-F2`, owned here, with epic `0228` depending on it; no item. Was: gap                                                                                                                                                                                                                         |
| Must      | Claim coverage                                                                | `specht trace --json`                          | `0001-F13`                                          | Approved; `blocked`; no item, still on 2026-10-30. Resolved 2026-10-30 by the owner: `specht trace` answers coverage, candidate A leaves it out, and cutting its items is on the Must path. Was: overlaps candidate A; which answers coverage is not decided here                                                                              |
| Open      | A check of one specification                                                  | None                                           | Nowhere                                             | Resolved 2026-10-30 by the owner: no single-specification form; MCP gives the verdict for the whole tree only, and scoping the output is epic `0228` candidate E. `specht frontmatter` (`0001-F8`) is built (`0232` to `0234` done), checks one document's frontmatter only, and has no `--json`. Was: gap; the open question stood            |
| Could     | Write the configuration                                                       | `specht init`                                  | `0001-F4`                                           | Built (`0185`, `0186` done)                                                                                                                                                                                                                                                                                                                    |
| Could     | Move the pin                                                                  | `specht schema pin`                            | `0001-F7` B-041 to B-051                            | Built (`0122`, `0123`, `0125` done); `0124` (pin policy) `blocked`                                                                                                                                                                                                                                                                             |
| Could     | Bring files to the pin                                                        | `specht upgrade`                               | `0001-F7`                                           | Resolved 2026-10-30: built (`0045` to `0048`, `0093`, `0094`, `0131` done). Was: specified, not built                                                                                                                                                                                                                                          |
| Could     | Reorder rows and keys                                                         | `specht format`                                | `0101-F5`                                           | Draft; `blocked`; no item, still on 2026-10-30                                                                                                                                                                                                                                                                                                 |
| Could     | The work queue                                                                | None                                           | Nowhere                                             | Gap                                                                                                                                                                                                                                                                                                                                            |

The two remaining Must criteria - parity with the command line, and one client
configuration entry - are properties of the host (candidates B and C), not
answers, and need no command.

**The minimum set.** MCP is valuable once the Must rows answer: `specht
--json` with `expected`, `--explain`, `specht rules --json`, `specht manifest
--json`, which also carries the pin, the tree's facts, claim coverage, and a
printer for the frontmatter schemas and templates. Of these, only `specht
--json` is built. Resolved 2026-10-30: `expected`, `--explain`, `specht rules
--json` and `specht manifest --json` are built too. Three Must answers remain
unbuilt: the tree's facts (`0132-F2`, draft), claim coverage (`0001-F13`,
approved, no item) and the printer for the frontmatter schemas and templates
(a new epic `0001` Feature, not yet specified). The published schemas for the
rule listing, the manifest and the facts are claimed and unbuilt (`0001-F10`
B-020, `0001-F9` B-021, `0132-F2` B-002).

**Findings for the owner.** Four answers have no specification. None is
decided here, and none is a Feature or an item. Each is answered on
2026-10-30, as its entry says:

1. The tree-facts command: candidate A; which epic owns it is epic `0228`
   OQ-2. Resolved 2026-10-30 by the owner: this epic owns it, as the draft
   `0132-F2`.
2. A printer for the frontmatter schemas and the templates: the open question
   below on schema knowledge, which `0001-F9` § 5 row 3 defers to this epic.
   Resolved 2026-10-30 by the owner: a new epic `0001` Feature, not yet
   specified.
3. `specht schema list --json`: the list of shipped versions has no document
   form. Whether that list is a Must answer at all is the open question
   below on the list of shipped versions. Resolved 2026-10-30 by the owner: it
   joins the new epic `0001` Feature.
4. A check of one specification: the open question below. Resolved
   2026-10-30 by the owner: no single-specification form; scoping is epic
   `0228` candidate E.

## Constraints

Every constraint is hard. None is this epic's invention: each is an existing
rule of the repository or the owner's word, and each is listed so that a
Feature specification cut from this epic carries it forward.

The sequence and related-record requests do not silently waive these
constraints. Their authorities must fit them, or their owning records must be
amended by an explicit decision before implementation. The sequence request
moved to epic `0324`, 2026-10-30, and its constraints are recorded there.

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

Each question travels to the Feature specification it binds, where it takes
an `OQ-n` id and is resolved in place. The owner answered the requirements
pass on 2026-10-30; the answers are recorded in each row.

| Question                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       | Who answers                                                                                                                                                                                                                                                                 |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Is the candidate cut in "Features" the cut?                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    | The owner. Resolved 2026-10-30 by the owner: confirmed as amended in "Features"                                                                                                                                                                                             |
| Is MCP delivered in the `specht.tool` package or in a package of its own?                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | The design, under the same-version constraint; it touches epic `0055`. Owner, 2026-10-30: left to the design, carried as an open question on candidate B's specification. Travels to candidate B's specification                                                            |
| Schema knowledge - the sections, the id grammars, the frontmatter keys, the templates - has no command that prints it today. Under the rule that MCP adds nothing, does it need one, or do the files `init` writes count as the command line's door? Noted 2026-10-11 by spec-author, not an answer: `0001-F9` (`specht manifest`) and `0001-F10` (`specht rules`) are now specified, as drafts, and print the manifest - sections, grammars and key names - and the rule list; they are candidate Features for this question. The frontmatter schemas and the templates still have no printing command (`0001-F9` § 5 row 3). | The owner. Resolved 2026-10-30 by the owner: it needs a command. A printer for the frontmatter schemas and the templates, with `specht schema list --json`, becomes a new Feature in epic `0001`, decided and pending                                                       |
| A check of one specification has no command-line form. Is one added to the command line, or does MCP give the verdict for the whole tree only?                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | The owner. Resolved 2026-10-30 by the owner: MCP gives the whole-tree verdict only, with no single-specification filter; scoping the output is epic `0228` candidate E                                                                                                      |
| What exactly are the tree's facts, in what shape, and do they get a published schema as the report has (`0001-F3` C-1)?                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | The `spec-author` in candidate A's specification, then the owner. Resolved 2026-10-30 by the owner: epic `0228` R1 in full, without claim coverage, for the whole tree or a path, with a published schema (`0132-F2` decision 0001); what remains is `0132-F2` OQ-1 to OQ-6 |
| How does a caller confirm a write made through MCP?                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            | The design. 2026-10-30: deferred with candidate D                                                                                                                                                                                                                           |
| Which MCP clients must the epic be proven against?                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             | The owner. 2026-10-30, the owner: travels to candidate C's specification as an open question, with how the sixth Must criterion is proven                                                                                                                                   |
| The work queue rests on this repository's item schema, and consumers use the `000X-NN` task id instead. Which does the work queue answer for?                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | The owner, if the Could criterion is taken up. 2026-10-30: waits with candidate E, parked                                                                                                                                                                                   |
| Does a person in an editor need anything an answer shaped for an agent lacks?                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | The owner. 2026-10-30, the owner: deferred to candidate C's specification                                                                                                                                                                                                   |
| Listing the shipped schema versions (`0001-F7` B-041) writes nothing, yet it is grouped with the pin command as blocking only the Could writes. Is the list of shipped versions part of the schema knowledge the first Must criterion names?                                                                                                                                                                                                                                                                                                                                                                                   | The owner. Raised while writing this file; it was not asked on 2026-10-09. Resolved 2026-10-30 by the owner: yes; `specht schema list --json` joins the new epic `0001` Feature                                                                                             |

### Questions for document sequence allocation

Moved to epic `0324`, 2026-10-30, with candidate F.

### Questions for related records

| Question                                                                                                                        | Who answers                         |
| ------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- |
| Does an MCP index over authoritative files improve agent answers enough, or does serving complete database documents help more? | Spike `0252`, then the owner        |
| Which record types, links and lifecycle fields must a first query cover, and how are missing or stale links reported?           | The Feature specification and owner |
| If full documents move into a database, which copy owns edits, Git history and conflicts between worktrees or machines?         | Moved to epic `0324`, 2026-10-30    |

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

`needs-decomposition` (2026-10-30). Was: `blocked`, because the owner decided
the epic waits on what it exposes; those capabilities are delivered ("What
this epic waits on"). The owner answered the requirements pass on 2026-10-30
and confirmed the cut in "Features". Candidate A is `0132-F2`, a draft with
open questions. Candidates B and C are confirmed and not yet specified, with
`0132-F3` and `0132-F4` reserved, and the new epic `0001` printer Feature is
pending, with `0001-F18` reserved; none is in `children` until its
specification exists. The service mode and candidate F moved to epic `0324`
on 2026-10-30. Was: decided to move to an epic not yet created. The epic
stays `needs-decomposition` until B, C and the printer Feature are specified.

## Out of this epic

| Item                                                                                | Where it lives instead                                                                |
| ----------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| A caller with no .NET SDK                                                           | Nowhere: the .NET SDK is required, and there is no second distribution                |
| Remote or hosted access, or a network port                                          | Never: the offline invariant binds MCP as it binds the check (AGENTS.md § Invariants) |
| A new authoring write - MCP scaffolding or editing a specification                  | Never: the list of writes stands unchanged (brief § 9)                                |
| Authoring guidance, method skills, role contracts and the delivery flow             | The consumer's own skills: `specht` serves facts and does not distribute skills       |
| Anything MCP can say that the command line cannot                                   | Never: MCP is additive, and the command line stays the contract of record             |
| Feedback while a document is being typed                                            | Not built: running the check on request is enough for the authoring loop              |
| Rule text, `--explain` and `expected`                                               | `0001-F3`; this epic only serves them                                                 |
| The list and pin commands, `init` and `upgrade`                                     | `0001-F7`, `0001-F4`; this epic only reaches them, and only under the Could criterion |
| `format`                                                                            | `0101-F5`; reached only under the Could criterion                                     |
| Document sequence allocation, the database-authority service mode and lifetime keys | Epic `0324`, moved 2026-10-30                                                         |

Moved to epic `0324`, 2026-10-30: the sequence request's boundaries -
reservation only, the offline scope and the write exclusion for reservation
state.
