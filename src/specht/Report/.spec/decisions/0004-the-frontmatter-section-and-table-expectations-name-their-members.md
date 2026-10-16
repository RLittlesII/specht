---
title: "Decision 0004: the frontmatter, section and table expectations name their members"
description: "B-009, B-010, B-011 and B-026 name the members of expected for SPEC001-SPEC004, SPEC010 and SPEC013, as B-036 does: schema, required, key, constraints, sections, section and headers"
type: decision
---

# Decision 0004: the frontmatter, section and table expectations name their members

**Date:** 2026-10-15
**Decided by:** the repository owner, answering a question on item `0037`: the four claims name their `expected` members, following B-036 and B-037. The member names and shapes are spec-author's reading; recorded by spec-author.

## The call

B-009, B-010, B-011 and B-026 are amended in place, ids kept, and each names the members of `expected` for its rules:

| Claim | Rule                | Members                        |
| ----- | ------------------- | ------------------------------ |
| B-026 | `SPEC001`           | `schema`, `required`           |
| B-009 | `SPEC002`-`SPEC004` | `schema`, `key`, `constraints` |
| B-010 | `SPEC010`           | `sections`                     |
| B-011 | `SPEC013`           | `section`, `headers`           |

- `schema` is the root-relative path `.specht/schema/frontmatter/` followed by the file name the manifest's `frontmatterSchemas` gives the document's kind (`feature`, `task`, `epic`), whichever schema source the run read. The document's `schemaSource` says which source that was.
- `required` is the schema's top-level `required` list in the schema's order, empty where the schema has none.
- `key` is the top-level frontmatter key the violation identifies, the same as its identifier, and is absent where the failure is on the frontmatter as a whole, as a `required` failure is.
- `constraints` is an object naming each schema keyword that failed with its value at that place in the schema: `required` with its list, `enum` with its values, `pattern`, `format`, and every other keyword the same way - `type`, `const`, `minLength`, `minimum`, `uniqueItems`. A failure inside an `if`/`then` is the keyword inside `then`, with `then`'s value. A key the schema does not declare under `additionalProperties: false` is `additionalProperties` with `false`.
- `sections` is the ordered section titles the run checked against: the manifest's `sections`, or the pinned schema version's where the manifest names none.
- `section` is the title of the section the violation identifies; `headers` is the ordered header list the manifest's `tables` gives that section.

`docs/schema/report.schema.json` is unchanged: `expected` stays an open object, as settled on 2026-10-14 for B-034 to B-042.

## Why

- B-036 and B-037 name their members; B-009 to B-011 and B-026 did not, so a test could not tell a right `expected` from a wrong one.
- One `SPEC002`-`SPEC004` violation is one failing place in the schema, and the rule joins every error found there into one message. A consumer's schema can put `minLength` and `pattern` on one key, and an empty value fails both. A single keyword and value cannot carry that without splitting the violation.
- An object keyed by keyword answers every keyword the same way, so no failure leaves `expected` undefined.
- The embedded, disk and upstream schemas all sit at, or are written to, the same path, the one `init --schemas` writes. Naming it under every source keeps the document a function of the tree, the manifest and the tool (C-3) and gives an agent on an embedded run a file to open.
- A manifest can replace the pinned version's `sections`, and the rule checks the manifest's list; B-010 names the list the run used.

## Rejected

**`constraint`, the keyword, and one member for its value.** The proposed shape.

- Two failed keywords on one key need two violations, which changes the counts and the baseline verdict, both `0001-F2`'s.
- Cost of rejecting: a reader looks up the keyword as a member name rather than as a value. Taken.

**`schema` only under the disk source.** What the rule names today.

- An agent on an embedded run gets no file name to compare against.
- Cost of rejecting: none. Taken.

**A `missing` member listing the absent keys.**

- It is `constraints.required` less the keys the document holds, which the agent already reads.
- Cost of rejecting: one comparison by the reader. Taken.

**The instance location, such as `children/1`, as `key`.**

- The message carries the location, and the identifier is the top-level key; `key` keeps to it.
- Cost of rejecting: the item at fault in a list is read from the message. Taken.

**Describing these members in `docs/schema/report.schema.json`.**

- It would close `expected` for four rules while nine stay open, reversing the reading of C-2 and C-4 settled on 2026-10-14.
- Cost of rejecting: a validator does not check these members' types. Taken.

## Affects

- `0001-F3` § 3 B-009, B-010, B-011, B-026; their `@B-009`, `@B-010`, `@B-011` and `@B-026` scenarios, three added under existing tags; § 10.
- Item `0037`: its summary line that each shape is added to the report schema does not hold.

## Reversal

None.
