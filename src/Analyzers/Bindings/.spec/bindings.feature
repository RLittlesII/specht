@0201-F1
Feature: Binding identity

  @B-001
  Scenario: Consumer grammars
    Given a manifest defining custom Feature and claim grammars and records using them
    When the declared analysis is requested
    Then references are validated and resolved using those configured grammars

  @B-002
  Scenario: Owning scope
    Given two Features declaring the same local claim ID
    When the declared analysis is requested
    Then a qualified claim reference resolves only in its declared owning Feature

  @B-003
  Scenario: No epic tier
    Given a consumer whose configured model has no epic tier
    When the declared analysis is requested
    Then a Feature and claim reference resolves without inventing an epic

  @B-004
  Scenario Outline: ADR ownership
    Given a repository ADR and a Feature ADR sharing the same local identity
    And an ADR reference with <qualification>
    And an enclosing type with a Feature authoring default
    When the ADR reference is resolved
    Then the selected owner is <owner>

    Examples:
      | qualification             | owner               |
      | no explicit Feature owner | the repository      |
      | an explicit Feature owner | the named Feature   |

  @B-005
  Scenario: Framework independence
    Given the same specification inputs and declared references with no Reqnroll or test-framework assemblies
    When the declared analysis is requested
    Then reference resolution yields the same identities and findings

  @B-006
  Scenario: Separate claim identity components
    Given a claim reference with a Feature identity and a claim identity
    When the reference is represented for analysis
    Then the epic, Feature and claim occupy separate identity components
    And each supplied component uses its configured local grammar

  @B-007 @boundary
  Scenario Outline: No ADR scope fallback
    Given an ADR reference selecting <owner>
    And the selected owner does not declare that ADR
    And another owner declares the same local ADR identity
    When the ADR reference is resolved
    Then the reference is reported as unresolved

    Examples:
      | owner          |
      | the repository |
      | a Feature      |

  @B-008
  Scenario Outline: Current claim eligibility
    Given a resolving reference to an <claim_state> claim in a <feature_state> Feature
    When target lifecycle eligibility is evaluated
    Then the target is eligible for current evidence

    Examples:
      | claim_state | feature_state |
      | Active      | draft         |
      | Active      | in-review     |
      | Active      | approved      |
      | Amended     | draft         |
      | Amended     | in-review     |
      | Amended     | approved      |

  @B-009 @boundary
  Scenario Outline: Historical claim eligibility
    Given a resolving reference to a <claim_state> claim in a <feature_state> Feature
    When target lifecycle eligibility is evaluated
    Then the target is ineligible for current evidence
    And its resolved identity and lifecycle state remain available

    Examples:
      | claim_state | feature_state |
      | Withdrawn   | draft         |
      | Withdrawn   | in-review     |
      | Withdrawn   | approved      |
      | Superseded  | draft         |
      | Superseded  | in-review     |
      | Superseded  | approved      |
      | Active      | superseded    |
      | Amended     | superseded    |
      | Withdrawn   | superseded    |
      | Superseded  | superseded    |

  @B-010
  Scenario: Accepted ADR eligibility
    Given a resolving reference to an accepted ADR
    When target lifecycle eligibility is evaluated
    Then the target is eligible for current evidence

  @B-011 @boundary
  Scenario Outline: Historical or proposed ADR eligibility
    Given a resolving reference to a <state> ADR
    When target lifecycle eligibility is evaluated
    Then the target is ineligible for current evidence
    And its resolved identity and lifecycle state remain available

    Examples:
      | state      |
      | proposed   |
      | rejected   |
      | deprecated |
      | superseded |

  @B-012 @boundary
  Scenario Outline: Checked target boundary
    Given a request to treat a "<kind>" record as a checked relationship target
    When the target kind is interpreted
    Then no additional checked target kind is created
    And checked targets remain claims or ADRs

    Examples:
      | kind         |
      | Feature-only |
      | epic-only    |
      | task         |
      | constraint   |
      | lesson       |

  @B-013 @boundary
  Scenario Outline: Epic component follows the consumer tier
    Given a consumer whose model <tier>
    And an otherwise complete reference to a <target> with <epic_component>
    When its declared owner identity is validated
    Then its owner identity is <verdict>

    Examples:
      | tier                     | target      | epic_component      | verdict    |
      | declares an epic tier    | claim       | a valid epic        | complete   |
      | declares an epic tier    | claim       | no epic             | incomplete |
      | declares an epic tier    | Feature ADR | a valid epic        | complete   |
      | declares an epic tier    | Feature ADR | no epic             | incomplete |
      | declares no epic tier    | claim       | no epic             | complete   |
      | declares no epic tier    | claim       | an epic             | invalid    |
      | declares no epic tier    | Feature ADR | no epic             | complete   |
      | declares no epic tier    | Feature ADR | an epic             | invalid    |

  @B-014 @boundary
  Scenario: Checked identity is not completed from authoring context
    Given a persisted member reference missing a required owner component
    And its containing type supplies that component as an authoring default
    When the member reference is resolved
    Then its explicit target identity remains incomplete
    And no checked binding is obtained by borrowing the default

  @B-015
  Scenario Outline: Exactly one declared target
    Given a complete reference conforming to the consumer identity grammars
    And <matches> matching declarations in its selected owner
    When the reference is resolved
    Then its resolution is <verdict>

    Examples:
      | matches | verdict    |
      | zero    | unresolved |
      | one     | resolved   |
      | two     | ambiguous  |

  @B-016
  Scenario Outline: Record moves preserve target identity
    Given a resolving reference to a <target>
    When the specification record moves without changing its declared identity or owner
    Then the reference resolves to the same target identity

    Examples:
      | target         |
      | claim          |
      | repository ADR |
      | Feature ADR    |

  @B-017 @boundary
  Scenario Outline: Supersession does not redirect a binding
    Given a resolving reference to a superseded <target> naming a current replacement
    When its lifecycle eligibility is evaluated
    Then the binding retains the original target identity
    And it remains ineligible for current evidence

    Examples:
      | target |
      | claim  |
      | ADR    |

  @B-018 @boundary
  Scenario: Coverage exemption does not retire a claim
    Given an Active claim in an approved Feature with a reasoned coverage exemption
    When a resolving reference to that claim is evaluated
    Then the claim remains eligible under the current-target lifecycle policy

  @B-019
  Scenario Outline: Permitted lifecycle omission follows the pin
    Given a pinned schema permitting omitted lifecycle information with an established <eligibility> interpretation
    And an existing target omitting that information
    When a resolving reference to that target is evaluated
    Then the target is <eligibility> for current evidence

    Examples:
      | eligibility |
      | eligible    |
      | ineligible  |

  @B-020 @boundary
  Scenario Outline: Unknown explicit lifecycle state
    Given a resolving reference whose <record> has an explicit unrecognized lifecycle state
    When target lifecycle eligibility is evaluated
    Then the target is ineligible for current evidence
    And its resolved identity and unrecognized state remain available

    Examples:
      | record         |
      | claim          |
      | owning Feature |
      | ADR            |

  @B-021 @boundary
  Scenario: Eligibility does not approve an agreement
    Given an eligible claim in a draft Feature
    When its binding is evaluated
    Then the Feature remains draft
    And the eligibility result does not represent agreement approval
