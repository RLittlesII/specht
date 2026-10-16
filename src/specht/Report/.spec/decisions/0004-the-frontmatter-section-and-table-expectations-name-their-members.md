---
title: "Decision 0004: the frontmatter, section and table expectations name their members"
description: "B-009, B-010, B-011 and B-026 name the members of expected for SPEC001-SPEC004, SPEC010 and SPEC013, as B-036 does: schema, required, key, constraints, sections, section and headers"
type: decision
---

# Decision 0004: the frontmatter, section and table expectations name their members

**Date:** 2026-10-15
**Decided by:** the repository owner, answering a question on item `0037`: the four claims name their `expected` members, following B-036 and B-037. The member names and shapes are spec-author's reading, except the resolution of `$ref`, which the owner decided on 2026-10-15 answering `spec-reviewer`'s block on item `0037`; recorded by spec-author.

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
- `constraints` is an object naming each assertion that failed at the violation's own place in the schema, with its value there. An assertion is a keyword of the JSON Schema 2020-12 Validation vocabulary - `type`, `enum`, `const`, `multipleOf`, `maximum`, `exclusiveMaximum`, `minimum`, `exclusiveMinimum`, `maxLength`, `minLength`, `pattern`, `maxItems`, `minItems`, `uniqueItems`, `maxContains`, `minContains`, `maxProperties`, `minProperties`, `required`, `dependentRequired` - or `format`.
  - An applicator - `properties`, `patternProperties`, `additionalProperties`, `items`, `prefixItems`, `contains`, `propertyNames`, `dependentSchemas`, `allOf`, `anyOf`, `oneOf`, `not`, `if`, `then`, `else`, `unevaluatedProperties`, `unevaluatedItems`, `$ref`, `$dynamicRef` - contributes no entry. Its failure is the failure of a subschema at another place, which is that place's violation.
  - A place reached through a same-document reference (`$ref` to `#/...`, such as `#/$defs/...`) is the place the reference resolves to, and names its failed assertions as any other place does; a place reached through a reference to another document gives an empty `constraints`. This is the owner's call (2026-10-15), not spec-author's reading.
  - A `then` or `else` place, and an `allOf` member, is one the document must satisfy, so its assertions are named with that place's value: the Feature schema's `then` gives `required` `["synced_at"]`.
  - A place under an `if`, a `not`, or an `anyOf` or `oneOf` branch is one the document need not satisfy, so `constraints` is empty there: the template's `scored_by: null` fails the `if` that tests for a string, and that violation names no constraint.
  - `constraints` is also empty where no assertion failed at the place: the frontmatter as a whole failing through `properties` alone, or a key the schema does not declare, which fails the boolean `false` under `additionalProperties`.
- `sections` is the ordered section titles the manifest's `sections` gives; `section` is the title of the section the violation identifies; `headers` is the ordered header list the manifest's `tables` gives that section. B-010, B-011 and B-026 each name the manifest's value and leave the pinned version's, where the manifest names none, to `0001-F5` B-019, as `0001-F7` B-074 and B-082 do.

Which violations `FrontmatterSchemaRule` reports does not change: the engine keeps its baseline verdicts (`0001-F1` B-004). That it reports the failures inside a failed `if` at all - an invalid frontmatter carrying the template's `scored_by: null` and `github_issue: null` gains a `SPEC002` for each - and the failures inside a passing `not`, is a bug for an item of its own, not this claim.

`docs/schema/report.schema.json` is unchanged: `expected` stays an open object, as settled on 2026-10-14 for B-034 to B-042.

## Why

- B-036 and B-037 name their members; B-009 to B-011 and B-026 did not, so a test could not tell a right `expected` from a wrong one.
- One `SPEC002`-`SPEC004` violation is one failing place in the schema, and the rule joins every error found there into one message. A consumer's schema can put `minLength` and `pattern` on one key, and an empty value fails both. A single keyword and value cannot carry that without splitting the violation.
- An object keyed by keyword answers every keyword the same way, so no failure leaves `expected` undefined.
- `expected` exists so an agent repairs the document (C-2; § 1). An applicator's value is a subtree of the schema, not a constraint on one value, and an assertion under an `if`, a `not` or an unmatched branch is one the document is not required to meet: naming either sends the agent to change a value the schema accepts. The JSON Schema 2020-12 vocabularies already draw the line between the two kinds of keyword, so the reading invents none.
- The disk and upstream schemas are read from, and `init --schemas` writes the embedded ones to, the same path. Naming it under every source keeps the document a function of the tree, the manifest and the tool (C-3) and names the file a repair edits or `init --schemas` creates. Under the embedded source no file need exist there, and one that does may differ from the schema the run read; `schemaSource` says which the run used, and `required` and `constraints` come from that one.
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

**Naming every failed keyword, applicators and conditions included.** The first wording of this decision.

- `properties` would carry the whole properties map, and the template's `scored_by: null` would read `type` `string`, a constraint the schema does not place on the document.
- Cost of rejecting: a violation at an applicator, inside a condition or at an undeclared key carries an empty `constraints`, and the agent reads the message and the key. Taken.

**Ending the reports from inside a failed `if` or a passing `not` here.**

- It changes which violations the rule reports and the baseline verdicts (`0001-F1` B-004), which a claim on the report's contents cannot do.
- Cost of rejecting: those violations stay in the report, with an empty `constraints`, until the bug's item ends them. Taken.

**Describing these members in `docs/schema/report.schema.json`.**

- C-1's Rules Out is "a field the schema does not name", and the reading settled on 2026-10-14 for B-034 to B-042 holds that `expected`, the field, is named, and its members are its open content (C-2, C-4). Members described under `properties` without `additionalProperties: false` would close nothing, but would describe four rules' members while nine others stay undescribed, a second reading of C-1 within one object.
- Cost of rejecting: a validator does not check these members' types. Taken.

## Affects

- `0001-F3` § 3 B-009, B-010, B-011, B-026; their `@B-009`, `@B-010`, `@B-011` and `@B-026` scenarios, four added under existing tags; § 10.
- Revised 2026-10-15 before merge, answering `spec-reviewer`'s round-1 block (§ 12, expected-members delta): `constraints` names assertions only, B-010, B-011 and B-026 share one reading of the default, and the reasons for `schema` and against the report schema are corrected.
- Amended 2026-10-15 by the owner's call on item `0037`'s review (§ 12, item `0037` row): a same-document `$ref` is resolved and a reference to another document gives an empty `constraints`; the failures inside a passing `not` join the failed-`if` bug.
- Item `0037`: its summary line that each shape is added to the report schema does not hold.

## Reversal

None.
