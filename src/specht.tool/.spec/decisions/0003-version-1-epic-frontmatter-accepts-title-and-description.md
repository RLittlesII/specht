---
title: "Decision 0003: version 1 epic frontmatter accepts title and description"
description: "Schema version 1's epic frontmatter schema accepts title and description as optional non-empty strings, so an epic file can meet AGENTS.md's documentation rule without a version 2"
type: decision
---

# Decision 0003: version 1 epic frontmatter accepts title and description

**Date:** 2026-10-08
**Decided by:** the repository owner (2026-10-08); recorded by spec-author

## The call

Schema version 1's epic frontmatter schema accepts `title` and `description`,
each an optional non-empty string. Neither is required, and
`additionalProperties: false` stays. It is a version 1 change, permitted
because version 1 is open until first publish (decision 0002). Resolves OQ-4.

## Why

- AGENTS.md § "Documentation structure" requires `title` and `description` on
  every tracked markdown file, and version 1 rejected both on an epic file, so
  the rule and the schema could not both hold.
- No package is published yet, so changing version 1 breaks no consumer pin.
- "Accepts", not "requires": the two existing epic files,
  `epics/0001-specht/epic.md` and `epics/0002-documentation/epic.md`, carry
  neither key and keep passing.

## Rejected

**Leave version 1 as it is and add the keys in version 2.** Cost: every epic
file violates AGENTS.md § "Documentation structure" until version 2 exists,
and version 2 would exist only for this.

**Exempt epic files from the AGENTS.md rule.** Cost: the epic becomes the one
tracked markdown type without a title and a description.

**Require both keys in version 1.** Cost: both existing epic files fail, as
does every consumer epic written to the prior shape.

## Affects

- `0001-F7` § 3 B-022; § 11 OQ-4.
- `0001-F7` § 3 B-023, added by the owner 2026-10-08: an empty `title` or
  `description` is a violation, as `minLength: 1` says.
- `0001-F1` § 11 OQ-1, which carried the question here.
- `.spec/schema/epic.frontmatter.schema.json`, the live copy of version 1.

## Reversal

None.
