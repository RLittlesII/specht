---
title: "Decision 0001: the facts are epic 0228 R1 on the command line, for the whole tree"
description: "Epic 0132 owns the tree-facts command, working name specht facts; it carries epic 0228 R1 in full without claim coverage, gives the whole tree or a path's specifications, exits 0 whatever the verdict, keeps one shape under every pin, and publishes a JSON Schema that changes only by addition, as every document MCP serves does"
type: decision
---

# Decision 0001: the facts are epic 0228 R1 on the command line, for the whole tree

**Date:** 2026-10-30
**Decided by:** the repository owner, 2026-10-30, answering the requirements
pass on epic `0132`; recorded by spec-author

## The call

1. Epic `0132` owns candidate A as this Feature; epic `0228` depends on it
   (epic `0228` OQ-2). The document carries epic `0228` R1 in full. Reading
   a decision, ADR or lesson record's number, title and status is a new read
   and is claimed explicitly.
2. Claim coverage is `specht trace` (`0001-F13`); this Feature leaves it out.
3. The whole tree by default, and an optional root-relative path selector;
   exit `4` when the selector matches nothing. The command's working name is
   `specht facts`, recorded as a working name.
4. Every `--json` document MCP serves gets a published JSON Schema and
   changes only by addition within a tool major version. This settles
   `0001-F9` OQ-4 and `0001-F10` OQ-4.
5. Exit `0` whatever the verdict; give what parses; one shape under every
   pin, with values read through the pinned manifest, as `0001-F13` A-2 and
   B-006 do.

## Why

- Epic `0228` R3, the facts diff, reads claim text and resolution state; a
  narrower document would need a second release.
- Two documents carrying coverage would both have to agree under epic
  `0132`'s parity criterion; `0001-F13` already has the published schema.
- An agent reads one Feature at a time; a path needs no id resolution, which
  epic `0228` candidate C and item `0307` have not specified.
- Without a published schema, the parity test between the command line and
  MCP has no fixed contract to hold.
- An agent repairing a violating tree needs the facts most.

## Rejected

- **Epic `0132`'s narrower list** (Features, claims, constraints, open
  questions, dependencies). Cost: `0228` R3 waits on an amendment.
- **Every section as structured data.** Cost: a larger contract with no asked
  reader.
- **Coverage repeated in the facts, or the trace folded into this Feature.**
  Cost: two answers to keep equal, or an approved Feature amended.
- **One specification per call, selected by id.** Cost: waits on
  qualified-key resolution.
- **No published schema, or one for this document only.** Cost: MCP serves
  documents with no contract, or with contracts of differing strength.
- **Refusing a tree the check fails, or a shape that varies with the pin.**
  Cost: no facts when they are most needed; a caller branches on the pin.

## Affects

B-001 to B-026; C-3, C-5; § 5 rows 1, 3; OQ-1 to OQ-6; `0001-F9` OQ-4;
`0001-F10` OQ-4 and § 5 row 9; epic `0228` OQ-2; epic `0132` § Features.

## Reversal

Narrowing the document or dropping the published schema is a breaking change
to a published contract: a new tool major version (C-3).

## Amended

2026-10-30, by the owner: call 5's "values read through the pinned manifest"
applies only to the roles and tables the shipped manifest declares, and call
1's record status is claimed in B-011 ([decision 0002](0002-only-the-sections-the-manifest-names-follow-it-and-a-records-status-is-claimed.md)).
