---
title: "ADR-0032: specht-mcp is built on the official C# MCP SDK"
description: "The MCP program, src/mcp and package specht.mcp, speaks its protocol through the official C# SDK, NuGet ModelContextProtocol 2.x over stdio only, not through a JSON-RPC layer of its own. Refines how ADR-0031's program is built and supersedes nothing. Records the rejected hand-rolled alternative with its cost and the checks the implementer makes against the committed 0132-F3 tests."
type: adr
---

# ADR-0032: specht-mcp is built on the official C# MCP SDK

## Status

accepted - 2026-11-04, by the owner, in session. Recorded by spec-author.
Refines [ADR-0031](0031-the-mcp-is-its-own-program-that-runs-the-repositorys-pinned-specht.md),
which stays whole: that record says where the MCP lives and what it runs, this
one says how its protocol layer is built. It supersedes nothing.

## Context

Read from `0132-F3` on 2026-11-04.

- **ADR-0031 decision 7 and `0132-F3` OQ-5**: protocol revision 2026-07-28 and
  nothing older, which has no initialize handshake.
- **The owner's ruling, 2026-11-04**: the MCP uses the official C# MCP SDK. C#
  was chosen partly for it.
- **The SDK**: NuGet `ModelContextProtocol`. Version 2.0 implements revision
  2026-07-28: no initialize handshake, `server/discover`, and the protocol
  version in each request's `_meta`. Source: Microsoft's announcement of the 2.0
  release,
  <https://devblogs.microsoft.com/dotnet/announcing-v20-of-the-official-mcp-csharp-sdk/>.
  That is secondary reporting; this repository has not read the SDK's own
  source or release notes, and the checks below are what settle it.
- **Committed tests** for `0132-F3` B-003 to B-006, B-019, B-032 and B-033 fix
  what the program must do on the wire, whoever writes the protocol layer.

## Decision drivers

1. The protocol layer is maintained by the protocol's own stewards, not by this
   repository.
2. A later protocol revision is a package update, not a rewrite of framing and
   errors.
3. The program stays small: its own code is pin resolution, running the tool and
   relaying (`0132-F3` B-017 to B-030).
4. B-003, B-004 and B-005 are absences and wire facts a dependency can break;
   each stays an explicit check, not an assumption.

## Considered options

1. **The official SDK, `ModelContextProtocol` 2.x, stdio transport only.**
   Chosen.
2. **A hand-rolled JSON-RPC layer on `System.Text.Json`.** Not chosen. Cost:
   this repository owns protocol revision tracking, message framing, error
   codes and capability shape, and every later revision (`0132-F3` § 5 row 18)
   is work it does itself. It was considered on an assumption that the SDK
   lagged the revision; the assumption was not verified and is wrong, per the
   announcement cited above.
3. **`ModelContextProtocol.AspNetCore`.** Not chosen. Cost: an HTTP host and a
   listening port, which B-003 rules out; a network transport is out
   of scope (`0132-F3` § 5 row 4, C-1).

## Decision

1. **`specht-mcp` is built on the official C# MCP SDK**, NuGet
   `ModelContextProtocol`, major version 2, and no hand-rolled JSON-RPC layer
   stands beside it.
2. **The transport is stdio only.** `ModelContextProtocol.AspNetCore` and any
   HTTP or other transport are not referenced.
3. **The SDK implements the protocol; the program implements the claims.**
   Pin resolution, running the pinned `specht` and relaying its output remain
   the program's own code, bound by ADR-0031 and `0132-F3`.

## Consequences

Gained:

- Revision tracking, framing and protocol errors belong to the SDK.
- A later revision is a dependency update and a new item (`0132-F3` A-6).

Paid:

- A dependency on a package this repository does not control, with its own
  transitive footprint.
- The program's behaviour on the wire is the SDK's where the claims do not
  override it.

**The implementer verifies each of these against the committed `0132-F3` tests
and records the result in the Feature's § 6 or § 7 (implementer-owned).** If any
fails, the implementer stops and the owner decides: no workaround, no fallback
to a hand-rolled layer and no test change is taken on the implementer's
authority.

1. **B-032.** A legacy `initialize` request is answered with an error naming
   revision 2026-07-28.
2. **B-019 and B-033.** A request naming an unsupported revision is answered
   with code `-32022`, and `data.supported` lists the supported revisions and
   equals the discovery answer's `supportedVersions`.
3. **B-005.** Standard output carries only protocol messages. SDK logging, host
   builder defaults and any console provider write to standard error or
   nowhere, never to standard output.
4. **B-003 and B-004.** The process holds no listening socket and makes no
   outbound call, and `specht.mcp`'s own assembly references no networking
   assembly; the packages the SDK brings in are read for the same.
5. **B-006.** The server version stated in discovery is the informational
   version of the `specht.mcp` package, not the SDK's.
6. **Footprint.** The SDK's transitive package set is read and listed in the
   pull request; the owner accepts it or rules otherwise.

## Reversal

If a check above fails and the owner rules against the SDK, a new record
supersedes this one and names the replacement layer, and the claims and tests
stay as they are: they bind the wire, not the library.
