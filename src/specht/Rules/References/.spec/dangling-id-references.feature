Feature: Dangling id references
  As the reviewer following a citation inside a specification
  I want every unqualified claim, constraint and open-question id to name a row of that specification
  So that a stale cross-reference is reported where it stands, not found in the next review round

  Background:
    Given a repository whose manifest pins a schema version carrying the dangling-citation rule
    And a Feature specification whose claims are B-001 and B-002, whose constraints are C-1 and C-2, and whose open questions are OQ-1 and OQ-2

  @B-001
  Scenario: A claim cited and never declared is reported
    Given the specification's prose cites B-009
    When the check runs
    Then the dangling-citation rule reports that line, with B-009 as its identifier

  @B-001 @boundary
  Scenario: A claim cited and declared is not reported
    Given the specification's prose cites B-002
    When the check runs
    Then the dangling-citation rule reports nothing

  @B-002
  Scenario: A constraint cited and never declared is reported
    Given the specification's prose cites C-7
    When the check runs
    Then the dangling-citation rule reports that line, with C-7 as its identifier

  @B-003
  Scenario: An open question cited and never declared is reported
    Given the specification's prose cites OQ-5
    When the check runs
    Then the dangling-citation rule reports that line, with OQ-5 as its identifier

  @B-004
  Scenario Outline: A citation is read wherever the body holds it
    Given the specification cites B-009 in <place>
    When the check runs
    Then the dangling-citation rule reports that line

    Examples:
      | place                        |
      | a paragraph                  |
      | a list item                  |
      | a constraint's Rules Out cell |
      | a claim's Source cell        |
      | a section stamp's note       |

  @B-005 @boundary
  Scenario Outline: An id in code is not a citation
    Given the specification holds B-009 only inside <code>
    When the check runs
    Then the dangling-citation rule reports nothing

    Examples:
      | code                |
      | an inline-code span |
      | a fenced code block |

  @B-006 @boundary
  Scenario: An id in frontmatter is not a citation
    Given the specification's frontmatter description names B-009
    When the check runs
    Then the dangling-citation rule reports nothing

  @B-007 @boundary
  Scenario: A row's own id is a declaration
    Given the specification's prose cites no id
    When the check runs
    Then the dangling-citation rule reports nothing for the rows that declare B-001, C-1 and OQ-1

  @B-008 @boundary
  Scenario: A traceability row's claim id is left to its own rule
    Given a traceability row for B-009, a claim the specification does not declare
    When the check runs
    Then the dangling-citation rule reports nothing for that row
    And the traceability rule reports it

  @B-009 @boundary
  Scenario: A claim in a Blocks cell is left to the Blocks rule
    Given an open question whose Blocks cell is "B-009"
    When the check runs
    Then the dangling-citation rule reports nothing for that row

  @B-010
  Scenario: A constraint in a Blocks cell is resolved
    Given an open question whose Blocks cell is "B-001; C-7"
    When the check runs
    Then the dangling-citation rule reports that row, with C-7 as its identifier

  @B-011 @boundary
  Scenario Outline: An id qualified with another Feature is not resolved here
    Given the specification's prose reads "<text>"
    When the check runs
    Then the dangling-citation rule reports nothing

    Examples:
      | text                                  |
      | as `0001-F7` C-11 requires            |
      | as 0001-F7 C-11 requires              |
      | `0001-F5` B-055, B-058 and OQ-24      |

  @B-012
  Scenario Outline: A Feature key qualifies ids up to the end of its clause
    Given the specification's prose reads "<text>"
    When the check runs
    Then the dangling-citation rule reports <findings>

    Examples:
      | text                                    | findings          |
      | `0001-F5` B-055; B-009                  | B-009             |
      | `0001-F5` B-055. Then B-009 holds.      | B-009             |
      | (`0001-F5` B-055) and B-009             | B-009             |
      | `0001-F5` B-055, `0001-F7` C-11, C-9    | nothing           |
      | `0001-F5` B-055; B-002                  | nothing           |

  @B-013
  Scenario Outline: A range cites its two ends
    Given the specification's prose cites "<range>"
    When the check runs
    Then the dangling-citation rule reports <findings>

    Examples:
      | range          | findings   |
      | B-001 to B-002 | nothing    |
      | B-001 to B-009 | B-009      |

  @B-014 @boundary
  Scenario: A citation of a withdrawn claim resolves
    Given claim B-002 is withdrawn
    And the specification's prose cites B-002
    When the check runs
    Then the dangling-citation rule reports nothing

  @B-015 @boundary
  Scenario: The companion scenarios are not read
    Given the companion scenario file names B-009 in a step
    When the check runs
    Then the dangling-citation rule reports nothing for the companion

  @B-016 @boundary
  Scenario Outline: A record beside the specification is not read
    Given a <record> beside the specification cites B-009
    When the check runs
    Then the dangling-citation rule reports nothing

    Examples:
      | record          |
      | decision record |
      | ADR             |
      | lesson          |

  @B-017 @boundary
  Scenario: Constraint citations are not resolved against ordinal constraint rows
    Given a specification whose constraint rows are numbered 1, 2 and 3
    And its prose cites C-7
    When the check runs
    Then the dangling-citation rule reports nothing for C-7

  @B-018
  Scenario: An undeclared id is reported once at each line that cites it
    Given the specification's prose cites B-009 on two lines, and twice on the second
    When the check runs
    Then the dangling-citation rule reports B-009 twice, once at each line

  @B-019
  Scenario: A legacy-layout specification is judged the same
    Given the specification sits at "epics/0003/F1/spec.md"
    And its prose cites B-009
    When the check runs
    Then the dangling-citation rule reports that line

  @B-020
  Scenario: The dangling-citation rule is a warning by default
    Given the manifest sets no level for the dangling-citation rule
    And the specification's prose cites B-009
    When the check runs
    Then the dangling-citation finding is a warning

  @B-021 @boundary
  Scenario: Under schema 0.2.1 the dangling-citation rule does not run
    Given a manifest that pins schema version 0.2.1
    And the specification's prose cites B-009
    When the check runs
    Then the dangling-citation rule reports nothing
    And the report is the one the tool gave that tree before this rule existed
