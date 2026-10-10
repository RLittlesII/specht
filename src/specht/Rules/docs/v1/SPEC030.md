---
title: "SPEC030: Claim ids are well formed and unique"
description: A claim id in § 3 does not match the claim id grammar, or is declared more than once in the same specification.
type: rule
---

# SPEC030: Claim ids are well formed and unique

## Metadata

| Property         | Value   |
| ---------------- | ------- |
| Rule ID          | SPEC030 |
| Family           | Claims  |
| Default severity | Error   |
| Schema version   | 1       |

## Cause

The first cell of a row in the table of a Feature specification's claims section is not a usable claim id. The claims section is the one the manifest's `claims` role names, `## 3. Acceptance Criteria` in the default manifest; the messages call it § 3 whatever its title, and so does this page. The rule reports two conditions, each at the line of the row:

| Condition                                           | Message                                                                                    |
| --------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| The id does not match the claim id grammar          | `claim id '{id}' does not match {grammar} - claim ids are permanent, so the form is fixed` |
| The id is the same as that of an earlier row in § 3 | `claim '{id}' is declared twice in § 3 (also at line {line})`                              |

`{grammar}` is the `claim` pattern under `identifiers` in the manifest, `.spec/schema/spec-structure.schema.json`. In the default manifest it is `^B-[0-9]{3}[a-z]?$`: `B-`, three digits, and an optional lowercase letter, such as `B-007` or `B-007a`.

The table read is the first table in § 3. A row whose first cell is empty is skipped. Surrounding spaces are ignored, and an id written as a code span reads as its content. Uniqueness is per specification: two Features may each have a `B-001`.

A row reported as malformed is not counted as a claim by the other rules, so the same mistake also surfaces as a § 9 row that cites a non-claim (SPEC031) and as a scenario tag that resolves to nothing (SPEC021). A specification with no § 3 is not reported here; the missing section is SPEC010's.

## Rule description

A claim id is how everything outside § 3 refers to a claim: the scenario tag `@B-007`, the § 9 row, a commit message citing `0007-F1 B-007`. For those citations to stay true the id has one fixed form that a tag can carry, and names exactly one claim in its specification for as long as the specification exists.

That is also why ids are never reused or renumbered. A claim that no longer holds keeps its row and its id with its Status changed to Withdrawn, and the next new claim takes the next unused number.

### Example violation

A claim numbered with one digit:

```markdown
## 3. Acceptance Criteria

| ID    | Claim                                   | Source | Status |
| ----- | --------------------------------------- | ------ | ------ |
| B-001 | An order with no lines is rejected.     | need 1 | Active |
| B-2   | An order total is the sum of its lines. | need 1 | Active |
```

```text
src/orders/.spec/README.md(43): error SPEC030: claim id 'B-2' does not match ^B-[0-9]{3}[a-z]?$ - claim ids are permanent, so the form is fixed
```

### Corrected

```markdown
## 3. Acceptance Criteria

| ID    | Claim                                   | Source | Status |
| ----- | --------------------------------------- | ------ | ------ |
| B-001 | An order with no lines is rejected.     | need 1 | Active |
| B-002 | An order total is the sum of its lines. | need 1 | Active |
```

## How to fix violations

- **Malformed id.** Rewrite it in the grammar's form, keeping its number: `B-2` becomes `B-002`. Then make the scenario tags and the § 9 row for that claim use the same spelling. If the row is not a claim at all - a sub-heading or a note inside the table - move it out of the table.
- **Declared twice.** Decide which row keeps the id. If the two rows are the same claim written twice, delete the later one. If they are different claims, the one added later takes the next unused number in the specification, and its scenarios and its § 9 row are retagged to it. Do not renumber the earlier one: it may already be cited.

Do not free a number by deleting a claim. A claim that is dropped stays in § 3 marked Withdrawn.
