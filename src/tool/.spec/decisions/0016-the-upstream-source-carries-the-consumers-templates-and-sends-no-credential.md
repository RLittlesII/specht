---
title: "Decision 0016: The upstream source carries the consumer's templates and sends no credential"
description: "The first release fetches a consumer's own upstream source on any HTTPS host without authentication; the template source extends the spechtSchema record with one SHA-256 per template, fetched only by init and upgrade; a template the manifest names as the consumer's is never written; a credential, next version, goes only to the recorded host"
type: decision
---

# Decision 0016: The upstream source carries the consumer's templates and sends no credential

**Date:** 2026-10-24
**Decided by:** the repository owner (2026-10-24), reviewing pull request
#270, asked whether `upgrade` can bring a consumer's own schemas and templates
from "an upstream source other than specht". Recorded by spec-author, who made
the calls marked as spec-author's reading below.

## The call

1. **A consumer's own source, without authentication (C-15, OQ-41).** The
   owner's words: "Private sources should be allowed in the first release."
   Asked where the credential comes from: "Assume the private source has no
   authentication for this version. We can do authenticated sources NEXT
   version." "Private" means the consumer's own source, not specht's: the
   first release fetches from any `https` host the record names (B-070), and
   the tool still reads and sends no credential (C-15, B-083, B-137). "v1" is
   this release and "v2" the next one. How the next version comes by a
   credential - a keychain, the environment, a CI secret - is deferred and not
   specified.
2. **For the next version, the recorded host only.** Asked which hosts
   receive a credential once one is sent, the owner picked "Recorded host
   only": a credential is sent only to the host of the recorded `location`,
   over `https`, and a redirect to another host does not carry it. This is
   decided direction for the next version, not a claim of this release.
   Spec-author's call: no constraint row is written for it. This release
   sends no credential to any host, so a constraint on which host gets one
   rules out nothing a test could fail today; C-15 already rules out every
   send. It becomes a claim, and a constraint if one is needed, when the next
   version's credential is specified (§ 5 row 20).
3. **The template source extends `spechtSchema` (OQ-26).** One record for the
   set: the same `location` and `version`, and, next version, the same
   credential, plus one SHA-256 hash for each fetched template beside the
   schema hashes. A template is fetched only by `init` and `upgrade` (C-8),
   written only when its content matches its recorded hash (C-9), and the
   recorded version is never moved by the tool (C-10).
4. **A template the manifest names as the consumer's is never written,
   whatever source is recorded (decision 0006).** An upstream source replaces
   only templates the pinned version ships, and never creates, modifies or
   deletes a file the tool does not ship (B-063).
5. **The member and its key form are spec-author's reading (OQ-65).** The
   template hashes sit in a `templates` member of `spechtSchema`, beside
   `hashes`, keyed by the template's file name as the pinned version ships it
   under `.specht/templates/`, such as `feature.md`, each value `sha256:` and
   64 lowercase hexadecimal digits (B-125, B-126). Not in `hashes`: its keys
   are frontmatter kinds, and `feature` is both a kind and a template.
6. **A template is fetched from `location` followed by its file name (B-131),**
   as a schema is (B-074), and written under `<root>/.specht/templates/` by
   that name. Spec-author's reading of "the same `location`".
7. **A hash for a file the pinned version does not ship is an invalid
   manifest (B-127).** Spec-author's call, by analogy with OQ-46's proposed
   default (a `hashes` entry for a kind that is not fetched is rejected) and
   OQ-27 reading (a). Ignoring it would leave the consumer believing a file is
   fetched that never is; writing it would break call 4 and B-063.
8. **The member is known from schema `0.3.0` (B-125, B-128).** It changes a
   manifest's verdict - a malformed template hash is now rejected - so by
   C-11 it is a `0.minor` step, and `0.3.0` is the next unpublished version
   on OQ-60's proposed default (a), as decision 0015 call 7 reads it. Under a
   pin below `0.3.0` the member changes nothing, which is how today's tool
   treats an unknown member of `spechtSchema`; spec-author's reading of C-1,
   since rejecting it would change a published version's verdict (OQ-66).
9. **The check reads no fetched template (§ 5 row 28).** B-065 and § 5 row 15
   stand: the check neither verifies a template against its hash nor stops
   when one is absent, as it does for a frontmatter schema (B-077, B-080).
   Spec-author's reading; the owner did not speak to the check.
10. **`init` writes an absent template from the source (`0001-F4` B-041 to
    B-044).** Whether `init` fetches an absent template the manifest names as
    the consumer's is not decided (`0001-F4` OQ-10); recording a template hash
    from `init`'s command line is not built (`0001-F4` § 5 row 19).

## Why

- One record means one address and one version label to move together. The
  consumer's schemas and templates are published together, and C-10 already
  makes the recorded version the consumer's alone.
- Hashes for each file keep C-9 as ratified in decision 0008: a maintainer
  can compute each with `shasum -a 256`.
- Ownership on disk was the first stage of decision 0006; letting a source
  overwrite a template the consumer named would make naming it meaningless.
- Without a credential, a host that serves the consumer's files without
  authentication works now, and the credential's mechanism, which nothing has
  specified, does not hold the release.

## Rejected

**A separate template record.** Considered by the owner and not taken. Cost:
none of substance; two records would carry two addresses and two versions for
files the consumer publishes together, and each would need its own rejections.

**One record per template.** Considered by the owner and not taken. Cost: a
consumer cannot take one template from one source and another from a second.
Nothing asks for it.

**An upstream template winning over a template the manifest names as the
consumer's.** Considered by the owner and not taken. Cost: a consumer that
names a template and records a source hashing it gets the named file left
alone; to take the source's copy it removes the name.

**Template hashes inside `hashes`.** Spec-author's. Cost: a second member to
validate. Mixing kinds and file names in one object makes `feature` ambiguous.

**A constraint now for the recorded-host rule.** Spec-author's (call 2). Cost:
the rule lives only in this record and OQ-41 until the next version writes
its claim.

**Authentication in the first release.** The owner's. Cost: a source that
asks for a credential is refused (B-133; `0001-F4` B-016, B-043).

## Affects

- § 3: B-125 to B-137 added; B-031 and B-062 amended (a template the record
  hashes is the source's, not the pinned version's).
- § 4: C-8, C-9, C-10, C-14 and C-15 amended.
- § 5: row 14 withdrawn; row 20 amended; rows 28 to 30 added.
- § 11: OQ-26 and OQ-41 resolved; OQ-65 and OQ-66 raised.
- `versioning.feature`: scenarios for B-125 to B-137.
- `0001-F4`: B-041 to B-044 added; B-034 amended; § 5 row 19; OQ-10.
- Decision 0008, ratification, "the manifest and the templates are never
  fetched": the templates half is superseded here. Decision 0008 is not
  edited.
- Not changed: B-074's text, which names the frontmatter kinds only, and its
  `@boundary` scenario, whose record hashes no template; `docs/configuration.md`.

## Reversal

None.
