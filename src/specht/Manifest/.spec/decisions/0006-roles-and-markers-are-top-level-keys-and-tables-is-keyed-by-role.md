---
title: "Decision 0006: Roles and markers are top-level keys, and tables is keyed by role"
description: "The manifest names section roles under roles and marker texts under markers, each filled per name; tables is re-keyed from section title to role name; a tables key that is no role, and an empty marker text, are each an invalid manifest"
type: decision
---

# Decision 0006: Roles and markers are top-level keys, and tables is keyed by role

**Date:** 2026-10-09
**Decided by:** the repository owner, asked during delivery of item `0015`; recorded by spec-author

## The call

Four calls, on the names the `implementer` proposed in § 7 (A-3).

1. **`roles` and `markers`.** Two top-level manifest keys, each an object
   filled per name from the default manifest, as `identifiers` is (B-019).

   | Key       | Name       | Default value            |
   | --------- | ---------- | ------------------------ |
   | `roles`   | `claims`   | `3. Acceptance Criteria` |
   | `roles`   | `matrix`   | `9. Traceability Matrix` |
   | `roles`   | `signOff`  | `12. Sign-off`           |
   | `markers` | `missing`  | `Missing`                |
   | `markers` | `draft`    | `🟡`                     |
   | `markers` | `blocked`  | `🔴`                     |
   | `markers` | `approved` | `approved`               |

   A `roles` value is a title from `sections` (B-001, B-015). A `markers`
   value is the text `SPEC060` and `SPEC061` look for (B-003).

2. **`tables` is keyed by role name.** The default manifest's entry becomes
   `"tables": { "matrix": ["Claim ID", "Scenario", "Test", "Status"] }`. The
   key was a section title, `9. Traceability Matrix`. The owner accepted that
   this changes a key consumers already write.

3. **A `tables` key that is not a role is an invalid manifest** (B-039): exit
   `3`, naming the key, as B-015 rejects a role naming an absent section. A
   manifest still keyed by a section title is rejected under this call.

4. **An empty marker text is an invalid manifest** (B-040): exit `3`, naming
   the marker.

## Why

- `roles` and `markers` have the shape `identifiers` and `frontmatterSchemas`
  already have, so the fill rule is the one B-019 states and a consumer learns
  one shape.
- B-002 reads the headers per role and brief § 5 lists "table headers per
  role". Keyed by title, a rename of the matrix section is written three
  times: in `sections`, in `roles` and in the `tables` key.
- A `tables` key the engine never reads declares no headers for any role, so
  `SPEC013` would stop checking that manifest's matrix without a word. Call 2
  makes every manifest written before it such a manifest; call 3 turns the
  silence into a rejection that names the key to change.
- `SPEC061` looks for a sign-off marker inside a cell, and an empty text is
  inside every cell, so an empty `draft` or `blocked` would report every
  sign-off row of an approved specification.

## Rejected

**Different key or member names.** None was put forward against the proposed
ones. Cost of rejecting: none recorded.

**One object per role, holding its section and its headers.** It reads well.
Rejected because the shipped `tables` key becomes either a second home for the
headers or an unknown key B-012 rejects in every manifest that carries it
today. Cost of rejecting: a role's title and its headers sit under two keys.

**`tables` left keyed by section title.** It changes neither manifest copy and
breaks no manifest. Rejected because it contradicts B-002's "per role". Cost of
rejecting: every manifest that declares `tables` today, `hooked`'s included,
is edited once, and is rejected until it is (call 3).

**A `tables` key that is no role is kept and never read.** That is what an
unread name inside `identifiers` gets today. Rejected because here it silently
stops `SPEC013` checking. Cost of rejecting: a manifest written before call 2
exits `3` where it ran before.

**An empty marker text is left as written.** Rejected because an empty `draft`
or `blocked` matches every sign-off cell. Cost of rejecting: a consumer cannot
switch a marker off by emptying it; a rule is switched off by B-011.

## Affects

- B-001, B-002, B-003: their keys have names; the claims' text is unchanged.
- B-019: `roles` and `markers` join the per-name fill; text unchanged.
- B-039, B-040: added.
- B-022, B-023: their list of rejections gains B-039 and B-040.
- B-016, C-4: the default manifest's values are the literals `hooked`'s engine
  hardcodes, so the golden report is unchanged.
- § 11 OQ-1: partly resolved, for roles, markers and the `tables` key.
- § 11 OQ-10, OQ-11: opened; neither was put to the owner.

## Reversal

None.
