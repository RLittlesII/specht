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

  @B-010
  Scenario: VSTest theory discovery
    Given a bound xUnit v2 theory with two inline data rows in a VSTest test project
    When its cases are discovered under VSTest
    Then each theory case is recorded with its own stable runner case identity
    And both cases link to the same bound method

  @B-011
  Scenario: VSTest theory execution
    Given a bound xUnit v2 theory with two inline data rows in a VSTest test project
    When its cases are executed under VSTest
    Then each case's outcome is recorded against that case's own runner identity

  @B-012
  Scenario: Method compiled from outside the project
    Given a bound test method whose source file sits outside its test project's directory and is linked into that project
    When its cases are discovered
    Then the method identity resolves to the bound method
    And its source location is the linked file's repository-relative path

  @B-013 @boundary
  Scenario Outline: Nonpassing VSTest outcomes
    Given a bound VSTest case that <outcome>
    When its evidence is collected
    Then the case is recorded with that outcome
    And the case is never recorded as passed

    Examples:
      | outcome          |
      | failed           |
      | was skipped      |
      | was not executed |

  @B-014 @boundary
  Scenario: Undiscovered VSTest cases
    Given a bound test method for which VSTest discovered no case
    When its evidence is collected
    Then its discovery is reported as unavailable or incomplete
    And it is reported as neither passing nor a complete empty set

  @B-015 @boundary
  Scenario Outline: One case never masks another
    Given a bound xUnit v2 theory whose first case passed and whose second case <outcome>
    When its evidence is collected
    Then each case keeps its own outcome
    And no single method-level outcome replaces them

    Examples:
      | outcome          |
      | failed           |
      | was skipped      |
      | was not executed |

  @B-016
  Scenario: Reqnroll-generated VSTest cases
    Given Reqnroll-generated tests for a Scenario Outline with two example rows executed under VSTest
    When their evidence is collected
    Then both generated cases and their outcomes are recorded in the common evidence contract
    And no binding is required on the generated code

  @B-017 @boundary
  Scenario: Clean checkout without evidence
    Given a clean checkout with no evidence artifact
    When the projects are built and analyzed
    Then the build succeeds
    And the analysis reports the evidence as missing with a diagnosis naming the step that produces it

  @B-018 @boundary
  Scenario: Stale evidence
    Given an evidence artifact produced before a test source file changed
    When the analysis is requested
    Then the evidence is reported as stale with a diagnosis naming the step that regenerates it

  @B-019
  Scenario: Clean-checkout bootstrap
    Given a clean checkout with no evidence artifact and no evidence cache
    When the documented build, discovery-and-run and analysis sequence is followed
    Then the analysis consumes current evidence produced by that sequence
    And no hand-written evidence or diagnostic suppression is needed

  @B-020 @B-021
  Scenario: Partitioned runs over shared claim files
    Given two acceptance projects compile the same central claim files
    And the first project's run selects "Category!=visual"
    And the second project's run selects "Category=visual"
    When both runs' evidence is collected
    Then each artifact records the cases its run discovered and, separately, the cases its run selected
    And every case keeps its project, assembly and source provenance
    And the combined evidence holds both runs' selections and outcomes
