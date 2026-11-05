---
title: "ADR-0031: the MCP is its own program, and it runs the repository's pinned specht"
description: "The Model Context Protocol server is a C# program with its own project and its own NuGet package in this repository, released on the tool's one v<version> tag. It runs the specht the repository's local tool manifest pins and relays what the command returns; it is not a command inside specht.tool and does not host the engine in process. Supersedes ADR-0015 decision 3 only. Records the rejected alternatives with their cost and what would reverse it."
type: adr
---

# ADR-0031: the MCP is its own program, and it runs the repository's pinned specht

## Status

accepted - 2026-11-04, by the owner, in session. Recorded by spec-author.
Supersedes decision 3 of
[ADR-0015](0015-tool-mcp-host-and-action-share-one-repository-and-one-tag.md)
and nothing else in it: decisions 1 and 2 of ADR-0015 stand. The owner's words
on the premise this record replaces: "An MCP wraps a command line tool, doesn't
live inside one."

## Context

Read from the specifications of epic `0132` on 2026-11-04.

- **ADR-0015 decision 3** recorded a preference, accepted as one, that MCP is a
  command inside `specht.tool`, and `0132-F3` OQ-1 resolved on it
  (2026-11-01): `specht mcp`, calling the engine in process.
- **Epic `0132` § Constraints**: the tool's answers and MCP's answers can never
  come from different versions, and MCP adds nothing the command line cannot
  say (owner, 2026-10-09).
- **`0001-F2` C-5**: only `src/tool` packs. **`0001-F2` C-6**: a consumer's build
  calls the tool through a committed local tool manifest.
- **`version.json` and `VerifyTag`** give the repository one version line and one
  tag, `v<version>` (ADR-0015 decision 2).
- **Owner decisions of 2026-11-04**, in session:
  1. The MCP is an AI protocol entry into the common `specht` commands run
     against a specification tree: the focused API an agent uses instead of a
     terminal. Its users are any `specht` consumer repository; its first
     version serves two clients the owner named in session, and the major MCP
     clients follow.
  2. Read-only operations first; writes are staged in later.
  3. The MCP's answers always come from the repository's pinned `specht.tool`,
     the exact version in `.config/dotnet-tools.json`, which also honours the
     `schemaVersion` pin in `.specht/manifest.json` because the tool reads it.
     "Pinned means THIS version. Period."
  4. The MCP is its own program that wraps the command-line tool: it runs the
     repository's pinned `specht` and relays what the command returns.
  5. C#, its own project and its own NuGet package in this repository, on the
     tool's `v<version>` tag.
  6. Names: package `specht.mcp`, command `specht-mcp`, in line with
     `specht.tool` and `specht`. It installs the two ways `specht.tool` does: a
     local tool manifest, run as `dotnet specht-mcp`, and a global tool, run as
     `specht-mcp`. `dnx` is not a documented path.
  7. This version never restores or installs a tool; a future version may.
  8. A long-lived interactive mode of `specht`, to avoid a process start per
     call, was floated as an idea out of the air and is not a direction.
  9. Pinned tool: the MCP finds the pinned `specht` through the root's local
     tool manifest, the root from `--root` or the working directory. With no
     pin it falls back to a global `specht` and reports the answer as
     unpinned, in a part of the result apart from the command's text. With a
     pin that is not restored it fails with an error naming what is missing
     and where to read how to restore it, and falls back to nothing ("Pinned
     means THIS version. Period.", the reading confirmed by the owner). With
     no pin and no global `specht` it fails with an error naming both as not
     found.
  10. The MCP declares a minimum pinned-tool version and checks the tool's
      `--version` once per session; a tool below it fails each answer that
      needs it, with a message naming the minimum and where to read how to
      update: "we have to put a safety on the foot gun". The check applies to
      the global fallback as to the pinned tool, "one rule, no special case".
  11. Protocol: the revision current when the tool is minted, 2026-07-28, which
      has no initialize handshake, and nothing older. A later revision is a new
      item.
  12. The version the server states about itself is its own package's. The MCP
      and the tool can be updated apart in the manifest, so their versions can
      differ; the release stays one tag.
  13. Distribution is a .NET tool package for now. The MCP is not specific to
      .NET, and a distribution outside the .NET ecosystem may be needed as
      demand grows.
- **Future, not decided here**: the MCP may keep and mint keys for documents and
  claims, with logic and state of its own.

## Decision drivers

1. The answer is the command's output, true by construction rather than proved:
   epic `0132` § Constraints, `0132-F4` C-1.
2. The answer is the pinned tool's, at the version the repository chose, with no
   fallback (decision 3 above).
3. The MCP's lifecycle, protocol revisions and distribution can change without
   changing the tool's.
4. The repository's gates run on the MCP's code: the Roslyn analyzers,
   `Specht.CodeAnalysis`, `.editorconfig` and `dotnet format` read C# only.
5. One repository, one tag, one release workflow (ADR-0015 decisions 1 and 2).

## Considered options

1. **A program of its own that runs the pinned `specht` as a process.** Chosen.
2. **A `specht mcp` command inside `specht.tool`, calling the engine in
   process** (ADR-0015 decision 3; `0132-F3` OQ-1, 2026-11-01). Not chosen.
   Cost: the MCP is coupled to engine internals, so it moves with every engine
   refactor of epic `0001`'s staged rollout (ADR-0027); byte identity with the
   command's output is a property to prove for every answer rather than true by
   construction; the MCP cannot answer for a pin other than the one it ships
   with, so a repository pinning an older tool gets the newer tool's answers or
   none.
3. **Node or Python.** Not chosen. Cost: a second runtime and a second
   distribution to install, version and secure; no gain, since a consumer needs
   the .NET SDK for the pinned tool anyway.
4. **F#.** Not chosen. Cost: the repository's gates, namely the Roslyn
   analyzers, `Specht.CodeAnalysis`, `.editorconfig` and `dotnet format`, read C#
   only, so the MCP's code would sit outside every one of them.
5. **A self-contained binary or a container image.** Deferred, not ruled out.
   Cost: a distribution burden this repository does not carry for the tool: a
   build per platform, a signing and trust story, and a registry.
6. **A second project the tool references, hosting the MCP.** Not chosen by
   the owner, 2026-11-04: the MCP is a separate program that wraps the
   command-line tool, reached through neither a `specht` command nor a project
   reference. Cost: the tool's package would carry the MCP and its protocol
   dependencies, which is option 2's coupling by another route.
7. **A repository of its own.** Not reopened: ADR-0015 decisions 1 and 2 stand,
   one repository and one tag.

## Decision

1. **The MCP is a program of its own**, a C# project in this repository with its
   own NuGet package, packed as a .NET tool. It is not a command of
   `specht.tool` and does not reference the engine or the tool project.
2. **It runs the `specht` the repository's local tool manifest pins**, as a
   process, and relays what the command returns. With a pin that is not
   restored it runs no other `specht`. With no pin it runs a global `specht`
   and reports the answer as unpinned. It never installs or restores a tool
   in this version; a future version may, a recorded future and not part of
   this decision.
3. **It releases on the tool's `v<version>` tag** with no version line of its
   own: ADR-0015 decision 2, unchanged. Its package version and the pinned
   tool's version can differ, and the pinned tool's decides every answer. A
   minimum pinned-tool version, checked once per session, guards the
   difference.
4. **Reads first.** Writes through MCP are staged in later releases, each bound
   by its command's write rule (brief § 9).
5. **Distribution is a .NET tool package for now.** The MCP is not specific to
   .NET; a distribution outside the .NET ecosystem is a recorded future.
6. **The package is `specht.mcp` and the command `specht-mcp`**, installed
   through a local tool manifest or globally (`0132-F3` OQ-2, OQ-6). Either
   way the pinned `specht` is found through the root's local tool manifest
   (`0132-F3` OQ-7).
7. **The protocol revision is 2026-07-28 and nothing older**, which has no
   initialize handshake (`0132-F3` OQ-5).

## Consequences

Gained:

- Every answer is the pinned tool's own output by construction, for the
  repository's pinned version and schema pin.
- The MCP's protocol work, its tests and its release cadence are separable from
  the engine's.
- The MCP's code meets every C# gate of the repository.

Paid:

- A second packable project: `0001-F2` C-5 and AGENTS.md's one-packable-project
  statements no longer hold, and epic `0055`'s pack and release targets carry
  two packages on one tag.
- A consumer carries two pins, the MCP's and `specht.tool`'s, which can
  differ, and the same version is no longer guaranteed; the minimum
  pinned-tool version check is the safety (`0132-F3` OQ-8, OQ-9).
- Each call starts a process; its cost is not measured, and ADR-0014's benchmark gate applies to any change that claims to avoid it.
- A repository with a pin that is not restored gets an error and no answer
  from the MCP, and one with no pin gets an answer from a global tool, reported
  as unpinned, in a part of the result apart from the command's text; one with
  neither gets an error naming both as not found (`0132-F3` OQ-7, OQ-12).
- A client that does not use revision 2026-07-28 cannot use the MCP until it
  does or the owner adds an older revision; a legacy initialize
  request gets an error naming the supported revision (`0132-F3` OQ-11).
- A second distribution is a future cost if demand outside .NET arrives.

## Reversal

- **Distribution outside .NET.** If demand arrives from clients that cannot run
  a .NET tool package, a self-contained binary or a container image of the same
  program is added beside the package; the program and the rule that it runs
  the pinned `specht` do not change.
- **The process boundary.** Moving the MCP in process again needs a new record
  that names the engine surface it would depend on and the byte-identity proof
  it would then owe.
