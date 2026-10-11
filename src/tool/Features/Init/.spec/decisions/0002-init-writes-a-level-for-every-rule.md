---
title: "Decision 0002: init writes a level for every rule"
description: "The manifest specht init writes carries an explicit rules map, every rule id of the pinned schema version at its default level, beside schemaVersion; the owner accepted that an explicit level pins the consumer against a later change of a default severity"
type: decision
---

# Decision 0002: init writes a level for every rule

**Date:** 2026-10-11
**Decided by:** the repository owner, answering the first of three questions on the need; recorded by `spec-author`

## The call

The owner's need: "ensure that init covers providing the user with all
default configuration."

The manifest `specht init` writes carries, beside `schemaVersion`
(`0001-F7` B-012), a `rules` map naming every rule id of the version it pins,
each at that rule's default level:

```json
{
  "schemaVersion": "0.1.0",
  "rules": { "SPEC001": "error", "SPEC002": "error" }
}
```

The map is shown short; the one written names every id of the vocabulary.

The trade-off was put to the owner and accepted: **an explicit level pins the
consumer.** A manifest that names a rule keeps that level when a later
version changes the rule's default severity (`0001-F5` B-010, B-046), where a
manifest that leaves the rule out would follow the new default
(`0001-F5` B-044).

`rules` stays consumer configuration. The embedded manifest now carries the
key, and decision 0001 would by its wording make it tool-owned; B-004 leaves
it out of the live-copy comparison by name, because this repository sets its
own levels.

## Why

- A consumer cannot change what it cannot see. With `rules` absent or empty,
  the ids a manifest may name are in brief § 4 and the rule pages, not in the
  file being edited.
- The set command (`0001-F11`) changes an entry in place; a manifest `init`
  wrote always has the entry.
- The level written is the rule's own, so the verdict on the day `init` runs
  is the one an empty map gives (`0001-F5` B-016, B-044).

## Rejected

**Leave `rules` out of the manifest `init` writes**, the state the addendum
to `0001-F5` decision 0008 left. A consumer follows every later default.
Cost of rejecting: the pinning above, and a manifest that names an id a
lower or renumbered version lacks is rejected until the entry is removed
(OQ-4).

**Write `"rules": {}`.** It shows the key and none of the ids. Cost of
rejecting: the same as above.

**List the rules in a comment or an annotation key and set none.** It shows
the ids without pinning. Cost of rejecting: the consumer types a level by
hand; nothing checks an annotation against the vocabulary.

## Affects

- `0001-F4` § 3 B-024 to B-026 added; B-004 amended; § 2 need 6 and A-4;
  § 5 rows 9 and 10; § 11 OQ-4 and OQ-5.
- `0001-F4` decision 0001: amended under Reversal for the `rules` key.
- `0001-F5` decision 0009: the default manifest, A-4.
- `0001-F7` B-012: unchanged; `schemaVersion` is claimed there.
- `0001-F7` B-016: what `upgrade` adds to `rules` is `0001-F7` OQ-31.

## Reversal

None.
