Feature: Section stamp form
  As the reader of a specification
  I want each section's stamp to hold one role and one date
  So that I see who wrote the section last without reading who wrote it before

  Background:
    Given a repository whose manifest pins a schema version that carries the section stamp rule
    And the roles the repository's manifest knows are spec-author, implementer, test-writer and spec-reviewer

  @B-001
  Scenario Outline: A stamp that lists more than one date is reported
    Given a specification with a section stamped "<stamp>"
    When the check runs
    Then one stamp finding is reported, on the line that stamp starts on

    Examples:
      | stamp                                                             |
      | spec-author, 2026-10-09, 2026-10-11                               |
      | spec-author, 2026-10-09, 2026-10-11 (row 4)                       |
      | spec-author, 2026-10-09 (row 2 added), 2026-10-11 (row 4 amended) |
      | spec-author, 2026-10-08, 2026-10-09 (row 2), 2026-10-11 (row 4)   |

  @B-002
  Scenario Outline: A stamp that names more than one role is reported
    Given a specification with a section stamped "<stamp>"
    When the check runs
    Then one stamp finding is reported, on the line that stamp starts on

    Examples:
      | stamp                                |
      | test-writer; spec-author, 2026-10-11 |
      | test-writer, spec-author, 2026-10-11 |

  @B-003
  Scenario: A stamp with several roles and several dates gets one finding
    Given a specification with a section stamped "test-writer, 2026-10-10; spec-author, 2026-10-11 (one row added)"
    When the check runs
    Then exactly one stamp finding is reported for that stamp

  @B-004
  Scenario: A stamp with one role and one date reports nothing
    Given a specification with a section stamped "spec-author, 2026-10-11"
    When the check runs
    Then no stamp finding is reported

  @B-004 @boundary
  Scenario: A stamp naming a role that does not own the section is not reported
    Given a specification whose acceptance criteria are stamped "implementer, 2026-10-11"
    When the check runs
    Then no stamp finding is reported

  @B-005
  Scenario: A stamp with one trailing note reports nothing
    Given a specification with a section stamped "spec-author, 2026-10-11 (row 4)"
    When the check runs
    Then no stamp finding is reported

  @B-006
  Scenario: A date inside the note is not counted
    Given a specification with a section stamped "test-writer, 2026-10-11 (every other row as it was left on 2026-10-09)"
    When the check runs
    Then no stamp finding is reported

  @B-007
  Scenario Outline: A role name inside the note is not counted
    Given a specification with a section stamped "<stamp>"
    When the check runs
    Then no stamp finding is reported

    Examples:
      | stamp                                                                 |
      | test-writer, 2026-10-11 (every other row as spec-author left it)      |
      | spec-author, 2026-10-11 (row 4 amended; after spec-reviewer, round 5) |

  @B-008
  Scenario: A second stamp under a sub-heading is judged on its own
    Given a specification whose testing section is stamped "test-writer, 2026-10-10"
    And a sub-heading of that section is stamped "implementer, 2026-10-11"
    When the check runs
    Then no stamp finding is reported

  @B-008
  Scenario: A listed stamp under a sub-heading is the only one reported
    Given a specification whose testing section is stamped "test-writer, 2026-10-10"
    And a sub-heading of that section is stamped "implementer, 2026-10-10, 2026-10-11"
    When the check runs
    Then one stamp finding is reported, on the line the sub-heading's stamp starts on

  @B-009
  Scenario: A stamp still carrying the template's date placeholder reports nothing
    Given a specification with a section stamped with the role spec-author and the template's unfilled date placeholder
    When the check runs
    Then no stamp finding is reported

  @B-010
  Scenario Outline: The finding says how many dates and roles the stamp holds
    Given a specification with a section stamped "<stamp>"
    When the check runs
    Then the stamp finding counts <dates> for the stamp's dates and <roles> for its roles

    Examples:
      | stamp                                                                | dates | roles |
      | spec-author, 2026-10-09, 2026-10-11                                  | 2     | 1     |
      | test-writer; spec-author, 2026-10-11                                 | 1     | 2     |
      | test-writer, 2026-10-10; spec-author, 2026-10-11 (one row added)     | 2     | 2     |
      | spec-author, 2026-10-08, 2026-10-09 (written 2026-10-07), 2026-10-11 | 3     | 1     |

  @B-011
  Scenario: The finding's path is relative to the repository
    Given a specification with a section stamped "spec-author, 2026-10-09, 2026-10-11"
    When the check runs
    Then the stamp finding names the specification by a path that starts at the repository root and is written with forward slashes

  @B-012
  Scenario: The same date listed twice counts twice
    Given a specification with a section stamped "spec-author, 2026-10-10 (row 2), 2026-10-10 (row 4)"
    When the check runs
    Then the stamp finding counts 2 for the stamp's dates and 1 for its roles

  @B-013
  Scenario: Stamp findings are warnings by default
    Given the manifest sets no level for the section stamp rule
    And a specification with a section stamped "spec-author, 2026-10-09, 2026-10-11"
    When the check runs
    Then the stamp finding is reported as a warning

  @B-013
  Scenario Outline: A stamp warning fails the run only when warnings are made to fail
    Given the manifest sets no level for the section stamp rule
    And a specification whose only fault is a section stamped "spec-author, 2026-10-09, 2026-10-11"
    When the check runs <mode>
    Then the run <outcome>

    Examples:
      | mode                               | outcome |
      | as it is                           | passes  |
      | with warnings made to fail the run | fails   |

  @B-014
  Scenario: A disabled stamp rule reports nothing
    Given the manifest disables the section stamp rule
    And a specification with a section stamped "spec-author, 2026-10-09, 2026-10-11"
    When the check runs
    Then no stamp finding is reported

  @B-015
  Scenario: A stamp rule raised to an error reports errors
    Given the manifest sets the section stamp rule to error
    And a specification with a section stamped "spec-author, 2026-10-09, 2026-10-11"
    When the check runs
    Then the stamp finding is reported as an error

  @B-016 @boundary
  Scenario: A repository pinned to 0.1.0 is not checked for stamps
    Given the manifest pins schema version 0.1.0 instead
    And a specification with a section stamped "spec-author, 2026-10-09, 2026-10-11"
    When the check runs
    Then no stamp finding is reported

  @B-017 @boundary
  Scenario Outline: A stamp quoted as code is not a stamp
    Given a specification whose prose quotes, as <code>, a stamp that lists two dates
    When the check runs
    Then no stamp finding is reported

    Examples:
      | code         |
      | inline code  |
      | a code block |

  @B-018
  Scenario: The finding proposes no replacement stamp
    Given a specification with a section stamped "spec-author, 2026-10-09, 2026-10-11"
    When the check runs
    Then the stamp finding offers no text to put in the stamp's place

  @B-019
  Scenario: The check leaves the specification as it was
    Given a specification with a section stamped "spec-author, 2026-10-09, 2026-10-11"
    When the check runs
    Then every file of the repository is as it was before the check ran

  @B-020
  Scenario Outline: A stamp is judged in either specification layout
    Given a specification in the <layout> layout with a section stamped "spec-author, 2026-10-09, 2026-10-11"
    When the check runs
    Then one stamp finding is reported, on the line that stamp starts on

    Examples:
      | layout     |
      | co-located |
      | legacy     |

  @B-023 @boundary
  Scenario: A date the check cannot read is not reported
    Given a specification with a section stamped "spec-author, the eleventh of October"
    When the check runs
    Then no stamp finding is reported

  @B-021 @boundary
  Scenario: Text after the note is not reported
    Given a specification with a section stamped "spec-author, 2026-10-11 (row 4) and the table re-padded"
    When the check runs
    Then no stamp finding is reported

  @B-021 @boundary
  Scenario Outline: Text after the note that mentions a role or a date is not reported
    Given a specification with a section stamped "<stamp>"
    When the check runs
    Then no stamp finding is reported

    Examples:
      | stamp                                                                 |
      | spec-author, 2026-10-11 (row 4) after spec-reviewer round 5           |
      | spec-author, 2026-10-11 (row 4); row 6 amended, spec-reviewer round 5 |
      | spec-author, 2026-10-11 (row 4) as it stood on 2026-10-09             |

  @B-021 @boundary
  Scenario: A second note is not reported
    Given a specification with a section stamped "spec-author, 2026-10-11 (row 4) (row 6)"
    When the check runs
    Then no stamp finding is reported

  @B-022 @boundary
  Scenario: A section with no stamp is not reported
    Given a specification with a section that carries no stamp
    When the check runs
    Then no stamp finding is reported
