---
title: "SPEC002: Feature frontmatter satisfies its schema"
description: The frontmatter of a Feature specification fails the Feature frontmatter schema.
type: rule
---

# SPEC002: Feature frontmatter satisfies its schema

## Metadata

| Property         | Value       |
| ---------------- | ----------- |
| Rule ID          | SPEC002     |
| Family           | Frontmatter |
| Default severity | Error       |
| Schema version   | 1           |

## Cause

The frontmatter of a Feature specification - `<area>/.spec/README.md`, or `epics/<epic>/<feature>/spec.md` in the legacy layout - does not validate against the schema version's Feature frontmatter schema, `feature-spec.frontmatter.schema.json`. A copy sits in `.spec/schema/`.

The rule reports nothing for frontmatter that validates. For frontmatter that does not, it prints one line for each failure the schema validator records, in two forms:

| Where the failure is | Line reported                   | Message                             |
| -------------------- | ------------------------------- | ----------------------------------- |
| The mapping itself   | The first line of the file      | `frontmatter {errors}`              |
| One key              | The line the key is declared on | `frontmatter '{location}' {errors}` |

`{location}` is the key, or the key and an index into it such as `children/0`. `{errors}` is the validator's own text, with several errors at one location joined by `; `. A mistake in one key therefore prints a mapping line that lists the key and a key line that says what is wrong with it. The failures the schema can give include:

- a required key is absent: `Required properties ["author"] are not present`, on a mapping line;
- a key the schema does not name is present: `All values fail against the false schema`, at that key;
- a value is not one the schema lists: `Value should match one of the values specified by the enum`;
- a value has the wrong type: `Value is "string" but should be "integer"`;
- a string does not match its pattern or its format;
- a conditional part of the schema fails: a specification whose `scored_by` is a string needs `value` and `risk` of at least 1, and one whose `github_issue` is a number needs `synced_at`.

**Lines that are not mistakes.** The two conditions in the last entry are tested by the schema against `scored_by` and `github_issue`. When either key is `null` - the value the Feature blank gives both - the test is recorded as not met, and whenever the frontmatter fails for any other reason the rule prints that record too, as a mapping line and a key line for each:

| Key and value        | Mapping line                                                                      | Key line                                                             |
| -------------------- | --------------------------------------------------------------------------------- | -------------------------------------------------------------------- |
| `scored_by: null`    | `frontmatter Some properties did not match the required schema: ["scored_by"]`    | `frontmatter 'scored_by' Value is "null" but should be "string"`     |
| `github_issue: null` | `frontmatter Some properties did not match the required schema: ["github_issue"]` | `frontmatter 'github_issue' Value is "null" but should be "integer"` |

`null` is a valid value for both keys. These four lines never appear on their own, name nothing to change, and go when the real failure is fixed. A specification that has been scored and linked to an issue does not print them.

A file with no frontmatter at all is SPEC001's, and is not validated.

## Rule description

A Feature's frontmatter is its tracking record: its identity (`id`, `epic`), its two statuses, its scoring, and its edges to other Features and to its items. The schema closes that record - every required key present, no key the schema does not name, each value of the stated type and vocabulary - so that the rules which read those keys read values of a known shape.

The schema file is the authority on the keys and their values. Read it there; this page does not repeat it.

### Example violation

A specification not yet scored or linked to an issue, whose `spec_status` is not one of the schema's values:

```yaml
---
title: "Specification: Orders"
description: "An order is validated before it is accepted."
type: feature
id: "F1"
epic: "0007"
spec_status: accepted
status: ready
priority: med
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Orders"
author: "spec-author"
milestone: null
children: []
depends_on: []
blocks: []
spikes: []
created: "2026-01-05"
updated: "2026-01-05"
github_issue: null
synced_at: null
---
```

```text
src/orders/.spec/README.md(1): error SPEC002: frontmatter Some properties did not match the required schema: ["spec_status"]
src/orders/.spec/README.md(1): error SPEC002: frontmatter Some properties did not match the required schema: ["scored_by"]
src/orders/.spec/README.md(1): error SPEC002: frontmatter Some properties did not match the required schema: ["github_issue"]
src/orders/.spec/README.md(7): error SPEC002: frontmatter 'spec_status' Value should match one of the values specified by the enum
src/orders/.spec/README.md(13): error SPEC002: frontmatter 'scored_by' Value is "null" but should be "string"
src/orders/.spec/README.md(24): error SPEC002: frontmatter 'github_issue' Value is "null" but should be "integer"
```

One key is wrong and six lines are printed. The first and the fourth are the mistake: the mapping line that lists `spec_status`, and the key line at line 7 that says its value is not one the schema lists. The other four are the `scored_by` and `github_issue` lines described under Cause; lines 13 and 24 are right as they stand.

### Corrected

Line 7 is the only change:

```yaml
spec_status: in-review
```

With that one value changed the tool prints no SPEC002 line for the file, the four lines about `scored_by` and `github_issue` included.

## How to fix violations

1. Set aside the lines about `scored_by` and `github_issue` whose text is `Value is "null" but should be "string"` or `"integer"`, and the two mapping lines that list only `["scored_by"]` or only `["github_issue"]`. Do not change either key because of them.
2. Of the lines left, take each one that names a key in quotes. It gives the key, the line it is on, and what the schema expected of it. Open `.spec/schema/feature-spec.frontmatter.schema.json` at that key and write a value it allows.
3. For `Required properties [...] are not present`, add each key listed. The blank at `.spec/templates/feature.md` carries every required key with a starting value.
4. For `All values fail against the false schema`, the key is not one the schema names. Remove it, or correct its spelling if it was meant to be a key the schema has.
5. A mapping line that only lists keys needs no fix of its own; it clears when the keys it lists do.
6. Run the check again. When the real failures are fixed, every SPEC002 line for the file is gone, the ones set aside in step 1 with them. If a `scored_by` or `github_issue` line says something else - a wrong type for a value that is not `null` - it is a real failure; fix it as in step 2.
