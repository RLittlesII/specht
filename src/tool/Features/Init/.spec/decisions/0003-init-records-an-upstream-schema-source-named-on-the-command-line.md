---
title: "Decision 0003: init records an upstream schema source named on the command line"
description: "Proposed and not ratified: into a root with no manifest, init takes an upstream schema source's address, label and hashes as arguments, verifies the fetched content against the hashes, and writes the record into the manifest it creates; beside an existing manifest the arguments are refused; no claim is written against any of it until the repository owner ratifies it"
type: decision
---

# Decision 0003: init records an upstream schema source named on the command line

**Date:** 2026-10-11
**Decided by:** nobody yet. Proposed by `spec-author` on the owner's ruling of 2026-10-11 that the first release waits for a schema sourced at a URL ([`0055-F6` decision 0004](../../../../../../.build/Releasing/.spec/decisions/0004-the-first-release-waits-for-a-schema-sourced-at-a-url.md)), awaiting the repository owner's ratification

## Status

**Proposed. Not ratified.** Nothing below is a decision until the repository
owner ratifies it. No claim, constraint or scenario is written against any
name or shape here, and no code reads one. Ratification is recorded in this
file, dated, with what the owner changed.

## What is specified today

Every upstream claim begins "Given a manifest that records an upstream schema
source" (B-014 to B-016), and nothing says how a root with no manifest gets
one. `init` takes `--root` and `--epics`. One path exists under the claims as
they stand, and needs no new behaviour:

1. The consumer writes `<root>/.spec/schema/spec-structure.schema.json` by
   hand, holding the record and nothing else. A key left out of a manifest
   takes the default manifest's value.
2. `specht init` leaves that manifest as it is (C-1), fetches the source into
   the absent frontmatter schema files (B-014) and writes the absent templates
   (B-010).

That manifest carries no `schemaVersion` and no `rules`, which a manifest
`init` writes does (`0001-F7` B-012; B-024), and `init` never adds them (C-1).

## The call

Four proposals, for OQ-6 and OQ-7.

**1. `init` takes the source as arguments.** The names follow the manifest
key [`0001-F7` decision 0008](../../../../.spec/decisions/0008-an-upstream-schema-source-is-one-manifest-key-with-a-base-address-a-label-and-a-hash-per-file.md)
proposes.

```
specht init --upstream <address> --upstream-version <label> \
    --upstream-hash feature=sha256:<digits> --upstream-hash task=sha256:<digits>
```

**2. Into a root with no manifest, `init` records what it was given.** It
fetches each frontmatter schema, checks it against the hash given for its
kind, and only then writes: the manifest it would have written, with the
record added, the fetched frontmatter schemas, and the templates. A hash that
does not match, or a source that cannot be reached, writes nothing and exits
`3`, as B-015 and B-016 have it.

**3. A hash for every fetched file is required (OQ-7).** `init` given an
address and no hash for a kind it fetches refuses before it fetches. It never
records a hash it computed itself.

**4. Beside an existing manifest the arguments are refused.** `init` never
rewrites a manifest (C-1), so it cannot record the source there. It names the
manifest, writes nothing and fetches nothing.

## Why

- The owner's words were that `specht` cannot `init` a repository to a schema
  sourced at a URL. The proposal makes `init` the command that does.
- A manifest `init` writes carries `schemaVersion` and a level for every rule
  (decision 0002). A manifest written by hand to hold the record carries
  neither.
- `0001-F7` C-9 rules out "content written before its hash is checked".
  Requiring the hashes keeps it as the owner decided it.

## Rejected

Each was turned down by `spec-author`. The owner may take any of them at
ratification.

**No new arguments: the hand-written manifest above is the way in.** Cost of
rejecting: `init` gains three arguments, and `0001-F7` C-8 is reworded. Taken
instead, nothing is built for the start; the setup guide documents a file
written by hand, and that manifest lacks what `init` writes until `upgrade`
adds the absent keys (`0001-F7` B-016).

**`init` computes the hashes when none is given, and records them.** Cost of
rejecting: the maintainer runs `shasum -a 256` on two or three files before
the first `init`. Taken instead, the first fetch is checked against nothing,
and `0001-F7` C-9 is amended to except it; the record it writes is a diff the
maintainer reviews.

**A second command that only records a source.** Cost of rejecting: none
found. A root with no manifest has nothing for it to write into.

**`init` adding the record to an existing manifest.** Cost of rejecting: a
repository that already ran `init` adds the record by hand, deletes the
embedded frontmatter schemas it was given and runs `init` again. It is ruled
out by C-1 and § 5 row 7.

## Calls left to the owner

1. **The exit code when the arguments are refused** - beside an existing
   manifest, or without a hash. `0001-F2` decision 0003 gives `2` to a missing
   root or manifest, `3` to an invalid manifest and `4` to a named thing not
   found. None is a set of arguments the command cannot honour.
2. **`--epics` with a source.** Whether the epic schema's hash is then
   required, as proposal 3 reads it.
3. **A root with no manifest and a frontmatter schema file already there.**
   C-1 keeps the file. Whether `init` then records a source the file did not
   come from, or refuses, is not proposed.

## Claims owed and not written

Each waits on ratification.

- `init` with a source, into a root with no manifest: the manifest written
  carries the record.
- The same run: the frontmatter schemas written are the fetched content, and
  B-003 and B-005 already except a root that records a source.
- The same run with a hash that does not match, or a source out of reach:
  nothing written, exit `3`.
- A source and no hash for a fetched kind: refused before any fetch.
- A source beside an existing manifest: refused, nothing written, no fetch.
- B-027 gaining the arguments among those it names.
- A-3 and § 5 row 6, which say what `init` takes no argument for.
- `0001-F7` C-8, reworded from "a source the manifest records" to cover the
  manifest the same run writes.

## Affects

Nothing until ratified. On ratification:

- § 11 OQ-6 and OQ-7; § 5 row 11.
- The claims "Claims owed" lists, with their scenarios.
- `0001-F7` § 4 C-8; § 5 row 19.
- `specht init` in AGENTS.md § CLI and `README.md`.
- Not decided here: the record's shape
  (`0001-F7` decision 0008); `init` beside a recorded source with every
  schema file present (OQ-8).

## Reversal

None.
