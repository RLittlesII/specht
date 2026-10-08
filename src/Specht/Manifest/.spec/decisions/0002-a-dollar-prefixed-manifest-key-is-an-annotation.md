---
title: "Decision 0002: A dollar-prefixed manifest key is an annotation"
description: "A manifest key whose name begins with $ is ignored rather than rejected as unknown, so the default manifest's $comment and an editor's $schema stay valid"
type: decision
---

# Decision 0002: A dollar-prefixed manifest key is an annotation

**Date:** 2026-10-07
**Decided by:** spec-author, on spec-reviewer's finding 6 (2026-10-07)

## The call

A manifest key whose name begins with `$` is an annotation. The engine ignores
it and does not reject the manifest for it (B-020). Every other key the engine
does not know is rejected (B-012).

## Why

The default manifest already carries `$comment`, so B-012 as first written
rejected the default manifest. The manifest lives in a `.schema.json` file
beside JSON Schemas, where `$comment`, `$schema` and `$id` are the annotation
keywords editors and authors add. One prefix rule covers all of them without a
list the engine has to keep.

## Rejected

- **Only `$comment` is known.** Rejects a `$schema` an editor adds for
  completion, for no gain.
- **A fixed list of `$` keywords.** A list to maintain, and a manifest
  rejected for the next annotation keyword.
- Cost of the call: a misspelt `$`-prefixed key is ignored rather than
  reported. No key the engine reads starts with `$`, so nothing it ignores can
  have been meant as configuration.

## Affects

B-012, B-020.

## Reversal

None.
