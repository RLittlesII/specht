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
    And the epic component is optional

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
