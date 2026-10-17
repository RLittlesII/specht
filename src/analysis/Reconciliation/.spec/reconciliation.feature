@0201-F5
Feature: Traceability reconciliation

  @B-001
  Scenario: Direct test chain
    Given a claim bound to a discovered xUnit or Playwright .NET test and a matching result with no scenario policy
    When the declared analysis is requested
    Then reconciliation presents the claim-to-method-to-case-to-result chain without requiring Reqnroll

  @B-002
  Scenario: Incomplete scope
    Given a declared repository scope omitting a required test project or input
    When the declared analysis is requested
    Then the report marks coverage incomplete and identifies the missing input

  @B-003
  Scenario: Outcome distinction
    Given a resolved binding with a failed, skipped or absent test result
    When the declared analysis is requested
    Then the report keeps that outcome distinct from a passing result

  @B-004
  Scenario: Policy gaps
    Given an explicit policy requiring a link absent from a claim's evidence
    When the declared analysis is requested
    Then the report identifies that missing link instead of treating reference resolution as complete coverage

  @B-005
  Scenario: Optional scenarios
    Given scenario bridge evidence supplied for part of a declared scope
    When the declared analysis is requested
    Then those scenario links augment the direct chains without inventing links for other tests

  @B-006
  Scenario: Unbound claim
    Given a declared specification claim requiring test evidence with no test binding at all
    When the declared analysis is requested
    Then reconciliation reports the missing evidence starting from that claim even when no method names it
