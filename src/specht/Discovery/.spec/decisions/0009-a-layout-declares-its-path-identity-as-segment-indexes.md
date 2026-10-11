---
title: "Decision 0009: a layout declares its path identity as segment indexes"
description: "A layouts entry may carry an optional identity object giving the zero-based path segments that hold the epic and the Feature id; SPEC011 follows that declaration and no layout name, the default manifest declares it on epics alone, and a malformed identity is an invalid manifest"
type: decision
---

# Decision 0009: a layout declares its path identity as segment indexes

**Date:** 2026-10-10
**Decided by:** the repository owner, asked during delivery of item `0006`; recorded by spec-author

## The call

Two calls. The first ends the interim of
[decision 0005](0005-spec011-selects-the-legacy-layout-by-name-until-path-identity-is-declared.md)
and extends the `layouts` entry of
[decision 0004](0004-the-discovery-inputs-are-five-top-level-manifest-keys.md).

**1. The shape.** A `layouts` entry may carry an optional `identity` object.
Its members are zero-based indexes of segments in a specification's
root-relative path.

```json
{
  "name": "epics",
  "glob": "epics/**/spec.md",
  "identity": { "epic": 1, "feature": 2 }
}
```

| Member    | Read as                                            |
| --------- | -------------------------------------------------- |
| `epic`    | The segment whose directory carries the epic id    |
| `feature` | The segment whose directory carries the Feature id |

- `SPEC011` checks a specification against the segments its layout declares,
  whatever the layout is named (B-004).
- A layout with no `identity` gets no `SPEC011` verdict.
- The default manifest declares `{ "epic": 1, "feature": 2 }` on `epics` and
  nothing on `features`.

**2. A malformed `identity` is rejected.** A member that is not a non-negative
integer, or a member other than `epic` and `feature` inside `identity`, makes
the manifest invalid. The tool exits `3`, and the fault is listed in the one
rejection the other manifest faults share, naming the layout and the member
(`0001-F5` B-043, B-022, B-023). A misspelt member at the level of the layout
entry, such as `identty`, stays ignored: § 11 OQ-7 (c) stays open.

## Why

- The interim selected `SPEC011`'s layout by the name `epics`, so a manifest
  that renamed the layout lost the check with no diagnostic (decision 0005).
  A declaration on the entry moves with the entry.
- `1` and `2` are the two segments the engine read by literal. Under the
  default manifest `SPEC011` reports what it reported, so the golden report is
  unchanged (`0001-F1` B-004).
- The rejection is `0001-F5`'s, as the exclusion entry's and the empty
  file-shape list's are (`0001-F5` B-021, B-041; § 5 row 9).

## Rejected

**Two flat members on the entry, `epicSegment` and `featureSegment`.** Cost of
rejecting: an entry nests one level, and the loader checks the inside of
`identity` itself, because `0001-F5` B-012 reaches top-level keys only.

**A path template with named placeholders.** It is a second path grammar
beside the glob dialect (C-6). Cost of rejecting: a consumer counts segments,
and a layout whose glob carries `**` before the declared segments has no fixed
index to name.

## Assumptions awaiting ratification

Not the owner's words. The implementer proposed each during delivery of item
`0006`; each waits on the owner's ratification in review of that item's pull
request, and is § 11 OQ-17 until then. No claim is written against any of
them.

1. `epic` and `feature` are each optional inside `identity`, so an epic-less
   repository declares `feature` alone. An `identity` object declaring neither
   is rejected.
2. A specification whose path has no directory segment at a declared index is
   not reported by `SPEC011`. This generalises the engine's guard on a path
   shorter than four segments, and keeps the golden report identical.
3. The comparison stays "the directory name starts with `<value>-`". It is not
   configurable by item `0006`.

## Affects

- B-004: amended. The epic segment is checked only when the manifest declares
  the epic grammar
  ([`0001-F5` decision 0007](../../../Manifest/.spec/decisions/0007-spec011s-epic-check-follows-the-epic-grammar.md),
  `0001-F5` B-042).
- B-001 and C-7: unchanged. Discovery reads a layout's name and glob;
  `identity` is `SPEC011`'s.
- `0001-F5` B-043 (new): a malformed `identity` is rejected. `0001-F5` B-022
  and B-023 carry the exit code and the output.
- `0001-F1` B-002: `SPEC011` applies in a layout that declares its path
  identity.
- Decision 0004: the `layouts` entry gains the optional `identity` member.
  The five keys, `name`, `glob`, the list replaced whole (C-7) and the glob
  dialect (C-6) stand.
- Decision 0005: its interim ends with item `0006`.
- § 5 row 7 amended; rows 9 and 10 added.
- § 11: OQ-5 amended in place; OQ-7 (c) restated and left open; OQ-17 and
  OQ-18 opened.
- `docs/rules/v1/SPEC011.md` and the `layouts` row of the repository-root
  `README.md`.
- Not decided here: the three assumptions above (OQ-17); whether `SPEC011`'s
  report of a missing `epic` reaches a layout that declares no `identity`
  (OQ-18).

## Reversal

None.
