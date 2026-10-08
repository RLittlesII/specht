Feature: Schema versioning
  As the maintainer of a repository on the specification model
  I want to pin one schema version and move to the next when I choose
  So that lagging is deliberate, catching up is one command, and only I change my documents

  Background:
    Given a repository root holding a manifest and the three frontmatter schemas

  @B-001
  Scenario: The pinned version is the one checked with
    Given the tool ships schema versions 1 and 2
    And the manifest pins version 1 and records no upstream schema source
    And the root holds a specification whose frontmatter version 1 accepts and version 2 rejects
    When the check runs
    Then no frontmatter violation is reported

  @B-002
  Scenario: A manifest without a version is version 1
    Given the manifest carries no schema version
    When the check runs with JSON output
    Then the document names schema version 1

  @B-003
  Scenario: A version the tool does not ship is invalid configuration
    Given the tool ships schema version 1 only
    And the manifest pins version 7
    When the check runs
    Then the standard error names version 7 and the versions the tool ships
    And the standard output is empty
    And the exit code is 3

  @B-004 @boundary
  Scenario: Every version ever shipped is still shipped
    Given the tool ships schema version n
    When its embedded versions are enumerated
    Then every version from 1 to n is present

  @B-005
  Scenario: Upgrade moves the schema set and templates to the next version
    Given the tool ships schema versions 1 and 2
    And the root is pinned to version 1 with the embedded source and no upstream schema source
    And one of its templates has been edited by hand
    When upgrade runs against it
    Then the frontmatter schemas and the templates are version 2's
    And the exit code is 0

  @B-006
  Scenario: Upgrade at the newest version changes nothing
    Given the tool ships schema version 1 only
    And the root is pinned to version 1 and records no upstream schema source
    When upgrade runs against it
    Then the standard output says the repository is current
    And no file was rewritten
    And the exit code is 0

  @B-007 @boundary
  Scenario: Upgrade never touches a document
    Given the tool ships schema versions 1 and 2
    And the root is pinned to version 1 and holds specifications, records and companions
    And a snapshot of every file under the root
    When upgrade runs against it
    Then the only files changed are under the schema folder and the templates folder

  @B-008
  Scenario: Upgrade moves one version at a time
    Given the tool ships schema versions 1, 2 and 3
    And the root is pinned to version 1
    When upgrade runs against it
    Then the manifest pins version 2
    When upgrade runs against it again
    Then the manifest pins version 3

  @B-009
  Scenario: The on-disk source is selected by configuration
    Given the root's on-disk Feature schema has been edited to require a key the embedded one does not
    And the manifest records no upstream schema source
    And the root holds a specification without that key
    When the check runs with the on-disk source selected
    Then the specification is reported for the missing key

  @B-010 @boundary
  Scenario: The embedded source ignores an on-disk edit
    Given the root's on-disk Feature schema has been edited to require a key the embedded one does not
    And the manifest records no upstream schema source
    And the root holds a specification without that key
    When the check runs without selecting a source
    Then no violation is reported for that key

  @B-011
  Scenario: The report names the selected source
    Given the on-disk source is selected
    When the check runs with JSON output
    Then the document says the schemas came from disk

  @B-012
  Scenario: Init pins the newest version
    Given the tool ships schema versions 1 and 2
    And a root directory with no schema folder
    When init runs against it
    Then the written manifest pins version 2

  @B-013 @boundary
  Scenario: A shipped version never changes
    Given the embedded schemas, manifest and templates for version 1 as the first published package shipped them
    When a later tool's embedded files for version 1 are read
    Then they are byte-identical

  @B-014
  Scenario: The pinned version's rules are the ones evaluated
    Given the tool ships schema versions 1 and 2
    And version 2 holds a rule version 1 does not
    And the manifest pins version 1
    And the root holds a specification that breaks only that rule
    When the check runs
    Then no violation is reported

  @B-015 @boundary
  Scenario: A shipped version's rules never change
    Given the rules version 1 held in the first published package
    When a later tool's rules for version 1 are enumerated
    Then they are the same rules

  @B-016
  Scenario: Upgrade keeps the consumer's manifest settings
    Given the tool ships schema versions 1 and 2
    And the root is pinned to version 1
    And its manifest lowers one rule to a warning, disables another and renames the claims section
    And its manifest already carries a key version 2 introduces, set to a value of the consumer's
    When upgrade runs against it
    Then the manifest pins version 2
    And the warning, the disabled rule and the renamed section are as the consumer set them
    And the key version 2 introduces keeps the consumer's value
    And every other key version 2 introduces is added

  @B-017
  Scenario: Upgrade prints what it changed
    Given the tool ships schema versions 1 and 2
    And the root is pinned to version 1
    When upgrade runs against it
    Then the standard output names the move from 1 to 2 and each file rewritten
    And the standard output carries no diff of any file

  @B-018
  Scenario: Upgrade under a missing root is a missing-input failure
    When upgrade runs against a path that is not a directory
    Then nothing is written
    And the exit code is 2

  @B-019
  Scenario: Upgrade without a manifest is a missing-input failure
    Given the manifest has been removed from the root
    When upgrade runs against it
    Then nothing is written
    And the exit code is 2

  @B-020
  Scenario: Upgrade with an invalid manifest is invalid configuration
    Given the root's manifest is not valid
    When upgrade runs against it
    Then nothing is written
    And the exit code is 3

  @B-021 @boundary
  Scenario: Upgrade from a version the tool does not ship rewrites nothing
    Given the tool ships schema version 1 only
    And the root is pinned to version 7
    And a snapshot of every file under the root
    When upgrade runs against it
    Then no file under the root has changed
    And the standard error names version 7 and the versions the tool ships
    And the exit code is 3

  @B-022
  Scenario: Version 1 accepts a title and a description on an epic
    Given the manifest pins version 1
    And the root holds an epic whose frontmatter carries a title and a description
    When the check runs
    Then no frontmatter violation is reported for the epic

  @B-023
  Scenario Outline: Version 1 rejects an empty <key> on an epic
    Given the manifest pins version 1
    And the root holds an epic whose frontmatter carries an empty <key>
    When the check runs
    Then the epic is reported for a frontmatter violation of its <key>

    Examples:
      | key         |
      | title       |
      | description |

  @B-024
  Scenario: The check reads an upstream schema from the local copy
    Given the manifest records an upstream schema source and selects no source
    And the root holds the copy of that source under its schema folder
    When the check runs
    Then the frontmatter is validated against the local copy

  @B-025 @boundary
  Scenario: The check makes no network call
    Given the manifest records an upstream schema source
    And the network is unavailable
    When the check runs
    Then the check completes
    And no network call was attempted

  @B-026
  Scenario: Upgrade writes a fetched schema that matches its hash
    Given the manifest records an upstream schema source with its version and content hash
    And the root's local copy does not match that hash
    And the source serves content matching that hash
    When upgrade runs
    Then the fetched schemas are written under the root's schema folder

  @B-027
  Scenario: Upgrade refuses a fetched schema that does not match its hash
    Given the manifest records an upstream schema source with its version and content hash
    And the source serves content that does not match that hash
    And a snapshot of every file under the root
    When upgrade runs
    Then no file under the root has changed
    And the standard error names the source
    And the exit code is 3

  @B-028
  Scenario: An upstream schema does not change the rule vocabulary
    Given the manifest pins version 1 and records an upstream schema source
    When the check runs
    Then exactly version 1's rule ids are evaluated

  @B-029 @boundary
  Scenario: Upgrade never writes the embedded schemas over the on-disk source
    Given the tool ships schema versions 1 and 2
    And the root is pinned to version 1 with the on-disk source selected
    When upgrade runs against it
    Then no frontmatter schema under the root is version 2's embedded schema

  @B-030
  Scenario: Upgrade says which schemas it skipped and why
    Given the tool ships schema versions 1 and 2
    And the root is pinned to version 1 with the on-disk source selected
    When upgrade runs against it
    Then the output names each frontmatter schema it skipped
    And the reason it skipped them

  @B-031
  Scenario: Upgrade still rewrites the templates under the on-disk source
    Given the tool ships schema versions 1 and 2
    And the root is pinned to version 1 with the on-disk source selected
    When upgrade runs against it
    Then the templates are version 2's
    And the output names each template it rewrote

  @B-032
  Scenario: Recording an upstream source selects the on-disk source
    Given the manifest records an upstream schema source and selects no source
    When the check runs with JSON output
    Then the document says the schemas came from the upstream copy

  @B-033 @boundary
  Scenario: Upgrade does not fetch a local copy that matches its hash
    Given the manifest records an upstream schema source with its version and content hash
    And the root's local copy matches that hash
    And the network is unavailable
    When upgrade runs
    Then no network call was attempted

  @B-034
  Scenario: Upgrade leaves the recorded upstream version alone
    Given the tool ships schema versions 1 and 2
    And the root is pinned to version 1 and records an upstream schema source at version 3
    When upgrade runs against it
    Then the manifest still records the upstream source at version 3 with the same content hash

  @B-035
  Scenario: Upgrade refuses an upstream source it cannot reach
    Given the manifest records an upstream schema source with its version and content hash
    And the root's local copy does not match that hash
    And the source cannot be reached
    And a snapshot of every file under the root
    When upgrade runs
    Then no file under the root has changed
    And the standard error names the source
    And the exit code is 3

  @B-036
  Scenario: An upstream source with the embedded source selected is rejected
    Given the manifest records an upstream schema source
    And the manifest explicitly selects the embedded source
    When the check runs
    Then the standard error names the contradiction
    And the standard output is empty
    And the exit code is 3
