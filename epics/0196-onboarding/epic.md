---
title: "Epic 0196: Guided onboarding into specification-driven development"
description: "A resumable journey that reads a repository's actual state, explains the next specification-driven-development step and its success signal, and guides a person or agent from an unconfigured repository to a first checked specification without replacing the product decisions they must make"
id: "0196"
type: epic
status: needs-decomposition
priority: med
milestone: null
children: []
created: "2026-10-12"
updated: "2026-10-12"
github_issue: null
---

# Epic 0196: Guided onboarding into specification-driven development

## Summary

`specht` can initialize its configuration and check a specification tree, but a
newcomer must already know the specification-driven-development journey that
connects those operations. They must know what to establish first, which
artifact comes next, when a draft is ready to check and what passing the check
does and does not prove. The tool validates each artifact after it exists; it
does not yet navigate someone toward the next one.

This epic gives a person or an agent one discoverable onboarding journey. The
journey derives its place from the repository, explains one next action and the
condition that completes it, and can be resumed without saved wizard state. It
begins before the root has a `.specht/manifest.json` and ends when the root has
a first checked Feature specification and the newcomer knows which artifact
continues the chain.

Onboarding orchestrates existing capabilities rather than owning them. `init`
still owns configuration, the check still owns the verdict, epic `0002` owns
reference documentation, epic `0193` owns JSON as a first-class product
surface, and epic `0194` owns Spectre CLI presentation. This epic owns the
journey, its states and the rule that selects the next action.

The need was stated by the repository owner on 2026-10-12: allow `specht` to
navigate someone into the specification-driven-development experience. The
success criteria and candidate cut below are a draft for grooming. No command
is named, no Feature is agreed and no working specification is created here.

## Business Value

**The problem.** Installation and validation are documented as separate
operations. A newcomer who does not already practice specification-driven
development can run both and still not know what to do first, what to write,
or whether an empty clean check means the repository is ready.

**The solved state.** Starting from a new or partially adopted repository, a
person or agent can ask `specht` where the repository is in the onboarding
journey, follow one concrete next action at a time, and reach a first checked
Feature specification without relying on this repository's chat history or
copied tribal knowledge.

| Persona                                            | Standing  | Goal                                                                                       |
| -------------------------------------------------- | --------- | ------------------------------------------------------------------------------------------ |
| A maintainer introducing SDD to a repository       | Primary   | Establish the model in a safe order and know when the repository is ready for Feature work |
| A person writing their first Feature specification | Primary   | Know the next artifact, why it exists and what proves the step complete                    |
| A coding agent in a consumer repository            | Secondary | Receive the same journey state and next action without guessing from prose                 |
| An experienced SDD practitioner                    | Secondary | Resume at the first incomplete step instead of being forced through introductory guidance  |

## Success criteria

| Criterion                                                                                                                                                                           | Tier  | Notes                                                                              |
| ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----- | ---------------------------------------------------------------------------------- |
| One discoverable entry point tells a newcomer the repository's current onboarding state, the next action, why that action comes next and the observable condition that completes it | Must  | The command name is open.                                                          |
| The journey works for a root with no `specht` configuration, a partially onboarded root and a root with existing specifications                                                     | Must  | Existing work is adopted in place; onboarding never requires an empty repository.  |
| Running the entry point again derives the same or later state from repository artifacts and requires no hidden progress file                                                        | Must  | The repository is the record.                                                      |
| The journey leads through configuration, the first Feature agreement and a check of that agreement, then names the next artifact in the specification-to-code chain                 | Must  | It does not claim that a passing structural check approves the product decision.   |
| Every structural instruction comes from the schema version the repository pins; before a manifest exists, the journey identifies the version `init` would pin                       | Must  | A stale, separately maintained checklist is not a source of schema facts.          |
| A person and an agent are given the same state, next action and completion condition                                                                                                | Must  | Presentation may differ. A structured form waits on the boundary with epic `0193`. |
| Onboarding is deterministic and offline except when it invokes an existing `init` path that is already permitted to fetch an upstream schema source                                 | Must  | The journey adds no network source of its own.                                     |
| A fixture starting with no configuration can be followed to a first specification that passes the pinned check, and a fixture starting midway resumes at its first incomplete step  | Must  | These are end-to-end proofs of the journey, not only of individual commands.       |
| The human-facing journey uses the richer tree, choice and colour presentation introduced by epic `0194`                                                                             | Could | The journey must remain usable before or without those widgets.                    |
| Onboarding can offer an experienced practitioner an abbreviated path that omits explanations while preserving the same state transitions                                            | Could | This is presentation over the same journey, not a second onboarding model.         |

## The onboarding journey

These are journey states, not yet Features or command names. A Feature cut may
join or split them when its invariant is written.

| State                 | Established fact                                                                   | Next outcome                                                                              |
| --------------------- | ---------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| Not configured        | The root has no `.specht/manifest.json`                                            | The root pins a schema version through the existing `init` contract                       |
| Contract available    | The manifest is valid and the pinned schema's generation contract is available     | The newcomer has a decided need and starts the Feature agreement from the pinned contract |
| Agreement in progress | A Feature specification and its companion scenario file exist but do not pass      | Each reported violation is repaired until the pinned check is clean                       |
| Agreement checkable   | The first Feature specification passes the structural check                        | The newcomer is told that stakeholder agreement still precedes work-item cutting          |
| Onboarded             | The first Feature agreement is checked and the next artifact in the chain is known | Normal specification-driven delivery continues outside the onboarding journey             |

The state names are provisional. In particular, "Agreement checkable" does
not mean `spec_status: approved`; a structural verdict cannot stand in for a
stakeholder or reviewer.

## Candidate Features

**A candidate cut, awaiting the owner's confirmation.** No candidate has a
Feature id, specification or place in `children`. A candidate takes its id
when its specification is written.

| Candidate | Name                    | Invariant it would carry                                                                                                                |
| --------- | ----------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| A         | Onboarding state        | The repository's observable artifacts determine exactly one journey state and the completion condition for its next transition          |
| B         | Next-action guidance    | Every state gives one actionable next step, its reason and its success signal from versioned sources rather than a duplicated checklist |
| C         | First Feature journey   | A newcomer is guided from a decided need to a first checked Feature agreement without `specht` supplying the product decision           |
| D         | Agent-facing onboarding | The state, next action and completion condition available to an agent are the same facts the human-facing journey presents              |

Why this cut is plausible:

- Candidate A can be correct while candidate B explains the wrong next action;
  state recognition and guidance can fail independently.
- Candidate C is cut from B because it owns a specific SDD boundary: product
  intent becomes a Feature agreement. It must preserve the distinction between
  structural validity and stakeholder approval.
- Candidate D is cut from B because structured output is a public contract and
  waits on epic `0193`; terminal prose does not.
- `init` is not a candidate. Candidate B may direct or invoke it, but
  `0001-F4` continues to own every file it writes and every refusal it makes.
- No candidate owns terminal widgets. Epic `0194` may present this journey,
  but changing the presentation must not change its states or next actions.
- The AGENTS.md `&` heuristic found no conjunction in a candidate name.

## What this epic waits on

None of these dependencies prevents grooming or writing candidate A's
specification. They constrain which end-to-end paths can be delivered.

| Capability                                            | Owned by                              | Why it matters                                                                                      |
| ----------------------------------------------------- | ------------------------------------- | --------------------------------------------------------------------------------------------------- |
| Initialize the manifest and optional schema/templates | `0001-F4`                             | The first transition reuses `init`; onboarding must not implement a second initializer              |
| Check the root and explain every finding              | `0001-F2`, `0001-F3`                  | The first-Feature journey ends in the check's verdict and repairs from its own explanation          |
| JSON as a first-class product surface                 | Epic `0193`                           | Candidate D needs one structured representation of journey state and next action                    |
| Spectre CLI widgets                                   | Epic `0194`                           | The human journey may gain choices, a tree and colour without coupling the journey to those widgets |
| Public method guidance                                | Epic `0002`, or this epic's open OQ-5 | One owner must hold the prose that explains SDD concepts rather than schema structure               |

## Constraints

| Constraint                                                                                                                   | Source                                         | What it rules out                                                                              |
| ---------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------- | ---------------------------------------------------------------------------------------------- |
| The specification remains the artifact of record; onboarding does not treat a conversation or wizard answer as the agreement | AGENTS.md § Specification-Driven Development   | Hidden product decisions or an onboarding transcript standing in for the Feature specification |
| A Feature begins with its specification and no work item exists until the agreement is accepted                              | AGENTS.md § The artifact chain                 | Creating implementation work before the first Feature agreement                                |
| A passing check proves conformance to the pinned schema, not stakeholder approval or delivery readiness                      | `specht-conventions`, specifications reference | Calling a clean structural verdict an approved Feature                                         |
| The repository's artifacts are the only progress record                                                                      | Owner's navigation need; success criteria      | An account, telemetry record, onboarding database or opaque local state needed to resume       |
| Structural guidance is versioned with the schema it describes                                                                | brief § 2 Must-2 and Must-6                    | Guidance that silently describes the newest schema while the repository pins an older one      |
| Every path in output is relative to the root                                                                                 | brief § 9; AGENTS.md § Invariants              | An onboarding instruction or diagnostic containing an absolute path                            |
| The journey is deterministic and offline except for an existing permitted `init` fetch                                       | AGENTS.md § Invariants                         | Telemetry, a hosted onboarding service or network-fetched guidance                             |
| Existing files are preserved unless a separately agreed Feature explicitly adds a write to the repository's invariant        | AGENTS.md § Invariants                         | Assuming that onboarding may scaffold or edit a working specification before OQ-1 is resolved  |
| Existing commands retain ownership of their behavior                                                                         | `0001-F2`, `0001-F4`; epic boundaries          | A second initializer, checker, rule explainer or formatter implemented inside onboarding       |

## Open questions

Each answer is a product decision and lands first in the Feature specification
it binds. None is resolved by this draft.

| OQ   | Question                                                                                                                                                       | Why it changes the product                                                                                                                             | Who answers |
| ---- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------- |
| OQ-1 | Does onboarding only navigate, or may it create and amend working SDD artifacts such as the first Feature specification and scenario file?                     | Writing them adds a new allowed write and requires overwrite, partial-file and recovery semantics; navigation alone preserves today's write invariant. | The owner   |
| OQ-2 | Is the onboarding entry point a new command, a mode of `init`, or guidance integrated across existing commands?                                                | The answer changes discoverability, help, invocation and which command owns orchestration.                                                             | The owner   |
| OQ-3 | Is the primary human journey interactive, sequential output, or both?                                                                                          | Interaction implies prompts and resumable choices; sequential output works in every terminal and composes with automation.                             | The owner   |
| OQ-4 | Does "onboarded" stop at the first checked Feature agreement, or also require a repeatable build or hook call site?                                            | The latter expands the journey into provider- and build-specific configuration that the tool does not own today.                                       | The owner   |
| OQ-5 | Does `specht` ship method guidance about how to make product decisions and write claims, or only schema facts and links to documentation that owns the method? | Method guidance is not part of today's versioned schema contract and needs one durable owner if it is shipped.                                         | The owner   |
| OQ-6 | Before a manifest exists, does onboarding always guide toward the newest shipped schema, or may the newcomer select a shipped version before `init`?           | Selection introduces a decision before the repository has a pin and overlaps the schema-list/pin experience.                                           | The owner   |
| OQ-7 | How does onboarding recognize a repository that already practices SDD but has not adopted `specht`?                                                            | Treating it as empty loses useful progress; inferring from arbitrary files risks claiming a state that the tool cannot prove.                          | The design  |
| OQ-8 | Which first Feature is used to prove the end-to-end journey: a fixed synthetic example, the newcomer's real decided need, or a disposable tutorial?            | A synthetic journey is reproducible but does not onboard the real repository; a real need cannot be supplied or judged by the tool.                    | The owner   |

## Placement

No Feature specification exists, so no candidate has a final placement.
Candidate A belongs with the engine model that reads the tree. Candidates B
and C belong with the command that OQ-2 names. Candidate D belongs beside the
structured surface that epic `0193` defines. Each specification is co-located
with the code it will govern once that location is decided.

This epic file lives at `epics/0196-onboarding/epic.md`, where schema version
1's epic glob discovers it (`0001-F6` decision 0001).

## Status

`needs-decomposition`: the need and boundary are recorded, the journey is a
candidate, and no child Feature exists. The next decision is OQ-1 because it
sets the safety boundary for every later candidate: navigation-only preserves
the current write invariant; artifact creation changes it. The remaining
questions can then be resolved while each candidate specification is written.

## Out of this epic

| Item                                                                                     | Where it lives instead                                                                  |
| ---------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------- |
| Deciding the newcomer's product need, claims or constraints                              | The newcomer and stakeholders; onboarding preserves their decision in the specification |
| Approving the first Feature agreement                                                    | The stakeholder and spec-reviewer; the check proves structure, not agreement            |
| Initializing the manifest, schema files or templates                                     | `0001-F4`; onboarding may direct or invoke `init` without duplicating its contract      |
| Defining or reporting the check verdict                                                  | `0001-F2` and `0001-F3`                                                                 |
| Generic trees, drop-downs, tables, warning colours and error colours                     | Epic `0194`; they are presentation available to onboarding                              |
| Making JSON the primary representation of schemas or commands                            | Epic `0193`; onboarding consumes that product boundary once defined                     |
| General reference documentation for installing, configuring and using `specht`           | Epic `0002`; onboarding may link to it                                                  |
| Implementing production code, tests or working Feature specifications in this epic draft | Later Feature specifications and their work items, after the epic is groomed            |
