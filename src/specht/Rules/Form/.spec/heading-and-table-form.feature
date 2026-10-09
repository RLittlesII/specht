Feature: Heading and table form
  As the reader of a specification
  I want every contracted heading and every table written the same way
  So that each specification reads like every other and a diff shows only what changed

  Background:
    Given a repository whose manifest pins schema version 2

  @B-001
  Scenario Outline: A contracted heading carrying markup is reported
    Given a specification whose acceptance criteria heading is written <form>
    When the check runs
    Then a form warning on that heading's line names the heading as the manifest writes it

    Examples:
      | form                                       |
      | with the title in emphasis                 |
      | with the title in inline code              |
      | with a closing run of hash marks           |
      | with two spaces after the heading marker   |

  @B-002
  Scenario: A table with no delimiter row is reported
    Given a specification whose constraints are written as table rows with no delimiter row under the header
    When the check runs
    Then a form warning on the constraints heading says its table has no delimiter row

  @B-003
  Scenario: A row ending in empty cells is reported
    Given a specification whose claim B-002 row ends in an empty cell beyond the header's columns
    When the check runs
    Then a form warning is reported on that row's line

  @B-004
  Scenario: A constraint row without exactly one id is reported
    Given a specification whose constraints include a row whose first cell reads "C-3, C-4"
    When the check runs
    Then a form warning is reported on that row's line

  @B-005
  Scenario: An open-question row without exactly one id is reported
    Given a specification whose open questions include a row whose first cell reads "Question 2"
    When the check runs
    Then a form warning is reported on that row's line

  @B-006
  Scenario: A claim or matrix row with an empty first cell is reported
    Given a specification whose traceability matrix holds a row with an empty first cell
    When the check runs
    Then a form warning is reported on that row's line

  @B-007
  Scenario: Form findings are warnings by default
    Given the manifest sets no severity for the form rules
    And a specification whose acceptance criteria heading is written with the title in emphasis
    When the check runs
    Then the form finding is reported as a warning

  @B-008
  Scenario: Disabled form rules report nothing
    Given the manifest disables the form rules
    And a specification whose acceptance criteria heading is written with the title in emphasis
    When the check runs
    Then no form warning is reported

  @B-009 @boundary
  Scenario: A version 1 repository is not checked for form
    Given the manifest pins schema version 1 instead
    And a specification whose acceptance criteria heading is written with the title in emphasis
    When the check runs
    Then no form warning is reported

  @B-010 @boundary
  Scenario: A misspelt heading is left to the section rule
    Given a specification whose acceptance criteria heading is misspelt
    When the check runs
    Then the section rule reports the contracted heading as missing
    And no form warning is reported for that heading

  @B-011 @boundary
  Scenario: A claim cell with two ids is left to the claim rule
    Given a specification whose claims include a row whose first cell reads "B-001, B-002"
    When the check runs
    Then the claim rule reports the cell
    And no form warning is reported for that row

  @B-012 @boundary
  Scenario: Column padding is not checked
    Given a specification whose tables are not padded to even column widths
    When the check runs
    Then no form warning is reported
