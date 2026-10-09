Feature: Frontmatter key order
  As the reader of a specification
  I want the frontmatter keys of every document of a kind in one declared order
  So that the same key is in the same place in every specification and every epic

  Background:
    Given a repository whose manifest pins schema version 2

  @B-001
  Scenario: A Feature specification's keys out of order are reported
    Given a specification whose delivery status is written after its updated date
    When the check runs
    Then one key-order warning is reported for that specification
    And it points at the first key out of place and names the key expected there

  @B-002
  Scenario: An epic file's keys out of order are reported
    Given an epic file whose children are written before its priority
    When the check runs
    Then one key-order warning is reported for that epic file
    And it points at the first key out of place and names the key expected there

  @B-003
  Scenario: Absent optional keys do not break the order
    Given a specification with no title and no description, its other keys in the declared order
    When the check runs
    Then no key-order warning is reported

  @B-004
  Scenario: The warning carries the declared order
    Given a specification whose delivery status is written after its updated date
    When the check runs with the report document requested
    Then the key-order warning's expected value lists the keys of a Feature specification in the declared order

  @B-005
  Scenario: Key-order findings are warnings by default
    Given the manifest sets no severity for the key-order rule
    And a specification whose delivery status is written after its updated date
    When the check runs
    Then the key-order finding is reported as a warning

  @B-006
  Scenario: A disabled key-order rule reports nothing
    Given the manifest disables the key-order rule
    And a specification whose delivery status is written after its updated date
    When the check runs
    Then no key-order warning is reported

  @B-007 @boundary
  Scenario: A version 1 repository is not checked for key order
    Given the manifest pins schema version 1 instead
    And a specification whose delivery status is written after its updated date
    When the check runs
    Then no key-order warning is reported

  @B-008 @boundary
  Scenario: The style of a value is not judged
    Given a specification whose keys are in the declared order
    And whose dates are quoted while its identifier is bare
    When the check runs
    Then no key-order warning is reported
