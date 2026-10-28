Feature: Open-question gate
  As the reviewer starting a review round
  I want every open question that blocks a live claim listed, and approval refused while one is open
  So that a question is answered before the round that would have found it

  Background:
    Given a repository whose manifest pins a schema version carrying the open-question rules
    And a Feature specification beside one companion scenario file

  @B-001
  Scenario: A Blocks cell citing an undeclared claim is reported
    Given a specification whose claims are B-001 and B-002
    And an open question OQ-1 whose Blocks cell is "B-009"
    When the check runs
    Then the Blocks rule reports OQ-1's row, with B-009 as its identifier

  @B-002
  Scenario Outline: Each undeclared claim in a Blocks cell is reported once
    Given a specification whose claims are B-001 and B-002
    And an open question OQ-1 whose Blocks cell is "<blocks>"
    When the check runs
    Then the Blocks rule reports <findings> for OQ-1's row

    Examples:
      | blocks                  | findings                   |
      | B-001, B-002            | nothing                    |
      | B-001, B-008            | one finding, B-008         |
      | B-007; B-008 and B-001  | two findings, B-007, B-008 |
      | B-001 to B-002          | nothing                    |
      | B-001 to B-005          | one finding, B-005         |

  @B-003 @boundary
  Scenario: A claim qualified with another Feature is not resolved here
    Given a specification whose claims are B-001 and B-002
    And an open question whose Blocks cell is "`0001-F7` B-115"
    When the check runs
    Then the Blocks rule reports nothing for that row

  @B-004 @boundary
  Scenario Outline: A Blocks cell citing no claim is not reported
    Given a specification whose claims are B-001 and B-002
    And an open question whose Blocks cell is "<blocks>"
    When the check runs
    Then the Blocks rule reports nothing for that row

    Examples:
      | blocks           |
      | C-9              |
      | A-4              |
      | OQ-12            |
      | None             |
      | § 5 row 9        |

  @B-005 @boundary
  Scenario: A Blocks cell citing a withdrawn claim is not reported
    Given a specification whose claims are B-001 and B-002
    And claim B-002 is withdrawn
    And an open question whose Blocks cell is "B-002"
    When the check runs
    Then the Blocks rule reports nothing for that row

  @B-006
  Scenario: An approved specification with an open question is reported
    Given an approved specification
    And an open question OQ-2 whose Resolution is "Open" and whose Blocks cell is "B-001"
    When the check runs
    Then the approval rule reports OQ-2's row, with OQ-2 as its identifier

  @B-006 @boundary
  Scenario: An approved specification whose questions are all answered passes the gate
    Given an approved specification
    And every open question's Resolution reads "Resolved 2026-10-28 by the owner"
    When the check runs
    Then the approval rule reports nothing

  @B-007
  Scenario: An open question that blocks no claim still stops approval
    Given an approved specification
    And an open question OQ-3 whose Resolution is "Open" and whose Blocks cell is "None"
    When the check runs
    Then the approval rule reports OQ-3's row

  @B-008 @boundary
  Scenario Outline: A specification that is not approved is not held to the approval gate
    Given a specification whose spec_status is "<status>"
    And an open question whose Resolution is "Open"
    When the check runs
    Then the approval rule reports nothing

    Examples:
      | status     |
      | draft      |
      | in-review  |
      | superseded |

  @B-009
  Scenario Outline: What reads as an open question
    Given an approved specification
    And an open question whose Resolution is "<resolution>"
    When the check runs
    Then the approval rule reports <findings> for that row

    Examples:
      | resolution                                      | findings    |
      | Open                                            | one finding |
      | Open. Proposed default: keep the current form   | one finding |
      |                                                 | one finding |
      | open                                            | nothing     |
      | Resolved 2026-10-28 by the owner: keep the form | nothing     |
      | RESOLVED 2026-10-28                             | nothing     |

  @B-010
  Scenario: An open question blocking a live claim is listed before approval
    Given a draft specification whose claims are B-001 and B-002
    And an open question OQ-4 whose Resolution is "Open" and whose Blocks cell is "B-002"
    When the check runs
    Then the readiness warning reports OQ-4's row as a warning, with OQ-4 as its identifier

  @B-011 @boundary
  Scenario Outline: An open question blocking no live claim is not listed
    Given a draft specification whose claims are B-001 and B-002
    And claim B-002 is withdrawn
    And an open question whose Resolution is "Open" and whose Blocks cell is "<blocks>"
    When the check runs
    Then the readiness warning reports nothing for that row

    Examples:
      | blocks |
      | B-002  |
      | C-1    |
      | None   |

  @B-011 @boundary
  Scenario: An answered question is not listed
    Given a draft specification whose claims are B-001 and B-002
    And a question whose Resolution reads "Resolved 2026-10-28 by the owner" and whose Blocks cell is "B-001"
    When the check runs
    Then the readiness warning reports nothing for that row

  @B-012
  Scenario: An approved specification's open question is reported once
    Given an approved specification whose claims are B-001 and B-002
    And an open question whose Resolution is "Open" and whose Blocks cell is "B-001"
    When the check runs
    Then the approval rule reports that row
    And the readiness warning reports nothing

  @B-013 @boundary
  Scenario: A specification with no open-question table is not judged
    Given an approved specification whose open-questions section reads "None."
    When the check runs
    Then none of the open-question rules reports anything for that specification

  @B-014
  Scenario: A legacy-layout specification is judged the same
    Given an approved specification under "epics/0003/F1/spec.md"
    And an open question whose Resolution is "Open"
    When the check runs
    Then the approval rule reports that row

  @B-015
  Scenario: The Blocks rule and the approval rule are errors by default
    Given the manifest sets no level for the open-question rules
    And an approved specification whose claims are B-001 and B-002
    And an open question whose Resolution is "Open" and whose Blocks cell is "B-009"
    When the check runs
    Then the Blocks rule's finding is an error
    And the approval rule's finding is an error

  @B-016
  Scenario: The readiness warning is a warning by default
    Given the manifest sets no level for the open-question rules
    And a draft specification whose claims are B-001 and B-002
    And an open question whose Resolution is "Open" and whose Blocks cell is "B-001"
    When the check runs
    Then the readiness warning's finding is a warning

  @B-017 @boundary
  Scenario: Under schema 0.2.1 none of the open-question rules runs
    Given a manifest that pins schema version 0.2.1
    And an approved specification with an open question whose Blocks cell is "B-009"
    When the check runs
    Then none of the open-question rules reports anything
    And the report is the one the tool gave that tree before these rules existed
