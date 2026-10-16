---
title: "ADR-0015: the tool, the MCP host and the action share one repository and one tag"
description: "The specht.tool package, the Model Context Protocol host of epic 0132 and the GitHub Action of epic 0142 live in this repository and release on one v<version> tag. A separate repository for either surface would need an engine package, which 0001-F2 decision 0001 rejected, or would answer from a tool version it does not ship with, which epic 0132 forbids. Records a preference, not a decision, for MCP as a command inside specht.tool; epic 0132's packaging question stays open. A Marketplace listing is the named reversal for the action."
type: adr
---

# ADR-0015: the tool, the MCP host and the action share one repository and one tag

## Status

accepted - 2026-10-15, by the owner, in session: "To be clear we are going
with the Recommendation". Recorded by spec-author. Epics `0132` and `0142`
each place their surface here in separate answers; this record states the
rule they share and what would reverse it. Decision 3 is a preference the
owner accepted as such; epic `0132`'s packaging question stays open. No
Feature specification exists for either epic.

## Context

Read from commit `b0431e98` when this record was written.

- **Brief § 3** holds two hard constraints: own repository, tool and schema
  together; consumers install the tool, and no engine code lands in a
  consumer repository.
- **`0001-F2` decision 0001** keeps the engine `specht` a library with no entry
  point, shipped only inside `specht.tool`, and rejects "a NuGet library
  consumers wire themselves": every adopter writes the same host slightly
  differently and the exit codes stop being one contract.
- **Epic `0132` § Constraints**: the tool's answers and MCP's answers can never
  come from different versions, and MCP adds nothing the command line cannot
  say (owner, 2026-10-09). § Personas and § Out of this epic: there is no
  second distribution. § Open questions: whether MCP ships in `specht.tool` or
  a package of its own is the design's, under the same-version constraint, and
  touches epic `0055`.
- **Epic `0142` § Success criteria**: R1, this repository's CI adopts the
  action and retires `AnnotateChangedFiles`; R4, the action installs nothing of
  its own and restores the tool from the consumer's local tool manifest; R6,
  it lives in a subfolder used as `uses: RLittlesII/specht/<folder>@<ref>`;
  R8, one `v<version>` tag covers the tool and the action (owner, 2026-10-10).
- **The release has one version line.** `version.json` gives
  Nerdbank.GitVersioning the tag `v{version}`; `VerifyTag` in
  `.build/SpechtBuild.Release.cs` fails a `v*` tag that is not the version
  computed for its commit, or that `main` does not reach. Tags `v0.1.90` and
  `v0.1.131` exist.
- **`specht-conventions` questions reference** § Already settled lists one
  repository for tool and schema, the command line as the contract of record,
  and the action shipped from this repository sharing the tool's tag.

## Decision drivers

1. One version answers: the tool, MCP and the action can never disagree about
   which tool produced an answer.
2. No consumer and no sibling repository carries the engine (brief § 3).
3. One release: one tag, one version line, one release workflow.

## Considered options

1. **One repository, one tag.** Chosen.
2. **The action in a repository of its own.** Not chosen: R6 and R8 are the
   owner's answers against it. The action would need its own tags and a rule
   for which tool tag each one matches, and R1's adoption by this repository's
   CI would cross a repository boundary.
3. **The MCP host in a repository of its own.** Not chosen: the host must
   reach the engine. Either it references an engine package, which `0001-F2`
   decision 0001 rejected, or it runs the installed `specht` command and
   answers for whatever tool version a consumer pinned, which epic `0132`'s
   same-version constraint forbids.
4. **Both in repositories of their own.** Not chosen: options 2 and 3 together,
   with three version lines and three release workflows.

## Decision

1. **`specht.tool`, the MCP host and the GitHub Action live in this
   repository.** The action is a subfolder (R6); its name is epic `0142` OQ-1.
2. **One `v<version>` tag releases all three.** `VerifyTag` stays the one gate;
   no surface has a version line of its own.
3. **Preference, not decision: MCP is a command inside `specht.tool`.** It
   meets the same-version constraint by construction, since one package holds
   both. The fallback is a second packable project built from the same tag,
   which reverses `IsPackable=false` for one more project and touches epic
   `0055`. Epic `0132`'s packaging question stays open for its Feature
   specification.

## Consequences

Gained:

- A consumer pins one tool version in its local tool manifest, and the action
  and MCP answer for that version.
- One release workflow, one version computation, one changelog.

Paid:

- The action's files sit beside the tool's and are tagged with every tool
  release, including one that changes nothing in the action.
- A GitHub Marketplace listing is not available: it needs a public repository
  holding one action with `action.yml` at its root, which a subfolder cannot
  be.

## Reversal

- **The action, for a Marketplace listing.** A thin mirror repository whose
  root `action.yml` delegates to `RLittlesII/specht/.github/actions/specht@<tag>`
  (or the folder OQ-1 names), its tags kept in step with this repository's. No
  epic asks for a listing today.
- **MCP, only if** the owner drops the same-version constraint, or MCP must
  ship outside NuGet.
