Feature: Row ordering rules
  As the reader of a specification
  I want the claims, the constraints and the open questions in id order, and the traceability matrix following the claims
  So that I find a row by its id and read the matrix beside the claims

  Background:
    Given a repository whose manifest pins schema version 2

  @B-001
  Scenario: Claims out of order are reported at the first misplaced row
    Given a specification whose claims are written B-001, B-003, B-002
    When the check runs
    Then one ordering warning is reported for the claims
    And it points at the row holding B-003 and names B-002 as the claim expected there

  @B-002
  Scenario: Constraints out of order are reported
    Given a specification whose constraints are written C-2, C-1
    When the check runs
    Then one ordering warning is reported for the constraints
    And it points at the row holding C-2 and names C-1 as the constraint expected there

  @B-003
  Scenario: Open questions out of order are reported
    Given a specification whose open questions are written OQ-3, OQ-1, OQ-2
    When the check runs
    Then one ordering warning is reported for the open questions
    And it points at the row holding OQ-3 and names OQ-1 as the open question expected there

  @B-004
  Scenario: A matrix that does not follow the claims is reported
    Given a specification whose claims are written B-001, B-002, B-003
    And whose traceability matrix is written B-001, B-003, B-002
    When the check runs
    Then one ordering warning is reported for the traceability matrix
    And it points at the matrix row holding B-003 and names B-002 as the claim expected there

  @B-005
  Scenario: Ids are ordered by number, not by text
    Given a specification whose constraints are written C-1, C-2, C-10
    When the check runs
    Then no ordering warning is reported

  @B-006
  Scenario: A lettered claim follows its number
    Given a specification whose claims are written B-006, B-006b, B-007
    And whose traceability matrix follows them
    When the check runs
    Then no ordering warning is reported

  @B-007
  Scenario: A withdrawn row in its slot is in order
    Given a specification whose claims are written B-001, B-002, B-003
    And claim B-002 is withdrawn
    When the check runs
    Then no ordering warning is reported

  @B-008
  Scenario: Tables in order report nothing
    Given a specification whose claims, constraints, open questions and traceability matrix are each in order
    When the check runs
    Then no ordering warning is reported

  @B-009
  Scenario: The warning carries the order expected
    Given a specification whose claims are written B-002, B-001
    When the check runs with the report document requested
    Then the ordering warning's expected value lists B-001 then B-002

  @B-010
  Scenario: Ordering findings are warnings by default
    Given the manifest sets no severity for the ordering rule
    And a specification whose claims are written B-002, B-001
    When the check runs
    Then the ordering finding is reported as a warning
    And the exit code is 0

  @B-011
  Scenario: A disabled ordering rule reports nothing
    Given the manifest disables the ordering rule
    And a specification whose claims are written B-002, B-001
    When the check runs
    Then no ordering warning is reported

  @B-012 @boundary
  Scenario: A version 1 repository is not checked for order
    Given the manifest pins schema version 1 instead
    And a specification whose claims are written B-002, B-001
    When the check runs
    Then no ordering warning is reported

  @B-013 @boundary
  Scenario: Ids in prose are not checked for order
    Given a specification whose tables are each in order
    And whose lessons section lists B-003 before B-001 in a bulleted list
    When the check runs
    Then no ordering warning is reported
