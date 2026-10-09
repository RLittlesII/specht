Feature: The format command
  As the maintainer of a repository on the specification model
  I want one command that puts table rows and frontmatter keys in their declared order and changes nothing else
  So that ordering findings are fixed mechanically and the diff is visibly a reordering

  Background:
    Given a repository whose manifest pins schema version 2

  @B-001
  Scenario: Claims are put in id order
    Given a specification whose claims are written B-002, B-001
    When format runs
    Then the claims read B-001, B-002

  @B-002
  Scenario: Constraints are put in id order
    Given a specification whose constraints are written C-10, C-2
    When format runs
    Then the constraints read C-2, C-10

  @B-003
  Scenario: Open questions are put in id order
    Given a specification whose open questions are written OQ-2, OQ-1
    When format runs
    Then the open questions read OQ-1, OQ-2

  @B-004
  Scenario: The matrix is put in the claims' order
    Given a specification whose claims are written B-001, B-002, B-003
    And whose traceability matrix is written B-003, B-001, B-002
    When format runs
    Then the traceability matrix reads B-001, B-002, B-003

  @B-005
  Scenario: Frontmatter keys are put in the declared order
    Given a specification whose delivery status is written after its updated date
    When format runs
    Then its frontmatter keys are in the declared order
    And each key's value is as it was

  @B-006
  Scenario: A reordered table holds the same lines
    Given a specification whose claims are written B-002, B-001
    When format runs
    Then the claims table holds exactly the lines it held before, in a new order

  @B-007
  Scenario: Reordered frontmatter holds the same lines
    Given a specification whose delivery status is written after its updated date
    And whose frontmatter quotes some values and not others
    When format runs
    Then the frontmatter holds exactly the lines it held before, in a new order

  @B-008
  Scenario: Nothing outside the moved rows and keys changes
    Given a specification whose claims are written B-002, B-001
    When format runs
    Then every line outside the claims table is byte-identical to before

  @B-009
  Scenario: A tree in order is left untouched
    Given a tree whose specifications are each in order
    When format runs
    Then no file has changed

  @B-010 @boundary
  Scenario: Format writes only the files it reorders
    Given a tree holding one specification out of order and others in order
    And a snapshot of every file under the root
    When format runs
    Then the only file modified is the specification out of order
    And no file was created or deleted

  @B-011 @boundary
  Scenario: Format never renumbers an id or fills a gap
    Given a specification whose claims are written B-003, B-001 with no B-002
    When format runs
    Then the claims read B-001, B-003
    And no claim was renumbered and no row was added

  @B-012 @boundary
  Scenario: Form findings are left for their author
    Given a specification whose acceptance criteria heading is written with the title in emphasis
    And one of whose rows ends in an empty cell beyond the header's columns
    When format runs
    Then the heading and the row are as they were
    And the check still reports both form findings

  @B-013 @boundary
  Scenario: Ids in prose are left as written
    Given a specification whose lessons section lists B-003 before B-001 in a bulleted list
    When format runs
    Then the list is as it was

  @B-014
  Scenario: A run that rewrote files lists them and succeeds
    Given a tree holding two specifications out of order
    When format runs
    Then the standard output lists both files relative to the root
    And the exit code is 0

  @B-015
  Scenario: Verify mode fails on a tree it would change
    Given a tree holding one specification out of order
    And a snapshot of every file under the root
    When format runs in verify mode
    Then no file has changed
    And the standard output lists that specification relative to the root
    And the exit code is 1

  @B-016
  Scenario: Verify mode passes a tree in order
    Given a tree whose specifications are each in order
    When format runs in verify mode
    Then no file has changed
    And the exit code is 0

  @B-017
  Scenario: No derived path in the output is absolute
    Given a tree holding one specification out of order
    When format runs against a root given as an absolute path
    Then every path it prints other than the root as typed is relative to the root and uses forward slashes

  @B-018
  Scenario: The check finds nothing to reorder after format
    Given a tree whose specifications have rows and keys out of order
    When format runs
    And the check runs
    Then no ordering or key-order warning is reported

  @B-019
  Scenario: A second run changes nothing
    Given a tree whose specifications have rows and keys out of order
    And format has run once
    When format runs again
    Then no file has changed
