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
    Given an outline whose first Examples block is marked active and whose second is marked planned
    When the declared analysis is requested
    Then the case links of the first block's rows are active
    And the case links of the second block's rows are planned

  @B-010
  Scenario: Planned and active rows keep one claim
    Given an outline carrying one claim whose rows are partly planned
    When the declared analysis is requested
    Then every row's case link keeps that same claim

  @B-011 @boundary
  Scenario: Planned row without a discovered case
    Given an outline row marked planned that has no discovered test case
    And an outline row marked active whose generator input is missing
    When the declared analysis is requested
    Then the planned row's link is planned and has no case
    And the active row's link is the unresolved link for missing input

  @B-012
  Scenario: Reqnroll scenarios recorded from a real run
    Given a selected Reqnroll test project holding a Scenario and a Scenario Outline
    When the tests are discovered and run with the shipped evidence collection
    Then its evidence carries one scenario record for each of them

  @B-013
  Scenario: Each Reqnroll outline row linked to its own case
    Given a Reqnroll Scenario Outline whose two Examples rows are discovered as separate test cases
    When the tests are discovered and run with the shipped evidence collection
    Then each row is linked to its own discovered case

  @B-014
  Scenario: Planned Reqnroll rows
    Given a Reqnroll Scenario Outline whose second Examples block carries a configured planned tag
    When the tests are discovered and run with the shipped evidence collection
    Then the second block's rows are linked as planned
    And the first block's rows are linked as active

  @B-015
  Scenario: Reqnroll row with no discovered case
    Given a Reqnroll Scenario Outline row that the runner did not discover
    When the tests are discovered and run with the shipped evidence collection
    Then that row is recorded with an unresolved link

  @B-016 @boundary
  Scenario: Reqnroll producer not selected
    Given a Reqnroll test project that does not select the scenario producer
    When the tests are discovered and run with the shipped evidence collection
    Then its evidence carries no scenario records

  @B-017 @boundary
  Scenario: Two Reqnroll scenarios sharing a title
    Given two Reqnroll scenarios with the same title
    When the tests are discovered and run with the shipped evidence collection
    Then each is recorded as its own scenario
    And each is linked only to its own discovered cases
