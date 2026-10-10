Feature: In-specification id uniqueness
  As whoever follows a citation into a specification
  I want every constraint id and open-question id declared exactly once
  So that a citation names one row and a number taken twice is caught by the check

  Background:
    Given a repository whose manifest pins a schema version that carries the duplicate-id rules

  @B-001
  Scenario: A constraint id declared twice is reported
    Given a specification whose constraints are C-1, C-2 and C-2
    When the check runs
    Then a duplicate-id finding names C-2 as a constraint declared twice

  @B-002
  Scenario: An open-question id declared twice is reported
    Given a specification whose open questions are OQ-1, OQ-2 and OQ-2
    When the check runs
    Then a duplicate-id finding names OQ-2 as an open question declared twice

  @B-003
  Scenario: Every row after the first is reported
    Given a specification whose constraints are C-1, C-1 and C-1
    When the check runs
    Then two duplicate-id findings are reported, one on each row after the first

  @B-003
  Scenario: The second of two rows carries the finding
    Given a specification whose constraints are C-1, C-2 and C-2
    When the check runs
    Then one duplicate-id finding is reported, on the second C-2 row

  @B-004
  Scenario: The finding names the id and where it was first declared
    Given a specification whose constraints are C-1, C-2 and C-2
    When the check runs
    Then the duplicate-id finding names C-2 and the line of the first row that declares it

  @B-005
  Scenario: A withdrawn id taken again is a duplicate
    Given a specification whose constraints are C-1, C-2 and C-2
    And the first C-2 row is marked withdrawn
    When the check runs
    Then a duplicate-id finding is reported on the second C-2 row

  @B-006 @boundary
  Scenario: An id cited outside a first cell is not a declaration
    Given a specification whose constraints are C-1 and C-2
    And the C-2 row cites C-1 in its text
    And a paragraph of the specification cites C-1
    When the check runs
    Then no duplicate-id finding is reported

  @B-007 @boundary
  Scenario: Repeated first cells that are not ids are left to the form rule
    Given a specification whose constraint rows are numbered 3 and 3, with no constraint id
    When the check runs
    Then no duplicate-id finding is reported

  @B-008 @boundary
  Scenario: A claim id declared twice is left to the claim rule
    Given a specification whose claims are B-001, B-002 and B-002
    When the check runs
    Then no duplicate-id finding is reported
    And SPEC030 reports B-002 once

  @B-009 @boundary
  Scenario: The same id in two specifications is not a duplicate
    Given two specifications that each declare the constraint C-1 once
    When the check runs
    Then no duplicate-id finding is reported

  @B-010
  Scenario Outline: A section with no table reports nothing
    Given a specification whose <section> section holds no table
    When the check runs
    Then no duplicate-id finding is reported

    Examples:
      | section        |
      | constraints    |
      | open questions |

  @B-011
  Scenario: Ids each declared once report nothing
    Given a specification whose constraints are C-1, C-2 and C-3
    And whose open questions are OQ-1 and OQ-2
    When the check runs
    Then no duplicate-id finding is reported

  @B-012
  Scenario: Duplicate-id findings are errors by default
    Given the manifest sets no severity for the duplicate-id rules
    And a specification whose constraints are C-1, C-2 and C-2
    When the check runs
    Then the duplicate-id finding is reported as an error

  @B-013
  Scenario: Disabled duplicate-id rules report nothing
    Given the manifest disables the duplicate-id rules
    And a specification whose constraints are C-1, C-2 and C-2
    When the check runs
    Then no duplicate-id finding is reported

  @B-014 @boundary
  Scenario: A repository pinned to 0.1.0 is not checked for duplicate ids
    Given the manifest pins schema version 0.1.0 instead
    And a specification whose constraints are C-1, C-2 and C-2
    When the check runs
    Then no duplicate-id finding is reported

  @B-015 @boundary
  Scenario: Ids that differ only by leading zeros are two ids
    Given a specification whose constraints are C-1 and C-01
    When the check runs
    Then no duplicate-id finding is reported
