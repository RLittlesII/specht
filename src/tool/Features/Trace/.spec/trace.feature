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

  @B-001
  Scenario: A skipped test counts in neither number
    Given claim B-002's row is "Waived" and names two tests
    When specht trace runs
    Then the line for B-002 shows 0 of 0 tests resolved

  @B-003
  Scenario: A skipped test in the JSON trace carries no file or line
    Given claim B-002's row is "Withdrawn" and names a test a file holds
    When specht trace --json runs
    Then that test carries its name and "skipped", and no file or line

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
  Scenario: The trace writes nothing without a report path
    When specht trace runs without a report path
    Then no file is created, modified or deleted

  @B-008
  Scenario: A report path is the only file the trace writes
    When specht trace runs with a report path
    Then no file but the one at that path is created, modified or deleted

  @B-009 @boundary
  Scenario: The check's report carries no trace
    When the check runs with --json
    Then the report holds no trace of the claims

  @B-010
  Scenario: Under schema 0.1.0 the traceability columns are read by their position
    Given the manifest pins schema version 0.1.0 and declares no tests
    And the manifest renames the traceability table's headers, and the specification uses the new names
    And claim B-002's row is "Covered" and names three tests
    When specht trace runs
    Then the line for B-002 holds "Covered" and 0 of 3 tests resolved

  @B-011
  Scenario Outline: Under schema 0.1.0 a waived, withdrawn or superseded row's tests are skipped
    Given the manifest pins schema version 0.1.0 and declares no tests
    And claim B-002's row is "<marker>" and names one test
    When specht trace --json runs
    Then that test's resolution is "skipped"

    Examples:
      | marker     |
      | Waived     |
      | Withdrawn  |
      | Superseded |

  @B-012
  Scenario: Under schema 0.1.0 every test is unconfigured
    Given the manifest pins schema version 0.1.0 and declares no tests
    And claim B-001 names one test a file holds and one no file holds
    When specht trace --json runs
    Then every test's resolution is "unconfigured"

  @B-013
  Scenario: A report path receives the trace document
    When specht trace runs with a report path
    Then the file at that path is the trace document
    And it validates against the published trace schema

  @B-014
  Scenario: A report path in a new directory is created
    When specht trace runs with a report path in a directory that does not yet exist
    Then the directory exists
    And the file at that path is the trace document

  @B-015
  Scenario: A report path that exists is replaced
    Given a file already exists at the report path from an earlier run
    When specht trace runs with that report path
    Then the file at that path is this run's trace document and nothing else

  @B-016
  Scenario: A report path keeps the trace lines on the output
    When specht trace runs with a report path and without JSON output
    Then the standard output carries the same lines as a run without a report path

  @B-017
  Scenario: The JSON trace and the report file are the same document
    When specht trace runs with JSON output and a report path
    Then the document on the standard output equals the document at the path

  @B-018
  Scenario Outline: A report path does not change the exit code
    Given <input>
    When specht trace runs with a report path
    Then the exit code is <code>, the one the same run gives without a report path

    Examples:
      | input                                                         | code |
      | a claim whose row is "Covered" and whose tests do not resolve | 0    |
      | the root named does not exist                                 | 2    |
      | the manifest is one the check rejects                         | 3    |

  @B-019
  Scenario Outline: A waived, withdrawn or superseded row's tests are skipped
    Given claim B-002's row is "<marker>" and names one test a file holds and one no file holds
    When specht trace --json runs
    Then both tests' resolution is "skipped"

    Examples:
      | marker     |
      | Waived     |
      | Withdrawn  |
      | Superseded |

  @B-020
  Scenario: With no test files declared a skipped test stays skipped
    Given the manifest declares no tests
    And claim B-002's row is "Waived" and names one test
    And claim B-001's row is "Covered" and names one test
    When specht trace --json runs
    Then the test of B-002 is "skipped"
    And the test of B-001 is "unconfigured"
