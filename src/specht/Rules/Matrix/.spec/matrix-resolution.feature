Feature: Matrix resolution
  As the reviewer reading a traceability matrix
  I want each row's scenario, test names and status held against the tree
  So that a row the scenarios and the tests do not bear out is reported where it stands

  Background:
    Given a repository whose manifest pins schema version 0.2.0
    And a Feature specification beside one companion scenario file

  @B-001
  Scenario: A renamed matrix header keeps its column's role
    Given the manifest names the matrix headers "Claim", "Scenario", "Tests" and "State"
    And the specification's matrix uses those headers
    And a row whose third cell names a test no declared file holds
    And the manifest declares the test files
    When the check runs
    Then the unresolved-test rule reports that row

  @B-002
  Scenario: A test name no declared file holds is reported
    Given the manifest declares the test files "test/**/*.cs"
    And a matrix row for claim B-001 whose Test cell is "`ScenarioNobodyWrote`"
    And no file under test holds that name
    When the check runs
    Then the unresolved-test rule reports that row, with B-001 as its identifier

  @B-002
  Scenario: A test name held only in an excluded file is reported
    Given the manifest declares the test files "test/**/*.cs"
    And the manifest excludes "obj"
    And a matrix row whose Test cell names a test held only under "test/obj/"
    When the check runs
    Then the unresolved-test rule reports that row

  @B-002 @boundary
  Scenario: A row marked missing has its Test cell left unread
    Given the manifest declares the test files "test/**/*.cs"
    And a matrix row whose Test cell is the missing marker
    When the check runs
    Then the unresolved-test rule reports nothing for that row

  @B-002 @boundary
  Scenario Outline: A waived, withdrawn or superseded row has its Test cell left unread
    Given the manifest declares the test files "test/**/*.cs"
    And a matrix row whose Status is "<status>" and whose Test cell is "<cell>"
    When the check runs
    Then the unresolved-test rule reports nothing for that row

    Examples:
      | status     | cell                          |
      | Waived     | spec-reviewer, 2026-10-14     |
      | Withdrawn  | `ScenarioNobodyWrote`         |
      | Superseded | replaced by B-009             |

  @B-003
  Scenario Outline: Test names are the inline-code spans of the Test cell
    Given the manifest declares the test files "test/**/*.cs"
    And a test file holding "AlphaTest" and "BetaTest"
    And a matrix row whose Test cell is "<cell>"
    When the check runs
    Then the unresolved-test rule reports <findings> for that row

    Examples:
      | cell                              | findings                |
      | `AlphaTest`, `BetaTest`           | nothing                 |
      | `AlphaTest; BetaTest`             | nothing                 |
      | `AlphaTest, GammaTest`            | one finding, GammaTest  |
      | `AlphaTest` and the GammaTest one | nothing                 |

  @B-004
  Scenario: A dotted test name resolves when one file holds both parts
    Given the manifest declares the test files "test/**/*.cs"
    And a test file holding the class "ReportSteps" and the method "CarriesExpected"
    And a matrix row whose Test cell is "`ReportSteps.CarriesExpected`"
    When the check runs
    Then the unresolved-test rule reports nothing for that row

  @B-004
  Scenario: A dotted test name whose parts sit in different files is reported
    Given the manifest declares the test files "test/**/*.cs"
    And one test file holding the class "ReportSteps" and another holding the method "CarriesExpected"
    And a matrix row whose Test cell is "`ReportSteps.CarriesExpected`"
    When the check runs
    Then the unresolved-test rule reports that row

  @B-004
  Scenario Outline: A name of three dotted segments needs every earlier segment in the same file
    Given the manifest declares the test files "test/**/*.cs"
    And a test file holding the method "CarriesExpected" and the words <words>
    And a matrix row whose Test cell is "`Report.Steps.CarriesExpected`"
    When the check runs
    Then the unresolved-test rule reports <findings> for that row

    Examples:
      | words                 | findings    |
      | "Report" and "Steps"  | nothing     |
      | "Steps" alone         | one finding |

  @B-005
  Scenario: With no declared form a test name resolves as a whole word
    Given the manifest declares the test files "test/**/*.cs" and no declaration forms
    And a test file holding the word "AlphaTest"
    And a matrix row whose Test cell is "`AlphaTest`"
    When the check runs
    Then the unresolved-test rule reports nothing for that row

  @B-005 @boundary
  Scenario: A test name inside a longer word does not resolve
    Given the manifest declares the test files "test/**/*.cs" and no declaration forms
    And a test file holding the word "AlphaTestCase" and not the word "AlphaTest"
    And a matrix row whose Test cell is "`AlphaTest`"
    When the check runs
    Then the unresolved-test rule reports that row

  @B-005
  Scenario: With a declared form only a declaration resolves the name
    Given the manifest declares the test files "test/**/*.fs" and the declaration form "let {test} "
    And a test file holding "let F2_B005_LookForRoleCarriesReport "
    And a matrix row whose Test cell is "`F2_B005_LookForRoleCarriesReport`"
    When the check runs
    Then the unresolved-test rule reports nothing for that row

  @B-005 @boundary
  Scenario: With a declared form a mention outside it, as in a comment, does not resolve the name
    Given the manifest declares the test files "test/**/*.fs" and the declaration form "let {test} "
    And a test file that holds "F2_B005_LookForRoleCarriesReport" only in a comment
    And a matrix row whose Test cell is "`F2_B005_LookForRoleCarriesReport`"
    When the check runs
    Then the unresolved-test rule reports that row

  @B-006
  Scenario: A Test cell with no inline code is reported
    Given the manifest declares the test files "test/**/*.cs"
    And a matrix row whose Test cell is "AlphaTest" with no inline code
    When the check runs
    Then the unresolved-test rule reports that row, expecting a test name in inline code

  @B-007 @boundary
  Scenario: With no test files declared no test name is reported
    Given the manifest declares no tests
    And a matrix row whose Test cell is "`ScenarioNobodyWrote`"
    When the check runs
    Then the unresolved-test rule reports nothing

  @B-008
  Scenario: A Scenario cell that matches no scenario title is reported
    Given the manifest turns scenario title matching on
    And a matrix row whose Scenario cell is "A title no scenario has"
    When the check runs
    Then the scenario-title rule reports that row

  @B-008
  Scenario Outline: A Scenario cell naming several titles is split on semicolons
    Given the manifest turns scenario title matching on
    And scenarios titled "First case" and "Second case", both tagged with the row's claim
    And a matrix row whose Scenario cell is "<cell>"
    When the check runs
    Then the scenario-title rule reports <findings> for that row

    Examples:
      | cell                          | findings    |
      | First case; Second case       | nothing     |
      | First case; A title none has  | one finding |

  @B-009
  Scenario: A Scenario cell naming a scenario without the row's tag is reported
    Given the manifest turns scenario title matching on
    And a scenario titled "Setting a level succeeds" tagged @B-002
    And the matrix row for claim B-001 whose Scenario cell is "Setting a level succeeds"
    When the check runs
    Then the scenario-title rule reports that row

  @B-010 @boundary
  Scenario Outline: Without title matching the Scenario cell is prose
    Given the manifest <setting>
    And a matrix row whose Scenario cell is "A title no scenario has"
    When the check runs
    Then the scenario-title rule reports nothing

    Examples:
      | setting                             |
      | does not mention title matching     |
      | turns scenario title matching off   |

  @B-011
  Scenario: A live claim no scenario carries is reported unless its row is missing
    Given an active claim B-004 whose tag no scenario carries
    And its matrix row's Status is "Covered"
    When the check runs
    Then the missing-scenario rule reports that row

  @B-011 @boundary
  Scenario: A live claim no scenario carries is not reported while its row is missing
    Given an active claim B-004 whose tag no scenario carries
    And its matrix row's Status is the missing marker
    When the check runs
    Then the missing-scenario rule reports nothing

  @B-011 @boundary
  Scenario: A withdrawn claim no scenario carries is not reported for it
    Given a claim B-004 marked "Withdrawn" whose tag no scenario carries
    And its matrix row's Status is "Withdrawn"
    When the check runs
    Then the missing-scenario rule reports nothing

  @B-011 @boundary
  Scenario: A waived row whose claim no scenario carries is not reported for it
    Given an active claim B-004 whose tag no scenario carries
    And its matrix row's Status is "Waived"
    When the check runs
    Then the missing-scenario rule reports nothing

  @B-012
  Scenario: A row reported for its missing scenario is not reported for its status
    Given an active claim B-004 whose tag no scenario carries
    And its matrix row's Status is "Covered"
    When the check runs
    Then the missing-scenario rule reports that row
    And the status rule reports nothing for that row

  @B-013
  Scenario Outline: A covered row whose scenario or tests do not resolve is reported
    Given the manifest declares the test files "test/**/*.cs"
    And the manifest turns scenario title matching on
    And a row whose Status is "Covered", whose scenario <scenario> and whose tests <tests>
    When the check runs
    Then the status rule reports <findings> for that row

    Examples:
      | scenario         | tests                  | findings    |
      | resolves         | all resolve            | nothing     |
      | does not resolve | all resolve            | one finding |
      | resolves         | resolve only in part   | one finding |
      | resolves         | are the missing marker | one finding |

  @B-013
  Scenario: A covered row whose Test cell is the missing marker is reported
    Given the manifest declares the test files "test/**/*.cs"
    And a row whose Status is "Covered", whose scenario resolves and whose Test cell is the missing marker
    When the check runs
    Then the status rule reports that row

  @B-014
  Scenario Outline: A partial row with nothing resolved is reported
    Given the manifest declares the test files "test/**/*.cs"
    And the manifest turns scenario title matching on
    And a row whose Status is "Partial", whose scenario <scenario> and whose tests <tests>
    When the check runs
    Then the status rule reports <findings> for that row

    Examples:
      | scenario         | tests                | findings    |
      | resolves         | none resolve         | nothing     |
      | does not resolve | resolve only in part | nothing     |
      | does not resolve | none resolve         | one finding |

  @B-015
  Scenario: A missing row whose tests all resolve is reported as stale
    Given the manifest declares the test files "test/**/*.cs"
    And a row whose Status is "Missing" and whose Test cell names tests that all resolve
    When the check runs
    Then the status rule reports that row as stale

  @B-015 @boundary
  Scenario: A missing row whose tests do not all resolve is not stale
    Given the manifest declares the test files "test/**/*.cs"
    And a row whose Status is "Missing" and whose Test cell names one test that resolves and one that does not
    When the check runs
    Then the status rule reports nothing for that row

  @B-016
  Scenario: With no test files declared a covered row is judged by its scenario and a named test
    Given the manifest declares no tests
    And a row whose Status is "Covered", whose scenario resolves and whose Test cell names a test no file holds
    When the check runs
    Then the status rule reports nothing for that row

  @B-016
  Scenario: With no test files declared a missing row is never stale
    Given the manifest declares no tests
    And a row whose Status is "Missing" and whose Test cell names a test
    When the check runs
    Then the status rule reports nothing for that row

  @B-016
  Scenario: With no test files declared a covered row naming no test is reported
    Given the manifest declares no tests
    And a row whose Status is "Covered", whose scenario resolves and whose Test cell is the missing marker
    When the check runs
    Then the status rule reports that row

  @B-017
  Scenario: A status that is no known marker is reported
    Given a row whose Status is "Done"
    When the check runs
    Then the status rule reports that row, expecting one of the covered, partial, missing, waived, withdrawn and superseded markers

  @B-018
  Scenario Outline: A waived row without a signed-off approver and a date is reported
    Given the manifest sets the waiver rule to error
    And the specification's sign-off has a row by "spec-reviewer" marked <sign-off>
    And a row whose Status is "Waived" and whose Test cell is "<cell>"
    When the check runs
    Then the waiver rule reports <findings> for that row

    Examples:
      | sign-off   | cell                        | findings    |
      | approved   | spec-reviewer, 2026-10-14   | nothing     |
      | approved   | spec-reviewer               | one finding |
      | approved   | implementer, 2026-10-14     | one finding |
      | draft      | spec-reviewer, 2026-10-14   | one finding |
      | blocked    | spec-reviewer, 2026-10-14   | one finding |

  @B-019
  Scenario: A withdrawn claim a scenario still carries is reported
    Given the manifest sets the retired-claim rule to error
    And a claim B-004 marked "Withdrawn"
    And a scenario tagged @B-004
    When the check runs
    Then the retired-claim rule reports claim B-004

  @B-019
  Scenario Outline: A retired claim whose row says otherwise is reported
    Given the manifest sets the retired-claim rule to error
    And a claim B-004 marked "<claim>" whose tag no scenario carries
    And its matrix row's Status is "<row>"
    When the check runs
    Then the retired-claim rule reports <findings>

    Examples:
      | claim      | row        | findings    |
      | Withdrawn  | Withdrawn  | nothing     |
      | Withdrawn  | Missing    | one finding |
      | Superseded | Superseded | nothing     |
      | Superseded | Withdrawn  | one finding |

  @B-020
  Scenario Outline: The waiver and retired-claim rules are silent unless raised
    Given the manifest <setting>
    And a row whose Status is "Waived" with nothing in its Test cell
    And a claim marked "Withdrawn" that a scenario still carries
    When the check runs
    Then the waiver rule reports nothing
    And the retired-claim rule reports nothing

    Examples:
      | setting                                                  |
      | sets no level for either rule                            |
      | sets the waiver rule and the retired-claim rule to off   |

  @B-021 @boundary
  Scenario: Under schema 0.1.0 none of the matrix rules runs
    Given the manifest pins schema version 0.1.0 instead
    And the specification's matrix has a covered row no scenario carries and a status that is no known marker
    When the check runs
    Then no scenario-title, missing-scenario, unresolved-test, status, waiver or retired-claim finding is reported
    And the report is byte-for-byte the one the tool gave that tree before this Feature
