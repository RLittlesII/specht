Feature: The onboarding report
  As a repository owner adopting Specht
  I want an explicit report that explains every finding from a check
  So that I can understand the state of the repository before choosing what to address

  Background:
    Given a repository root with a valid Specht manifest

  @B-001
  Scenario: One check supplies the complete report
    Given a repository root whose specifications would produce findings with rule ids, severities, paths, lines, and identifiers
    When an onboarding report is requested
    Then the check runs once
    And the report includes every finding from that execution
    And each finding retains its rule id, severity, repository-relative path, and line
    And each finding includes its identifier when one is present

  @B-002
  Scenario: The report explains each finding
    Given a repository root whose specifications would produce a finding for a rule
    When an onboarding report is requested
    Then the report gives the selected schema version's definition of the rule
    And the report identifies the violation message and evidence that triggered the finding

  @B-003
  Scenario: The terminal and file reports contain the same findings
    Given a repository root whose check reports findings
    When an onboarding report is requested with a path for its file
    Then a Markdown report is written at that path
    And the terminal report and the file contain the same complete set of findings from the check

  @B-004
  Scenario: The report summarizes findings by rule
    Given a repository root whose check reports findings under multiple rules
    When an onboarding report is requested
    Then the report groups findings by rule
    And the report gives the total number of findings

  @B-005
  Scenario: A clean check is explained
    Given a repository root whose check reports no findings
    When an onboarding report is requested
    Then the report says that no findings were reported

  @B-006 @boundary
  Scenario: An ordinary check keeps its existing output and exit code
    Given the check reports findings
    When the check runs without an onboarding report request
    Then its output and exit code are unchanged by this Feature

  @B-007
  Scenario Outline: A written report succeeds whatever the check found
    Given <situation>
    When an onboarding report is requested with a path for its file
    Then the report is written at that path
    And the command exits with code 0

    Examples:
      | situation                     |
      | the check reports findings    |
      | the check reports no findings |

  @B-008
  Scenario: A root that is not a directory is a missing-input failure
    Given the check reports findings
    When an onboarding report is requested for a root that does not exist, with a path for its file
    Then the reason for the failure is written to standard error
    And nothing is written to standard output
    And the command exits with code 2

  @B-009
  Scenario: A request without a path for its file is a usage failure
    Given the check reports findings
    When an onboarding report is requested without a path for its file
    Then the command's usage is written to standard error
    And nothing is written to standard output
    And the command exits with code 2

  @B-010
  Scenario: A write the file system refuses fails the request
    Given the check reports findings
    When an onboarding report is requested with a path the file system refuses to write
    Then the reason the file could not be written is written to standard error
    And nothing is written to standard output
    And the command exits with code 4

  @B-011
  Scenario: A root without a manifest is a missing-input failure
    Given the manifest has been removed from the root
    When an onboarding report is requested with a path for its file
    Then the reason for the failure is written to standard error
    And nothing is written to standard output
    And the command exits with code 2

  @B-012
  Scenario: An invalid manifest is an invalid-configuration failure
    Given the manifest has been replaced with one that is not valid
    When an onboarding report is requested with a path for its file
    Then the reason for the failure is written to standard error
    And nothing is written to standard output
    And the command exits with code 3

  @B-013
  Scenario Outline: The first of several input failures is reported
    Given <situation>
    When an onboarding report is requested <request>
    Then only the <failure> failure is reported
    And the command exits with code <code>

    Examples:
      | situation                                                 | request                                                    | failure | code |
      | the manifest has been removed from the root               | without a path for its file                                | usage   | 2    |
      | the manifest has been replaced with one that is not valid | without a path for its file                                | usage   | 2    |
      | the check reports findings                                | for a root that does not exist, without a path for its file | usage   | 2    |

  @B-014
  Scenario: A path in a new directory is created
    Given the check reports findings
    When an onboarding report is requested with a path under directories that do not exist
    Then those directories are created
    And the report is written at that path

  @B-015
  Scenario: A path that exists is replaced
    Given a file an earlier run wrote at the report's path
    When an onboarding report is requested with that path
    Then the file holds only this run's report

  @B-016
  Scenario: A relative path is resolved from the working directory
    Given a root other than the working directory
    When an onboarding report is requested for that root with a relative path for its file
    Then the report is written at that path under the working directory

  @B-017
  Scenario Outline: A request that cannot run removes an earlier file
    Given a file an earlier run wrote at the report's path
    And <situation>
    When an onboarding report is requested with that path
    Then no file is left at that path

    Examples:
      | situation                                                 |
      | the manifest has been removed from the root               |
      | the manifest has been replaced with one that is not valid |

  @B-018
  Scenario: A request that cannot run creates no directory
    Given the manifest has been removed from the root
    When an onboarding report is requested with a path under directories that do not exist
    Then none of those directories is created
