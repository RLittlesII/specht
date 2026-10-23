@0201-F4
Feature: Optional scenario bridge

  @B-001
  Scenario: Scenario claim
    Given a scenario carrying a local claim tag and a determined effective scope
    When the declared analysis is requested
    Then the tag resolves through the same identity contract as a code binding

  @B-002
  Scenario: Scenario outline
    Given an outline producing two discovered test cases
    When the declared analysis is requested
    Then each case links to the originating scenario and row through generator or runner identity

  @B-003
  Scenario: Shared steps
    Given two scenarios sharing a handwritten step method
    When the declared analysis is requested
    Then the scenario-to-test links remain separate and are not inferred from step documentation

  @B-004
  Scenario: Missing generated input
    Given a selected Reqnroll project missing its required generator or discovery input
    When the declared analysis is requested
    Then the bridge reports an unresolved link

  @B-005 @boundary
  Scenario: Bridge disabled
    Given a consumer selecting only bound xUnit or Playwright .NET tests
    When the declared analysis is requested
    Then the absence of Reqnroll scenarios does not invalidate the core test bindings

  @B-006
  Scenario Outline: Feature scope default
    Given Gherkin Feature-level scope
    And a "<kind>" without an explicit scope override
    When effective scope is determined
    Then it uses the Feature-level scope

    Examples:
      | kind     |
      | Scenario |
      | Outline  |

  @B-007 @boundary
  Scenario Outline: Complete scenario scope override
    Given Gherkin Feature-level scope including an epic
    And a "<kind>" with its own explicit Feature scope and no epic
    When effective scope is determined
    Then only the explicit override components are used
    And no epic is merged from the Gherkin Feature-level scope

    Examples:
      | kind     |
      | Scenario |
      | Outline  |

  @B-008
  Scenario Outline: Common scenario bridge contract
    Given generator or runner scenario-to-case metadata supplied by "<producer>"
    When scenario evidence is mapped
    Then it uses the common runner-evidence contract

    Examples:
      | producer                     |
      | the consumer claim bridge    |
      | optional Reqnroll integration |

  @B-009 @boundary
  Scenario: Planned and active rows
    Given an outline carrying one claim whose first Examples block is active and whose second is planned in the claim catalogue
    When the declared analysis is requested
    Then the case links of the first block's rows are active
    And the case links of the second block's rows are planned
    And every case link keeps the same claim
