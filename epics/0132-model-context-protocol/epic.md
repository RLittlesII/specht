---
title: "Epic 0132: specht through the Model Context Protocol"
description: "A second door onto specht for agents and editors: rule and schema knowledge for the pinned version, the specification tree as structured data and the check verdict, each identical to the command line's answer, served by a local process that one client configuration entry starts"
id: "0132"
type: epic
status: blocked
priority: med
milestone: null
children: []
created: "2026-10-09"
updated: "2026-10-09"
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

The need and its scope were decided by the repository owner on 2026-10-09.
This file is the record of that decision. Nothing here is designed: no MCP
tool is named, no transport is chosen, no project is laid out and no package
is decided. The split into Features is a candidate the owner has not
confirmed, and no Feature specification exists yet.

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

## Success criteria

Each criterion carries the tier the owner gave it. A Must is the epic; a Could
is delivered only if it is cheap once the Musts exist. The work queue is the
first thing dropped, and the writes are dropped with it.

| Criterion                                                                                                                                                                                     | Tier  | Notes                                                                                                                  |
| --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----- | ---------------------------------------------------------------------------------------------------------------------- |
| Rule and schema knowledge for the pinned schema version is answerable: what each `SPEC###` requires, the sections, the table headers, the id grammars, the frontmatter keys and the templates | Must  | Facts only; no guidance on how to author                                                                               |
| The tree's facts are answerable as structured data: its Features, claims, constraints, open questions, dependencies and claim coverage                                                        | Must  | From the command line and through MCP, as one answer, with a test proving they agree. The command line gains a command |
| The check's verdict is answerable on request                                                                                                                                                  | Must  | On request only; not feedback while a document is being typed                                                          |
| Every answer is the answer for the schema version the repository pins                                                                                                                         | Must  | Proven by a test across two schema versions                                                                            |
| A verdict or a fact given through MCP is identical to the `specht` command's on the same tree                                                                                                 | Must  | Proven by a test                                                                                                       |
| One client configuration entry gives a fresh repository every fact: an agent with no `specht`-specific skill authors a Feature specification that passes the check                            | Must  | The claim covers facts about `specht` and its schema. Method skills - how to write a good specification - are outside  |
| The existing writes are reachable through MCP: `init`, `upgrade`, the pin command and `format`                                                                                                | Could | Each bound by the same write rule as its command (brief § 9). Dropped alongside the work queue                         |
| The work queue is answerable: items, their status, their derived priority and rank, and which are startable                                                                                   | Could | Only if cheap once the tree's facts exist. The first thing dropped                                                     |

## Features

**A candidate cut, awaiting the owner's confirmation.** Nothing below is
agreed. No Feature here has an id, a specification or a place in `children`:
a candidate takes its Feature id when its specification is written, and joins
`children` then, so that every child resolves to a file.

The candidates are cut by capability dimension, where the invariant changes:

| Candidate | Name                           | Invariant it would carry                                                                                                 | Tier  |
| --------- | ------------------------------ | ------------------------------------------------------------------------------------------------------------------------ | ----- |
| A         | Tree facts on the command line | The tree's facts are a read-only function of the tree and the pinned schema version, given as structured data            | Must  |
| B         | The MCP host                   | A local process the client starts, the same version as the tool, reached by one client configuration entry, with no port | Must  |
| C         | Reading through MCP            | Every answer MCP gives is the command line's answer on the same tree, and nothing is written                             | Must  |
| D         | Writing through MCP            | A write through MCP is one an existing command already makes, bound by that command's write rule                         | Could |
| E         | The work queue                 | The work items' delivery state is answerable, read-only                                                                  | Could |

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
- **Two things no candidate owns yet.** The sixth Must criterion - a fresh
  repository's agent authoring a passing Feature specification - is a
  property of candidates A, B and C together. And if the owner decides that
  schema knowledge or a check of one specification needs a command-line form
  (open questions below), that command has no candidate here and no Feature
  in epic `0001`.

## What this epic waits on

This epic exposes what other Features build and builds none of it. It starts
`blocked` because the Must criteria cannot be met until two of those
capabilities exist. None of the work below is delivered yet.

| Capability                          | Specified in                          | Work items                                               | Blocks                |
| ----------------------------------- | ------------------------------------- | -------------------------------------------------------- | --------------------- |
| A rule's full text                  | `0001-F3` B-019, B-020; `0001-F3` C-6 | `0040` (`--explain`); `0129` (a rule page per `SPEC###`) | The Must criteria     |
| What a rule expected, per finding   | `0001-F3` C-2                         | `0037`, `0038`, `0039`                                   | The Must criteria     |
| Listing and pinning schema versions | `0001-F7` B-041 to B-051              | `0121` to `0125`                                         | Only the Could writes |
| `format`                            | `0101-F5`                             | None cut; the Feature is itself `blocked`                | Only the Could writes |

Item `0129` is not on `main` at this writing; it is open as pull request #79.
The rule text it adds is the source the first Must criterion answers from, so
it lands before this epic can unblock.

The tree-facts command is not in this table: it is this epic's own
(candidate A).

## Constraints

Every constraint is hard. None is this epic's invention: each is an existing
rule of the repository or the owner's word, and each is listed so that a
Feature specification cut from this epic carries it forward.

| Constraint                                                                                                                                   | Source                                                       | What it rules out                                                                                       |
| -------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------- |
| MCP is served by a local process the client starts. It opens no network port and makes no outbound call; only `init` and `upgrade` may fetch | AGENTS.md § Invariants; `0001-F7` decision 0004              | A hosted or remote service; a listening port; a fetch from any answer that is not `init` or `upgrade`   |
| No answer carries an absolute path                                                                                                           | brief § 9                                                    | A path that is not relative to the root the tool was given                                              |
| Nothing is written outside the existing list of writes                                                                                       | brief § 9                                                    | A write MCP can make that no command makes; scaffolding or editing a specification                      |
| `--json` and `--explain` stay the agent-facing contract of record; MCP adds nothing the command line cannot say                              | Owner, 2026-10-09; `specht-conventions`, questions reference | An answer available only through MCP; a field MCP has and the command line lacks                        |
| The tool's answers and MCP's answers can never come from different versions                                                                  | Owner, 2026-10-09                                            | A host that can be installed or upgraded apart from the tool it answers for. How is the design's choice |
| The epic waits on what it exposes and builds no command-line capability another epic owns                                                    | Owner, 2026-10-09                                            | Building rule text, `expected`, the list and pin commands or `format` here to unblock sooner            |
| The rule vocabulary is fixed per schema version                                                                                              | AGENTS.md § Invariants; `0001-F7` C-11                       | A rule, or a rule's text, that MCP knows and the pinned schema version does not                         |

## Open questions

None is resolved here. Each travels to the Feature specification it binds,
where it takes an `OQ-n` id and is resolved in place.

| Question                                                                                                                                                                                                                                             | Who answers                                                               |
| ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| Is the candidate cut in "Features" the cut?                                                                                                                                                                                                          | The owner                                                                 |
| Is MCP delivered in the `specht.tool` package or in a package of its own?                                                                                                                                                                            | The design, under the same-version constraint; it touches epic `0055`     |
| Schema knowledge - the sections, the id grammars, the frontmatter keys, the templates - has no command that prints it today. Under the rule that MCP adds nothing, does it need one, or do the files `init` writes count as the command line's door? | The owner                                                                 |
| A check of one specification has no command-line form. Is one added to the command line, or does MCP give the verdict for the whole tree only?                                                                                                       | The owner                                                                 |
| What exactly are the tree's facts, in what shape, and do they get a published schema as the report has (`0001-F3` C-1)?                                                                                                                              | The `spec-author` in candidate A's specification, then the owner          |
| How does a caller confirm a write made through MCP?                                                                                                                                                                                                  | The design                                                                |
| Which MCP clients must the epic be proven against?                                                                                                                                                                                                   | The owner                                                                 |
| The work queue rests on this repository's item schema, and consumers use the `000X-NN` task id instead. Which does the work queue answer for?                                                                                                        | The owner, if the Could criterion is taken up                             |
| Does a person in an editor need anything an answer shaped for an agent lacks?                                                                                                                                                                        | The owner                                                                 |
| Listing the shipped schema versions (`0001-F7` B-041) writes nothing, yet it is grouped with the pin command as blocking only the Could writes. Is the list of shipped versions part of the schema knowledge the first Must criterion names?         | The owner. Raised while writing this file; it was not asked on 2026-10-09 |

## Placement

No Feature specification exists, so none is placed. Where each will sit
follows the project layout, which is the design's choice and is not made
here. Candidate A is a command and would sit in its command folder under
`src/tool/Features/`, as every command's specification does.

This epic file lives at `epics/0132-model-context-protocol/epic.md`, where
schema version 1's epic glob `epics/**/epic.md` discovers it (`0001-F6`
decision 0001).

## Status

`blocked`: the owner decided the epic waits on what it exposes. It unblocks
when the rule text and `expected` rows of "What this epic waits on" are
delivered. The next step does not wait for that: the owner confirms or
changes the candidate cut, and each confirmed Feature then begins with its
own specification.

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
