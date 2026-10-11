---
title: "Decision 0008: an upstream schema source is one manifest key with a base address, a label and a hash per file"
description: "Proposed and not ratified: the manifest records an upstream schema source in one upstreamSchemaSource key holding an HTTPS base address, a version label the tool does not interpret and a SHA-256 hash for each frontmatter schema file; a GitHub repository is addressed as any HTTPS host is; no claim is written against any of it until the repository owner ratifies it"
type: decision
---

# Decision 0008: an upstream schema source is one manifest key with a base address, a label and a hash per file

**Date:** 2026-10-11
**Decided by:** nobody yet. Proposed by `spec-author` on the owner's ruling of 2026-10-11 that the first release waits for a schema sourced at a URL ([`0055-F6` decision 0004](../../../../.build/Releasing/.spec/decisions/0004-the-first-release-waits-for-a-schema-sourced-at-a-url.md)), awaiting the repository owner's ratification

## Status

**Proposed. Not ratified.** Nothing below is a decision until the repository
owner ratifies it. No claim, constraint or scenario is written against any
name or shape here, no code reads one, and no manifest carries one.
Ratification is recorded in this file, dated, with what the owner changed.

[Decision 0004](0004-an-upstream-schema-source-is-fetched-only-by-init-and-upgrade.md)
is what is decided: a manifest may record a URL or a package with its version
and a content hash, only `init` and `upgrade` fetch, content is written only
when it matches the hash, and the check never fetches. It names no key, no
file set, no hash and no host. This record proposes them.

## The call

Seven proposals. Each names the open question it would answer.

**1. One key, `upstreamSchemaSource` (OQ-1).** A top-level manifest object
with three members.

```json
{
  "upstreamSchemaSource": {
    "location": "https://raw.githubusercontent.com/acme/specs/v3/schema/",
    "version": "v3",
    "hashes": {
      "feature": "sha256:9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08",
      "task": "sha256:2c26b46b68ffc68ff99b453c1d30413413422d706483bfa0f98a5e886266e7ae"
    }
  }
}
```

**2. `location` is a base address, and the files keep their manifest names
(OQ-32).** Each frontmatter schema is fetched from `location` followed by the
file name the manifest's `frontmatterSchemas` gives its kind
(`0001-F5` B-008), and written under `<root>/.spec/schema/` by that name. The
epic schema is fetched only when the manifest declares the epic grammar. The
manifest and the templates are never fetched (C-4; § 5 row 14).

**3. `hashes` holds one SHA-256 per file (OQ-33).** Keyed by kind, as
`frontmatterSchemas` is. Written `sha256:` and 64 lowercase hexadecimal
digits, over the file's bytes as served, so `shasum -a 256` prints it. A kind
that is fetched and has no hash makes the manifest invalid.

**4. `version` is a label (OQ-34).** A non-empty string the consumer chooses.
The tool compares it with nothing, orders it against nothing and puts it in
no address. It is what the report and a message name.

**5. Any HTTPS address; GitHub gets no code of its own (OQ-35).** `location`
is an `https` address and nothing else. A schema set kept in a GitHub
repository is addressed by its raw file address at a tag or a commit. A
package as the source, which decision 0004 took, is not built for the first
release and is not withdrawn.

**6. A token from the environment for a private source (OQ-36).** When the
environment variable `SPECHT_SCHEMA_TOKEN` is set, `init` and `upgrade` send
it as a bearer credential with the fetch. It is never written to the
manifest, the report or any output.

**7. The explicit source selection is `schemaSource` (OQ-1).** A top-level
manifest string, `embedded` or `disk`, the two values the report's
`schemaSource` member already uses for them, and `--schema-source` on the
command line. It is not on the path to the first release
([`0055-F6` decision 0004](../../../../.build/Releasing/.spec/decisions/0004-the-first-release-waits-for-a-schema-sourced-at-a-url.md)).

Two further proposals sit with the check, in § 11: with a source recorded and
a schema file absent the check exits `3` (OQ-37), and the check verifies the
local copy against the recorded hashes (OQ-38).

## Why

- `upstreamSchemaSource` is the term decision 0004 and every claim use.
- A base address with the manifest's own file names adds no second list of
  files, and the on-disk source already reads by those names
  (`0001-F5` B-008).
- A hash for each file is one a maintainer can compute and compare by hand. A
  hash over the set needs a rule for joining the files that nobody can run
  from a shell.
- A label needs no grammar. Decision 0004's addendum already makes the
  upstream version independent of `schemaVersion` (C-10).
- An HTTPS address at a tag or a commit is what a GitHub repository already
  serves, so the tool learns nothing about GitHub.
- The owner named GitHub, and a consumer's own schema repository can be
  private. Whether the first one is has not been asked, so proposal 6 is the
  one with the least behind it.

## Rejected

Each was turned down by `spec-author`. The owner may take any of them at
ratification.

**Three flat keys beside `schemaVersion`.** Cost of rejecting: one more level
of nesting. Flat, a manifest can carry a location without a hash, and each
half-record needs its own rejection.

**One file, or an archive.** Cost of rejecting: the upstream publishes two or
three files, not one. One file cannot hold three schemas the engine loads by
name; an archive adds unpacking, and a path inside it that leaves the folder.

**One hash over the whole set.** Cost of rejecting: two or three hashes to
record where decision 0004 says "a content hash". The reading of that phrase
as one hash for each file is the author's.

**A `{version}` placeholder in `location`.** Cost of rejecting: moving the
upstream version is an edit to the address, the label and the hashes, not to
two of them.

**A semver upstream version.** Cost of rejecting: the tool cannot say which of
two upstream versions is newer. Nothing decided asks it to.

**A GitHub shorthand, `owner/repo@ref`, or a release asset fetched through
GitHub's API.** Cost of rejecting: the consumer writes the raw file address in
full. A shorthand is a second address grammar, and the API ties the tool to
one host.

**Public sources only.** Cost of rejecting: the tool reads a credential from
the environment, the one place it handles a secret. Public only, a private
schema repository cannot be a source.

**A token in the manifest, or passed as an argument.** Cost of rejecting:
none. Either puts a secret in a tracked file or in shell history.

## Calls left to the owner

1. **Is "a content hash" one hash for each file?** Proposal 3 reads it so.
2. **Which hosts get the token.** Proposal 6 sends it wherever `location`
   points. Whether it is sent only to named hosts, and whether a redirect to
   another host carries it, is not proposed. Not verified: that GitHub's raw
   file host accepts a bearer token for a private repository.
3. **An address that names a branch.** The hash refuses content that moved.
   Whether a branch is also refused by its form is not proposed; nothing in an
   address says it is a branch.
4. **A hand edit to the local copy.** Under OQ-38's proposal the check refuses
   it. A consumer who wants to edit a fetched schema removes the record, and
   selecting the on-disk copy without one is B-009, which is not on the path
   to the first release.

## Claims owed and not written

Each waits on ratification: a claim written on an unratified name would have
to be withdrawn.

- A rejection of an `upstreamSchemaSource` that lacks a member, whose
  `location` is not an `https` address, or whose `hashes` lacks a fetched
  kind or holds a value that is not `sha256:` and 64 digits, with its place
  in `0001-F5` B-022.
- `0001-F5` B-012 leaving `upstreamSchemaSource` and `schemaSource` alone.
- Which address each file is fetched from, and the name it is written under.
- The epic schema fetched only under the epic grammar.
- The check with a source recorded and a schema file absent (OQ-37).
- The check with a local copy that does not match its hash (OQ-38).
- The token: sent when set, absent from every output.
- C-9 restated for one hash for each file.

## Affects

Nothing until ratified. On ratification:

- § 11 OQ-1, for the names; OQ-32 to OQ-38.
- B-024 to B-036, B-067 and `0001-F4` B-014 to B-016, B-027: the names their
  tests are written on. Their texts are unchanged.
- § 4 C-9, as "Claims owed" lists.
- `0001-F5` B-012 and B-022; `docs/configuration.md`.
- Not decided here: how a root with no manifest comes to record a source
  ([`0001-F4` decision 0003](../../Features/Init/.spec/decisions/0003-init-records-an-upstream-schema-source-named-on-the-command-line.md));
  where the fetch sits in the engine and how a test serves a source (OQ-39);
  the schema version that first carries the keys (OQ-40); the report's
  members (`0001-F3` OQ-6).

## Reversal

None.
