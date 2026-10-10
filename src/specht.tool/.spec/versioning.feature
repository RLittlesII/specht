Feature: Schema versioning
  As the maintainer of a repository on the specification model
  I want to pin one schema version, see from its number whether a move can fail my build, and move the pin when I choose
  So that lagging is deliberate, moving is one command each way, and only I change my documents

  Background:
    Given a repository root holding a manifest and the three frontmatter schemas

  @B-001
  Scenario: The pinned version is the one checked with
    Given the tool ships schema versions 0.1.0 and 1.0.0
    And the manifest pins version 0.1.0 and records no upstream schema source
    And the root holds a specification whose frontmatter version 0.1.0 accepts and version 1.0.0 rejects
    When the check runs
    Then no frontmatter violation is reported

  @B-002
  Scenario: A manifest without a version is version 0.1.0
    Given the manifest carries no schema version
    When the check runs with JSON output
    Then the document names schema version 0.1.0

  @B-003
  Scenario: A version the tool does not ship is invalid configuration
    Given the tool ships schema version 0.1.0 only
    And the manifest pins version 7.0.0
    When the check runs
    Then the standard error names version 7.0.0 and the versions the tool ships
    And the standard output is empty
    And the exit code is 3

  @B-004 @boundary
  Scenario: Every version ever published is still shipped
    Given the schema versions published in packages so far
    When a later tool's embedded versions are enumerated
    Then every published version is present

  @B-005
  Scenario Outline: Upgrade brings the schema set and templates to the pinned version
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root's frontmatter schemas and templates are version <files>'s
    And the root is pinned to version <pin> with the embedded source and no upstream schema source
    And one of its templates has been edited by hand
    When upgrade runs against it
    Then the frontmatter schemas and the templates are version <pin>'s
    And the exit code is 0

    Examples:
      | files | pin   |
      | 0.1.0 | 0.2.0 |
      | 0.2.0 | 0.1.0 |

  @B-006
  Scenario: Upgrade on a root already at its pin changes nothing
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.1.0 and records no upstream schema source
    And its frontmatter schemas, templates and manifest keys are already version 0.1.0's
    When upgrade runs against it
    Then the standard output says the repository is current
    And no file was rewritten
    And the exit code is 0

  @B-007 @boundary
  Scenario: Upgrade never touches a document
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 and its frontmatter schemas and templates are version 0.1.0's
    And the root holds specifications, records and companions
    And a snapshot of every file under the root
    When upgrade runs against it
    Then the only files changed are under the schema folder and the templates folder

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
    Given the tool ships schema versions 0.9.0 and 0.10.0
    And a root directory with no schema folder
    When init runs against it
    Then the written manifest pins version 0.10.0

  @B-013 @boundary
  Scenario: A shipped version never changes
    Given the embedded schemas, manifest and templates for version 0.1.0 as the first published package shipped them
    When a later tool's embedded files for version 0.1.0 are read
    Then they are byte-identical

  @B-014
  Scenario: The pinned version's rules are the ones evaluated
    Given the tool ships schema versions 0.1.0 and 1.0.0
    And version 1.0.0 holds a rule version 0.1.0 does not
    And the manifest pins version 0.1.0
    And the root holds a specification that breaks only that rule
    When the check runs
    Then no violation is reported

  @B-015 @boundary
  Scenario: A shipped version's rules never change
    Given the rules version 0.1.0 held in the first published package
    When a later tool's rules for version 0.1.0 are enumerated
    Then they are the same rules

  @B-016
  Scenario: Upgrade keeps the consumer's manifest settings
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 and its frontmatter schemas and templates are version 0.1.0's
    And its manifest lowers one rule to a warning, disables another and renames the claims section
    And its manifest already carries a key version 0.2.0 defines, set to a value of the consumer's
    When upgrade runs against it
    Then the warning, the disabled rule and the renamed section are as the consumer set them
    And the key version 0.2.0 defines keeps the consumer's value
    And every other key version 0.2.0 defines is added

  @B-017
  Scenario: Upgrade prints what it changed
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 and its frontmatter schemas and templates are version 0.1.0's
    When upgrade runs against it
    Then the standard output names version 0.2.0 and each file rewritten
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

  @B-020
  Scenario: Upgrade beside a contradictory schema source is invalid configuration
    Given the root's manifest records an upstream schema source and explicitly selects the embedded source
    When upgrade runs against it
    Then nothing is written
    And the exit code is 3

  @B-021 @boundary
  Scenario: Upgrade under a version the tool does not ship rewrites nothing
    Given the tool ships schema version 0.1.0 only
    And the manifest pins version 7.0.0
    And a snapshot of every file under the root
    When upgrade runs against it
    Then no file under the root has changed
    And the standard error names version 7.0.0 and the versions the tool ships, as the check's does
    And the exit code is 3

  @B-021 @boundary
  Scenario: Upgrade under a malformed schema version rewrites nothing
    Given the manifest's schema version is 1
    And a snapshot of every file under the root
    When upgrade runs against it
    Then no file under the root has changed
    And the standard error names 1, as the check's does
    And the exit code is 3

  @B-022
  Scenario: Version 0.1.0 accepts a title and a description on an epic
    Given the manifest pins version 0.1.0
    And the root holds an epic whose frontmatter carries a title and a description
    When the check runs
    Then no frontmatter violation is reported for the epic

  @B-023
  Scenario Outline: Version 0.1.0 rejects an empty <key> on an epic
    Given the manifest pins version 0.1.0
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
    Given the manifest pins version 0.1.0 and records an upstream schema source
    When the check runs
    Then exactly version 0.1.0's rule ids are evaluated

  @B-029 @boundary
  Scenario: Upgrade never writes the embedded schemas over the on-disk source
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 with the on-disk source selected
    When upgrade runs against it
    Then no frontmatter schema under the root is version 0.2.0's embedded schema

  @B-030
  Scenario: Upgrade says which schemas it skipped and why
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 with the on-disk source selected
    When upgrade runs against it
    Then the output names each frontmatter schema it skipped
    And the reason it skipped them

  @B-031
  Scenario: Upgrade still rewrites the templates under the on-disk source
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 with the on-disk source selected
    And the root's templates are version 0.1.0's
    When upgrade runs against it
    Then the templates are version 0.2.0's
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
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 and records an upstream schema source at a version of its own
    When upgrade runs against it
    Then the manifest still records the upstream source at that version with the same content hash

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

  @B-039
  Scenario Outline: A schema version that is not major.minor.patch is invalid configuration
    Given the manifest's schema version is <value>
    When the check runs
    Then the standard error names <value>
    And the standard output is empty
    And the exit code is 3

    Examples:
      | value  |
      | 1      |
      | 0.1    |
      | v0.1.0 |

  @B-040
  Scenario: A new rule is a new major
    Given two schema versions the tool ships, one higher than the other by version precedence
    And the higher one holds a rule the lower one does not
    When their numbers are compared
    Then the higher one's major is greater than the lower one's

  @B-041
  Scenario: Listing names every shipped version
    Given the tool ships schema versions 0.1.0 and 0.2.0
    When the shipped versions are listed
    Then the standard output is 0.1.0 and 0.2.0, one to a line
    And the exit code is 0

  @B-042
  Scenario: Listing orders versions by precedence
    Given the tool ships schema versions 0.9.0 and 0.10.0
    When the shipped versions are listed
    Then 0.9.0 is printed before 0.10.0

  @B-043
  Scenario: Pinning a newer version moves the pin up
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the manifest pins version 0.1.0
    When version 0.2.0 is pinned
    Then the manifest pins version 0.2.0
    And the exit code is 0

  @B-044
  Scenario: Pinning an older version moves the pin down
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the manifest pins version 0.2.0
    When version 0.1.0 is pinned
    Then the manifest pins version 0.1.0
    And the exit code is 0

  @B-045
  Scenario: Pinning changes only the version in the manifest
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the manifest pins version 0.1.0
    And its manifest lowers one rule to a warning, disables another and renames the claims section
    And a snapshot of the manifest
    When version 0.2.0 is pinned
    Then the manifest differs from the snapshot only in the version it pins

  @B-046 @boundary
  Scenario: Pinning touches no other file
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.1.0 and its frontmatter schemas and templates are version 0.1.0's
    And the root holds specifications, records and companions
    And a snapshot of every file under the root
    When version 0.2.0 is pinned
    Then the only file changed is the manifest
    And the frontmatter schemas and the templates are still version 0.1.0's

  @B-047
  Scenario: Pinning adds a version to a manifest that has none
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the manifest carries no schema version
    When version 0.2.0 is pinned
    Then the manifest pins version 0.2.0
    And every other manifest key keeps its value

  @B-048
  Scenario Outline: Pinning a version the tool does not ship is a not-found failure
    Given the tool ships schema version 0.1.0 only
    And a snapshot of every file under the root
    When version <value> is pinned
    Then the standard error names <value> and the versions the tool ships
    And no file under the root has changed
    And the exit code is 4

    Examples:
      | value |
      | 7.0.0 |
      | 0.1   |

  @B-049
  Scenario: Pinning under a missing root is a missing-input failure
    When a version is pinned against a path that is not a directory
    Then nothing is written
    And the exit code is 2

  @B-050
  Scenario: Pinning without a manifest is a missing-input failure
    Given the manifest has been removed from the root
    When a version is pinned
    Then nothing is written
    And the exit code is 2

  @B-051
  Scenario: Pinning in an invalid manifest is invalid configuration
    Given the root's manifest is not valid for a reason other than its schema version
    When a version is pinned
    Then nothing is written
    And the exit code is 3

  @B-052 @boundary
  Scenario: Upgrade never moves the pin
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.1.0 and its frontmatter schemas and templates are version 0.2.0's
    When upgrade runs against it
    Then the manifest pins version 0.1.0

  @B-053
  Scenario: A latest-patch policy floats to the newest patch
    Given the tool ships schema versions 0.1.0, 0.1.1 and 0.2.0
    And the manifest pins version 0.1.0 with the pin policy latest-patch
    When the check runs with JSON output
    Then the document names schema version 0.1.1

  @B-054
  Scenario: A latest-minor policy floats to the newest minor
    Given the tool ships schema versions 0.1.0, 0.2.0 and 1.0.0
    And the manifest pins version 0.1.0 with the pin policy latest-minor
    When the check runs with JSON output
    Then the document names schema version 0.2.0

  @B-055 @boundary
  Scenario: A pin is exact unless a policy floats it
    Given the tool ships schema versions 0.1.0, 0.1.1 and 0.2.0
    And the manifest pins version 0.1.0 with no pin policy
    When the check runs with JSON output
    Then the document names schema version 0.1.0

  @B-056 @boundary
  Scenario Outline: A pin policy the tool does not define is invalid configuration
    Given the manifest pins version 0.1.0 with the pin policy <policy>
    When the check runs
    Then the standard error names <policy>
    And the standard output is empty
    And the exit code is 3

    Examples:
      | policy       |
      | latest-major |
      | latest       |

  @B-057
  Scenario: Pinning without a version offers the shipped versions
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And an interactive terminal
    When a version is pinned without naming one
    Then versions 0.1.0 and 0.2.0 are offered as a list to choose from with the arrow keys

  @B-058
  Scenario: The version chosen from the list is pinned
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the manifest pins version 0.1.0
    And an interactive terminal
    When a version is pinned without naming one and 0.2.0 is chosen
    Then the manifest pins version 0.2.0

  @B-059
  Scenario: Upgrade that only adds a manifest key succeeds
    Given the tool ships schema versions 0.1.0 and 0.2.0
    And the root is pinned to version 0.2.0 and its frontmatter schemas and templates are already version 0.2.0's
    And its manifest lacks a key version 0.2.0 defines
    When upgrade runs against it
    Then the exit code is 0
