Feature: The frontmatter command
  As the author of a GitHub issue on the specification model
  I want to check one document's frontmatter against the schema for its kind before it leaves my hands
  So that frontmatter that is not in the tree gets the same verdict the check would give it

  Background:
    Given a repository root holding a manifest that pins schema version 1

  @B-001
  Scenario: A document named by a path is checked
    Given a task document outside the tree whose frontmatter lacks a required key
    When its frontmatter is checked as a task by its path
    Then one violation is reported for the missing key

  @B-002
  Scenario: A document on standard input is checked
    Given a task document whose frontmatter lacks a required key
    When its frontmatter is checked as a task from standard input
    Then one violation is reported for the missing key

  @B-003
  Scenario Outline: A failure is reported under its kind's rule id
    Given a <kind> document whose frontmatter lacks a required key
    When its frontmatter is checked as a <kind>
    Then the violation carries rule id <rule>

    Examples:
      | kind         | rule    |
      | feature-spec | SPEC002 |
      | task         | SPEC003 |
      | epic         | SPEC004 |

  @B-004
  Scenario: A failure on a key is reported at that key's line
    Given a task document whose creation date on its fifth line is not a date
    When its frontmatter is checked as a task
    Then the violation is reported at line 5

  @B-005
  Scenario: A document with no frontmatter is reported
    Given a document with no frontmatter
    When its frontmatter is checked as a task
    Then the violation carries rule id SPEC001 at line 1

  @B-006
  Scenario: The schema is the one the check selects for the root
    Given a task document that pinned version 1 accepts and a stricter version rejects
    And the root's manifest pins the stricter version
    When its frontmatter is checked as a task
    Then a violation is reported

  @B-007 @boundary
  Scenario: A rule that needs the tree does not run
    Given a task document whose valid frontmatter depends on a file that does not exist
    When its frontmatter is checked as a task
    Then no violation is reported
    And the exit code is 0

  @B-007 @boundary
  Scenario: A Feature specification's sections are not checked
    Given a feature-spec document with valid frontmatter and none of the twelve sections
    When its frontmatter is checked as a feature-spec
    Then no violation is reported

  @B-008
  Scenario: The root's own specifications are not checked
    Given the root holds a specification whose frontmatter fails its schema
    And a task document with valid frontmatter
    When the task document's frontmatter is checked as a task
    Then no violation is reported

  @B-009
  Scenario: A document in the tree gets the check's verdict
    Given the root holds a specification whose frontmatter fails its schema on two keys
    When that specification's frontmatter is checked as a feature-spec
    Then the violations are the frontmatter violations the check reports for that file

  @B-010
  Scenario: A violation is printed as a diagnostic line
    Given a task document whose frontmatter lacks a required key
    When its frontmatter is checked as a task
    Then the standard output carries one line naming the document, the line, the severity, the rule id and the message in the build's diagnostic form

  @B-011
  Scenario: A document named by a path is reported by that path
    Given a task document at drafts/issue.md whose frontmatter lacks a required key
    When its frontmatter is checked as a task by the path drafts/issue.md
    Then the violation line names drafts/issue.md

  @B-012
  Scenario: A document on standard input is reported as stdin
    Given a task document whose frontmatter lacks a required key
    When its frontmatter is checked as a task from standard input
    Then the violation line names <stdin>

  @B-013
  Scenario: A run ends with the error and warning counts
    Given a task document whose frontmatter fails its schema on two keys
    When its frontmatter is checked as a task
    Then the standard output ends with an error count of 2 and a warning count of 0

  @B-014
  Scenario: An error fails the run
    Given a task document whose frontmatter lacks a required key
    When its frontmatter is checked as a task
    Then the exit code is 1

  @B-015
  Scenario: A clean document succeeds
    Given a task document with valid frontmatter
    When its frontmatter is checked as a task
    Then the exit code is 0

  @B-016
  Scenario: A missing root is a missing-input failure
    Given a root path that is not a directory
    When a task document's frontmatter is checked against that root
    Then the standard error names the root as it was given
    And the standard output is empty
    And the exit code is 2

  @B-017
  Scenario: A root without a manifest is a missing-input failure
    Given a root with no manifest
    When a task document's frontmatter is checked against that root
    Then the standard error names the manifest path
    And the standard output is empty
    And the exit code is 2

  @B-018
  Scenario: An unreadable manifest is an invalid-manifest failure
    Given a root whose manifest is not well-formed
    When a task document's frontmatter is checked against that root
    Then the standard error names the manifest path relative to the root
    And the standard output is empty
    And the exit code is 3

  @B-019
  Scenario: A version the tool does not ship is invalid configuration
    Given a root whose manifest pins a version the tool does not ship
    When a task document's frontmatter is checked against that root
    Then the standard error names the pinned version and the versions the tool ships
    And the standard output is empty
    And the exit code is 3

  @B-020
  Scenario: An unknown kind is not found
    Given a task document with valid frontmatter
    When its frontmatter is checked as a kind named story
    Then the standard error names story and the three kinds
    And the standard output is empty
    And the exit code is 4

  @B-021
  Scenario: A path that names no file is not found
    Given no file at drafts/missing.md
    When the frontmatter at drafts/missing.md is checked as a task
    Then the standard error names drafts/missing.md
    And the standard output is empty
    And the exit code is 4

  @B-022
  Scenario: Messages about the tool go to standard error
    Given a task document whose frontmatter lacks a required key
    When its frontmatter is checked as a task
    Then the standard output carries only the violation line and the summary

  @B-023 @boundary
  Scenario: A run writes nothing
    Given a task document on standard input whose frontmatter lacks a required key
    And a snapshot of every file under the root
    When its frontmatter is checked as a task
    Then no file under the root was created, modified or deleted

  @B-024
  Scenario: No derived path in the output is absolute
    Given a root with no manifest
    When a task document's frontmatter is checked against that root given as an absolute path
    Then every path the output names other than those typed is relative to the root and uses forward slashes

  @B-025
  Scenario: The same input gives the same verdict
    Given a task document whose frontmatter fails its schema on two keys
    When its frontmatter is checked as a task twice
    Then both runs print the same standard output and exit with the same code
