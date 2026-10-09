Feature: Retired-id rule
  As whoever follows a citation into a specification
  I want every id ever issued to keep its row, marked when it is retired
  So that no citation points at nothing and no number is taken twice

  Background:
    Given a repository whose manifest pins schema version 2

  @B-001
  Scenario: A skipped claim number is reported
    Given a specification whose claims are B-001 and B-003
    When the check runs
    Then a retired-id warning names B-002 as an id with no row

  @B-002
  Scenario: A skipped constraint number is reported
    Given a specification whose constraints are C-1, C-2 and C-5
    When the check runs
    Then retired-id warnings name C-3 and C-4 as ids with no row

  @B-003
  Scenario: A skipped open-question number is reported
    Given a specification whose open questions are OQ-2 and OQ-3
    When the check runs
    Then a retired-id warning names OQ-1 as an id with no row

  @B-004
  Scenario: A sequence that starts above one is reported
    Given a specification whose claims are B-002 and B-003
    When the check runs
    Then a retired-id warning names B-001 as an id with no row

  @B-005
  Scenario: A lettered claim is not a gap
    Given a specification whose claims are B-005, B-006, B-006b and B-007, with every lower number present
    When the check runs
    Then no retired-id warning is reported

  @B-006
  Scenario: A complete sequence with withdrawn rows reports nothing
    Given a specification whose claims are B-001, B-002 and B-003
    And claim B-002 is withdrawn
    When the check runs
    Then no retired-id warning is reported

  @B-007 @boundary
  Scenario: The finding asks for the row back, never a renumbering
    Given a specification whose claims are B-001 and B-003
    When the check runs with the report document requested
    Then the warning's expected value names B-002 and the marker a restored row carries
    And it does not propose renumbering B-003

  @B-008
  Scenario: Retired-id findings are warnings by default
    Given the manifest sets no severity for the retired-id rule
    And a specification whose claims are B-001 and B-003
    When the check runs
    Then the retired-id finding is reported as a warning

  @B-009
  Scenario: A disabled retired-id rule reports nothing
    Given the manifest disables the retired-id rule
    And a specification whose claims are B-001 and B-003
    When the check runs
    Then no retired-id warning is reported

  @B-010 @boundary
  Scenario: A version 1 repository is not checked for gaps
    Given the manifest pins schema version 1 instead
    And a specification whose claims are B-001 and B-003
    When the check runs
    Then no retired-id warning is reported
