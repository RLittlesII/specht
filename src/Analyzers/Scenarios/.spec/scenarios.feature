@0201-F4
Feature: Optional scenario bridge

  @B-001
  Scenario: Scenario claim
    Given a scenario carrying a claim tag in its owning Feature scope
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
