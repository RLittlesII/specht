---
title: "ADR-0034: the MCP supports the SDK's handshake revisions beside 2026-07-28"
description: "The MCP host stops pinning the SDK to protocol revision 2026-07-28 alone and supports the revisions the SDK negotiates through the initialize handshake, 2024-11-05, 2025-03-26, 2025-06-18 and 2025-11-25, beside 2026-07-28. Supersedes decision 7 of ADR-0031 only. Proposed pending the owner's ratification, 0132-F3 OQ-13."
type: adr
---

# ADR-0034: the MCP supports the SDK's handshake revisions beside 2026-07-28

## Status

proposed - 2026-11-08, by spec-author, from bug item 0425. The owner has not
ruled; `0132-F3` OQ-13 asks. Supersedes decision 7 of
[ADR-0031](0031-the-mcp-is-its-own-program-that-runs-the-repositorys-pinned-specht.md)
and its consequence on clients, and nothing else: ADR-0031 and
[ADR-0032](0032-specht-mcp-is-built-on-the-official-csharp-mcp-sdk.md) are
accepted records and are not edited.

## Context

Observed 2026-11-08 on `specht-mcp` built on `ModelContextProtocol` 2.2.0.

- The host set the SDK's protocol version to 2026-07-28. Every `initialize`,
  whatever revision it named, got error `-32022` ("not available through the
  initialize handshake") with `supported` empty, and the client closed the
  connection: Claude Code reported `specht (CONNECTION_CLOSED)`.
- With the setting removed, the SDK negotiates `initialize` for 2024-11-05,
  2025-03-26, 2025-06-18 and 2025-11-25; an unknown older revision is answered
  with 2025-11-25. An `initialize` naming 2026-07-28, or a later revision, is
  refused: 2026-07-28 has no handshake and is discovered through
  `server/discover`.
- ADR-0031 decision 7 and `0132-F3` OQ-5 chose 2026-07-28 alone and said a
  client that does not use it "is sorted out as a bug". This is that bug.

## Decision

1. The host does not restrict the SDK to one revision. It supports 2026-07-28
   through discovery and each handshake revision the SDK negotiates.
2. An `initialize` naming 2026-07-28 is still an error naming that revision
   (`0132-F3` B-032), as is one naming a later revision (B-039).
3. An `initialize` naming a revision earlier than 2024-11-05 is answered with
   2025-11-25 (B-038).
4. A later revision, or a handshake revision the SDK adds, is a dependency
   update and a new item (`0132-F3` A-6; ADR-0032).

## Consequences

Gained:

- A client that opens with `initialize` is served instead of closing.

Paid:

- The supported set is the SDK's, not this repository's: a handshake revision
  the SDK drops leaves it with no change here.
- `0132-F3` B-019's unsupported-version error is reached only by a stateless
  request, since an `initialize` is answered by B-032, B-038 or B-039.
