@0201-F6
Feature: Consumer analysis adoption

  @B-001
  Scenario: Selected scope
    Given a consumer selecting a subset of projects and declarations for enforcement
    When the declared analysis is requested
    Then only that selected scope is required to carry bindings and the scope is visible in evidence

  @B-002
  Scenario: Editor build parity
    Given the same valid pinned configuration and specification inputs supplied in editor and build
    When the declared analysis is requested
    Then the same binding yields the same resolution verdict

  @B-003
  Scenario: Invalid inputs
    Given a selected analysis scope with missing or invalid manifest or specification inputs
    When the declared analysis is requested
    Then analysis reports the input failure rather than silently reporting clean coverage

  @B-004
  Scenario: Optional Reqnroll
    Given a consumer selecting xUnit and Playwright .NET analysis without the scenario bridge
    When the declared analysis is requested
    Then adoption requires no Reqnroll installation
