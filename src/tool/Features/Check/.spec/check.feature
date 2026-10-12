Feature: The check command
  As the maintainer of a repository on the specification model
  I want one command that checks my .spec/ tree and tells me the verdict the same way everywhere
  So that a pre-commit hook, a CI step and an agent mid-write all see the same answer

  Background:
    Given a repository root holding a manifest and the three frontmatter schemas

  @B-001
  Scenario: A run prints every violation as a diagnostic line
    Given the root holds a specification whose claim B-002 has no traceability row
    When the check runs
    Then the standard output carries one line per violation
    And each line names the file, the line, the severity, the rule id and the message in the build's diagnostic form
    And the line reporting that claim ends with B-002 in square brackets

  @B-002
  Scenario: A run ends with the summary
    Given the root holds two specifications in the epics layout and one in the features layout
    When the check runs
    Then the standard output ends with the specification count per layout
    And the item count and the count of rule ids evaluated
    And the error count and the warning count

  @B-003
  Scenario: An error-severity violation fails the run
    Given the root holds a specification with one error-severity violation
    When the check runs
    Then the exit code is 1

  @B-003
  Scenario: A clean tree succeeds
    Given the root holds specifications with no violation
    When the check runs
    Then the exit code is 0

  @B-004
  Scenario: Strict mode fails the run on a warning
    Given the root holds a specification with one warning-severity violation and no error
    When the check runs in strict mode
    Then the exit code is 1

  @B-003
  Scenario: A warning alone does not fail a non-strict run
    Given the root holds a specification with one warning-severity violation and no error
    When the check runs without strict mode
    Then the exit code is 0

  @B-005
  Scenario: A root that is not a directory is a missing-input failure
    Given a root path that does not exist
    When the check runs against it
    Then the standard error names that path as it was typed
    And the standard output is empty
    And the exit code is 2

  @B-006
  Scenario: A root without a manifest is a missing-input failure
    Given a root directory with no manifest at the manifest path
    When the check runs against it
    Then the standard error names the manifest path
    And the standard output is empty
    And the exit code is 2

  @B-037
  Scenario: A manifest left at the retired manifest path is a missing-input failure
    Given a root directory with a manifest at the retired manifest path and none at the manifest path
    When the check runs against it
    Then the exit code is 2

  @B-038
  Scenario: The failure says the manifest at the retired manifest path is not read
    Given a root directory with a manifest at the retired manifest path and none at the manifest path
    When the check runs against it
    Then the standard error names the manifest path
    And the standard error says the manifest at the retired manifest path is not read

  @B-039 @boundary
  Scenario: A manifest at the retired manifest path is never read
    Given the root holds a specification with one violation
    And a file that is not a manifest at the retired manifest path
    When the check runs
    Then the standard output, the standard error and the exit code are those of the same root without that file

  @B-007
  Scenario: A manifest that does not parse is an invalid-configuration failure
    Given the root's manifest is not well-formed JSON
    When the check runs
    Then the standard error names the manifest path relative to the root
    And the standard output is empty
    And the exit code is 3

  @B-008
  Scenario: Help names the check's options
    When the tool is asked for help
    Then the output names the root option and the strict option
    And the exit code is 0

  @B-009
  Scenario: The product goes to stdout and the tool's own messages to stderr
    Given the root holds a specification with one violation
    When the check runs without asking for the JSON document
    Then the standard output holds the violation lines and the summary and nothing else
    And nothing about the tool itself appears on the standard output

  @B-010
  Scenario: Three call sites get one verdict
    Given the tool honours rule settings in the manifest
    And a prepared root holding one error-severity and one warning-severity violation
    When the check runs against that root from a shell
    And the check runs against that root through the build target
    And the check runs against that root through the pre-commit hook
    Then all three print the same lines
    And all three exit with the same code

  @B-011
  Scenario: This repository checks itself with the tool
    Given the tool honours rule settings in the manifest
    And this repository's tree at one commit
    When the build's Specht target runs
    Then the check runs against this repository's root
    And the target exits with the check's exit code

  @B-014
  Scenario: The build reaches the tool through the local tool manifest
    Given the tool has been published as a package
    And this repository's local tool manifest names that package
    When the build's Specht target runs
    Then it reaches the tool through the local tool manifest
    And it does not build the tool from this repository's source

  @B-012 @boundary
  Scenario: A check writes nothing but the named report
    Given the root holds a specification with one violation
    And a snapshot of every file under the root
    When the check runs
    Then no file under the root was created, modified or deleted

  @B-013
  Scenario: No derived path in the output is absolute
    Given the root is a deeply nested directory on this machine
    And the root holds a specification with one violation
    When the check runs from inside the root without naming it
    Then every path on the standard output is relative to the root
    And every path on the standard error is relative to the root
    And no path in either uses the platform's directory separator where it differs from a forward slash

  @B-015
  Scenario: A rule that throws exits 1, not -1
    Given the root holds a specification
    And one rule fails while it is evaluated
    When the check runs
    Then the exit code is 1

  @B-016 @boundary
  Scenario: A violation with no identifier prints no brackets
    Given the root holds a specification with no frontmatter
    When the check runs
    Then the line reporting the missing frontmatter ends with its message
    And nothing in square brackets follows the message

  @B-017
  Scenario: The package is named specht.tool
    When the tool is packed
    Then the package is named specht.tool

  @B-018
  Scenario: The package names its repository
    When the tool is packed
    Then the package names the git repository https://github.com/RLittlesII/specht as its source

  @B-019
  Scenario: The package records the commit it was built from
    When the tool is packed
    Then the package records the commit it was packed from

  @B-020
  Scenario: The package names its project page
    When the tool is packed
    Then the package names https://github.com/RLittlesII/specht as its project page

  @B-021
  Scenario: The package states its licence
    When the tool is packed
    Then the package states its licence as MIT

  @B-022
  Scenario: The package names its author
    When the tool is packed
    Then the package names Rodney Littles, II as its author

  @B-023
  Scenario: The package carries a copyright notice
    When the tool is packed
    Then the package's copyright notice names Rodney Littles, II

  @B-024
  Scenario: The package says what the tool does
    When the tool is packed
    Then the package describes the tool as one that checks a repository's specification documents against a versioned schema

  @B-025
  Scenario: The package carries its readme
    When the tool is packed
    Then the package carries the readme it names

  @B-026 @boundary
  Scenario: Nothing in the packed readme points inside the repository
    When the tool is packed
    Then every link in the readme the package carries is a full web address
    And every image in it is a full web address

  @B-027
  Scenario: The package carries its icon
    When the tool is packed
    Then the package carries the icon it names

  @B-028
  Scenario: The package carries its tags
    When the tool is packed
    Then the package carries the tags specification, spec-driven-development, traceability, gherkin and dotnet-tool

  @B-029
  Scenario: The package points at the repository's releases
    When the tool is packed
    Then the package's release notes point at the repository's releases

  @B-030
  Scenario: The packed tool names the commit it was built from
    When the tool is packed
    Then every program file of the tool in the package states a version naming the commit it was built from

  @B-031
  Scenario: The packed tool's sources are found in the repository at that commit
    When the tool is packed
    Then the debug information in the package points each program file's sources at https://github.com/RLittlesII/specht
    And it points them at the commit the tool was built from

  @B-032
  Scenario: A tool built by continuous integration names no directory of the machine that built it
    Given the tool is built the way a continuous-integration run builds it
    When that build is packed
    Then no program file of the tool in the package names a directory of the machine that built it
    And no debug information in the package names one

  @B-033 @boundary
  Scenario: The package carries no specification, scenario or work-item file
    When the tool is packed
    Then the package carries no specification file
    And it carries no scenario file
    And it carries no work-item file

  @B-034
  Scenario: The packed readme opens with what the tool does
    When the tool is packed
    Then the readme the package carries opens with the package's description

  @B-035
  Scenario: The packed readme links to the install document
    When the tool is packed
    Then the readme the package carries links to the install document in the repository

  @B-036 @boundary
  Scenario: The packed readme names no package feed
    When the tool is packed
    Then the readme the package carries holds no package feed's address
