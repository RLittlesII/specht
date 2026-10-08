Feature: The report contract
  As the agent authoring or migrating a specification
  I want every violation to say what the rule expected, in one document with a schema
  So that I repair the document from the report alone, without opening the catalogue or the template

  Background:
    Given a repository root holding a manifest and the three frontmatter schemas

  @B-001
  Scenario: JSON output replaces the diagnostic lines
    Given the root holds a specification with one violation
    When the check runs with JSON output
    Then the standard output parses as a single JSON document
    And no diagnostic line and no summary line precede or follow it

  @B-002
  Scenario: A report path receives the document
    Given the root holds specifications
    When the check runs with a report path in a directory that does not yet exist
    Then the directory is created
    And the file at that path is the report document
    And the standard output still carries the diagnostic lines and the summary

  @B-003
  Scenario: Both outputs are the same document
    Given the root holds a specification with one violation
    When the check runs with JSON output and a report path
    Then the document on the standard output equals the document at the path

  @B-004
  Scenario: The document form does not change the verdict
    Given the root holds a specification with one error-severity violation
    When the check runs with JSON output
    Then the exit code is 1

  @B-005
  Scenario: The document carries the counts and the version
    Given the root holds two specifications in the legacy layout and one in the co-located layout
    When the check runs with JSON output
    Then the document names the schema version checked against
    And each layout by name with its specification count
    And the item count, the count of rules evaluated, the error count and the warning count

  @B-006 @boundary
  Scenario: The document carries nothing from the clock
    Given the root holds specifications
    When the check runs with JSON output twice, a minute apart
    Then the two documents are identical

  @B-007
  Scenario: A violation carries what the rule expected
    Given the root holds a specification whose claim B-002 has no traceability row
    When the check runs with JSON output
    Then the violation carries the rule id, the severity, the file, the line, the identifier and the message
    And the violation carries what the rule expected

  @B-008
  Scenario: Every document validates against the published schema
    Given the root holds specifications with violations of every rule family
    When the check runs with JSON output
    Then the document validates against the report schema published in this repository

  @B-009
  Scenario: A frontmatter violation says what the schema requires
    Given the root holds a specification whose frontmatter priority is "urgent"
    When the check runs with JSON output
    Then the violation's expectation names the schema file
    And the key at fault
    And the values the schema allows for it

  @B-010
  Scenario: A section violation says the section order
    Given the root holds a specification missing its constraints section
    When the check runs with JSON output
    Then the violation's expectation carries the ordered section titles of the pinned schema version

  @B-011
  Scenario: A table violation says the headers
    Given the root holds a specification whose traceability table has a column named "Tests"
    When the check runs with JSON output
    Then the violation's expectation carries the ordered header list for that section

  @B-012
  Scenario: An identity violation says the identity implied
    Given the root holds a legacy specification whose epic directory does not match its frontmatter
    When the check runs with JSON output
    Then the violation's expectation carries the identity form
    And the identity the directory implies

  @B-013
  Scenario: A companion violation says what must resolve
    Given the root holds a specification whose companion carries a tag for a claim it does not declare
    When the check runs with JSON output
    Then the violation's expectation carries the claim ids the specification declares

  @B-014
  Scenario: A claim violation says the grammar
    Given the root holds a specification declaring a claim as "B-12"
    When the check runs with JSON output
    Then the violation's expectation carries the claim grammar

  @B-015
  Scenario: A matrix violation says which claims need a row
    Given the root holds a specification whose claim B-003 has two traceability rows
    When the check runs with JSON output
    Then the violation's expectation names B-003 as needing exactly one row
    And the number of rows it has

  @B-016
  Scenario: A child-item violation says the file shape or the next number
    Given the root holds a specification whose children skip from 0001-02 to 0001-04
    When the check runs with JSON output
    Then the violation's expectation names 0001-03 as the next number in the sequence

  @B-017
  Scenario: A dependency violation says the missing edge or the cycle
    Given the root holds Feature F1 depending on F2
    And F2 declares no blocks edge back to F1
    When the check runs with JSON output
    Then the violation's expectation carries the edge F2 must declare

  @B-018
  Scenario: An approval violation says what must change first
    Given the root holds an approved specification with one missing cell in its traceability table
    When the check runs with JSON output
    Then the violation's expectation names the cell that must change before the approved status is honest

  @B-019
  Scenario: Explain prints a rule's full text
    When the tool is asked to explain SPEC031
    Then the standard output carries what the rule checks
    And what the rule expects
    And the schema version the rule belongs to
    And the exit code is 0
    And no check ran

  @B-020
  Scenario: Explain of an unknown rule is a missing-input failure
    When the tool is asked to explain SPEC999
    Then the standard error names SPEC999
    And the standard output is empty
    And the exit code is 2

  @B-021
  Scenario: No path in the document is absolute
    Given the root is a deeply nested directory on this machine
    And the root holds a specification with one violation
    When the check runs with JSON output
    Then every path in the document is relative to the root
    And no path uses the platform's directory separator where it differs from a forward slash
