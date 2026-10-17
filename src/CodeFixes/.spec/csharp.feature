@0201-F2
Feature: CSharp binding authoring

  @B-001
  Scenario: Explicit evidence
    Given a member carrying a complete explicit Feature and claim binding
    When the declared analysis is requested
    Then checked traceability uses that member identity regardless of its containing type default

  @B-002 @boundary
  Scenario: Inherited binding
    Given a member carrying only a claim under a type default
    When the declared analysis is requested
    Then analysis reports that the member lacks explicit identity and does not count it as checked evidence

  @B-003
  Scenario: Materialize default
    Given a claim-only member with one unambiguous type default and an invoked repair
    When the declared analysis is requested
    Then the repair persists its complete target identity while preserving other XML documentation

  @B-004
  Scenario: Unsafe repair
    Given missing, conflicting or ambiguous default targets
    When the declared analysis is requested
    Then no repair silently selects a target

  @B-005
  Scenario: Invalid binding
    Given a malformed, unresolved or ambiguous member reference
    When the declared analysis is requested
    Then analysis reports a diagnostic at that declaration with its target reference

  @B-006
  Scenario: Multiple targets
    Given one member explicitly verifying two distinct declared claims
    When the declared analysis is requested
    Then both bindings are retained independently

  @B-007
  Scenario: Missing member binding
    Given a selected test method with no binding documentation
    When the declared analysis is requested
    Then analysis reports the missing binding and an invoked fix offers existing claim choices without inventing the target
