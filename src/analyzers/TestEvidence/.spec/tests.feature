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
    Given a bound Playwright .NET test with conforming runner evidence and no Reqnroll
    When the declared analysis is requested
    Then the discovered case links through its CSharp method to the declared claim

  @B-004 @boundary
  Scenario: Unavailable evidence
    Given a selected test project with missing collection integration or insufficient case and method metadata
    When the declared analysis is requested
    Then evidence is reported as unavailable or unresolved rather than empty complete coverage

  @B-005
  Scenario: Stable correlation
    Given two discovered cases with duplicate display names but distinct runner identities
    When the declared analysis is requested
    Then they remain distinct and correlate through runner identity and method metadata

  @B-006
  Scenario Outline: Evidence identity matching
    Given supplied "<artifact>" carrying source, assembly and configuration identities
    When it is used for current analysis
    Then its identities are checked against the current analysis inputs

    Examples:
      | artifact  |
      | discovery |
      | results   |

  @B-007
  Scenario Outline: Framework-independent runner evidence
    Given conforming runner evidence from "<framework>"
    When case and method identities are mapped
    Then the common evidence contract is used without a framework-name allowlist

    Examples:
      | framework                              |
      | xUnit                                  |
      | NUnit                                  |
      | MSTest                                 |
      | Playwright .NET hosted by a test runner |
      | another .NET test framework             |

  @B-008 @boundary
  Scenario: Runtime-only case enumeration
    Given a bound parameterized test whose cases are available only at execution
    When the runner has not supplied those cases
    Then case enumeration remains incomplete
    And no cases or case counts are invented

  @B-009
  Scenario: Versioned runner evidence
    Given collected test evidence in versioned JSON
    When its identities are read
    Then runner case IDs and method metadata are available
    And project, framework, source, assembly, configuration, adapter version and run identities are available
