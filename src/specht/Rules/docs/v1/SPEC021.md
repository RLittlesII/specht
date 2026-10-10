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

The companion `.feature` file of a Feature specification carries a claim tag whose id is not the first cell of any row in the table of the specification's claims section. The claims section is the one the manifest's `claims` role names, `## 3. Acceptance Criteria` in the default manifest; the message calls it § 3 whatever its title, and so does this page. The rule reports once for each such tag, against the `.feature` file at the tag's line, with the message:

`scenario tag '@{id}' does not resolve to a claim in § 3 of {specification}`

A claim tag is `@B-` followed by three digits and an optional lowercase letter, such as `@B-004` or `@B-004a`, anywhere on a line that is not a `#` comment. Other tags, such as `@boundary`, are not read. A tag repeated on several scenarios is checked each time.

The claims it is compared with are the § 3 row ids that match the manifest's claim id grammar. Three consequences follow:

- a claim whose id is malformed in § 3 (SPEC030) is not a claim here, so tags naming it are reported too;
- a specification with no § 3 has no claims, so every claim tag in its file is reported;
- a claim's Status is not read: a tag naming a claim marked Withdrawn still resolves.

The rule runs only when the specification has exactly one `.feature` file beside it; otherwise SPEC020 reports and the tags are not checked.

## Rule description

A tag is a citation from a scenario to the claim it proves. A tag that names no claim is a scenario proving something the specification does not say: either the claim was never written, or it was removed and the scenario was left behind, or the tag is a typing mistake and the scenario's real claim looks uncovered.

The rule checks one direction only. A claim with no tagged scenario is not reported here; § 9 is where a claim's coverage is recorded (SPEC031).

### Example violation

§ 3 of `src/orders/.spec/README.md` declares `B-001` and `B-002`. Its Gherkin file tags a third:

```gherkin
  @B-002
  Scenario: A total is the sum of its lines
    Given an order with two lines

  @B-004
  Scenario: A cancelled order is refunded
    Given a cancelled order
```

```text
src/orders/.spec/orders.feature(11): error SPEC021: scenario tag '@B-004' does not resolve to a claim in § 3 of src/orders/.spec/README.md
```

### Corrected

The claim is added to § 3, and to § 9 with it:

```markdown
| ID    | Claim                                       | Source | Status |
| ----- | ------------------------------------------- | ------ | ------ |
| B-001 | An order with no lines is rejected.         | need 1 | Active |
| B-002 | An order total is the sum of its lines.     | need 1 | Active |
| B-004 | A cancelled order is refunded to its payer. | need 2 | Active |
```

## How to fix violations

Which fix applies depends on why the tag has no claim.

- **The scenario states behaviour the specification should claim.** Add the claim to § 3 under the id the tag uses, and a row for it to § 9. The claim comes first; a scenario does not create one.
- **The tag is mistyped.** Correct it to the id of the claim the scenario proves.
- **The claim was deleted from § 3.** Restore the row and mark its Status Withdrawn; a claim id is permanent and is never removed or reused. Then remove the scenario, or keep it if the behaviour still holds under another claim and retag it.
- **The scenario belongs to another Feature.** Move it to the `.feature` file beside that Feature's specification.

If the message names a claim that is in § 3, look at how the id is written there: an id SPEC030 reports as malformed is not counted as a claim.
