@0201-F7
Feature: Native Gherkin claim catalogue

  Background:
    Given the claim file "specifications/arithmetic.feature" configured as Feature "arithmetic"
    And a Scenario Outline "Add two operands" under the Rule "Addition returns the sum" tagged "@REQ-FIX-001" with two example rows
    And the claim tag encoding is "@{claim}"
    And the consumer's claim grammar accepts "REQ-FIX-001"
    And no epic identity is configured

  @B-001
  Scenario: Native claim input selected
    Given the manifest selects native Gherkin claim input
    When the declared analysis is requested
    Then the catalogue for Feature "arithmetic" holds claim "REQ-FIX-001" read from the claim file

  @B-002 @boundary
  Scenario: Markdown catalogue unchanged
    Given the manifest does not select native Gherkin claim input
    And Feature "arithmetic" is declared by a metadata record with a claims table
    When the declared analysis is requested
    Then the catalogue holds exactly the claims of that claims table

  @B-003
  Scenario: File mapped to its Feature identity
    Given the manifest selects native Gherkin claim input
    When the declared analysis is requested
    Then claim "REQ-FIX-001" belongs to Feature "arithmetic"

  @B-004
  Scenario: Claim identifier kept as authored
    Given the manifest selects native Gherkin claim input
    When the declared analysis is requested
    Then the claim identifier is "REQ-FIX-001" exactly as tagged

  @B-005
  Scenario: No claims table required
    Given the manifest selects native Gherkin claim input
    And the area metadata record for Feature "arithmetic" contains no claims table
    When the declared analysis is requested
    Then no input failure is reported for a missing claims section

  @B-006
  Scenario: Central specification tree
    Given the manifest selects native Gherkin claim input
    And the claim file sits in a central specification tree outside every project directory
    And two selected projects reference claim "REQ-FIX-001"
    When the declared analysis is requested for each project
    Then both projects resolve the same claim "REQ-FIX-001" of Feature "arithmetic"

  @B-007
  Scenario: No epic identity
    Given the manifest selects native Gherkin claim input
    And the consumer's identifier grammar declares no epic
    When a relationship names Feature "arithmetic" and claim "REQ-FIX-001"
    Then it resolves without an epic identity

  @B-008
  Scenario: Scenario claim location
    Given the manifest selects native Gherkin claim input
    And a Scenario tagged "@REQ-FIX-002" at line 14 of the claim file
    When the declared analysis is requested
    Then claim "REQ-FIX-002" is located at that Scenario's path and line 14

  @B-009
  Scenario: Scenario Outline declares one claim
    Given the manifest selects native Gherkin claim input
    When the declared analysis is requested
    Then the catalogue holds one claim "REQ-FIX-001" and not one claim per example row
    And the claim is located at the Scenario Outline

  @B-010
  Scenario: Rule claim
    Given the manifest selects native Gherkin claim input
    And the Rule "Addition returns the sum" is tagged "@REQ-FIX-003" and holds two scenarios
    When the declared analysis is requested
    Then the catalogue holds one claim "REQ-FIX-003" located at the Rule
    And no duplicate claim is reported for the two scenarios

  @B-011
  Scenario: Stable claim identity
    Given the manifest selects native Gherkin claim input
    And the Scenario Outline has been retitled and moved below another scenario
    When the declared analysis is requested
    Then claim "REQ-FIX-001" of Feature "arithmetic" keeps its identity

  @B-012
  Scenario: Duplicate claim
    Given the manifest selects native Gherkin claim input
    And a second scenario in Feature "arithmetic" is also tagged "@REQ-FIX-001"
    When the declared analysis is requested
    Then a duplicate-claim diagnostic is reported at each declaration's path and line

  @B-025
  Scenario: Ordinary tags
    Given the manifest selects native Gherkin claim input
    And a scenario tagged "@REQ-FIX-004 @visual @ignore @boundary @issue-42"
    When the declared analysis is requested
    Then one claim "REQ-FIX-004" is recorded
    And "@visual", "@ignore", "@boundary" and "@issue-42" declare no claim and draw no diagnostic

  @B-025 @boundary
  Scenario: A mistyped claim tag is an ordinary tag
    Given the manifest selects native Gherkin claim input
    And the claim grammar requires three digits after "REQ-FIX-"
    And a scenario tagged "@REQ-FIX-5"
    When the declared analysis is requested
    Then no claim is recorded for "@REQ-FIX-5"
    And no malformed-claim diagnostic is reported for "@REQ-FIX-5"

  @B-014
  Scenario Outline: Missing or invalid claim file
    Given the manifest selects native Gherkin claim input
    And the configured claim file is <defect>
    When the declared analysis is requested
    Then an input failure names the claim file
    And Feature "arithmetic" is not reported as having no claims

    Examples:
      | defect                 |
      | missing                |
      | not valid Gherkin      |

  @B-015
  Scenario: Scenario without a claim
    Given the manifest selects native Gherkin claim input
    And a Scenario in the claim file carries no claim tag of its own or from its Rule
    When the declared analysis is requested
    Then a missing-claim diagnostic is reported at that Scenario's line

  @B-016 @boundary
  Scenario: Planned or ignored scenario
    Given the manifest selects native Gherkin claim input
    And the Scenario Outline carries a tag the consumer configures as ignored
    When the declared analysis is requested
    Then that scenario counts as neither implemented nor passing evidence for claim "REQ-FIX-001"

  @B-017
  Scenario Outline: Required relationship missing
    Given the manifest selects native Gherkin claim input
    And a mandatory enforcement scope selects the <declaration>
    And the <declaration> carries no <relationship> relationship
    When the declared analysis is requested
    Then the same required-link diagnostic a Markdown-declared claim receives is reported

    Examples:
      | declaration                 | relationship |
      | addition production method  | implements   |
      | addition test method        | verifies     |

  @B-018
  Scenario: Unknown claim
    Given the manifest selects native Gherkin claim input
    And the addition test method verifies claim "REQ-FIX-999" of Feature "arithmetic"
    When the declared analysis is requested
    Then the same target-resolution failure a Markdown-declared claim receives is reported

  @B-019 @boundary
  Scenario: No adapter written
    Given the manifest selects native Gherkin claim input
    When the declared analysis is requested
    Then the claim file and the area metadata record are unchanged
    And no claims table or other adapter is written

  @B-020
  Scenario: Untagged claim is Active
    Given the manifest selects native Gherkin claim input
    And the Scenario Outline carries no lifecycle tag
    When the declared analysis is requested
    Then claim "REQ-FIX-001" is Active

  @B-021
  Scenario Outline: Configured lifecycle tag
    Given the manifest selects native Gherkin claim input
    And the consumer configures a tag meaning <state>
    And the Scenario Outline carries that tag
    When the declared analysis is requested
    Then claim "REQ-FIX-001" is <state>

    Examples:
      | state      |
      | Withdrawn  |
      | Superseded |

  @B-022
  Scenario Outline: Feature lifecycle source
    Given the manifest selects native Gherkin claim input
    And <metadata>
    When the declared analysis is requested
    Then Feature "arithmetic" is <lifecycle>

    Examples:
      | metadata                                                                     | lifecycle  |
      | the configured metadata record for Feature "arithmetic" states "superseded" | superseded |
      | no metadata record is configured for Feature "arithmetic"                    | current    |

  @B-023
  Scenario: Feature tag disagrees
    Given the manifest selects native Gherkin claim input
    And the claim file carries a Feature-level tag naming Feature "geometry"
    When the declared analysis is requested
    Then an input failure names the claim file

  @B-024 @boundary
  Scenario: Ignored scenario keeps its claim
    Given the manifest selects native Gherkin claim input
    And the Scenario Outline carries a tag the consumer configures as ignored
    And a mandatory enforcement scope selects the addition test method, which carries no verifies relationship
    When the declared analysis is requested
    Then claim "REQ-FIX-001" is still declared
    And the required-link diagnostic is still reported

  @B-026 @boundary
  Scenario: Mixed Examples blocks
    Given the manifest selects native Gherkin claim input
    And the Scenario Outline tagged "@REQ-FIX-001" has a second Examples block tagged "@ignore"
    When the declared analysis is requested
    Then claim "REQ-FIX-001" keeps its identity
    And the rows of the first Examples block remain required
    And only the rows of the "@ignore" Examples block are planned

  @B-028
  Scenario: Claim tag on an Examples block
    Given the manifest selects native Gherkin claim input
    And an Examples block of the Scenario Outline is tagged "@REQ-FIX-006"
    When the declared analysis is requested
    Then a malformed-claim diagnostic is reported at the path and line of "@REQ-FIX-006"
    And no claim "REQ-FIX-006" is recorded

  @B-029
  Scenario: Claim file in another language
    Given the manifest selects native Gherkin claim input
    And the configured claim file declares the Gherkin language "fr"
    When the declared analysis is requested
    Then an input failure names the claim file

  @B-030
  Scenario: Catalogue exported with the evidence
    Given the manifest selects native Gherkin claim input
    When the analyzer writes the test evidence
    Then the evidence holds claim "REQ-FIX-001" of Feature "arithmetic" with its two example rows
