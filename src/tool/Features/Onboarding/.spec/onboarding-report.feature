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
    Then the report explains what the rule checks
    And the report identifies the violation message and evidence that triggered the finding

  @B-003
  Scenario: The terminal and file reports contain the same findings
    Given a repository root whose check reports findings
    When an onboarding report is requested
    Then both outputs contain the complete set of findings from the check

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
