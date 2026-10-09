Feature: The engine, extracted unchanged
  As the maintainer of four repositories on one specification model
  I want the rule engine moved out of hooked into a library of its own
  So that the same verdict can be given everywhere, and the move itself changes nothing

  Background:
    Given a repository root holding a manifest and the three frontmatter schemas of schema version 1

  @B-001
  Scenario: Every rule in the vocabulary is applied, and no other
    Given the root holds specifications that between them break each of the twenty-one version 1 rules once
    When the engine runs
    Then a violation is reported under each of the twenty-one rule ids
    And no violation is reported under any other rule id

  @B-002
  Scenario: Both layouts are held to the same rules
    Given the root holds one specification in the legacy layout missing its traceability section
    And one specification in the co-located layout missing its traceability section
    When the engine runs
    Then each specification is reported for the missing section
    And neither is reported for the layout it is in

  @B-003
  Scenario: Identity comes from the frontmatter, not the path
    Given the root holds a co-located specification declaring epic "0001" and id "F2" under a folder named nothing like it
    When the engine runs
    Then the specification is read as "0001-F2"
    And no identity violation is reported

  @B-011
  Scenario: One identity at two paths is reported
    Given the root holds two specifications both declaring epic "0001" and id "F2"
    When the engine runs
    Then the duplicate identity is reported once

  @B-004
  Scenario: The verdicts on the baseline tree match the golden report
    Given the baseline tree the tests build, which breaks each of the twenty-one version 1 rules in each layout the rule applies to
    And the golden report the engine gave on that tree at the commit the copy landed on main
    When the engine runs on that tree
    Then it reports the same violations as the golden report, with the same rule, severity, file, line, identifier and message
    And in the same order

  @B-005 @boundary
  Scenario: Nothing carries the old repository's name
    Given the library is built
    When its assembly name and root namespace are read
    Then both are "specht"
    And neither contains "Hooked"

  @B-006
  Scenario: Nothing about the machine reaches the verdict
    Given a tree with violations, and the violations the engine reports on it
    When the engine runs again with a different clock, environment, locale, machine name and root location
    Then it reports the same violations in the same order

  @B-007
  Scenario: Every path is relative to the root
    Given the root is a deeply nested directory on this machine
    And the root holds a specification with one violation
    When the engine runs
    Then the violation's file is relative to the root
    And it uses a forward slash where the platform's separator differs

  @B-008 @boundary
  Scenario: A grid table is read as no table
    Given the root holds a specification whose traceability section holds a grid table
    When the engine runs
    Then the section is reported as carrying no table
    And no row of the grid is read as a claim

  @B-010
  Scenario: A violation carries the six baseline fields
    Given the root holds a specification whose claim B-002 has no traceability row
    When the engine runs
    Then the violation carries a rule id, a severity, a file, a line, an identifier and a message
