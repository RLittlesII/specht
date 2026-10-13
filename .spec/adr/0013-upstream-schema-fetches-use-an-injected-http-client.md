---
title: "ADR-0013: upstream schema fetches use an injected HTTP client"
description: Confine upstream network access to one engine adapter and substitute HTTP message handlers in offline tests
type: adr
---

# ADR-0013: upstream schema fetches use an injected HTTP client

## Status

Accepted, 2026-10-12, item `0095`.

## Context

`0001-F4` B-014 to B-016 require `init` to fetch absent frontmatter schemas
from the manifest's source and refuse a failed fetch or a content hash mismatch
before writing anything. `0001-F7` C-8 confines network access to `init` and
`upgrade`; C-9 requires verification over the served bytes; C-15 permits public
sources only. ADR-0010 describes file-system stores but no network adapter.
`0001-F7` OQ-39 asks where the fetch belongs and how a test serves content
without a network request.

## Considered options

1. Add an `IUpstreamSchemaFetcher` interface and replace it in tests.
2. Use one concrete engine fetcher over `HttpClient`, substituting its existing
   `HttpMessageHandler` seam in tests.
3. Run an HTTP server on the test machine and exercise the real transport.

## Decision

[`UpstreamSchemaFetcher`](../../src/specht/Manifest/UpstreamSchemaFetcher.cs)
is the network adapter in `Specht.Manifest`, beside the source and file records
it consumes. It takes an injected `HttpClient`, reads a file's bytes and checks
their SHA-256 before returning them. It has no file-system dependency. Only
write operations that permit a fetch call it; the check continues to read local
files and never resolves or calls a fetcher.

The host constructs the client without credentials, default credentials or
cookies. Redirects are disabled so a recorded HTTPS address cannot redirect a
request onto HTTP. URI user information is removed before sending a request
or naming the address in a diagnostic. Transport failure details are never
copied into product output.

The init writer prepares every absent selected schema's verified bytes before
creating a directory or writing any selected file, including templates. An
existing file is skipped without being fetched. The writer keeps its existing
`CreateNew` write mode, so a file appearing during preparation is preserved.
The writer's optional fetcher preserves its existing embedded-only callers;
an upstream fetch without a configured fetcher fails explicitly.

Tests provide a synthetic `HttpMessageHandler` to the real fetcher. Acceptance
bindings that need a served source compose the real command, writer, manifest
parser and fetcher in process over that handler. No socket or local server is
needed. The handler is already the substitution seam, so adding another
interface would duplicate it.

## Consequences

The network boundary is explicit, and success, failure, exact byte verification
and credential behavior can be exercised offline. No dependency is added.
The fetcher carries no policy for upstream version ordering or authentication.
The source's `Version` remains a label and is never added to its address.

Preparing the bytes uses memory proportional to the absent schema files.
Disabled redirects mean a source must name its serving address directly.
In-process acceptance tests do not prove operating-system transport behavior;
unit tests own the substituted responses and the production host owns client
configuration. This record settles the architecture and test seam of OQ-39.
Init's later-file and HTTP-refusal outcomes are specified in `0001-F4` B-015
and B-016; `upgrade`'s corresponding policies remain open in OQ-44 and OQ-45.
