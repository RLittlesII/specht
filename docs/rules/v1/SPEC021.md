---
title: "SPEC021: Scenario claim tags resolve to a claim"
description: A claim tag in a specification's Gherkin file names an id that is not a claim in § 3 of that specification.
type: rule
---

# SPEC021: Scenario claim tags resolve to a claim

## Metadata

| Property         | Value        |
| ---------------- | ------------ |
| Rule ID          | SPEC021      |
| Family           | Feature file |
| Default severity | Error        |
| Schema version   | 1            |

## Cause

A claim tag in a specification's `.feature` file - `@B-`, three digits and an optional lowercase letter - names an id that is not a claim in the specification's claims section. That section is the one the manifest's `claims` role names, `## 3. Acceptance Criteria` in the default manifest; the message says § 3 whatever its title. Reported in the `.feature` file, at the tag's line:

`scenario tag '@{id}' does not resolve to a claim in § 3 of {specification}`

Tags are checked only when exactly one `.feature` file sits beside the specification (SPEC020).

## Rule description

Every claim tag is the id of a row in the claims table of the specification in the same directory.

### Example violation

§ 3 declares `B-001` and `B-002`. The `.feature` file carries:

```gherkin
@B-004
Scenario: A cancelled order is refunded
```

```text
src/orders/.spec/orders.feature(11): error SPEC021: scenario tag '@B-004' does not resolve to a claim in § 3 of src/orders/.spec/README.md
```

### Corrected

A row added to § 3:

```markdown
| B-004 | A cancelled order is refunded. | need 2 | Active |
```

## How to fix violations

- **The claim is not written.** Add it to § 3 under the tag's id, with its row in § 9.
- **The tag is mistyped.** Correct it to the claim the scenario proves.
- **The scenario belongs to another Feature.** Move it to that Feature's `.feature` file.
