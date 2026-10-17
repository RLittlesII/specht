@0201-F3
Feature: Test evidence adapters

  @B-001
  Scenario: xUnit fact
    Given a bound xUnit fact in a selected test project without Reqnroll
    When the declared analysis is requested
    Then its discovered test case is linked to that method and claim

  @B-002
  Scenario: xUnit theory
    Given a bound xUnit theory discovering two data rows without Reqnroll
    When the declared analysis is requested
    Then both test cases link to the same bound method without treating them as two methods

  @B-003
  Scenario: Playwright dotnet
    Given a bound Playwright .NET test hosted by the supported xUnit adapter without Reqnroll
    When the declared analysis is requested
    Then the discovered case links through its CSharp method to the declared claim

  @B-004 @boundary
  Scenario: Unsupported adapter
    Given a selected test project with no supported discovery adapter
    When the declared analysis is requested
    Then the project is reported as unsupported rather than empty complete coverage

  @B-005
  Scenario: Stable correlation
    Given two discovered cases with duplicate display names but distinct runner identities
    When the declared analysis is requested
    Then they remain distinct and correlate through runner identity and method metadata
