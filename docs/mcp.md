---
title: "specht-mcp"
description: "The local MCP server that runs the repository's pinned specht: client configuration, the tools it offers, and how to restore, pin and update the tool it runs"
type: reference
---

# MCP

`specht-mcp` is a local MCP server that speaks over standard input and output
and runs the repository's pinned `specht`, relaying what it prints. It opens no
port, makes no outbound call, writes nothing and never restores or installs a
tool; the .NET SDK is needed to run it.

## Install

```sh
dotnet tool install specht.mcp --prerelease           # local tool manifest, runs as dotnet specht-mcp
dotnet tool install --global specht.mcp --prerelease  # on PATH, runs as specht-mcp
```

`specht-mcp --help` lists its options. The one option, `--root <DIR>`, names
the repository root to answer for; it defaults to the working directory, and a
client may start the server in another directory, so name the root; `.` is
relative to where the client starts it.

## Client configuration

A client starts the server from one entry. Through the local tool manifest:

```json
{
  "mcpServers": {
    "specht": {
      "command": "dotnet",
      "args": ["specht-mcp", "--root", "."]
    }
  }
}
```

From a global install:

```json
{
  "mcpServers": {
    "specht": {
      "command": "specht-mcp",
      "args": ["--root", "."]
    }
  }
}
```

Where a client reads its configuration from is the client's own documentation.

The server speaks protocol revision 2026-07-28 and, for a client that opens with
`initialize`, revisions 2025-11-25, 2025-06-18, 2025-03-26 and 2024-11-05. An
`initialize` naming 2026-07-28, or a later revision, is refused with an error
naming it as having no handshake; 2026-07-28 is discovered through
`server/discover`. One earlier than 2024-11-05 is answered with 2025-11-25.

## Tools

Each tool runs one `specht` command on the root and returns its output
unchanged; the shape of the answer is that command's, in
[Command line](cli.md).

### `check`

Serves `specht --json`: the verdict for the whole tree. Optional `schemaSource`
(`embedded` or `disk`) is `--schema-source`, and `strict` (true) is `--strict`.

### `explain`

Serves `specht --explain <rule>`: the page of one rule for the pinned schema
version. `rule` is required, such as `SPEC4004`.

### `rules`

Serves [`specht rules --json`](cli.md#specht-rules): the rules of the pinned
schema version. It takes no argument.

### `manifest`

Serves [`specht manifest --json`](cli.md#specht-manifest): the manifest in
effect. Optional `defaults` (true) is `--defaults`.

### `mint`

Serves [`specht mint <type> [--feature <dir>]`](cli.md#specht-mint): one new id
for a type the manifest opts in, by `ids.<type>.mint` from schema `0.4.0`. `type` is required, such as `adr`; optional
`feature`, the Feature's folder, is `--feature`.

## Restore the tool

The nearest local tool manifest at or above the root, `.config/dotnet-tools.json`,
pins `specht.tool`.
The server never restores it. A pin that is not restored fails a call with an
error naming `specht.tool` and the pinned version. Restore it, then call again:

```sh
dotnet tool restore
```

## Pin the tool

The server runs the `specht` the root's local tool manifest pins. With no pin it
runs the global `specht` and reports beside the answer that it is unpinned; with
neither, a call fails. Pin the version the repository chose:

```sh
dotnet new tool-manifest          # once, if the repository has none
dotnet tool install specht.tool   # records the version in .config/dotnet-tools.json
```

## Update the tool

The server runs a `specht.tool` of `0.6.1` or later. An older one fails a call
with an error naming that minimum, and is run for nothing but its version.
Update it:

```sh
dotnet tool update specht.tool
```
