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

  @B-022
  Scenario: A report path receives the document
    Given the root holds a specification with one violation
    When the check runs with a report path
    Then the file at that path is the report document

  @B-023
  Scenario: A report path in a new directory is created
    Given the root holds specifications
    When the check runs with a report path in a directory that does not yet exist
    Then the directory exists
    And the file at that path is the report document

  @B-024
  Scenario: A report path that exists is replaced
    Given the root holds a specification with one violation
    And a file already exists at the report path from an earlier run
    When the check runs with that report path
    Then the file at that path is this run's report document and nothing else
    And the run's exit code is the one it gives without a report path

  @B-025
  Scenario: A report path keeps the diagnostic lines on the output
    Given the root holds a specification with one violation
    When the check runs with a report path and without JSON output
    Then the standard output carries the same diagnostic lines and summary as a run without a report path

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
    Given the root holds two specifications in the epics layout and one in the features layout
    When the check runs with JSON output
    Then the document names the schema version checked against
    And whether the schemas came from the tool or from the repository's own files
    And each layout by name with its specification count
    And the item count, the count of rule ids evaluated, the error count and the warning count

  @B-006 @boundary
  Scenario: The document carries nothing from the clock
    Given the root holds specifications
    When the check runs with JSON output twice, with the tool's clock set to two different times
    Then the two documents are identical

  @B-006 @boundary
  Scenario: The document carries nothing from the machine
    Given the same specifications are held under two different root directories
    When each is checked with JSON output, under a different user and machine name
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
    Then the violation's expectation names the Feature schema file by its path under the configuration folder
    And the key "priority"
    And the failed constraint "enum" with the values the schema allows for it

  @B-009
  Scenario: A frontmatter violation of another kind says that constraint
    Given the root holds a specification whose frontmatter value is "high"
    When the check runs with JSON output
    Then the violation's expectation names the key "value"
    And the failed constraint "type" with the type the schema allows for it

  @B-009
  Scenario: A missing frontmatter key says the required keys
    Given the root holds a specification whose frontmatter has no priority
    When the check runs with JSON output
    Then the violation's expectation names no key
    And the failed constraint "required" with the keys the schema requires

  @B-009
  Scenario: A failure inside a condition the schema only tests names no constraint
    Given the root holds a specification whose frontmatter priority is "urgent" and whose scored_by is null, the template's value
    When the check runs with JSON output
    Then the violation's expectation for the key "scored_by" names no failed constraint

  @B-010
  Scenario: A section violation says the section order
    Given the root holds a specification missing its constraints section
    When the check runs with JSON output
    Then the violation's expectation carries the section titles the manifest contracts, in order

  @B-011
  Scenario: A table violation says the headers
    Given the root holds a specification whose traceability table has a column named "Tests"
    When the check runs with JSON output
    Then the violation's expectation names the traceability section
    And carries the ordered header list the manifest gives that section

  @B-012
  Scenario: An identity violation says the identity implied
    Given the root holds a specification in the epics layout whose epic directory does not match its frontmatter
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

  @B-026
  Scenario: A missing frontmatter says what the schema requires
    Given the root holds a specification with no frontmatter
    When the check runs with JSON output
    Then the violation's expectation names the Feature schema file by its path under the configuration folder
    And the keys that schema requires, in the schema's order

  @B-026
  Scenario: A missing frontmatter names the schema file when the schemas are the tool's own
    Given the frontmatter schema files are deleted from the root
    And the root holds a specification with no frontmatter
    When the check runs with JSON output
    Then the violation's expectation names the Feature schema file by the path it has under the configuration folder

  @B-017
  Scenario: A dependency violation says the missing edge or the cycle
    Given the root holds Feature F1 depending on F2
    And F2 declares no blocks edge back to F1
    When the check runs with JSON output
    Then the violation's expectation carries the edge F2 must declare

  @B-027
  Scenario: A dependency on nothing says what it could name
    Given the root holds Features F1 and F2 in one epic
    And F1 depends on F9
    When the check runs with JSON output
    Then the violation's expectation carries F1 and F2 as the Features a dependency could name

  @B-028
  Scenario: A reused item id names its first holder
    Given the root holds two items both declaring the id 0001-02
    When the check runs with JSON output
    Then the violation's expectation names the file of the item that declared 0001-02 first

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
  Scenario: Explain of an unknown rule names it and fails
    When the tool is asked to explain SPEC999
    Then the standard error names SPEC999
    And the standard output is empty
    And the exit code is 4

  @B-021
  Scenario: No path in the document is absolute
    Given the root is a deeply nested directory on this machine
    And the root holds a specification with one violation
    When the check runs with JSON output
    Then every path in the document is relative to the root
    And no path uses the platform's directory separator where it differs from a forward slash

  @B-029
  Scenario: Explain without a manifest fails as the check does
    Given the manifest is deleted from the root
    When the tool is asked to explain SPEC031
    Then no rule text is printed
    And the standard output is empty
    And the exit code is 2

  @B-030
  Scenario: Explain beside a manifest that does not parse fails as the check does
    Given the root's manifest is replaced by a file that is not well-formed JSON
    When the tool is asked to explain SPEC031
    Then no rule text is printed
    And the standard output is empty
    And the exit code is 3

  @B-030
  Scenario: Explain beside a rejected manifest fails as the check does
    Given the root's manifest is replaced by one carrying a key the tool does not know
    When the tool is asked to explain SPEC031
    Then no rule text is printed
    And the standard output is empty
    And the exit code is 3

  @B-030
  Scenario: Explain beside a contradictory schema source fails as the check does
    Given the root's manifest records an upstream schema source and explicitly selects the embedded source
    When the tool is asked to explain SPEC031
    Then no rule text is printed
    And the standard output is empty
    And the exit code is 3

  @B-030 @boundary
  Scenario: Explain beside a pin the tool does not ship fails as the check does
    Given the tool ships schema version 0.1.0 only
    And the root's manifest is replaced by one pinning version 7.0.0
    When the tool is asked to explain SPEC031
    Then no rule text is printed
    And the standard output is empty
    And the exit code is 3

  @B-031
  Scenario: Every rule of a shipped version has one rule page
    Given a schema version the tool ships
    When the rule pages for that version are gathered
    Then every rule in that version's vocabulary has exactly one page

  @B-031 @boundary
  Scenario: No rule page is for a rule outside its version
    Given a schema version the tool ships
    When the rule pages for that version are gathered
    Then no page is for a rule outside that version's vocabulary

  @B-032
  Scenario: A rule page follows the one page shape
    Given the rule pages for a schema version the tool ships
    When each page is read
    Then it declares itself a rule page, with a title and a description
    And its heading names the rule it is for, then the rule's title
    And it gives the rule's id, its family, its default severity and its schema version
    And it says what the rule checks
    And it describes what the rule expects, with an example violation, the line the tool prints for it, and the corrected example
    And it says how to fix a violation
    And those parts come in that order

  @B-032 @boundary
  Scenario: A rule page has no part on suppressing or re-grading the rule
    Given the rule pages for a schema version the tool ships
    When each page is read
    Then it has no part beyond the one page shape
    And it says nothing on suppressing, disabling or changing the severity of the rule

  @B-033
  Scenario: A rule page gives the schema version it ships under
    Given the rule pages for a schema version the tool ships
    When each page is read
    Then the schema version it gives is the version it ships under, written as major.minor.patch

  @B-034
  Scenario: A required-specification violation says the glob and the layouts
    Given the manifest pins schema version "0.2.0"
    And the manifest requires a specification in each directory "skills/*"
    And the root holds "skills/next/" with no specification inside it
    When the check runs with JSON output
    Then the violation's expectation carries the glob "skills/*" as what required it
    And the specification file names the layouts discover

  @B-035
  Scenario: A section-owner violation says the owners
    Given the manifest pins schema version "0.2.0"
    And the manifest names spec-author as the only owner of the acceptance criteria
    And the root holds a specification whose acceptance criteria are stamped "implementer, 2026-10-14"
    When the check runs with JSON output
    Then the violation's expectation carries the owners "spec-author"

  @B-036
  Scenario: A scenario-title violation says the tag and the titles
    Given the manifest pins schema version "0.2.0"
    And the manifest turns scenario title matching on
    And the root holds a specification whose row for claim B-001 names a title no scenario tagged @B-001 has
    When the check runs with JSON output
    Then the violation's expectation carries the tag "@B-001"
    And the titles of the scenarios tagged @B-001

  @B-037
  Scenario: A missing-scenario violation says the tag and the missing marker
    Given the manifest pins schema version "0.2.0"
    And the root holds a specification whose covered claim B-001 no scenario carries
    When the check runs with JSON output
    Then the violation's expectation carries the tag "@B-001"
    And the missing marker as the status

  @B-038
  Scenario: An orphan-companion violation says the specification expected
    Given the manifest pins schema version "0.2.0"
    And the root holds a ".spec/" directory with a scenario file and no specification
    When the check runs with JSON output
    Then the violation's expectation carries the specification file names expected beside it

  @B-039
  Scenario: An unresolved-test violation says the name, the files and the forms
    Given the manifest pins schema version "0.2.0"
    And the manifest declares the test files "test/**/*.cs" and the declaration form "void {test}("
    And the root holds a specification whose row names the test "ScenarioNobodyWrote"
    When the check runs with JSON output
    Then the violation's expectation carries the name "ScenarioNobodyWrote"
    And the files "test/**/*.cs"
    And the declaration forms "void {test}("

  @B-040
  Scenario: A status violation says the supported status and what resolved
    Given the manifest pins schema version "0.2.0"
    And the manifest declares the test files "test/**/*.cs"
    And the root holds a specification whose covered row's scenario resolves and whose one test does not
    When the check runs with JSON output
    Then the violation's expectation carries the status the row's resolution supports
    And that the scenario resolved and the tests did not

  @B-041
  Scenario: A waiver violation says the form and the approvers
    Given the manifest pins schema version "0.2.0"
    And the manifest sets the waiver rule to error
    And the root holds a specification whose waived row names no approver
    When the check runs with JSON output
    Then the violation's expectation carries the waiver form "<approver>, <YYYY-MM-DD>"
    And the reviewer names the sign-off holds

  @B-042
  Scenario: A retired-claim violation says the status and no tags
    Given the manifest pins schema version "0.2.0"
    And the manifest sets the retired-claim rule to error
    And the root holds a specification whose withdrawn claim a scenario still carries
    When the check runs with JSON output
    Then the violation's expectation carries the status "Withdrawn"
    And a tag count of 0

  @B-043
  Scenario: A done Feature's missing-coverage violation says what must change first
    Given the manifest pins schema version "0.3.0"
    And the root holds a specification whose delivery status is done with one missing cell in its traceability table
    When the check runs with JSON output
    Then the violation's expectation names the cell that must change before the done status is honest
