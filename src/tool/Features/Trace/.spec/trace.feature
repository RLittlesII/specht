Feature: The trace command
  As the maintainer reviewing where every claim stands
  I want one listing of each claim's status, scenarios and tests
  So that I see the whole matrix held against the tree, not only the rows that fail

  Background:
    Given a repository whose manifest pins schema version 0.2.0
    And a Feature specification with three claims beside one companion scenario file
    And the manifest declares the test files "test/**/*.cs"

  @B-001
  Scenario: The trace prints one line per claim
    Given claim B-002's row is "Covered", two scenarios carry its tag, and it names three tests of which two resolve
    When specht trace runs
    Then it prints three lines for that specification
    And the line for B-002 holds the specification's path, "B-002", "Covered", 2 scenarios and 2 of 3 tests resolved

  @B-002
  Scenario: The JSON trace validates against its schema
    When specht trace --json runs
    Then stdout is one JSON document
    And the document validates against the published trace schema

  @B-001
  Scenario: A claim with no traceability row shows no status and no tests
    Given claim B-003 has no traceability row
    When specht trace runs
    Then the line for B-003 shows the status "none" and 0 of 0 tests resolved

  @B-003
  Scenario: The JSON trace carries each claim's row, scenarios and tests
    Given claim B-001 names one test that resolves and one that does not
    When specht trace --json runs
    Then the specification carries its path, its id and its three claims
    And claim B-001 carries its line, its claim status, its matrix row's line and four cells, and each scenario's title, file and line
    And the test that resolves carries its name, "resolved", its file and its line
    And the test that does not carries its name and "unresolved", and no file or line

  @B-004
  Scenario: With no test files declared every test is unconfigured
    Given the manifest declares no tests
    And claim B-001 names a test no file holds
    When specht trace --json runs
    Then every test's resolution is "unconfigured"
    And no test's resolution is "unresolved"

  @B-005
  Scenario: A rule switched off does not hide a resolution
    Given the manifest sets the unresolved-test rule to off
    And claim B-001 names a test no file holds
    When specht trace --json runs
    Then that test's resolution is "unresolved"

  @B-006
  Scenario: The trace succeeds whatever gaps it shows
    Given a claim whose row is "Covered" and whose tests do not resolve
    When specht trace runs
    Then the exit code is 0

  @B-006
  Scenario Outline: The trace fails as the check does on its inputs
    Given <input>
    When specht trace runs
    Then the exit code is <code>

    Examples:
      | input                                     | code |
      | the root named does not exist             | 2    |
      | the root holds no manifest                | 2    |
      | the manifest is one the check rejects     | 3    |

  @B-007
  Scenario: The same tree gives the same trace
    Given a second Feature specification discovered after the first
    When specht trace --json runs twice
    Then the two outputs are byte-for-byte the same
    And the specifications appear in discovery order and each one's claims in claim order

  @B-008
  Scenario: The trace writes nothing
    When specht trace runs
    Then no file is created, modified or deleted

  @B-009 @boundary
  Scenario: The check's report carries no trace
    When the check runs with --json
    Then the report holds no trace of the claims
