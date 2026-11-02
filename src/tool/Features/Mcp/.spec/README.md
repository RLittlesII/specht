---
title: "Specification: The MCP host"
description: "specht mcp is the tool's own Model Context Protocol server, started by the client from one configuration entry that docs/mcp.md gives: a command inside the specht.tool package that speaks over its standard input and output, opens no port, makes no outbound call, writes nothing and states the tool's own version, serving the answers 0132-F4 defines"
type: feature
id: "F3"
epic: "0132"
spec_status: approved
status: ready
priority: med
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Model Context Protocol"
author: "spec-author"
milestone: null
children: []
depends_on: ["0001/F2"]
blocks: ["F4"]
spikes: []
created: "2026-10-31"
updated: "2026-11-01"
github_issue: null
synced_at: null
---

# Specification: The MCP host

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-31 -->

An agent or an editor's assistant in a repository that pins `specht` reaches the tool only through a terminal session and output it scrapes, and nothing ties an MCP server to the tool version the repository pinned. This Feature removes that failure state: one client configuration entry starts `specht`'s own MCP server, a local process from the same package and version as the command, which opens no port, calls nothing outside the machine and writes nothing, so that every answer `0132-F4` serves comes from the tool the repository pinned.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-31, 2026-11-01 (A-1 resolved with OQ-2) -->

| #   | Persona                                 | Need                                                         | Pain Point Today                                       |
| --- | --------------------------------------- | ------------------------------------------------------------ | ------------------------------------------------------ |
| 1   | A coding agent in a consumer repository | Reach `specht` through its MCP client, with no terminal      | Runs the command and scrapes its output                |
| 2   | A maintainer configuring a client       | One entry that starts the server for the pinned tool version | No server exists, and nothing would tie one to the pin |
| 3   | A person in an editor or a chat         | The same server their assistant reaches                      | Asks in a terminal, or not at all                      |
| 4   | `0132-F4`, reading through MCP          | A host that serves its answers                               | N/A (internal dependency; no host exists)              |

### Assumptions

| ID  | Assumption                                                                                                                                         |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | Resolved 2026-11-01 by the owner (OQ-2): the command is `specht mcp`. Was: `specht mcp` is a working name (spec-author's reading).                 |
| A-2 | The root, the manifest and the missing-input exit code `2` are the check's; this Feature depends on `0001-F2`.                                     |
| A-3 | What the host serves, and the shape of each answer, is `0132-F4`'s; this Feature serves what `0132-F4` registers and defines no answer of its own. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-31, 2026-11-01 (B-015 and B-016 added; sources of B-001, B-007, B-011 and B-013 updated for OQ-1, OQ-2 and OQ-4) -->

| ID    | Claim                                                                                                                                                                                                  | Source                                                                                                                                | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------- | ------ |
| B-001 | Given a client configuration entry that runs `dotnet specht mcp` in a repository whose local tool manifest lists `specht.tool`, this Feature starts and completes the protocol's initialize handshake. | epic `0132` candidate B and its sixth Must criterion; `0001-F2` C-6; the name is OQ-2, resolved 2026-11-01                            | Active |
| B-002 | Given a session, this Feature exchanges protocol messages over its standard input and output only.                                                                                                     | epic `0132` § Constraints; spec-author's reading: of the protocol's standard transports, only standard input and output opens no port | Active |
| B-003 | Given a session, this Feature opens no listening network port.                                                                                                                                         | epic `0132` § Constraints; AGENTS.md § Invariants                                                                                     | Active |
| B-004 | Given a session, this Feature makes no outbound network call.                                                                                                                                          | epic `0132` § Constraints; AGENTS.md § Invariants                                                                                     | Active |
| B-005 | Given a session, everything this Feature writes on stdout is a protocol message.                                                                                                                       | spec-author's reading of the protocol's standard input and output transport; as `0001-F2` C-3                                         | Active |
| B-006 | Given the initialize handshake, the server version this Feature states is the informational version `specht --version` prints from the same installation.                                              | epic `0132` § Constraints (one version); `0001-F2` B-040                                                                              | Active |
| B-007 | Given `specht.tool` installed and no other `specht` package, this Feature starts and completes the initialize handshake.                                                                               | ADR-0015 decision 3; `0001-F2` C-5; OQ-1, resolved 2026-11-01                                                                         | Active |
| B-008 | Given a session from start to end, no file under the root or elsewhere is created, modified or deleted.                                                                                                | brief § 9; epic `0132` § Constraints                                                                                                  | Active |
| B-009 | Given `--root <dir>`, this Feature answers for that directory, and with no `--root`, for the working directory the client starts it in, as the check does.                                             | `0001-F2` B-001; OQ-3                                                                                                                 | Active |
| B-010 | Given `--root` naming a path that is not a directory, this Feature writes a message naming that path as given on stderr and exits `2` before any session starts.                                       | `0001-F2` B-005                                                                                                                       | Active |
| B-011 | Given a root with no manifest, or with a manifest the check rejects, this Feature still completes the initialize handshake.                                                                            | the owner, 2026-11-01 (OQ-4); was spec-author's reading                                                                               | Active |
| B-012 | Given the client closes this Feature's standard input, this Feature exits `0`.                                                                                                                         | spec-author's reading of the protocol's standard input and output shutdown                                                            | Active |
| B-013 | Given `specht mcp --help`, this Feature prints usage naming its options and exits `0`.                                                                                                                 | `0001-F2` B-008; `spectre-cli` § Help; the name is OQ-2, resolved 2026-11-01                                                          | Active |
| B-014 | Given an initialize request naming a protocol revision this Feature supports it answers with that revision, and given one it does not support, with the newest it supports.                            | the protocol's version negotiation; spec-author's reading; OQ-5                                                                       | Active |
| B-015 | Given the host, this Feature documents in `docs/mcp.md` a client configuration entry that runs `dotnet specht mcp --root <dir>`.                                                                       | the owner, 2026-11-01; B-001, B-009; C-8                                                                                              | Active |
| B-016 | Given the tools the host offers, the tool entries in `docs/mcp.md` name exactly those tools, one entry each.                                                                                           | the owner, 2026-11-01; held to the host as `docs/cli.md` is held to the help (`0002-F1` C-7); C-8                                     | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-31, 2026-11-01 (C-8 added) -->

| ID  | Constraint                                                                                                                                                         | Rules Out                                                                                                                                                              |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | The host is a local process the client starts; it opens no port and makes no outbound call (epic `0132` § Constraints; AGENTS.md § Invariants).                    | A hosted or remote service; any network transport, one on a local port included; a fetch from a session, `init`'s and `upgrade`'s included.                            |
| C-2 | The host's answers and the command's can never come from different versions (owner, 2026-10-09; epic `0132` § Constraints).                                        | A host installed, upgraded or pinned apart from `specht.tool`; a host that runs whatever `specht` the path or another tool manifest resolves.                          |
| C-3 | The host writes nothing, and every path it gives is relative to the root with `/` separators (brief § 9).                                                          | A log, cache or lock file; an absolute path in a protocol message or on stderr.                                                                                        |
| C-4 | The host is composed by the one factory that composes the command-line host (ADR-0007).                                                                            | A second composition root; a registration replaced for the host.                                                                                                       |
| C-5 | The host defines no answer; every answer it serves is `0132-F4`'s (epic `0132` § Features, candidates B and C).                                                    | A rule, tree-fact or verdict answer specified or built in this Feature.                                                                                                |
| C-6 | The host does not waive ADR-0014's benchmark gate.                                                                                                                 | Adopting ADR-0010's target architecture, or a mediator, as a side effect of the host.                                                                                  |
| C-7 | The command is listed in `docs/cli.md`, which a test holds to the help (`0002-F1`; AGENTS.md § CLI).                                                               | A command missing from `docs/cli.md`; a second command-line reference page.                                                                                            |
| C-8 | `docs/mcp.md` is the one MCP page: the client configuration entry and the tools the host offers; the command's options stay in `docs/cli.md` (C-7; `0002-F1` C-6). | A second page listing the command's options; MCP configuration in `docs/cli.md`; an entry for a tool the host does not offer, or a planned marker standing in for one. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-31, 2026-11-01 (row 5 resolved with OQ-1; row 10 added) -->

| #   | Item                                                                          | Exclusion Reason                                                                                                                                    |
| --- | ----------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Every answer served: the verdict, rule and schema knowledge, the tree's facts | `0132-F4`; C-5.                                                                                                                                     |
| 2   | A write through MCP: `init`, `upgrade`, the pin and set commands, `format`    | Epic `0132` candidate D, deferred, 2026-10-30.                                                                                                      |
| 3   | The work queue                                                                | Epic `0132` candidate E, parked, 2026-10-30.                                                                                                        |
| 4   | A network transport, one on a local port included                             | Never: epic `0132` § Out of this epic; C-1.                                                                                                         |
| 5   | A package or an install of its own for the host                               | Resolved 2026-11-01 by the owner (OQ-1): the host is a command inside `specht.tool`; B-007. Was: not decided (OQ-1); B-007 proceeds on the default. |
| 6   | Writing a client's configuration file                                         | Not asked; brief § 9's list of writes is unchanged, and `init`'s writes are `0001-F4`'s.                                                            |
| 7   | One host shared between clients, users or machines                            | Epic `0324`, the service mode, moved 2026-10-30.                                                                                                    |
| 8   | A persistent index or cache behind the host                                   | `0132-F1`; C-3.                                                                                                                                     |
| 9   | A caller with no .NET SDK                                                     | Epic `0132` § Out of this epic: there is no second distribution.                                                                                    |
| 10  | The shape of a tool's answer, restated on `docs/mcp.md`                       | Each answer is its command's output (`0132-F4` C-1), documented where that command is; C-5.                                                         |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-31 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-31 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-31 -->

Pending: owned by `test-writer`, written after agreement.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-31; test-writer, 2026-11-01 (B-015 and B-016 rows added) -->

| Claim ID | Scenario                                                    | Test    | Status  |
| -------- | ----------------------------------------------------------- | ------- | ------- |
| B-001    | One configuration entry starts the server                   | Missing | Missing |
| B-002    | The server speaks over its standard input and output        | Missing | Missing |
| B-003    | The server opens no network port                            | Missing | Missing |
| B-004    | The server calls nothing outside the machine                | Missing | Missing |
| B-005    | The standard output carries only protocol messages          | Missing | Missing |
| B-006    | The server states the tool's own version                    | Missing | Missing |
| B-007    | The server needs no package beyond the tool                 | Missing | Missing |
| B-008    | A session writes nothing                                    | Missing | Missing |
| B-009    | The server answers for the root the check would take        | Missing | Missing |
| B-010    | A root that is not a directory stops the server             | Missing | Missing |
| B-011    | A repository without a usable manifest still gets a session | Missing | Missing |
| B-012    | Closing the server's input ends it cleanly                  | Missing | Missing |
| B-013    | Help names the server command's options                     | Missing | Missing |
| B-014    | The server agrees a protocol revision                       | Missing | Missing |
| B-015    | The MCP page gives the configuration entry                  | Missing | Missing |
| B-016    | The MCP page documents exactly the tools the server offers  | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-31 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-31, 2026-11-01 (OQ-1, OQ-2 and OQ-4 resolved by the owner) -->

| ID   | Question                                                                                                                                                                                                      | Blocks           | Resolution                                                                                                                                                                                                                                                                                                                                                                            |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Is the host delivered in the `specht.tool` package or in a package of its own? It touches epic `0055`.                                                                                                        | B-007; § 5 row 5 | Resolved 2026-11-01 by the owner: a command inside the `specht.tool` package, ADR-0015 decision 3's preference. Was: open, left to the design by the owner, 2026-10-30 (epic `0132` § Open questions); proceeding on a command inside `specht.tool`, which meets C-2 by construction and keeps `0001-F2` C-5, with a second packable project built from the same tag as the fallback. |
| OQ-2 | What is the server command's name, and so the configuration entry a client carries?                                                                                                                           | B-001, B-013     | Resolved 2026-11-01 by the owner: `specht mcp`, run as `dotnet specht mcp` through the local tool manifest. Was: open, proceeding on that name; owner not asked.                                                                                                                                                                                                                      |
| OQ-3 | Does the host take its root from the workspace roots a client can send through the protocol, or only from `--root` and the working directory? Clients differ in the working directory they start a server in. | B-009            | Open. Proceeds on `--root` and the working directory only, as the check takes them. Owner not asked.                                                                                                                                                                                                                                                                                  |
| OQ-4 | Given a root with no manifest or one the check rejects, does the host refuse to start, exiting `2` or `3` as the check does, or start and fail each answer that needs the manifest?                           | B-011            | Resolved 2026-11-01 by the owner: the host starts, and each answer that needs the manifest fails, as B-011 says and `0132-F4` B-012 gives the error. Was: open, proceeding on starting, so that a client shows each answer's error rather than a server that failed to start; owner not asked.                                                                                        |
| OQ-5 | Which protocol revisions does the host support?                                                                                                                                                               | B-014            | Open. Proceeds on the revisions the clients `0132-F4` OQ-1 names need; until it is answered, the newest revision published when the first item is cut. Owner not asked.                                                                                                                                                                                                               |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-11-01 (round 2; round 1 moved to Review history) -->

| Section   | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
| --------- | ------ | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1-5, 9-11 | 🟢     | spec-reviewer | Round 2 (2026-11-01) - approved, reviewing the uncommitted working tree on `a189f53d`. The round-1 finding is closed: the B-011 Outline now removes the Background's manifest or replaces it with one the check rejects. § 9 has one row per claim and every tag resolves; the `@boundary` scenarios match § 5 row 4 and C-1; B-015, B-016 and C-8 agree with `0002-F1` C-6 and C-7; OQ-1, OQ-2 and OQ-4 are resolved in place with what they replaced; OQ-3 and OQ-5 stay open on recorded defaults, which do not block agreement. No claim depends on a `0132-F4` claim deferred past milestone 0.5.0. §§ 6-8 are pending after agreement and are reviewed when written; § 9 `Missing` rows block items, not this agreement. |

### Review history

| Section   | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| --------- | ------ | ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5, 9-11 | 🟡     | spec-reviewer | Round 1 (2026-11-01) - draft, reviewing the uncommitted working tree on `a189f53d`. One finding, routed to `spec-author`: the B-011 Outline's `Given the repository's manifest <state>` ("is missing", "is one the check rejects") contradicts the Background's `And a manifest the check accepts`; phrase it as a change to the Background's tree, as `0132-F4` B-012 does ("is removed", "is replaced by one the check rejects"). Otherwise sound: § 9 has one row per claim and every tag resolves; B-015, B-016 and C-8 agree with `0002-F1` C-6 and C-7; OQ-1, OQ-2 and OQ-4 are resolved in place with what they replaced; OQ-3 and OQ-5 stay open on recorded defaults, which do not block agreement. No claim depends on a `0132-F4` claim deferred past milestone 0.5.0. |

## Tasks

None yet. Cut from § 3 after agreement, together with `0132-F4`'s, so that no item here is done while the host serves nothing (owner, 2026-10-30; epic `0132` § Features).

## Scoring

| Field | Value | Basis                                    |
| ----- | ----- | ---------------------------------------- |
| value | 0     | Not scored while the agreement is draft. |
| risk  | 0     | Not scored while the agreement is draft. |
